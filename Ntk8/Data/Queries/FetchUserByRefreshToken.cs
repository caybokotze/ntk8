using System.Collections.Generic;
using System.Linq;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    /// <summary>
    /// Will return the user and the refresh token on the user object.
    /// </summary>
    public class FetchUserByRefreshToken<T> : Query<T?> where T : class, IBaseUser, new()
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

                const string sql = @"
SELECT u.id,
       u.title,
       u.first_name,
       u.last_name,
       u.email,
       u.is_active,
       u.guid,
       u.tel_number,
       u.username,
       u.access_failed_count,
       u.lockout_enabled,
       u.password_hash,
       u.password_salt,
       u.accepted_terms,
       u.reset_token,
       u.verification_token,
       u.date_created,
       u.date_modified,
       u.date_verified,
       u.date_of_password_reset,
       u.date_reset_token_expires,
       rt.id,
       rt.user_id,
       rt.token,
       rt.expires,
       rt.date_created,
       rt.created_by_ip,
       rt.date_revoked,
       rt.revoked_by_ip,
       rt.replaced_by_token,
       r.id,
       r.role_name
FROM refresh_tokens rt
LEFT JOIN users u on rt.user_id = u.id
LEFT JOIN user_roles ur on u.id = ur.user_id
LEFT JOIN roles r on ur.role_id = r.id
WHERE rt.token = @Token;";

                var result = QueryList<T, RefreshToken, Role, T>(
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