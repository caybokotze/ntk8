using System;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByVerificationToken<T> : Query<T?> where T : class, IBaseUser, new()
    {
        private string Token { get; }
        public string? Sql { get; set; }

        public FetchUserByVerificationToken(string token)
        {
            Token = token;
        }
        
        public override void Execute()
        {
            try
            {
                Sql ??= @"SELECT 
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
                from users u where u.verification_token = @Token;";
                
                Result = QueryFirst<T>(Sql, new { Token });
            }
            catch (Exception)
            {
                Result = null;
            }
        }
    }
}