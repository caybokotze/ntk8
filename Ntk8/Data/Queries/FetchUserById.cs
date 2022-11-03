using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.CQRS;
using Microsoft.Extensions.Logging;
using Ntk8.Exceptions;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    internal class FetchUserById<T> : Query<T?> where T : class, IUserEntity, IUserRoles, IUserRefreshToken, new()
    {
        public int Id { get; }

        public FetchUserById(int id)
        {
            if (id < 1)
            {
                throw new InvalidUserException("Can not query a user with a negative or 0 value primary key");
            }
            
            Id = id;
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
       r.id,
       r.role_name
FROM users u
         LEFT JOIN user_roles ur ON u.id = ur.user_id
         LEFT JOIN refresh_tokens rt ON rt.id = (SELECT refresh_tokens.id
                                                 FROM refresh_tokens
                                                 WHERE refresh_tokens.user_id = u.id
                                                 ORDER BY refresh_tokens.date_created DESC
                                                 LIMIT 1)
         LEFT JOIN roles r on ur.role_id = r.id
WHERE u.id = @Id;";

                var result = QueryList<T, RefreshToken, Role, T>(sql,
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
                        Id
                    }
                );

                var user = result.First();
                user.Roles = roles.ToArray();
                Result = user;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to fetch the user by id: FetchUserById.cs");
                Result = null;
            }
        }
    }
}