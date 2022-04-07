using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByResetToken : Query<IBaseUser>
    {
        public string Token { get; }

        public FetchUserByResetToken(string token)
        {
            Token = token;
        }

        public override void Execute()
        {
            Result = QueryFirst<IBaseUser>(@"SELECT * FROM users 
            WHERE reset_token = @ResetToken;", new
            {
                ResetToken = Token
            });
        }
    }
}