using System.ComponentModel.DataAnnotations;

namespace Ntk8.Dto
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}