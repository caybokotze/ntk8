using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    internal class InsertRole : Command<int>
    {
        public Role Role { get; }

        public InsertRole(Role role)
        {
            Role = role;
        }
        
        public override void Execute()
        {
            Result = QueryFirst<int>("INSERT INTO roles (id, role_name) VALUES (@Id, @RoleName); SELECT last_insert_id();", Role);
        }
    }
}