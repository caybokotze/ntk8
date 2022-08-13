#nullable enable
using System;
using Ntk8.Dto.Interfaces;

namespace Ntk8.Models
{
    public interface IBaseUser : IUserPrimaryProperties
    {
        int Id { get; set; }
        bool IsActive { get; set; }
        Guid Guid { get; set; }
        string? TelNumber { get; set; }
        string? Username { get; set; }
        int? AccessFailedCount { get; set; }
        bool? LockoutEnabled { get; set; }
        string? PasswordHash { get; set; }
        string? PasswordSalt { get; set; }
        bool AcceptedTerms { get; set; }
        string? ResetToken { get; set; }
        string? VerificationToken { get; set; }
        DateTime DateCreated { get; set; }
        DateTime DateModified { get; set; }
        DateTime? DateVerified { get; set; }
        DateTime? DateOfPasswordReset { get; set; }
        DateTime? DateResetTokenExpires { get; set; }
        RefreshToken? RefreshToken { get; set; }
        Role[]? Roles { get; set; }
        bool IsVerified => DateVerified.HasValue || DateOfPasswordReset.HasValue;
    }
}