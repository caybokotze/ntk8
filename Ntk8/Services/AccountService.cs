﻿using System;
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
        public static int VERIFICATION_TOKEN_TTL = 14400; // in seconds.
        public static int RESET_TOKEN_TTL = 43200; // in seconds
        
        private readonly IQueryExecutor _queryExecutor;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ITokenService _tokenService;
        private readonly IAuthSettings _authSettings;

        public AccountService(
            IQueryExecutor queryExecutor,
            ICommandExecutor commandExecutor,
            ITokenService tokenService,
            IAuthSettings authSettings)
        {
            _queryExecutor = queryExecutor;
            _commandExecutor = commandExecutor;
            _tokenService = tokenService;
            _authSettings = authSettings;
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
        public AuthenticatedResponse Authenticate(AuthenticateRequest model)
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
            var refreshToken = _tokenService.GenerateRefreshToken();
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
        /// <returns>A new instance of AuthenticatedResponse, which includes some basic user information</returns>
        public AuthenticatedResponse GenerateNewRefreshToken(string token)
        {
            var newRefreshToken = _tokenService
                .GenerateRefreshToken();
            
            var user = _tokenService.FetchUserAndCheckIfRefreshTokenIsActive(token);
            
            var refreshToken = user
                .RefreshTokens
                .First();
            
            _tokenService.RevokeRefreshToken(refreshToken);

            _commandExecutor.Execute(new InsertRefreshToken(newRefreshToken));

            var jwtToken = _tokenService.GenerateJwtToken(user);

            var response = user.MapFromTo<BaseUser, AuthenticatedResponse>();
            
            response.JwtToken = jwtToken;
            _tokenService.SetRefreshTokenCookie(refreshToken.Token);
            
            return response;
        }

        public void Register(RegisterRequest model)
        {
            var existingUser = _queryExecutor
                .Execute(new FetchUserByEmailAddress(model.Email));
            
            if (existingUser is not null)
            {
                if (existingUser.IsVerified)
                {
                    throw new UserAlreadyExistsException();
                }

                existingUser.VerificationToken = _tokenService.RandomTokenString();
                existingUser.DateModified = DateTime.UtcNow;
                existingUser.DateResetTokenExpires = DateTime.UtcNow
                    .AddSeconds(_authSettings.UserVerificationTokenTTL == 0
                        ? VERIFICATION_TOKEN_TTL
                        : _authSettings.UserVerificationTokenTTL);

                _commandExecutor.Execute(new UpdateUser(existingUser));
                return;
            }

            var user = model.MapFromTo<RegisterRequest, BaseUser>();
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

        public void VerifyUserByVerificationToken(string token)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByVerificationToken(token));

            if (user is null)
            {
                throw new UserNotFoundException("User does not exist");
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
        public void ForgotPassword(ForgotPasswordRequest model)
        {
            var user = _queryExecutor.Execute(new FetchUserByEmailAddress(model.Email));
            
            if (user == null)
            {
                throw new UserNotFoundException("Email address does not exist.");
            }

            user.ResetToken = _tokenService.RandomTokenString();
            user.DateResetTokenExpires = DateTime.UtcNow
                .AddSeconds(_authSettings.PasswordResetTokenTTL == 0
                    ? RESET_TOKEN_TTL
                    : _authSettings.PasswordResetTokenTTL);

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