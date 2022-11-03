using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ntk8.DatabaseServices;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Models;
using Ntk8.Utilities;
using BC = BCrypt.Net.BCrypt;

namespace Ntk8.Services
{
    public class AccountService<T> : IAccountService where T : class, IUserEntity, new()
    {
        private readonly IUserCommands _userCommands;
        private readonly IUserQueries _userQueries;
        private readonly ITokenService _tokenService;
        private readonly IAuthSettings _authSettings;
        private readonly IUserEntity _userEntity;
        private readonly ILogger<AccountService<T>> _logger;
        private readonly IGlobalSettings _globalSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(
            IUserCommands userCommands,
            IUserQueries userQueries,
            ITokenService tokenService,
            IAuthSettings authSettings,
            IUserEntity userEntity,
            ILogger<AccountService<T>> logger,
            IGlobalSettings globalSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _userCommands = userCommands;
            _userQueries = userQueries;
            _tokenService = tokenService;
            _authSettings = authSettings;
            _userEntity = userEntity;
            _logger = logger;
            _globalSettings = globalSettings;
            _httpContextAccessor = httpContextAccessor;
        }

        public IUserEntity? CurrentUser => _httpContextAccessor.HttpContext.GetCurrentUser();
        public bool IsUserAuthenticated => CurrentUser is not null;

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
            var user = _userQueries.FetchUserByEmailAddress<T>(authenticationRequest.Email 
                                                                         ?? string.Empty);
            
            if (user is null)
            {
                throw new UserNotFoundException();
            }

            if (!user.IsVerified)
            {
                throw new UserIsNotVerifiedException();
            }

            if (!BC.Verify($"{authenticationRequest.Password}{user.PasswordSalt ?? string.Empty}", $"{user.PasswordHash}"))
            {
                throw new InvalidPasswordException();
            }

            var jwtToken = _tokenService.GenerateJwtToken(user.Id, user.Roles);

            _tokenService.RevokeRefreshToken(user.RefreshToken);

            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            try
            {
                _userCommands.InsertRefreshToken(refreshToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                _logger.LogInformation("Trying again");
                
                var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);
                _userCommands.InsertRefreshToken(newRefreshToken);
            }

            var response = user.MapFromTo(new AuthenticatedResponse());

            if (_globalSettings.UseJwt)
            {
                response.JwtToken = jwtToken.Token;
            }
            
            _tokenService.SetRefreshTokenCookie(refreshToken.Token ?? string.Empty);
            return response;
        }

        
        public void RegisterUser(RegisterRequest registerRequest)
        {
            var existingUser = _userQueries
                .FetchUserByEmailAddress<T>(registerRequest.Email ?? string.Empty);
            
            if (existingUser is not null)
            {
                if (existingUser.IsVerified)
                {
                    throw new UserAlreadyExistsException();
                }
                
                existingUser.DateModified = DateTime.UtcNow;
                existingUser.DateResetTokenExpires = DateTime.UtcNow
                    .AddSeconds(_authSettings.UserVerificationTokenTTL);
                _userCommands.UpdateUser(existingUser);
                return;
            }

            var user = registerRequest.MapFromTo((T)_userEntity);
            
            user.IsActive = true;
            user.VerificationToken = TokenHelpers.GenerateCryptoRandomToken();
            user.DateCreated = DateTime.UtcNow;
            user.DateModified = DateTime.UtcNow;
            user.DateResetTokenExpires = DateTime.UtcNow
                .AddSeconds(_authSettings.UserVerificationTokenTTL);
            user.PasswordSalt = TokenHelpers.GenerateCryptoRandomToken();
            user.PasswordHash = BC.HashPassword($"{registerRequest.Password}{user.PasswordSalt}");

            _userCommands.InsertUser(user);
        }

        /// <summary>
        /// Verifies the user using the email
        /// </summary>
        /// <param name="email"></param>
        /// <exception cref="UserNotFoundException"></exception>
        /// <exception cref="UserIsVerifiedException"></exception>
        public void VerifyUserByEmail(string? email)
        {
            if (email is null)
            {
                throw new InvalidEmailAddressException("The email provided was null");
            }
            
            var user = _userQueries.FetchUserByEmailAddress<T>(email);

            if (user is null)
            {
                throw new UserNotFoundException();
            }

            if (user.IsVerified)
            {
                throw new UserIsVerifiedException();
            }

            user.IsActive = true;
            user.DateVerified = DateTime.UtcNow;
            user.VerificationToken = null;
            
            _userCommands.UpdateUser(user);
        }

        /// <summary>
        /// Verifies the user using the verification token
        /// </summary>
        /// <param name="token"></param>
        /// <exception cref="InvalidValidationTokenException"></exception>
        /// <exception cref="VerificationTokenExpiredException"></exception>
        /// <exception cref="UserIsVerifiedException"></exception>
        public void VerifyUserByVerificationToken(string token)
        {
            var user = _userQueries.FetchUserByVerificationToken<T>(token);

            if (user is null)
            {
                throw new InvalidValidationTokenException();
            }

            if (user.DateResetTokenExpires != null
                && user.DateResetTokenExpires
                    .Value
                    .AddSeconds(_authSettings.UserVerificationTokenTTL) < DateTime.UtcNow)
            {
                throw new VerificationTokenExpiredException();
            }

            if (user.IsVerified)
            {
                throw new UserIsVerifiedException();
            }

            user.DateVerified = DateTime.UtcNow;
            user.DateOfPasswordReset = null;
            user.VerificationToken = null;
            user.IsActive = true;

            _userCommands.UpdateUser(user);
        }

        /// <summary>
        /// Initialises the process of resetting the user password. This is the first step
        /// </summary>
        /// <param name="forgotPasswordRequest"></param>
        /// <returns></returns>
        /// <exception cref="UserNotFoundException"></exception>
        public (string resetToken, IUserEntity user) ResetUserPassword(ForgotPasswordRequest forgotPasswordRequest)
        {
            var user = _userQueries.FetchUserByEmailAddress<T>(forgotPasswordRequest.Email ?? string.Empty);
            
            if (user is null)
            {
                throw new UserNotFoundException();
            }

            if (user.ResetToken is not null)
            {
                user.DateResetTokenExpires = DateTime.UtcNow
                    .AddSeconds(_authSettings.PasswordResetTokenTTL);
                
                _userCommands.UpdateUser(user);
                
                return (user.ResetToken, user);
            }
            
            user.ResetToken = TokenHelpers.GenerateCryptoRandomToken();
            user.DateResetTokenExpires = DateTime.UtcNow
                .AddSeconds(_authSettings.PasswordResetTokenTTL);

            _userCommands.UpdateUser(user);

            return (user.ResetToken, user);
        }
        
        /// <summary>
        /// After the token has been sent this overload can be used to reset the password
        /// </summary>
        /// <param name="resetPasswordRequest"></param>
        /// <exception cref="InvalidResetTokenException"></exception>
        public void ResetUserPassword(ResetPasswordRequest resetPasswordRequest)
        {
            var user = _userQueries.FetchUserByResetToken<T>(resetPasswordRequest.Token ?? string.Empty);

            if (user is null)
            {
                throw new InvalidResetTokenException();
            }

            if (user.DateResetTokenExpires != null
                && user.DateResetTokenExpires
                    .Value
                    .AddSeconds(_authSettings.PasswordResetTokenTTL) < DateTime.UtcNow)
            {
                throw new PasswordResetTokenExpiredException();
            }

            user.PasswordSalt = TokenHelpers.GenerateCryptoRandomToken();
            user.PasswordHash = BC.HashPassword($"{resetPasswordRequest.Password}{user.PasswordSalt}");
            user.DateOfPasswordReset = DateTime.UtcNow;
            user.ResetToken = null;
            user.DateResetTokenExpires = null;

            _userCommands.UpdateUser(user);
        }
    }
}