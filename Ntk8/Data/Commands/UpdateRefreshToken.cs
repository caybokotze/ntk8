using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    internal class UpdateRefreshToken : Command<int>
    {
        public readonly RefreshToken RefreshToken;

        public UpdateRefreshToken(RefreshToken refreshToken)
        {
            RefreshToken = refreshToken;
        }

        public override void Execute()
        {
            Result = QueryFirst<int>(@"UPDATE refresh_tokens 
                        SET expires = @Expires,
                        date_revoked = @DateRevoked,
                        revoked_by_ip = @RevokedByIp
                        WHERE token = @Token; SELECT last_insert_id();",
                RefreshToken);
        }
    }
}