﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.CQRS;
using HigLabo.Core;
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
        // private readonly AuthSettings _authSettings;
        private readonly IQueryExecutor _queryExecutor;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ITokenService _tokenService;

        public AccountService(
            IQueryExecutor queryExecutor,
            ICommandExecutor commandExecutor,
            ITokenService tokenService)
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
            
            var jwtToken = _tokenService.GenerateJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);

            _commandExecutor.Execute(new InsertRefreshToken(refreshToken));

            _tokenService.RemoveOldRefreshTokens(user);
            _commandExecutor.Execute(new UpdateUser(user));

            var response = user.MapTo<AuthenticatedResponse>();

            response.JwtToken = jwtToken;
            response.RefreshToken = refreshToken.Token;
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

            var response = user.MapTo<AuthenticatedResponse>();
            
            response.JwtToken = jwtToken;
            response.RefreshToken = newRefreshToken.Token;
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
            var dbUserModel = _queryExecutor
                .Execute(new FetchUserByEmailAddress(model.Email));
            
            if (dbUserModel != null)
            {
                if (dbUserModel.IsVerified)
                {
                    throw new UserAlreadyExistsException();
                }

                dbUserModel.VerificationToken = TokenService.RandomTokenString();
                dbUserModel.DateModified = DateTime.Now;
                _commandExecutor.Execute(new UpdateUser(dbUserModel));
                return;
            }

            var user = model.Map((BaseUser) new object());
            user.IsActive = true;
            user.VerificationToken = TokenService.RandomTokenString();
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
        

        public void VerifyEmail(string token)
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
            if (user == null) return;

            // create reset token that expires after 1 day
            user.ResetToken = TokenService.RandomTokenString();
            user.DateResetTokenExpires = DateTime.UtcNow.AddDays(1);

            _commandExecutor.Execute(new UpdateUser(user));
        }

        public void ValidateResetToken(ValidateResetTokenRequest model)
        {
            var user = _queryExecutor
                .Execute(new FetchUserByResetToken(model.Token));

            if (user == null)
            {
                throw new InvalidTokenException(AuthenticationConstants.InvalidAuthenticationMessage);
            }
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

            return user.MapTo<AccountResponse>();
        }

        public AccountResponse Create(CreateRequest model)
        {
            if (_queryExecutor.Execute(new FetchUserByEmailAddress(model.Email)).Email is not null)
            {
                throw new UserAlreadyExistsException();
            }

            var user = model.Map((BaseUser) new object());
            user.DateCreated = DateTime.UtcNow;
            user.DateVerified = DateTime.UtcNow;

            user.PasswordHash = BC.HashPassword(model.Password);

            _commandExecutor.Execute(new InsertUser(user));

            return user.MapTo<AccountResponse>();
        }

        public AccountResponse Update(int id, UpdateRequest model)
        {
            var user = _tokenService.GetAccount(id);

            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = BC.HashPassword(model.Password);
            }
            
            model.Map(user);
            user.DateModified = DateTime.UtcNow;

            _commandExecutor.Execute(new UpdateUser(user));

            var response = user.MapTo<AccountResponse>();
            return response;
        }

        public void Delete(int id)
        {
            _commandExecutor.Execute(new DeleteUserById(id));
        }
    }
}