using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class UpdateRefreshToken : Command<int>
    {
        private readonly RefreshToken _refreshToken;

        public UpdateRefreshToken(RefreshToken refreshToken)
        {
            _refreshToken = refreshToken;
        }
        
        public override void Execute()
        {
            Result = Execute("UPDATE refresh_tokens SET (re)");
        }
    }
}