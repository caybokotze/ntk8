using System.Linq;
using Dapper;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    /// <summary>
    /// Will return the user and the refresh token on the user object.
    /// </summary>
    public class FetchUserByRefreshToken : Query<BaseUser>
    {
        private readonly string _token;

        public FetchUserByRefreshToken(string token)
        {
            _token = token;
        }

        public override void Execute()
        {
            var sql = "SELECT * FROM users LEFT JOIN refresh_tokens ON users.id = refresh_tokens.user_id;";

            Result = Raw()
                .Query<BaseUser, RefreshToken, BaseUser>(
                    sql, (user, refreshToken) =>
                    {
                        user.RefreshTokens.Add(refreshToken);
                        return user;
                    },
                    new
                    {
                        Token = _token
                    }).FirstOrDefault();
        }
    }
}