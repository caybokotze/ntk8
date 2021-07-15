using Dapper.CQRS;

namespace Ntk8.Data.Commands
{
    public class DeleteUserById : Command<int>
    {
        public int Id { get; }

        public DeleteUserById(int id)
        {
            Id = id;
        }
        
        public override void Execute()
        {
            Result = Execute("DELETE FROM users WHERE id = @Id", new
            {
                Id
            });
        }
    }
}