using Ntk8.Dto;
using Ntk8.Models;

namespace Ntk8.Services
{
    public interface IAccountService
    {
        IUserEntity? CurrentUser { get; }
        bool IsUserAuthenticated { get; }
        AuthenticatedResponse AuthenticateUser(AuthenticateRequest authenticationRequest);
        AuthenticatedResponse RegisterUser(RegisterRequest registerRequest);
        void VerifyUserByEmail(VerifyUserByEmailRequest? request);
        void VerifyUserByVerificationToken(VerifyUserByVerificationTokenRequest? request);
        (string resetToken, IUserEntity user) ResetUserPassword(ForgotPasswordRequest forgotPasswordRequest);
        void ResetUserPassword(ResetPasswordRequest resetPasswordRequest);
    }
}