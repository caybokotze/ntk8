using System;
using System.ComponentModel.DataAnnotations;
using Dispatch.K8.Dto;
using Ntk8.Attributes;

namespace Ntk8.Dto
{
    public class RegisterRequest : IRequiredUserAttributes
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string TelNumber { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        // [NotMapped]
        // [Compare(nameof(Password))]
        // public string ConfirmPassword { get; set; }
        
        [Range(typeof(bool), "true", "true")]
        public bool AcceptedTerms { get; set; }

        public void Validate()
        {
            var propertyInfo = typeof(RegisterRequest).GetProperty(Password);
            var passwordValidator =
                (PasswordValidator) Attribute
                    .GetCustomAttribute(propertyInfo ?? throw new InvalidOperationException(), typeof(PasswordValidator));

            if (passwordValidator != null 
                && passwordValidator.MeetsStrengthRequirement())
            {
                var something = (string)propertyInfo.GetValue(Password, null);
            }
        }
    }
}