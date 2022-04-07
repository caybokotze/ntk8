using System.ComponentModel.DataAnnotations;

namespace Ntk8.Dto
{
    public class AuthenticateRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}