using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    internal class InsertRefreshToken : Command<long>
    {
        public RefreshToken RefreshToken { get; }

        public InsertRefreshToken(RefreshToken refreshToken)
        {
            RefreshToken = refreshToken;
        }

        public override void Execute()
        {
            Result =
                QueryFirst<long>(@"INSERT INTO refresh_tokens (
                            user_id, 
                            token, 
                            expires,
                            date_created, 
                            created_by_ip, 
                            date_revoked, 
                            revoked_by_ip) 
            VALUES (@UserId, 
                    @Token, 
                    @Expires, 
                    @DateCreated, 
                    @CreatedByIp, 
                    @DateRevoked, 
                    @RevokedByIp);

            SELECT last_insert_id();", 
                    RefreshToken);
        }
    }
}