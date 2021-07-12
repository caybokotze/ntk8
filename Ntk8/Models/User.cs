using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dapper.CQRS;
using Newtonsoft.Json;
using Ntk8.Data.Queries;
using Ntk8.Model;

namespace Ntk8.Models
{
    public class User
    {
        public User()
        {
            ReferenceId = Guid.NewGuid();
            DateCreated = DateTime.Now;
            DateModified = DateTime.Now;
        }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        
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
        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }
        [JsonIgnore]
        public ICollection<UserRole> UserRoles { get; set; }
        
        public bool OwnsToken(string token)
        {
            return RefreshTokens?
                .Find(x => x.Token == token) != null;
        }
        
        public bool IsVerified => VerificationDate.HasValue || PasswordResetDate.HasValue;
        
        public IEnumerable<UserRole> GetUserRoles(IQueryExecutor queryExecutor)
        {
            UserRoles = queryExecutor
                .Execute(new FetchUserRolesForUserId(Id));
            return UserRoles;
        }
    }
}