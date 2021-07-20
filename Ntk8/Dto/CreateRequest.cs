using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ntk8.Dto.Interfaces;
using Ntk8.Models;

namespace Ntk8.Dto
{
    public class CreateRequest : IUserPrimaryProperties
    {
        public string Title { get; set; }

        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public List<UserRole> UserRoles { get; set; }

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