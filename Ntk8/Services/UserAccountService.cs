using System;
using Dapper.CQRS;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Helpers;
using Ntk8.Models;
using BC = BCrypt.Net.BCrypt;

namespace Ntk8.Services
{
    public class UserAccountService<T> : IUserAccountService where T : class, IBaseUser, new()
    {
        public static int VERIFICATION_TOKEN_TTL = 14400; // in seconds.
        public static int RESET_TOKEN_TTL = 43200; // in seconds
        
        private readonly IQueryExecutor _queryExecutor;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ITokenService _tokenService;
        private readonly IAuthSettings _authSettings;
        private readonly IBaseUser _baseUser;
        private readonly Ntk8CustomSqlStatements? _statements;

        public UserAccountService(
            IQueryExecutor? queryExecutor,
            ICommandExecutor? commandExecutor,
            ITokenService? tokenService,
            IAuthSettings? authSettings,
            IBaseUser? baseUser,
            Ntk8CustomSqlStatements? statements)
        {
            _queryExecutor = queryExecutor ?? throw new ArgumentNullException();
            _commandExecutor = commandExecutor ?? throw new ArgumentNullException();
            _tokenService = tokenService ?? throw new ArgumentNullException();
            _authSettings = authSettings ?? throw new ArgumentNullException();
            _baseUser = baseUser ?? throw new ArgumentNullException();
            _statements = statements;
        }

        /// <summary>
        /// Authenticate will fetch a user by their email address, ensure that the user is verified, and then make sure that their passwords match.
        /// A refresh and JWT token is also generated for the user and send back to the caller.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="UserNotFoundException"></exception>
        /// <exception cref="InvalidPasswordException"></exception>
        public AuthenticatedResponse AuthenticateUser(AuthenticateRequest model)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByEmailAddress<T>(model.Email));
            
            if (user is null)
            {
                throw new UserNotFoundException();
            }

            if (!user.IsVerified)
            {
                throw new UserIsNotVerifiedException();
            }

            if (!BC.Verify(model.Password, user.PasswordHash))
            {
                throw new InvalidPasswordException();
            }

            var jwtToken = _tokenService.GenerateJwtToken(user.Id, user.Roles);

            var refreshToken = _tokenService.GenerateRefreshToken();
            refreshToken.UserId = user.Id;

            if (user.RefreshToken is not null)
            {
                _commandExecutor.Execute(new InvalidateRefreshToken(user.RefreshToken.Token));
            }
            
            _commandExecutor.Execute(new InsertRefreshToken(refreshToken));

            var response = user.MapFromTo(new AuthenticatedResponse());
            
            response.JwtToken = jwtToken.Token;
            _tokenService.SetRefreshTokenCookie(refreshToken.Token);
            return response;
        }

        public void RegisterUser(RegisterRequest model)
        {
            var existingUser = _queryExecutor
                .Execute(new FetchUserByEmailAddress<T>(model.Email ?? string.Empty)
                {
                    Sql = _statements.FetchUserByEmailAddressStatement
                });
            
            if (existingUser is not null)
            {
                if (existingUser.IsVerified)
                {
                    throw new UserAlreadyExistsException();
                }
                
                existingUser.DateModified = DateTime.UtcNow;
                existingUser.DateResetTokenExpires = DateTime.UtcNow
                    .AddSeconds(_authSettings.UserVerificationTokenTTL == 0
                        ? VERIFICATION_TOKEN_TTL
                        : _authSettings.UserVerificationTokenTTL);

                _commandExecutor.Execute(new UpdateUser(existingUser));
                return;
            }

            var user = model.MapFromTo((T)_baseUser);
            
            user.IsActive = true;
            user.VerificationToken = _tokenService.RandomTokenString();
            user.DateCreated = DateTime.UtcNow;
            user.DateModified = DateTime.UtcNow;
            user.DateResetTokenExpires = DateTime.UtcNow
                .AddSeconds(_authSettings.UserVerificationTokenTTL == 0
                    ? VERIFICATION_TOKEN_TTL
                    : _authSettings.UserVerificationTokenTTL);
            user.PasswordHash = BC.HashPassword(model.Password);

            _commandExecutor
                .Execute(new InsertUser(user)
                {
                    Sql = _statements.InsertUserStatement
                });
        }

        public void AutoVerifyUser(RegisterRequest model)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByEmailAddress<T>(model.Email ?? string.Empty));

            if (user.IsVerified)
            {
                return;
            }
            
            user.DateVerified = DateTime.UtcNow;
            user.VerificationToken = null;

            _commandExecutor.Execute(new UpdateUser(user));
        }

        public void VerifyUserByVerificationToken(string token)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByVerificationToken(token));

            if (user is null)
            {
                throw new InvalidValidationTokenException();
            }

            if (user.DateResetTokenExpires != null
                && user.DateResetTokenExpires
                    .Value
                    .AddSeconds(_authSettings.UserVerificationTokenTTL == 0
                        ? VERIFICATION_TOKEN_TTL
                        : _authSettings.UserVerificationTokenTTL) < DateTime.UtcNow)
            {
                throw new VerificationTokenExpiredException();
            }

            if (user.IsVerified)
            {
                throw new UserIsVerifiedException();
            }

            user.DateVerified = DateTime.UtcNow;
            user.VerificationToken = null;
            user.IsActive = true;

            _commandExecutor.Execute(new UpdateUser(user));
        }

        /// <summary>
        /// Set's the user with a password reset token, which will expire in a few hours (as per app-settings configured.)
        /// </summary>
        /// <param name="model"></param>
        /// <para>Tip: Catch the UserNotFoundException to prevent email enumeration attacks.</para>
        public string ForgotUserPassword(ForgotPasswordRequest model)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByEmailAddress<T>(model.Email));
            
            if (user is null)
            {
                throw new UserNotFoundException();
            }

            if (user.ResetToken is not null)
            {
                user.DateResetTokenExpires = DateTime.UtcNow
                    .AddSeconds(_authSettings.PasswordResetTokenTTL == 0
                        ? RESET_TOKEN_TTL
                        : _authSettings.PasswordResetTokenTTL);
                
                _commandExecutor.Execute(new UpdateUser(user));
                return user.ResetToken;
            }
            
            user.ResetToken = _tokenService.RandomTokenString();
            user.DateResetTokenExpires = DateTime.UtcNow
                .AddSeconds(_authSettings.PasswordResetTokenTTL == 0
                    ? RESET_TOKEN_TTL
                    : _authSettings.PasswordResetTokenTTL);

            _commandExecutor.Execute(new UpdateUser(user));

            return user.ResetToken;
        }
        
        public void ResetUserPassword(ResetPasswordRequest model)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByResetToken(model.Token));

            if (user is null)
            {
                throw new InvalidResetTokenException();
            }
            
            user.PasswordHash = BC.HashPassword(model.Password);
            user.DateOfPasswordReset = DateTime.UtcNow;
            user.ResetToken = null;
            user.DateResetTokenExpires = null;

            _commandExecutor.Execute(new UpdateUser(user));
        }

        public UserAccountResponse GetUserById(int id)
        {
            var user = _queryExecutor
                .Execute(new FetchUserById<T>(id));
            
            if (user is null)
            {
                throw new UserNotFoundException();
            }

            return user.MapFromTo(new UserAccountResponse());
        }

        public UserAccountResponse UpdateUser(int id, UpdateRequest model)
        {
            var user = _queryExecutor.Execute(new FetchUserById<T>(id));

            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = BC.HashPassword(model.Password);
            }
            
            user.DateModified = DateTime.UtcNow;

            _commandExecutor.Execute(new UpdateUser(user));

            var response = user.MapFromTo(new UserAccountResponse());
            return response;
        }

        public void DeleteUser(int id)
        {
            _commandExecutor.Execute(new DeleteUserById(id));
        }
    }
}