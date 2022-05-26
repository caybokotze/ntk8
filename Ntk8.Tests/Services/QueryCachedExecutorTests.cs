using System;
using System.Threading;
using System.Transactions;
using Dapper.CQRS;
using Microsoft.Extensions.Caching.Memory;
using NExpect;
using NSubstitute;
using Ntk8.Data.Queries;
using Ntk8.Services;
using Ntk8.Tests.Helpers;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;

namespace Ntk8.Tests.Services;

public class QueryCachedExecutorTests : TestFixtureRequiringServiceProvider
{
    [TestFixture]
    public class WhenResolvingIMemoryCache : TestFixtureRequiringServiceProvider
    {
        [Test]
        public void ShouldBeAbleToSetAndGetValues()
        {
            using (new TransactionScope())
            {
                // arrange
                var sut = Resolve<IMemoryCache>();
                var randomValue = RandomValueGen.GetRandomString();
                // act
                sut.Set("key", randomValue);
                sut.TryGetValue("key", out var result);
                // assert
                Expect(result).To.Equal(randomValue);
            }
        }    
    }

    [TestFixture]
    public class WhenResolvingForDependencies : TestFixtureRequiringServiceProvider
    {
        [Test]
        public void ShouldNotBeNull()
        {
            using (new TransactionScope())
            {
                // arrange
                var sut = Resolve<IQueryCachedExecutor>();
                // act
                // assert
                Expect(sut)
                    .To.Not.Be.Null();
            }
        }
        
        [TestFixture]
        public class WhenResolvingForUsers : TestFixtureRequiringServiceProvider
        {
            [Test]
            public void ShouldOnlyExecuteQueryOnceWhenCalledTwice()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var memoryCache = Resolve<IMemoryCache>();
                    var queryExecutor = Substitute.For<IQueryExecutor>();
                    queryExecutor.Execute(Arg.Any<Query<TestUser>>()).Returns(RandomValueGen.GetRandom<TestUser>());
                    var sut = Substitute.For<QueryCachedExecutor>(memoryCache, queryExecutor);
                    var userId = RandomValueGen.GetRandomInt();
                    var query = new FetchUserById<TestUser>(userId);
                    // act
                    sut.GetAndSet(query, $"{nameof(FetchUserById<TestUser>)}--{userId}");
                    sut.GetAndSet(query, $"{nameof(FetchUserById<TestUser>)}--{userId}");
                    // assert
                    Expect(queryExecutor).To.Have.Received(1).Execute(query);
                }
            }

            [Test]
            public void ShouldExecuteTwiceWhenCalledTwiceWhenCacheExpires()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var memoryCache = Resolve<IMemoryCache>();
                    var queryExecutor = Substitute.For<IQueryExecutor>();
                    queryExecutor.Execute(Arg.Any<Query<TestUser>>()).Returns(RandomValueGen.GetRandom<TestUser>());
                    var sut = Substitute.For<QueryCachedExecutor>(memoryCache, queryExecutor);
                    var userId = RandomValueGen.GetRandomInt();
                    var query = new FetchUserById<TestUser>(userId);
                    // act
                    sut.GetAndSet(query, $"{nameof(FetchUserById<TestUser>)}--{userId}", TimeSpan.FromMilliseconds(2));
                    Thread.Sleep(3);
                    sut.GetAndSet(query, $"{nameof(FetchUserById<TestUser>)}--{userId}", TimeSpan.FromMilliseconds(2));
                    // assert
                    Expect(queryExecutor).To.Have.Received(2).Execute(query);
                }
            }
        }
    }
}