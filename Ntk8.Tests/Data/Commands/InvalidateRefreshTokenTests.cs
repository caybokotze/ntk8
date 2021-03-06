using System.Transactions;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.Data.Commands
{
    [TestFixture]
    public class InvalidateRefreshTokenTests
    {
        [TestFixture]
        public class WhenInvalidatingRefreshToken : TestFixtureRequiringServiceProvider
        {
            [Test]
            public void ShouldBeValidBeforeInvalidation()
            {
                // arrange
                // act
                var refreshToken = TokenHelpers.CreateRefreshToken();
                // assert
                Expect(refreshToken.IsExpired).To.Be.False();
                Expect(refreshToken.IsActive).To.Be.True();
            }
            
            [Test]
            public void ShouldInvalidateRefreshToken()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var queryExecutor = Resolve<IQueryExecutor>();

                    var user = TestUser.Create();
                    
                    var userId = commandExecutor.Execute(new InsertUser(user));
                    
                    var refreshToken = TokenHelpers.CreateRefreshToken();
                    refreshToken.UserId = userId;
                    user.Id = userId;
                    var retrievedRefreshToken = user.RefreshToken;

                    commandExecutor.Execute(new InsertRefreshToken(refreshToken));
                
                    // act
                    queryExecutor.Execute(new FetchUserByRefreshToken<TestUser>(refreshToken.Token ?? string.Empty));

                    commandExecutor.Execute(new InvalidateRefreshToken(refreshToken.Token ?? string.Empty));

                    user = queryExecutor.Execute(new FetchUserByRefreshToken<TestUser>(refreshToken.Token ?? string.Empty));
                    var retrievedRefreshTokenAfterInvalidation = user?.RefreshToken;
                    
                    // assert
                    Expect(retrievedRefreshToken?.IsExpired).To.Be.False();
                    Expect(retrievedRefreshToken?.IsActive).To.Be.True();
                    Expect(retrievedRefreshTokenAfterInvalidation?.IsExpired).To.Be.True();
                    Expect(retrievedRefreshTokenAfterInvalidation?.IsActive).To.Be.False();
                }
            }
        }
    }
}