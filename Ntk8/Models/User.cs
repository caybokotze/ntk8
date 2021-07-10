using System;
using System.ComponentModel.DataAnnotations;

namespace Ntk8.Model
{
    public class User
    {
        public User()
        {
            ReferenceId = Guid.NewGuid();
            DateCreated = DateTime.Now;
            DateUpdated = DateTime.Now;
        }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        
        public int Id { get; set; }
        public Guid ReferenceId { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [StringLength(50)]
        public string Surname { get; set; }

        [StringLength(15)]
        [DataType(DataType.PhoneNumber)]
        public string TelNumber { get; set; }

        [StringLength(30)]
        public string Username { get; set; }
        public int AccessFailedCount { get; set; }
        public bool LockoutEnabled { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public bool AcceptedTerms { get; set; }
        public string ResetToken { get; set; }
        public string VerificationToken { get; set; }
        public DateTime? VerificationDate { get; set; }
        public DateTime? PasswordResetDate { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
    }
}