using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using Ntk8.Dto.Interfaces;

namespace Ntk8.Models
{
    public interface IBaseUser : IUserPrimaryProperties
    {
        DateTime DateCreated { get; set; }
        DateTime DateModified { get; set; }
        bool IsActive { get; set; }
        long Id { get; set; }
        Guid Guid { get; set; }
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
        ICollection<RefreshToken> RefreshTokens { get; set; }
        Role[] Roles { get; set; }
        bool IsVerified { get; }
        bool OwnsToken(string token);
    }

    /// <summary>
    /// Do not use this class directly. Create a derived class like 'User' and inherit from 'BaseUser'.
    /// This class can not be made abstract due to Dapper constraints on QueryRow<T>.
    /// </summary>
    public class BaseUser : IBaseUser
    {
        public BaseUser()
        {
            Guid = Guid.NewGuid();
            DateCreated = DateTime.UtcNow;
            DateModified = DateTime.UtcNow;
            Roles = new Role[] {};
            UserRoles = new List<UserRole>();
            RefreshTokens = new List<RefreshToken>();
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
        public long Id { get; set; }
        public Guid Guid { get; set; }
        public string Title { get; set; }
        
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }
        
        [DataType(DataType.PhoneNumber)]
        public string TelNumber { get; set; }
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
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<UserRole> UserRoles { get; set; }
        
        [JsonIgnore]
        public virtual Role[] Roles { get; set; }
        
        public bool OwnsToken(string token)
        {
            return RefreshTokens
                .ToList()?
                .Find(x => x.Token == token) != null;
        }
        
        public bool IsVerified => DateVerified.HasValue || DateOfPasswordReset.HasValue;
    }
}