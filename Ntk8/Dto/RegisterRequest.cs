using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ntk8.Dto.Interfaces;
using Ntk8.Models;

namespace Ntk8.Dto
{
    public class RegisterRequest : IUserPrimaryProperties
    {
        public string Title { get; set; }
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // todo: remove...
        public ICollection<UserRole> UserRoles { get; set; }

        public string TelNumber { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [NotMapped]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
        
        [Range(typeof(bool), "true", "true")]
        public bool AcceptedTerms { get; set; }
    }
}