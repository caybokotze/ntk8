using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByResetToken<T> : Query<T> where T : class, IBaseUser, new()
    {
        public string Token { get; }

        public FetchUserByResetToken(string token)
        {
            Token = token;
        }

        public override void Execute()
        {
            Result = QueryFirst<T>(@"SELECT * FROM users 
            WHERE reset_token = @ResetToken;", new
            {
                ResetToken = Token
            });
        }
    }
}