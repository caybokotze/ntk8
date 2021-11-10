using System;
using System.Linq;
using Dapper.CQRS;
using Ntk8.Constants;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Helpers;
using Ntk8.Models;
using BC = BCrypt.Net.BCrypt;

namespace Ntk8.Services
{
    public class AccountService : IAccountService
    {
        public static int USER_VERIFICATION_EXPIRATION_HOURS = 4;
        private readonly IQueryExecutor _queryExecutor;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ITokenService _tokenService;

        public AccountService(
            IQueryExecutor queryExecutor,
            ICommandExecutor commandExecutor,
            ITokenService tokenService,
            IAuthenticationContextService contextService)
        {
            _queryExecutor = queryExecutor;
            _commandExecutor = commandExecutor;
            _tokenService = tokenService;
        }
        
        /// <summary>
        /// Authenticate will fetch a user by their email address, ensure that the user is verified, and then make sure that their passwords match.
        /// A refresh and JWT token is also generated for the user and send back to the caller.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        /// <exception cref="UserNotFoundException"></exception>
        /// <exception cref="InvalidPasswordException"></exception>
        public AuthenticatedResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByEmailAddress(model.Email));

            if (user is null)
            {
                throw new UserNotFoundException("The user does not exist");
            }

            if (!user.IsVerified || 
                !BC.Verify(model.Password, user.PasswordHash))
            {
                throw new InvalidPasswordException("User is not verified or password is incorrect");
            }
            
            var roles = _queryExecutor.Execute(new FetchUserRolesForUserId(user.Id));
            user.Roles = roles;
            var jwtToken = _tokenService.GenerateJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);
            refreshToken.UserId = user.Id;
            
            _commandExecutor.Execute(new InsertRefreshToken(refreshToken));

            // _tokenService.RemoveOldRefreshTokens(user); todo: see if required.
            _commandExecutor.Execute(new UpdateUser(user));

            var response = user.MapFromTo<BaseUser, AuthenticatedResponse>();

            response.Roles = roles.Select(s => s.RoleName).ToArray();
            response.JwtToken = jwtToken;
            _tokenService.SetRefreshTokenCookie(refreshToken.Token);
            return response;
        }

        /// <summary>
        /// GenerateRefreshToken is responsible for generating a new refresh token for a user and making sure that all the old refresh tokens for that user is deleted.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress"></param>
        /// <returns>A new instance of AuthenticatedResponse, which includes some basic user information</returns>
        public AuthenticatedResponse RevokeRefreshTokenAndGenerateNewRefreshToken(
            string token,
            string ipAddress)
        {
            var newRefreshToken = _tokenService.GenerateRefreshToken(ipAddress);
            
            var user = _tokenService.FetchUserAndCheckIfRefreshTokenIsActive(token);
            
            var refreshToken = user
                .RefreshTokens
                .First();
            
            RevokeRefreshToken(refreshToken, ipAddress, newRefreshToken.Token);

            _commandExecutor.Execute(new InsertRefreshToken(newRefreshToken));

            var jwtToken = _tokenService.GenerateJwtToken(user);

            var response = user.MapFromTo<BaseUser, AuthenticatedResponse>();
            
            response.JwtToken = jwtToken;
            _tokenService.SetRefreshTokenCookie(refreshToken.Token);
            
            return response;
        }

        /// <summary>
        /// Revoke token will make sure that a token is set to invalid. It will also return the user for the associated refresh token.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ipAddress"></param>
        /// <param name="newToken"></param>
        public void RevokeRefreshToken(
            RefreshToken token, 
            string ipAddress, 
            string newToken = null)
        {
            token.DateRevoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            
            if (newToken is not null)
            {
                token.ReplacedByToken = newToken;
            }

            _commandExecutor.Execute(new UpdateRefreshToken(token));
        }

        public void Register(RegisterRequest model, string origin)
        {
            var existingUser = _queryExecutor
                .Execute(new FetchUserByEmailAddress(model.Email));
            
            if (existingUser is not null)
            {
                if (existingUser.IsVerified)
                {
                    throw new UserAlreadyExistsException();
                }

                existingUser.VerificationToken = TokenService.RandomTokenString();
                existingUser.DateModified = DateTime.UtcNow;
                existingUser.DateResetTokenExpires = DateTime.UtcNow.AddHours(USER_VERIFICATION_EXPIRATION_HOURS);
                _commandExecutor.Execute(new UpdateUser(existingUser));
                return;
            }

            var user = model.MapFromTo<RegisterRequest, BaseUser>();
            user.IsActive = true;
            user.VerificationToken = TokenService.RandomTokenString();
            user.DateCreated = DateTime.UtcNow;
            user.DateModified = DateTime.UtcNow;
            user.DateResetTokenExpires = DateTime.UtcNow.AddHours(USER_VERIFICATION_EXPIRATION_HOURS);
            user.PasswordHash = BC.HashPassword(model.Password);
            
            _commandExecutor
                .Execute(new InsertUser(user));
        }

        public void AutoVerifyUser(RegisterRequest model)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByEmailAddress(model.Email));

            if (user.IsVerified)
            {
                return;
            }
            
            user.DateVerified = DateTime.UtcNow;
            user.VerificationToken = null;

            _commandExecutor.Execute(new UpdateUser(user));
        }

        public BaseUser GetUserByEmail(string email)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByEmailAddress(email));

            return user;
        }

        public void VerifyEmailByVerificationToken(string token)
        {
            //todo: Make sure that the verification token hasn't' expired...
            
            var user = _queryExecutor
                .Execute(new FetchUserByVerificationToken(token));

            if (user.IsVerified)
            {
                throw new UserIsVerifiedException();
            }

            if (user is null)
            {
                throw new NullReferenceException("Verification failed. " +
                                                 "This record either no longer exists or the account has already been verified.");
            }

            user.DateVerified = DateTime.UtcNow;
            user.VerificationToken = null;
            user.IsActive = true;

            _commandExecutor.Execute(new UpdateUser(user));
        }

        public void ForgotPassword(ForgotPasswordRequest model, string origin)
        {
            var user = _queryExecutor.Execute(new FetchUserByEmailAddress(model.Email));

            // always return ok response to prevent email enumeration
            if (user == null)
            {
                return;
            }

            // create reset token that expires after 1 day
            user.ResetToken = TokenService.RandomTokenString();
            user.DateResetTokenExpires = DateTime.UtcNow.AddDays(1);

            _commandExecutor.Execute(new UpdateUser(user));
        }

        public void ValidateResetToken(ResetTokenRequest model)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByResetToken(model.Token));

            if (user == null)
            {
                throw new InvalidTokenException(AuthenticationConstants.InvalidAuthenticationMessage);
            }
            
            // todo: complete...
        }

        public void ResetPassword(ResetPasswordRequest model)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByResetToken(model.Token));

            if (user is null)
            {
                throw new InvalidTokenException(AuthenticationConstants.InvalidAuthenticationMessage);
            }
            
            user.PasswordHash = BC.HashPassword(model.Password);
            user.DateOfPasswordReset = DateTime.UtcNow;
            user.ResetToken = null;
            user.DateResetTokenExpires = null;

            _commandExecutor.Execute(new UpdateUser(user));
        }

        public AccountResponse GetById(int id)
        {
            var user = _queryExecutor
                .Execute(new FetchUserById(id));
            
            if (user is null)
            {
                throw new NoUserFoundException();
            }

            return user.MapFromTo<BaseUser, AccountResponse>();
        }

        public AccountResponse Update(int id, UpdateRequest model)
        {
            var user = _tokenService.GetAccount(id);

            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = BC.HashPassword(model.Password);
            }
            
            user.DateModified = DateTime.UtcNow;

            _commandExecutor.Execute(new UpdateUser(user));

            var response = user.MapFromTo<BaseUser, AccountResponse>();
            return response;
        }

        public void Delete(int id)
        {
            _commandExecutor.Execute(new DeleteUserById(id));
        }
    }
}