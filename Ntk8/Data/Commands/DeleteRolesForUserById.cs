using Dapper.CQRS;

namespace Ntk8.Data.Commands
{
    public class DeleteRolesForUserById : Command
    {
        public int UserId { get; }

        public DeleteRolesForUserById(int userId)
        {
            UserId = userId;
        }

        public override void Execute()
        {
            Execute("DELETE FROM user_roles WHERE user_id = @UserId;", new { UserId });
        }
    }
}