using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByVerificationToken : Query<BaseUser>
    {
        public string Token { get; }

        public FetchUserByVerificationToken(string token)
        {
            Token = token;
        }
        
        public override void Execute()
        {
            Result = QueryFirst<BaseUser>(@"SELECT * FROM users 
            WHERE verification_token = @Token", new { Token });
        }

        private string sql = @"SELECT id, 
                                guid, 
                                title, 
                                email, 
                                first_name, 
                                last_name, 
                                tel_number, 
                                username, 
                                access_failed_count, 
                                lockout_enabled, 
                                password_hash, 
                                concurrency_stamp, 
                                security_stamp, 
                                password_salt, 
                                accepted_terms, 
                                reset_token, 
                                verification_token, 
                                date_verified, 
                                date_of_password_reset, 
                                date_reset_token_expires, 
                                date_created, 
                                date_modified, 
                                is_active;";
    }
}