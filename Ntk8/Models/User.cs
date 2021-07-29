using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dapper.CQRS;
using Newtonsoft.Json;
using Ntk8.Data.Queries;

namespace Ntk8.Models
{
    public interface IUser
    {
        DateTime DateCreated { get; set; }
        DateTime DateModified { get; set; }
        bool IsActive { get; set; }
        int Id { get; set; }
        Guid ReferenceId { get; set; }
        string Title { get; set; }
        string Email { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string TelNumber { get; set; }
        string Username { get; set; }
        int AccessFailedCount { get; set; }
        bool LockoutEnabled { get; set; }
        string PasswordHash { get; set; }
        string PasswordSalt { get; set; }
        bool AcceptedTerms { get; set; }
        string ResetToken { get; set; }
        string VerificationToken { get; set; }
        DateTime? VerificationDate { get; set; }
        DateTime? PasswordResetDate { get; set; }
        DateTime? ResetTokenExpires { get; set; }
        List<RefreshToken> RefreshTokens { get; set; }
        ICollection<UserRole> UserRoles { get; set; }
        bool IsVerified { get; }
        bool OwnsToken(string token);
        IEnumerable<UserRole> GetUserRoles(IQueryExecutor queryExecutor);
    }

    public class User : IUser
    {
        public User()
        {
            ReferenceId = Guid.NewGuid();
            DateCreated = DateTime.Now;
            DateModified = DateTime.Now;
        }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsActive { get; set; }
        
        public int Id { get; set; }
        public Guid ReferenceId { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [StringLength(50)]
        [Required]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

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