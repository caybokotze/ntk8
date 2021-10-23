using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class UpdateRefreshToken : Command<int>
    {
        public readonly RefreshToken RefreshToken;

        public UpdateRefreshToken(RefreshToken refreshToken)
        {
            RefreshToken = refreshToken;
        }
        
        public override void Execute()
        {
            Result = Execute("UPDATE refresh_tokens SET refresh_tokens = @RefreshToken",
                new
                {
                    RefreshToken = RefreshToken.Token
                });
        }
    }
}