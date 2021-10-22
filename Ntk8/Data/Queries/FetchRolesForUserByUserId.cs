using System.Collections.Generic;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserRolesForUserId : Query<List<Role>>
    {
        public int Id { get; }

        public FetchUserRolesForUserId(int id)
        {
            Id = id;
        }

        public override void Execute()
        {
            Result = QueryList<Role>(
                @"SELECT ur.*, r.* FROM user_roles ur 
                LEFT JOIN roles r 
                ON ur.role_id = r.id 
                WHERE ur.user_id = @UserId",
                new
                {
                    UserId = Id
                });
        }
    }
}