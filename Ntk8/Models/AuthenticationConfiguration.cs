namespace Ntk8.Models
{
    public class AuthenticationConfiguration
    {
        public string RefreshTokenSecret { get; set; }
        public int RefreshTokenTTL { get; set; }
    }
}