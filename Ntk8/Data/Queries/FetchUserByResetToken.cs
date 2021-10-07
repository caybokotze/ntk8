using System;
using System.Globalization;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchUserByResetToken : Query<BaseUser>
    {
        public string Token { get; }

        public FetchUserByResetToken(string token)
        {
            Token = token;
        }

        public override void Execute()
        {
            Result = QueryFirst<BaseUser>(@"SELECT * FROM users 
            WHERE reset_token = @ResetToken 
              AND reset_token_expires > " + $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}", new
            {
                ResetToken = Token
            });
        }
}
}