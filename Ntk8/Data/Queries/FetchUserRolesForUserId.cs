using System.Collections.Generic;
using System.Linq;
using Dapper;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserRolesForUserId : Query<List<UserRole>>
    {
        public int Id { get; }

        public FetchUserRolesForUserId(int id)
        {
            Id = id;
        }

        public override void Execute()
        {
            Result = GetIDbConnection()
                .Query<UserRole, BaseUser, Role, UserRole>(
                    $"SELECT u.*, ur.*, r.* FROM user_roles ur" +
                    $"LEFT JOIN roles r" +
                    $"ON ur.role_id = r.id" +
                    $"LEFT JOIN users u" +
                    $"ON ur.user_id = u.id" +
                    $"WHERE ur.user_id = @Id", (userRole, user, role) =>
                    {
                        userRole.BaseUser = user;
                        userRole.Role = role;
                        return userRole;
                    }, new
                    {
                        Id = Id
                    }).ToList();
        }
    }
}