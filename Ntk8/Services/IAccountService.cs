using Ntk8.Dto;
using Ntk8.Models;

namespace Ntk8.Services
{
    public interface IAccountService
    {
        IUserEntity? CurrentUser { get; }
        bool IsUserAuthenticated { get; }
        AuthenticatedResponse AuthenticateUser(AuthenticateRequest authenticationRequest);
        void RegisterUser(RegisterRequest registerRequest);
        void VerifyUserByEmail(string email);
        void VerifyUserByVerificationToken(string token);
        (string resetToken, IUserEntity user) ResetUserPassword(ForgotPasswordRequest forgotPasswordRequest);
        void ResetUserPassword(ResetPasswordRequest resetPasswordRequest);
    }
}