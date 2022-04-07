using System;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByVerificationToken : Query<IBaseUser?>
    {
        public string Token { get; }

        public FetchUserByVerificationToken(string token)
        {
            Token = token;
        }
        
        public override void Execute()
        {
            try
            {
                Result = QueryFirst<IBaseUser>(@"SELECT * FROM users 
                WHERE verification_token = @Token", new { Token });
            }
            catch (Exception)
            {
                Result = null;
            }
        }
    }
}