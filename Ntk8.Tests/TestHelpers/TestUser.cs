#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Ntk8.Models;
using Ntk8.Utilities;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.TestHelpers
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
            user.UserRoles = CreateRandomUserRoles(user);
            user.Roles = user.UserRoles.Select(s => s.Role).ToArray();
            user.RefreshToken = CreateValidRefreshToken();
            user.RefreshToken.Expires = DateTime.UtcNow.AddDays(1);
            user.RefreshToken.DateRevoked = null;
            user.IsActive = true;
            return user;
        }

        public static RefreshToken CreateInvalidRefreshToken()
        {
            var refreshToken = GetRandom<RefreshToken>();
            refreshToken.DateRevoked = DateTime.UtcNow.AddHours(-1);
            refreshToken.Expires = DateTime.UtcNow.AddHours(-1);
            return refreshToken;
        }

        public static RefreshToken CreateValidRefreshToken()
        {
            var refreshToken = new RefreshToken(TokenHelpers.GenerateCryptoRandomToken())
            {
                DateRevoked = null,
                Expires = DateTime.UtcNow.AddHours(1),
                CreatedByIp = GetRandomIPv4Address(),
                DateCreated = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
                RevokedByIp = null,
                UserId = 0
            };
            return refreshToken;
        }

        private static UserRole[] CreateRandomUserRoles(IBaseUser user)
        {
            var roleId = GetRandomInt(100);
            var userRoles = new List<UserRole>
            {
                new()
                {
                    Role = new Role
                    {
                        Id = roleId,
                        RoleName = GetRandomString()
                    },
                    RoleId = roleId,
                    UserId = user.Id
                },
                new()
                {
                    Role = new Role
                    {
                        Id = roleId + 1,
                        RoleName = GetRandomString()
                    },
                    RoleId = roleId + 1,
                    UserId = user.Id
                }
            };
            return userRoles.ToArray();
        }

        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int Id { get; set; }
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
        public UserRole[]? UserRoles { get; set; }
    }
}