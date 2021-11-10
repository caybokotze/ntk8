using System.ComponentModel.DataAnnotations;

namespace Ntk8.Dto
{
    public class VerifyEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class VerifyEmailByTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}