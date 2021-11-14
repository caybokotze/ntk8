using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByVerificationToken : Query<BaseUser>
    {
        public string Token { get; }

        public FetchUserByVerificationToken(string token)
        {
            Token = token;
        }
        
        public override void Execute()
        {
            Result = QueryFirst<BaseUser>(@"SELECT * FROM users 
            WHERE verification_token = @Token", new { Token });
        }
    }
}