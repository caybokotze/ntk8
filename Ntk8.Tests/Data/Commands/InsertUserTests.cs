using System;
using NExpect;
using Ntk8.DatabaseServices;
using Ntk8.Models;
using Ntk8.Tests.TestHelpers;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;

namespace Ntk8.Tests.Data.Commands
{
    [TestFixture]
    public class InsertUserTests : TestFixtureRequiringServiceProvider
    {
        [Test]
        public void ShouldInsertUser()
        {
            using (Transactions.UncommittedRead())
            {
                // arrange
                var user = TestUser.Create();
                var accountCommands = Resolve<IAccountCommands>();
                var accountQueries = Resolve<IAccountQueries>();
                
                // act
                var id = accountCommands.InsertUser(user);
                var expectedUser = accountQueries.FetchUserById<TestUser>(id);

                // assert
                Expect(id)
                    .To.Be.Greater.Than(1);
                Expect(expectedUser?.DateCreated).To.Approximately.Equal(user.DateCreated);
                Expect(expectedUser?.DateModified).To.Approximately.Equal(user.DateModified);
                Expect(expectedUser?.DateVerified).To.Approximately.Equal((DateTime)user.DateVerified!);
                Expect(expectedUser?.DateOfPasswordReset).To.Approximately
                    .Equal((DateTime) user.DateOfPasswordReset!);
                Expect(expectedUser?.DateResetTokenExpires).To.Approximately
                    .Equal((DateTime) user.DateResetTokenExpires!);
                user.RefreshToken = null;

                expectedUser!.DateCreated = user.DateCreated;
                expectedUser.DateModified = user.DateModified;
                expectedUser.DateVerified = user.DateVerified;
                expectedUser.DateOfPasswordReset = user.DateOfPasswordReset;
                expectedUser.DateResetTokenExpires = user.DateResetTokenExpires;
                
                user.Id = id;
                user.Roles = Array.Empty<Role>();
                Expect(expectedUser)
                    .To.Deep.Equal(user);
            }
        }

        [Test]
        public void ShouldReturnExpectedDatesAndNotBeNull()
        {
            using (Transactions.UncommittedRead())
            {
                // arrange
                var user = RandomValueGen.GetRandom<IUserEntity>();
                var accountCommands = Resolve<IAccountCommands>();
                var accountQueries = Resolve<IAccountQueries>();
                
                // act
                var id = accountCommands.InsertUser(user);

                var expectedUser = accountQueries.FetchUserById<TestUser>(id);

                // assert
                Expect(expectedUser?.DateCreated)
                    .To.Approximately.Equal(user.DateCreated);
                Expect(expectedUser?.DateModified)
                    .To.Approximately.Equal(user.DateModified);
                
                if (user.DateVerified is not null)
                {
                    Expect(expectedUser?.DateVerified)
                        .To.Approximately.Equal((DateTime) user.DateVerified);
                }

                if (user.DateOfPasswordReset is not null)
                {
                    Expect(expectedUser?.DateOfPasswordReset)
                        .To.Approximately.Equal((DateTime) user.DateOfPasswordReset);
                }

                if (user.DateResetTokenExpires is not null)
                {
                    Expect(expectedUser?.DateResetTokenExpires)
                        .To.Approximately.Equal((DateTime) user.DateResetTokenExpires);
                }

                Expect(expectedUser?.DateVerified).To.Not.Be.Null();
                Expect(expectedUser?.DateOfPasswordReset).To.Not.Be.Null();
                Expect(expectedUser?.DateResetTokenExpires).To.Not.Be.Null();
            }
        }
    }
}