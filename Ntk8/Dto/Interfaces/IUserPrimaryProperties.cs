using Ntk8.Models;

namespace Ntk8.Dto.Interfaces
{
    public interface IUserPrimaryProperties
    {
        string Title { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Email { get; set; }
    }
}