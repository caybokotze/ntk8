using System.ComponentModel.DataAnnotations;

namespace Ntk8.Dto
{
    public class VerifyUserByEmailRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }

    public class VerifyUserByVerificationTokenRequest
    {
        [Required]
        public string? Token { get; set; }
    }
}