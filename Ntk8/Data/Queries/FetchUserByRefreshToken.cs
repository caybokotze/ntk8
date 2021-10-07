using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByRefreshToken : Query<BaseUser>

    {
        private readonly string _token;

        public FetchUserByRefreshToken(string token)
        {
            _token = token;
        }

        public override void Execute()
        {
            Result = QueryFirst<BaseUser>(
                "SELECT * FROM users LEFT JOIN refresh_tokens ON users.id = refresh_tokens.user_id;",
                new
                {
                    Token = _token
                });
        }
    }
}