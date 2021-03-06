using Ntk8.Dto.Interfaces;
using Ntk8.Models;

namespace Ntk8.Dto
{
    public class UserAccountResponse : IUserPrimaryProperties
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public Role[]? Roles { get; set; }
        public bool IsVerified { get; set; }
    }
}