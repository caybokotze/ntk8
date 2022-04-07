#nullable enable
using System;
using System.Collections.Generic;
using Ntk8.Models;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Helpers
{
    public class TestUser : IBaseUser
    {
        public TestUser()
        {
            DateCreated = DateTime.UtcNow;
            DateModified = DateTime.UtcNow;
        }
        
        public static TestUser Create()
        {
            var user = GetRandom<TestUser>();
            user.Roles = GetRandomArray<Role>();
            user.RefreshToken = GetRandom<RefreshToken>();
            return user;
        }

        public static UserRole[] CreateRandomUserRoles(IBaseUser baseUser)
        {
            var userRoles = new List<UserRole>();
            for (var i = 0; i < 5; i++)
            {
                userRoles.Add(new UserRole()
                {
                    Role = GetRandom<Role>(),
                    Id = GetRandomInt(),
                    BaseUser = baseUser,
                    UserId = baseUser.Id
                });
            }

            return userRoles.ToArray();
        }

        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public Guid Guid { get; set; }
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
        public Role[]? Roles { get; set; }
    }
}