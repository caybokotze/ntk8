using System;
using System.Data;
using Dapper.CQRS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NExpect;
using Ntk8.Exceptions.Middleware;
using Ntk8.Middleware;
using Ntk8.Models;
using Ntk8.Services;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests.Infrastructure;

public class DependencyResolutionTests
{
     [TestFixture]
        public class ScopeResolutionTests
        {
            [TestFixture]
            public class Dependencies : TestFixtureRequiringServiceProvider
            {
                [TestCase(typeof(IHttpContextAccessor))]
                [TestCase(typeof(IAuthSettings))]
                [TestCase(typeof(IUserEntity))]
                public void ShouldResolveAsSingleton(Type type)
                {
                    // arrange
                    // act
                    var firstResolve = ServiceProvider?.GetService(type);
                    var secondResolve = ServiceProvider?.GetService(type);
                    // assert
                    Expect(firstResolve).To.Not.Be.Null();
                    Expect(secondResolve).To.Not.Be.Null();
                    Expect(firstResolve).To.Equal(secondResolve);
                    Expect(firstResolve?.GetHashCode()).To.Equal(secondResolve?.GetHashCode());
                }
            
                [TestCase(typeof(IQueryExecutor))]
                [TestCase(typeof(ICommandExecutor))]
                [TestCase(typeof(IDbConnection))]
                [TestCase(typeof(IAccountService))]
                [TestCase(typeof(ITokenService))]
                public void ShouldResolveAsTransient(Type type)
                {
                    // arrange
                    // act
                    var firstResolve = ServiceProvider?.GetService(type);
                    var secondResolve = ServiceProvider?.GetService(type);
                    // assert
                    Expect(firstResolve).To.Not.Be.Null();
                    Expect(secondResolve).To.Not.Be.Null();
                    Expect(firstResolve).Not.To.Equal(secondResolve);
                    Expect(firstResolve?.GetHashCode()).Not.To.Equal(secondResolve?.GetHashCode());
                }
            }

            [TestFixture]
            public class Middleware : TestFixtureRequiringServiceProvider
            {
                [TestCase(typeof(UserNotAuthorisedExceptionMiddleware))]
                [TestCase(typeof(UserNotAuthenticatedExceptionMiddleware))]
                [TestCase(typeof(UserNotFoundExceptionMiddleware))]
                [TestCase(typeof(UserAlreadyExistsExceptionMiddleware))]
                [TestCase(typeof(UserIsVerifiedExceptionMiddleware))]
                [TestCase(typeof(UserIsVerifiedExceptionMiddleware))]
                [TestCase(typeof(VerificationTokenExpiredExceptionMiddleware))]
                public void ShouldResolveAsSingleton(Type type)
                {
                    // arrange
                    // act
                    var firstResolve = ServiceProvider?.GetService(type);
                    var secondResolve = ServiceProvider?.GetService(type);
                    // assert
                    Expect(firstResolve).To.Not.Be.Null();
                    Expect(secondResolve).To.Not.Be.Null();
                    Expect(firstResolve).To.Equal(secondResolve);
                    Expect(firstResolve?.GetHashCode()).To.Equal(secondResolve?.GetHashCode());
                }
            }
        }

        [TestFixture]
        public class WhenResolvingForJwtMiddleware : TestFixtureRequiringServiceProvider
        {
            [Test]
            public void ShouldResolveAsTransient()
            {
                // arrange
                // act
                var firstResolve = ServiceProvider?.GetRequiredService<JwtMiddleware<TestUserEntity>>();
                var secondResolve = ServiceProvider?.GetRequiredService<JwtMiddleware<TestUserEntity>>();
                // assert
                Expect(firstResolve).To.Not.Be.Null();
                Expect(secondResolve).To.Not.Be.Null();
                Expect(firstResolve).Not.To.Equal(secondResolve);
            }
        }
}