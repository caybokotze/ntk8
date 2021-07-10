using System.ComponentModel.DataAnnotations;

namespace Ntk8.Dto
{
    public class VerifyEmailRequest
    {
        [Required]
        public string Token { get; set; }
    }
}