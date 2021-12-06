using Dapper.CQRS;

namespace Ntk8.Data.Commands
{
    public class InvalidateRefreshToken : Command<int>
    {
        public InvalidateRefreshToken(string token)
        {
            Token = token;
        }

        public string Token { get; }
        
        public override void Execute()
        {
            Result = Execute(@"UPDATE refresh_tokens 
                SET date_revoked = utc_timestamp(3), revoked_by_ip = '0.0.0.0' 
                WHERE token = @Token;", new { Token });
        }
    }
}