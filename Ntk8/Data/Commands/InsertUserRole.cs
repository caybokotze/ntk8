using Dapper.CQRS;

namespace Ntk8.Data.Commands
{
    public class InsertUserRole : Command<int>
    {
        public int RoleId { get; }
        public int UserId { get; }

        public InsertUserRole(int roleId, int userId)
        {
            RoleId = roleId;
            UserId = userId;
        }
        
        public override void Execute()
        {
            Result = Execute(@"INSERT INTO user_roles (role_id, user_id) 
            VALUES (@RoleId, @UserId)", new
            {
                RoleId,
                UserId
            });
        }
    }
}