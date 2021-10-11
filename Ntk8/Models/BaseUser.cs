using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dapper.CQRS;
using Newtonsoft.Json;
using Ntk8.Data.Queries;

namespace Ntk8.Models
{
    public interface IBaseUser
    {
        DateTime DateCreated { get; set; }
        DateTime DateModified { get; set; }
        bool IsActive { get; set; }
        int Id { get; set; }
        Guid Guid { get; set; }
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
        DateTime? DateVerified { get; set; }
        DateTime? DateOfPasswordReset { get; set; }
        DateTime? DateResetTokenExpires { get; set; }
        List<RefreshToken> RefreshTokens { get; set; }
        ICollection<UserRole> UserRoles { get; set; }
        bool IsVerified { get; }
        bool OwnsToken(string token);
        IEnumerable<UserRole> GetUserRoles(IQueryExecutor queryExecutor);
    }

    public abstract class BaseUser : IBaseUser
    {
        protected BaseUser()
        {
            Guid = Guid.NewGuid();
            DateCreated = DateTime.Now;
            DateModified = DateTime.Now;
        }

        protected BaseUser(IBaseUser user)
        {
            DateCreated = user.DateCreated;
            DateModified = user.DateModified;
            IsActive = user.IsActive;
            Id = user.Id;
            Guid = user.Guid;
            Title = user.Title;
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            TelNumber = user.TelNumber;
            Username = user.Username;
            AccessFailedCount = user.AccessFailedCount;
            LockoutEnabled = user.LockoutEnabled;
            PasswordHash = user.PasswordHash;
            PasswordSalt = user.PasswordSalt;
            DateOfPasswordReset = user.DateOfPasswordReset;
            AcceptedTerms = user.AcceptedTerms;
            ResetToken = user.ResetToken;
            VerificationToken = user.VerificationToken;
            DateVerified = user.DateVerified;
            DateOfPasswordReset = user.DateOfPasswordReset;
            DateResetTokenExpires = user.DateResetTokenExpires;
        }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsActive { get; set; }
        public int Id { get; set; }
        public Guid Guid { get; set; }

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
        public DateTime? DateVerified { get; set; }
        public DateTime? DateOfPasswordReset { get; set; }
        public DateTime? DateResetTokenExpires { get; set; }
        
        [JsonIgnore]
        public virtual List<RefreshToken> RefreshTokens { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<UserRole> UserRoles { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<Role> Roles { get; set; }
        
        public bool OwnsToken(string token)
        {
            return RefreshTokens?
                .Find(x => x.Token == token) != null;
        }
        
        public bool IsVerified => DateVerified.HasValue || DateOfPasswordReset.HasValue;
        
        public IEnumerable<UserRole> GetUserRoles(IQueryExecutor queryExecutor)
        {
            UserRoles = queryExecutor
                .Execute(new FetchUserRolesForUserId(Id));
            return UserRoles;
        }
    }
}