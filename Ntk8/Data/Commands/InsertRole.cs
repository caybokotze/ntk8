using Dapper.CQRS;

namespace Ntk8.Data.Commands
{
    public class InsertRole : Command<int>
    {
        public string RoleName { get; }

        public InsertRole(string roleName)
        {
            RoleName = roleName;
        }
        
        public override void Execute()
        {
            Result = Execute("INSERT INTO roles (name) VALUES (@Name)", new
            {
                Name = RoleName
            });
        }
    }
}