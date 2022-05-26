using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class InsertUserAndRole : Command
    {
        public IBaseUser BaseUser { get; }
        public UserRole UserRole { get; }

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
            InsertUserRole(userId);
        }

        private int InsertUser()
        {
            return CommandExecutor.Execute(new InsertUser(BaseUser));
        }

        private void InsertUserRole(int userId)
        {
            UserRole.UserId = userId;
            CommandExecutor
                .Execute(new InsertUserRole(UserRole));
        }
    }
}