using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    internal class InsertOrUpdateUserRole : Command<int>
    {
        public int RoleId { get; }
        public int UserId { get; }

        public InsertOrUpdateUserRole(UserRole userRole)
        {
            RoleId = userRole.RoleId;
            UserId = userRole.UserId;
        }
        
        public override void Execute()
        {
            Result = QueryFirst<int>(@"INSERT IGNORE INTO user_roles (role_id, user_id) 
            VALUES (@RoleId, @UserId); SELECT last_insert_id();",
                new
                {
                    RoleId,
                    UserId
                });
        }
    }
}