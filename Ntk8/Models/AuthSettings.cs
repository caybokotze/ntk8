namespace Ntk8.Models
{
    public interface IAuthSettings
    {
        string RefreshTokenSecret { get; set; }
        int RefreshTokenTTL { get; set; }
        int JwtTTL { get; set; }
    }

    public class AuthSettings : IAuthSettings
    {
        public string RefreshTokenSecret { get; set; }
        public int RefreshTokenTTL { get; set; }
        public int JwtTTL { get; set; }
    }
}