using Ntk8.Dto;
using Ntk8.Models;

namespace Ntk8.Services
{
    public interface IAccountService
    {
        AuthenticatedResponse Authenticate(AuthenticateRequest model);
        AuthenticatedResponse GenerateNewRefreshToken(string token);
        void Register(RegisterRequest model);
        void VerifyUserByVerificationToken(string token);
        void ForgotPassword(ForgotPasswordRequest model);
        void ResetPassword(ResetPasswordRequest model);
        AccountResponse GetById(int id);
        BaseUser GetUserByEmail(string email);
        AccountResponse Update(int id, UpdateRequest model);
        void Delete(int id);
        void AutoVerifyUser(RegisterRequest model);
    }
}