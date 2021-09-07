using System;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByEmailAddress : Query<BaseBaseUser>
    {
        private string EmailAddress { get; }
        private BaseBaseUser BaseBaseUserModel { get; }

        public FetchUserByEmailAddress(string emailAddress)
        {
            EmailAddress = emailAddress.ToLowerInvariant();
            BaseBaseUserModel = new BaseBaseUser();
        }

        public override void Execute()
        {
            try
            {
                Result = QueryFirst<BaseBaseUser>(@"SELECT * FROM users 
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