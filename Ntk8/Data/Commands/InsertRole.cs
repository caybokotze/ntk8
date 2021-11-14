using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class InsertRole : Command<int>
    {
        public Role Role { get; }

        public InsertRole(Role role)
        {
            Role = role;
        }
        
        public override void Execute()
        {
            Result = Execute("INSERT INTO roles (id, role_name) VALUES (@Id, @RoleName);", Role);
        }
    }
}