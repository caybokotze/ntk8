using Ntk8.Dto;
using Ntk8.Models;

namespace Ntk8.Services
{
    public interface IAccountService
    {
        AuthenticatedResponse Authenticate(AuthenticateRequest model, string ipAddress);
        AuthenticatedResponse RevokeRefreshTokenAndGenerateNewRefreshToken(string token, string ipAddress);
        void RevokeRefreshToken(RefreshToken token, string ipAddress, string newToken = null);
        void Register(RegisterRequest model, string origin);
        void VerifyEmailByVerificationToken(string token);
        void ForgotPassword(ForgotPasswordRequest model, string origin);
        void ResetPassword(ResetPasswordRequest model);
        AccountResponse GetById(int id);
        BaseUser GetUserByEmail(string email);
        AccountResponse Update(int id, UpdateRequest model);
        void Delete(int id);
        void AutoVerifyUser(RegisterRequest model);
    }
}