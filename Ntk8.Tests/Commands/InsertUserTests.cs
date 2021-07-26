using System;
using Dapper.CQRS;
using NExpect;
using NSubstitute;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Models;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;

namespace Ntk8.Tests.Commands
{
    [TestFixture]
    public class InsertUserTests : TestBase
    {
        [Test]
        public void InsertShouldInsert()
        {
            // arrange
            var user = RandomValueGen.GetRandom<User>();
            var commandExecutor = Resolve<ICommandExecutor>();
            var queryExecutor = Resolve<IQueryExecutor>();
            // act
            using (Transactions.UncommittedRead())
            {
                var id = commandExecutor
                    .Execute(new InsertUser(user));
                var expectedUser = queryExecutor
                    .Execute(new FetchUserById(id)); 
                // assert
                Expect(id)
                    .To.Be.Greater.Than(1);
                expectedUser.DateCreated = user.DateCreated;
                expectedUser.DateModified = user.DateModified;
                expectedUser.VerificationDate = user.VerificationDate;
                expectedUser.PasswordResetDate = user.PasswordResetDate;
                expectedUser.ResetTokenExpires = user.ResetTokenExpires;
                user.RefreshTokens = null;
                user.UserRoles = null;
                user.Id = id;
                Expect(expectedUser)
                    .To.Deep.Equal(user);
            }
        }

        [Test]
        public void ShouldReturnExpectedDatesAndNotBeNull()
        {
            // arrange
            var user = RandomValueGen.GetRandom<User>();
            var commandExecutor = Resolve<ICommandExecutor>();
            var queryExecutor = Resolve<IQueryExecutor>();
            // act
            using (Transactions.UncommittedRead())
            {
                var id = commandExecutor
                    .Execute(new InsertUser(user));

                var expectedUser = queryExecutor
                    .Execute(new FetchUserById(id));
                // assert
                Expect(expectedUser.DateCreated)
                    .To.Approximately.Equal(user.DateCreated);
                Expect(expectedUser.DateModified)
                    .To.Approximately.Equal(user.DateModified);
                
                if (user.VerificationDate is not null)
                {
                    Expect(expectedUser.VerificationDate)
                        .To.Approximately.Equal((DateTime) user.VerificationDate);
                }

                if (user.PasswordResetDate is not null)
                {
                    Expect(expectedUser.PasswordResetDate)
                        .To.Approximately.Equal((DateTime) user.PasswordResetDate);
                }

                if (user.ResetTokenExpires is not null)
                {
                    Expect(expectedUser.ResetTokenExpires)
                        .To.Approximately.Equal((DateTime) user.ResetTokenExpires);
                }

                Expect(user.VerificationDate).To.Not.Be.Null();
                Expect(user.PasswordResetDate).To.Not.Be.Null();
                Expect(user.ResetTokenExpires).To.Not.Be.Null();
            }
        }
    }
}