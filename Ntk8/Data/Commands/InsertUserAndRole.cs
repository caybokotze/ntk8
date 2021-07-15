using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class InsertUserAndRole : Command<int>
    {
        private User User { get; }
        private UserRole UserRole { get; }

        public InsertUserAndRole(
             User user,
             UserRole userRole)
        {
            User = user;
            UserRole = userRole;
        }
        
        public override void Execute()
        {
            var userId = InsertUser();
            var _ = InsertUserRole(userId);
            Result = userId;
        }

        private int InsertUser()
        {
            return CommandExecutor.Execute(new InsertUser(User));
        }

        private int InsertUserRole(int userId)
        {
            UserRole.UserId = userId;
            return CommandExecutor
                .Execute(new InsertUserRole(UserRole));
        }
    }
}