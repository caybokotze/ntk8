using System.Linq;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserById : Query<BaseUser>
    {
        public int Id { get; }

        public FetchUserById(int id)
        {
            Id = id;
        }
        
        public override void Execute()
        {
            var sql = "SELECT *, refresh_tokens.date_created as dim FROM users LEFT JOIN refresh_tokens ON users.id = refresh_tokens.user_id WHERE users.id = @Id;";
            Result = Query<BaseUser, RefreshToken, BaseUser>(sql,
                (user, token) =>
                {
                    user.RefreshTokens.Add(token);
                    return user;
                },
                new { Id })
                .First();
        }
    }
}