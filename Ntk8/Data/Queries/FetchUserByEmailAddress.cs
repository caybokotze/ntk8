using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByEmailAddress : Query<BaseUser>
    {
        private string EmailAddress { get; }

        public FetchUserByEmailAddress(string emailAddress)
        {
            EmailAddress = emailAddress.ToLowerInvariant();
        }

        public override void Execute()
        {
            try
            {
                Result = QueryFirst<BaseUser>(
                    @"SELECT * FROM users WHERE email = @EmailAddress", new
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