using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class InsertUserAndRole : Command<long>
    {
        private IBaseUser BaseUser { get; }
        private UserRole UserRole { get; }

        public InsertUserAndRole(
             IBaseUser baseUser,
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

        private long InsertUser()
        {
            return CommandExecutor.Execute(new InsertUser(BaseUser));
        }

        private long InsertUserRole(long userId)
        {
            UserRole.UserId = userId;
            return CommandExecutor
                .Execute(new InsertUserRole(UserRole));
        }
    }
}