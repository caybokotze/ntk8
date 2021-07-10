using System.ComponentModel.DataAnnotations;
using Dispatch.K8.Dto;

namespace Ntk8.Dto
{
    public class CreateRequest : IRequiredUserAttributes
    {
        [Required]
        public string Title { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Surname { get; set; }
        
        [Required]
        public string TelNumber { get; set; }

        public bool AcceptedTerms { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}