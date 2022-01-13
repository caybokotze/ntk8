using System.ComponentModel;
using System.Linq;
using System.Transactions;
using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Models;
using Ntk8.Services;
using Ntk8.Tests.Helpers;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;

namespace Ntk8.Tests.Data.Commands.RefreshToken
{
    [TestFixture]
    public class InvalidateRefreshTokenTests
    {
        [TestFixture]
        public class WhenInvalidatingRefreshToken : TestFixtureWithServiceProvider
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

                    var user = RandomValueGen.GetRandom<BaseUser>();
                    
                    var userId = commandExecutor.Execute(new InsertUser(user));
                    
                    var refreshToken = TokenHelpers.CreateRefreshToken();
                    refreshToken.UserId = userId;
                    user.Id = userId;

                    commandExecutor.Execute(new InsertRefreshToken(refreshToken));
                
                    // act
                    user = queryExecutor.Execute(new FetchUserByRefreshToken(refreshToken.Token));
                    var retrievedRefreshToken = user?.RefreshTokens.First();

                    commandExecutor.Execute(new InvalidateRefreshToken(refreshToken.Token));

                    user = queryExecutor.Execute(new FetchUserByRefreshToken(refreshToken.Token));
                    var retrievedRefreshTokenAfterInvalidation = user?.RefreshTokens.First();
                    
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