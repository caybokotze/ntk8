using System;
using System.Globalization;
using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class FetchUserByResetToken : Query<BaseBaseUser>
    {
        public string Token { get; }

        public FetchUserByResetToken(string token)
        {
            Token = token;
        }

        public override void Execute()
        {
            Result = QueryFirst<BaseBaseUser>(@"SELECT * FROM users 
            WHERE reset_token = @ResetToken 
              AND reset_token_expires > " + $"{DateTime.Now.ToString(CultureInfo.InvariantCulture)}", new
            {
                ResetToken = Token
            });
        }
}
}