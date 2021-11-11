using System.Linq;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    /// <summary>
    /// Will return the user and the refresh token on the user object.
    /// </summary>
    public class FetchUserByRefreshToken : Query<BaseUser>
    {
        public string Token { get; }

        public FetchUserByRefreshToken(string token)
        {
            Token = token;
        }

        public override void Execute()
        {
            var sql = @"SELECT u.*, rt.* FROM users u
            LEFT JOIN refresh_tokens rt
            ON u.id = rt.user_id
            WHERE rt.token = @Token;";

            Result = Query<BaseUser, RefreshToken, BaseUser>(
                    sql, (user, refreshToken) =>
                    {
                        user.RefreshTokens.Add(refreshToken);
                        return user;
                    },
                    new
                    {
                        Token
                    }).FirstOrDefault();
        }
    }
}