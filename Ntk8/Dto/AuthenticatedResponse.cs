using Ntk8.Models;

namespace Ntk8.Dto
{
    public class AuthenticatedResponse
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public Role[]? Roles { get; set; }
        public string? JwtToken { get; set; }
        public string? TelNumber { get; set; }
    }
}