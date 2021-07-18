using System;

namespace Ntk8.Dto
{
    public interface IUserPrimaryProperties
    {
        string Title { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Email { get; set; }
        string Role { get; set; }
        DateTime Created { get; set; }
        DateTime? Updated { get; set; }
        bool IsVerified { get; set; }
    }
}