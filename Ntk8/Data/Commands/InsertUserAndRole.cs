using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class InsertUserAndRole : Command<int>
    {
        private BaseUser BaseUser { get; }
        private UserRole UserRole { get; }

        public InsertUserAndRole(
             BaseUser baseUser,
             UserRole userRole)
        {
            BaseUser = baseUser;
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
            return CommandExecutor.Execute(new InsertUser(BaseUser));
        }

        private int InsertUserRole(int userId)
        {
            UserRole.UserId = userId;
            return CommandExecutor
                .Execute(new InsertUserRole(UserRole));
        }
    }
}