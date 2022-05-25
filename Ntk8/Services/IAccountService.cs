using Ntk8.Dto;
using Ntk8.Models;

namespace Ntk8.Services
{
    public interface IAccountService
    {
        bool IsUserAuthenticated { get; }
        AuthenticatedResponse AuthenticateUser(AuthenticateRequest model);
        void RegisterUser(RegisterRequest model);
        void VerifyUserByVerificationToken(string token);
        string GetPasswordResetToken(ForgotPasswordRequest model);
        void ResetUserPassword(ResetPasswordRequest model);
        UserAccountResponse GetUserById(int id);
        UserAccountResponse UpdateUser(int id, UpdateRequest model);
        void DeleteUser(int id);
        void AutoVerifyUser(RegisterRequest model);
    }
}