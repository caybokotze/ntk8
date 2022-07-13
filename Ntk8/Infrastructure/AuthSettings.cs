using Ntk8.Utilities;

namespace Ntk8.Models
{
    public interface IGlobalSettings
    {
        public bool UseJwt { get; set; }    
    }

    public class GlobalSettings : IGlobalSettings
    {
        public bool UseJwt { get; set; }
    }
    
    public interface IAuthSettings
    {
        string? RefreshTokenSecret { get; set; }
        int RefreshTokenTTL { get; set; }
        int JwtTTL { get; set; }
        int PasswordResetTokenTTL { get; set; }
        int UserVerificationTokenTTL { get; set; }
        int RefreshTokenLength { get; set; }
    }

    public class AuthSettings : IAuthSettings
    {
        public AuthSettings()
        {
            RefreshTokenSecret = TokenHelpers.GenerateCryptoRandomToken();
            RefreshTokenTTL = 3600;
            JwtTTL = 900;
            PasswordResetTokenTTL = 900;
            UserVerificationTokenTTL = 900;
        }
        
        public string? RefreshTokenSecret { get; set; }
        public int RefreshTokenTTL { get; set; }
        public int JwtTTL { get; set; }
        public int PasswordResetTokenTTL { get; set; }
        public int UserVerificationTokenTTL { get; set; }
        public int RefreshTokenLength { get; set; }
    }
}