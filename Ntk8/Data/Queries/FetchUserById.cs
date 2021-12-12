using System.Collections.Generic;
using System.Linq;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserById : Query<BaseUser>
    {
        public long Id { get; }

        public FetchUserById(long id)
        {
            Id = id;
        }
        
        public override void Execute()
        {
            try
            {
                var roles = new List<Role>();
                var result = Query<BaseUser, RefreshToken, Role, BaseUser>(
                    @"SELECT u.*, rt.*, r.* FROM users u
                        LEFT JOIN user_roles ur on u.id = ur.user_id
                        LEFT JOIN refresh_tokens rt on rt.id = (
                            SELECT refresh_tokens.id
                            FROM refresh_tokens
                            WHERE refresh_tokens.user_id = u.id
                            ORDER BY refresh_tokens.date_created
                            DESC LIMIT 1)
                        LEFT JOIN roles r on ur.role_id = r.id
                        WHERE u.id = @Id",
                    (user, token, role) =>
                    {
                        if (token is not null)
                        {
                            user.RefreshTokens.Add(token);
                        }
                        if (role is not null)
                        {
                            roles.Add(role);
                        }
                        return user;
                    }, new
                    {
                        Id
                    }
                );

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