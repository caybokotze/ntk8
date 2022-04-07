using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByEmailAddress<T> : Query<IBaseUser> where T : class, IBaseUser, new()
    {
        public string EmailAddress { get; }
        public string? Sql { get; set; }

        public FetchUserByEmailAddress(string emailAddress)
        {
            EmailAddress = emailAddress.ToLowerInvariant();
        }
        
        public override void Execute()
        {
            var something =
                @"SELECT 
                u.id, 
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
                u.date_reset_token_expires
                from users u where u.email = @EmailAddress;";
            
            Sql ??= something;

            try
            {
                var roles = new List<Role>();
                var result = QueryFirst<T>(Sql, new { EmailAddress });
                // var result = Query<T, RefreshToken, Role, T>(
                //     Sql,
                //     (user, token, role) =>
                //     {
                //         if (token is not null)
                //         {
                //             user.RefreshToken = token;
                //         }
                //
                //         if (role is not null)
                //         {
                //             roles.Add(role);
                //         }
                //         return user;
                //     }, new
                //     {
                //         EmailAddress
                //     }
                // );

                // var user = result.First();
                // user.Roles = roles.ToArray();
                Result = result;
            }
            catch (Exception)
            {
                Result = null;
            }
            
        }
    }
}