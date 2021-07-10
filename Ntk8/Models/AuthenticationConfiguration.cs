namespace Ntk8.Models
{
    public class AuthenticationConfiguration
    {
        public int RefreshTokenSecret { get; set; }
        public string RefreshTokenTTL { get; set; }
    }
}