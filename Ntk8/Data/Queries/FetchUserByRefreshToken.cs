using System.Collections.Generic;
using System.Linq;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    /// <summary>
    /// Will return the user and the refresh token on the user object.
    /// </summary>
    public class FetchUserByRefreshToken : Query<IBaseUser>
    {
        public string Token { get; }

        public FetchUserByRefreshToken(string token)
        {
            Token = token;
        }

        public override void Execute()
        {
            try
            {
                var roles = new List<Role>();
                var sql = @"
                    SELECT u.*, rt.*, r.*
                    FROM refresh_tokens rt
                    LEFT JOIN users u on rt.user_id = u.id
                    LEFT JOIN user_roles ur on u.id = ur.user_id
                    LEFT JOIN roles r on ur.role_id = r.id
                    WHERE rt.token = @Token;";

                var result = Query<IBaseUser, RefreshToken, Role, IBaseUser>(
                    sql,
                    (user, token, role) =>
                    {
                        if (token is not null)
                        {
                            user.RefreshToken = token;
                        }

                        if (role is not null)
                        {
                            roles.Add(role);
                        }

                        return user;
                    }, new
                    {
                        Token
                    });

                var user = result.First();
                user.Roles = roles.ToArray();
                Result = user;
            }
            catch
            {
                Result = null;
            }
        }
    }
}