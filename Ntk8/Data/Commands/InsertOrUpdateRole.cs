using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    internal class InsertOrUpdateRole : Command<int>
    {
        public Role Role { get; }

        public InsertOrUpdateRole(Role role)
        {
            Role = role;
        }
        
        public override void Execute()
        {
            Result = QueryFirst<int>("INSERT IGNORE INTO roles (id, role_name) VALUES (@Id, @RoleName); SELECT last_insert_id();", Role);
        }
    }
}