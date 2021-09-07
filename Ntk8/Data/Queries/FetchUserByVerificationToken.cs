using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByVerificationToken : Query<BaseBaseUser>
    {
        public string Token { get; }

        public FetchUserByVerificationToken(string token)
        {
            Token = token;
        }
        
        public override void Execute()
        {
            Result = QueryFirst<BaseBaseUser>(@"SELECT * FROM users 
            WHERE verification_token = @VerificationToken");
        }
    }
}