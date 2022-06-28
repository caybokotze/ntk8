using System.Transactions;
using Dapper.CQRS;
using NExpect;
using NSubstitute;
using Ntk8.DatabaseServices;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Models;
using Ntk8.Services;
using Ntk8.Tests.TestHelpers;
using Ntk8.Utilities;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Services
{
    [TestFixture]
    public class UserAccountServiceTests
    {
        [TestFixture]
        public class DependencyInjection : TestFixtureRequiringServiceProvider
        {
            [Test]
            public void ShouldNotBeNull()
            {
                using (new TransactionScope())
                {
                    // arrange
                    var accountService = Resolve<IAccountService>();
                    var commandExecutor = Resolve<ICommandExecutor>();
                    var queryExecutor = Resolve<IQueryExecutor>();
                    var tokenService = Resolve<ITokenService>();
                    // act
                    // assert
                    Expect(accountService).Not.To.Be.Null();
                    Expect(commandExecutor).Not.To.Be.Null();
                    Expect(queryExecutor).Not.To.Be.Null();
                    Expect(tokenService).Not.To.Be.Null();
                }
            }
        }

        [TestFixture]
        public class AuthenticateUser
        {
            [Test]
            public void ShouldGenerateNewJwtToken()
            {
                // arrange
                var ntk8Queries = Substitute.For<INtk8Queries<TestUser>>();
                var tokenService = Substitute.For<ITokenService>();
                var user = GetRandom<IBaseUser>();
                var validJwtToken = TestTokenHelpers
                    .CreateValidJwtToken(GetRandomString(40), user.Id);
                var token = new ResetTokenResponse
                {
                    Token = TestTokenHelpers.CreateValidJwtTokenAsString(validJwtToken)
                };
                var authenticateRequest = user.MapFromTo(new AuthenticateRequest());
                authenticateRequest.Password = GetRandomString();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(authenticateRequest.Password);

                ntk8Queries.FetchUserByEmailAddress(Arg.Any<string>()).Returns(user);
                
                tokenService.GenerateJwtToken(user.Id, user.Roles)
                    .Returns(token);

                tokenService.GenerateRefreshToken()
                    .Returns(TestTokenHelpers.CreateRefreshToken());

                var accountService = Create(ntk8Queries, null, tokenService);
                // act
                var authenticatedResponse = accountService.AuthenticateUser(authenticateRequest);
                // assert
                Expect(authenticatedResponse.JwtToken).Not.To.Be.Null();
                Expect(authenticatedResponse.JwtToken).To.Equal(token.Token);
            }

            [TestFixture]
            public class WhenUserIsNotVerified
            {
                [Test]
                public void ShouldThrow()
                {
                    // arrange
                    var ntk8Queries = Substitute.For<INtk8Queries<TestUser>>();
                    var user = TestUser.Create();
                    user.DateVerified = null;
                    user.DateOfPasswordReset = null;
                    ntk8Queries.FetchUserByEmailAddress(user.Email ?? string.Empty)
                        .Returns(user);
                    var tokenService = Substitute.For<ITokenService>();
                    var authenticateRequest = user.MapFromTo<IBaseUser, AuthenticateRequest>(new AuthenticateRequest());
                    var sut = Create(ntk8Queries, null, tokenService);
                    // act
                    // assert
                    Expect(() => sut.AuthenticateUser(authenticateRequest))
                        .To.Throw<UserIsNotVerifiedException>();
                }
            }

            [Test]
            public void ShouldGenerateNewRefreshToken()
            {
                // arrange

                // act
                // assert
            }

            [Test]
            public void ShouldFetchTheUserFromDb()
            {
                // arrange

                // act
                // assert
            }

            [Test]
            public void ShouldAttachUserRolesToUserResponse()
            {
                // arrange

                // act
                // assert
            }

            [Test]
            public void ShouldNotUpdateUser()
            {
                // arrange
                
                // act
                // assert
            }

            [Test]
            public void ShouldRespondWithUserRoles()
            {
                // arrange
                
                // act
                // assert
            }

            [Test]
            public void ShouldSetRefreshTokenCookie()
            {
                // arrange
                
                // act
                // assert
            }
        }

        [TestFixture]
        public class RegisterUser : TestFixtureRequiringServiceProvider
        {
            [TestFixture]
            public class WhenUserDoesExist
            {
                [Test]
                public void ShouldThrowIfUserIsVerifiedAndExists()
                {
                    // arrange
                    var ntk8Queries = Substitute.For<INtk8Queries<TestUser>>();
                    var user = GetRandom<IBaseUser>();
                    var registerRequest = user.MapFromTo(new RegisterRequest());
                    ntk8Queries.FetchUserByEmailAddress(user.Email ?? string.Empty).Returns(user);
                    
                    var accountService = Create(ntk8Queries);
                    // act
                    // assert
                    Expect(() => accountService.RegisterUser(registerRequest))
                        .To.Throw<UserAlreadyExistsException>()
                        .With.Message.Containing("User already exists");
                }

                [TestFixture]
                public class WhenUserIsNotVerified
                {
                    [Test]
                    public void ShouldUpdateVerificationToken()
                    {
                        // arrange
                        var ntk8Queries = Substitute.For<INtk8Queries<TestUser>>();
                        var ntk8Commands = Substitute.For<INtk8Commands>();
                        var user = GetRandom<IBaseUser>();
                        user.DateVerified = null;
                        user.DateOfPasswordReset = null;
                        var registerRequest = user.MapFromTo(new RegisterRequest());

                        ntk8Queries.FetchUserByEmailAddress(Arg.Any<string>()).Returns(user);

                        var accountService = Create(ntk8Queries, ntk8Commands);
                        accountService.RegisterUser(registerRequest);
                        // act
                        // assert
                        Expect(ntk8Commands)
                            .To.Have.Received(1)
                            .UpdateUser(Arg.Is(user));
                    }
                }
            }

            [TestFixture]
            public class WhenUserDoestExist
            {
                [Test]
                public void ShouldSetVerificationToken()
                {
                    // arrange
                    // act
                    // assert
                }

                [TestFixture]
                public class WhenUserHasNotAcceptedTerms
                {
                    [Test]
                    public void ShouldThrow()
                    {
                        // arrange

                        // act
                        // assert
                    }
                }

                [Test]
                public void ShouldHaveAcceptedTerms()
                {
                    // arrange

                    // act
                    // assert
                }
                
                [Test]
                public void ShouldHashUserPassword()
                {
                    // arrange

                    // act
                    // assert
                }
            }
        }

        [TestFixture]
        public class VerifyUserByVerificationToken
        {
        }


        [TestFixture]
        public class ForgotUserPassword
        {
            [Test]
            public void ShouldUpdateResetToken()
            {
                // arrange

                // act
                // assert
            }

            [Test]
            public void ShouldUpdateDateResetTokenExpires()
            {
                // arrange

                // act
                // assert
            }

            [Test]
            public void ShouldReturnResetToken()
            {
                // arrange

                // act
                // assert
            }
        }


        [TestFixture]
        public class ResetUserPassword
        {
            [Test]
            public void ShouldResetUserPassword()
            {
                // arrange
                
                // act
                // assert
            }
        }

        [TestFixture]
        public class GetUserById
        {
            [Test]
            public void ShouldGetUserById()
            {
                // arrange
                
                // act
                // assert
            }
        }

        [TestFixture]
        public class UpdateUserTests
        {
            [Test]
            public void ShouldUpdateUserDetails()
            {
                // arrange
                
                // act
                // assert
            }
        }

        [TestFixture]
        public class DeleteUser
        {
            [Test]
            public void ShouldDeleteUser()
            {
                // arrange
                
                // act
                // assert
            }
        }

        [TestFixture]
        public class AutoVerifyUser
        {
            [Test]
            public void ShouldVerifyUser()
            {
                // arrange
                
                // act
                // assert
            }
        }


        [TestFixture]
        public class ForgotPassword
        {
            [Test]
            public void ShouldGenerateResetTokenForUser()
            {
                // arrange
                
                // act
                // assert
            }
        }

        [TestFixture]
        public class ResetPassword
        {
            [Test]
            public void ShouldSetResetTokenToNull()
            {
                // arrange

                // act
                // assert
            }
        }


        private static IAccountService Create(
            INtk8Queries<TestUser>? ntk8Queries = null,
            INtk8Commands? ntk8Commands = null,
            ITokenService? tokenService = null,
            IAuthSettings? authSettings = null,
            IAccountState? accountState = null)
        {
            return new AccountService<TestUser>(
                ntk8Commands ?? Substitute.For<INtk8Commands>(),
                ntk8Queries ?? Substitute.For<INtk8Queries<TestUser>>(),
                tokenService ?? Substitute.For<ITokenService>(),
                authSettings ?? CreateAuthSettings(),
                GetRandom<IBaseUser>(), 
                accountState ?? Substitute.For<IAccountState>());
        }

        private static AuthSettings CreateAuthSettings()
        {
            return new AuthSettings
            {
                RefreshTokenSecret = GetRandomAlphaString(40),
                RefreshTokenTTL = 604800,
                JwtTTL = 1000
            };
        }
    }
}