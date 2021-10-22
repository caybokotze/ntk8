using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Commands
{
    public class InsertRefreshToken : Command<int>
    {
        public RefreshToken RefreshToken { get; }

        public InsertRefreshToken(RefreshToken refreshToken)
        {
            RefreshToken = refreshToken;
        }

        public override void Execute()
        {
            Result =
                QueryFirst<int>(@"INSERT INTO refresh_tokens (user_id, 
                            token, 
                            expires,
                            date_created, 
                            created_by_ip, 
                            date_revoked, 
                            revoked_by_ip, 
                            replaced_by_token) 
            VALUES (@UserId, 
                    @Token, 
                    @Expires, 
                    @DateCreated, 
                    @CreatedByIp, 
                    @DateRevoked, 
                    @RevokedByIp, 
                    @ReplacedByToken); 
            SELECT last_insert_id();", RefreshToken);
        }
    }
}