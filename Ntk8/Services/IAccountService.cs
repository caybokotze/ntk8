using Ntk8.Dto;
using Ntk8.Models;

namespace Ntk8.Services
{
    public interface IAccountService
    {
        IBaseUser? CurrentUser { get; }
        bool IsUserAuthenticated { get; }
        AuthenticatedResponse AuthenticateUser(AuthenticateRequest authenticationRequest);
        void RegisterUser(RegisterRequest registerRequest);
        void VerifyUserByVerificationToken(string token);
        string GetPasswordResetToken(ForgotPasswordRequest forgotPasswordRequest);
        void ResetUserPassword(ResetPasswordRequest resetPasswordRequest);
        UserAccountResponse GetUserById(int id);
        UserAccountResponse UpdateUser(int id, UpdateRequest updateRequest);
        void DeleteUser(int id);
        void AutoVerifyUser(RegisterRequest registerRequest);
    }
}