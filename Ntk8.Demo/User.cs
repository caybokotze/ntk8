#nullable enable
using System;
using Ntk8.Models;

namespace Ntk8.Demo;

public class User : IBaseUser
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public Guid? Guid { get; set; }
    public string? TelNumber { get; set; }
    public string? Username { get; set; }
    public int? AccessFailedCount { get; set; }
    public bool? LockoutEnabled { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }
    public bool AcceptedTerms { get; set; }
    public string? ResetToken { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
    public DateTime? DateVerified { get; set; }
    public DateTime? DateOfPasswordReset { get; set; }
    public DateTime? DateResetTokenExpires { get; set; }
    public RefreshToken? RefreshToken { get; set; }
    public Role?[]? Roles { get; set; }
}