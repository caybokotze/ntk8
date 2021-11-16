using Dapper.CQRS;

namespace Ntk8.Data.Commands
{
    public class DeleteRolesForUser : Command
    {
        public long UserId { get; }

        public DeleteRolesForUser(long userId)
        {
            UserId = userId;
        }

        public override void Execute()
        {
            Execute("DELETE FROM user_roles WHERE user_id = @UserId;", new { UserId });
        }
    }
}