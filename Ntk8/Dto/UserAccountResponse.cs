using System;
using Ntk8.Dto.Interfaces;
using Ntk8.Models;

namespace Ntk8.Dto
{
    public class UserAccountResponse : IUserPrimitiveProperties, IUserRoles, IUserVerificationStatus
    {
        public UserAccountResponse()
        {
            Roles = Array.Empty<Role>();
        }

        public UserAccountResponse(bool isVerified)
        {
            Roles = Array.Empty<Role>();
            IsVerified = isVerified;
        }
        
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? TelNumber { get; set; }
        public Role[] Roles { get; set; }
        public bool IsVerified { get; }
    }
}