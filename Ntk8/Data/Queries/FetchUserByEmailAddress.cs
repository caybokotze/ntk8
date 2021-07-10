using System;
using Dapper.CQRS;
using Ntk8.Model;

namespace Ntk8.Data.Queries
{
    public class FetchUserByEmailAddress : Query<User>
    {
        private string EmailAddress { get; }
        private User UserModel { get; }

        public FetchUserByEmailAddress(string emailAddress)
        {
            EmailAddress = emailAddress.ToLowerInvariant();
            UserModel = new User();
        }

        public override void Execute()
        {
            try
            {
                Result = QueryFirst<User>(@"SELECT * FROM users 
                WHERE email = @EmailAddress", new
                {
                    EmailAddress
                });
            }
            catch
            {
                Result = null;
            }
        }
    }
}