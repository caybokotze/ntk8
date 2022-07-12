using System;
using Ntk8.DatabaseServices;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Models;
using Ntk8.Utilities;
using BC = BCrypt.Net.BCrypt;

namespace Ntk8.Services
{
    public class AccountService<T> : IAccountService where T : class, IBaseUser, new()
    {
        public static readonly int VERIFICATION_TOKEN_TTL = 14400; // in seconds.
        public static readonly int RESET_TOKEN_TTL = 43200; // in seconds

        private readonly INtk8Commands _ntk8Commands;
        private readonly INtk8Queries<T> _ntk8Queries;
        private readonly ITokenService _tokenService;
        private readonly IAuthSettings _authSettings;
        private readonly IBaseUser _baseUser;
        private readonly IAccountState _accountState;

        public AccountService(
            INtk8Commands ntk8Commands,
            INtk8Queries<T> ntk8Queries,
            ITokenService tokenService,
            IAuthSettings authSettings,
            IBaseUser baseUser,
            IAccountState accountState)
        {
            _ntk8Commands = ntk8Commands;
            _ntk8Queries = ntk8Queries;
            _tokenService = tokenService;
            _authSettings = authSettings;
            _baseUser = baseUser;
            _accountState = accountState;
        }

        public IBaseUser? CurrentUser => _accountState.CurrentUser;
        public bool IsUserAuthenticated => _accountState.CurrentJwtToken is not null 
                                           || _accountState.CurrentRefreshToken is not null;

        /// <summary>
        /// Authenticate will fetch a user by their email address, ensure that the user is verified, and then make sure that their passwords match.
        /// A refresh and JWT token is also generated for the user and send back to the caller.
        /// </summary>
        /// <param name="authenticationRequest"></param>
        /// <returns></returns>
        /// <exception cref="UserNotFoundException"></exception>
        /// <exception cref="InvalidPasswordException"></exception>
        public AuthenticatedResponse AuthenticateUser(AuthenticateRequest authenticationRequest)
        {
            var user = _ntk8Queries.FetchUserByEmailAddress(authenticationRequest.Email ?? string.Empty);
            
            if (user is null)
            {
                throw new UserNotFoundException();
            }

            if (!user.IsVerified)
            {
                throw new UserIsNotVerifiedException();
            }

            if (!BC.Verify(authenticationRequest.Password, user.PasswordHash))
            {
                throw new InvalidPasswordException();
            }

            var jwtToken = _tokenService.GenerateJwtToken(user.Id, user.Roles);

            var refreshToken = _tokenService.GenerateRefreshToken();
            refreshToken.UserId = user.Id;

            if (user.RefreshToken is not null)
            {
                _ntk8Commands.InvalidateRefreshToken(user.RefreshToken?.Token ?? string.Empty);
            }

            _ntk8Commands.InsertRefreshToken(refreshToken);

            var response = user.MapFromTo(new AuthenticatedResponse());
            
            response.JwtToken = jwtToken.Token;
            _tokenService.SetRefreshTokenCookie(refreshToken.Token ?? string.Empty);
            return response;
        }

        
        public void RegisterUser(RegisterRequest registerRequest)
        {
            var existingUser = _ntk8Queries
                .FetchUserByEmailAddress(registerRequest.Email ?? string.Empty);
            
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
                _ntk8Commands.UpdateUser(existingUser);
                return;
            }

            var user = registerRequest.MapFromTo((T)_baseUser);
            
            user.IsActive = true;
            user.VerificationToken = _tokenService.RandomTokenString();
            user.DateCreated = DateTime.UtcNow;
            user.DateModified = DateTime.UtcNow;
            user.DateResetTokenExpires = DateTime.UtcNow
                .AddSeconds(_authSettings.UserVerificationTokenTTL == 0
                    ? VERIFICATION_TOKEN_TTL
                    : _authSettings.UserVerificationTokenTTL);
            user.PasswordHash = BC.HashPassword(registerRequest.Password);

            _ntk8Commands.InsertUser(user);
        }

        public void AutoVerifyUser(RegisterRequest registerRequest)
        {
            var user = _ntk8Queries.FetchUserByEmailAddress(registerRequest.Email ?? string.Empty);

            if (user is null)
            {
                throw new UserNotFoundException();
            }

            if (user.IsVerified)
            {
                throw new UserIsVerifiedException();
            }
            
            user.DateVerified = DateTime.UtcNow;
            user.VerificationToken = null;
            
            _ntk8Commands.UpdateUser(user);
        }

        public void VerifyUserByVerificationToken(string token)
        {
            var user = _ntk8Queries.FetchUserByVerificationToken(token);

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

            _ntk8Commands.UpdateUser(user);
        }

        /// <summary>
        /// Set's the user with a password reset token, which will expire in a few hours (as per app-settings configured.)
        /// </summary>
        /// <param name="forgotPasswordRequest"></param>
        /// <para>Tip: Catch the UserNotFoundException to prevent email enumeration attacks.</para>
        public string GetPasswordResetToken(ForgotPasswordRequest forgotPasswordRequest)
        {
            var user = _ntk8Queries.FetchUserByEmailAddress(forgotPasswordRequest.Email ?? string.Empty);
            
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
                
                _ntk8Commands.UpdateUser(user);
                return user.ResetToken;
            }
            
            user.ResetToken = _tokenService.RandomTokenString();
            user.DateResetTokenExpires = DateTime.UtcNow
                .AddSeconds(_authSettings.PasswordResetTokenTTL == 0
                    ? RESET_TOKEN_TTL
                    : _authSettings.PasswordResetTokenTTL);

            _ntk8Commands.UpdateUser(user);

            return user.ResetToken;
        }
        
        public void ResetUserPassword(ResetPasswordRequest resetPasswordRequest)
        {
            var user = _ntk8Queries.FetchUserByResetToken(resetPasswordRequest.Token ?? string.Empty);

            if (user is null)
            {
                throw new InvalidResetTokenException();
            }
            
            user.PasswordHash = BC.HashPassword(resetPasswordRequest.Password);
            user.DateOfPasswordReset = DateTime.UtcNow;
            user.ResetToken = null;
            user.DateResetTokenExpires = null;

            _ntk8Commands.UpdateUser(user);
        }

        public UserAccountResponse GetUserById(int id)
        {
            var user = _ntk8Queries.FetchUserById(id);

            if (user is null)
            {
                throw new UserNotFoundException();
            }

            return user.MapFromTo(new UserAccountResponse());
        }

        public UserAccountResponse UpdateUser(int id, UpdateRequest updateRequest)
        {
            var user = _ntk8Queries.FetchUserById(id);
            
            if (user is null)
            {
                throw new UserNotFoundException();
            }

            if (!string.IsNullOrEmpty(updateRequest.Password))
            {
                user.PasswordHash = BC.HashPassword(updateRequest.Password);
            }
            
            user.DateModified = DateTime.UtcNow;

            _ntk8Commands.UpdateUser(user);

            var response = user.MapFromTo(new UserAccountResponse());
            return response;
        }

        public void DeleteUser(int id)
        {
            _ntk8Commands.DeleteUserById(id);
        }
    }
}