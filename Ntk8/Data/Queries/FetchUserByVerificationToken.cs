using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByVerificationToken : Query<User>
    {
        public string Token { get; }

        public FetchUserByVerificationToken(string token)
        {
            Token = token;
        }
        
        public override void Execute()
        {
            Result = QueryFirst<User>(@"SELECT * FROM users 
            WHERE verification_token = @VerificationToken");
        }
    }
}