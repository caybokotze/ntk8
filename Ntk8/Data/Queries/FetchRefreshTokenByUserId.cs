using Dapper.CQRS;
using Ntk8.Models;

namespace Ntk8.Data.Queries
{
    public class FetchRefreshTokenByUserId : Query<RefreshToken>
    {
        public override void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}