using System.Collections.Generic;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserRolesForUserId : Query<List<Role>>
    {
        public long Id { get; }

        public FetchUserRolesForUserId(long id)
        {
            Id = id;
        }

        public override void Execute()
        {
            Result = QueryList<Role>(
                @"SELECT r.id, r.role_name
                    FROM user_roles ur
                             INNER JOIN roles r
                                        ON ur.role_id = r.id
                    WHERE ur.user_id = @UserId;",
                new
                {
                    UserId = Id
                });
        }
    }
}