using Dapper.CQRS;
using NExpect;
using Ntk8.Data.Commands;
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
            // act
            using (Transactions.UncommittedRead())
            {
                var id = commandExecutor
                    .Execute(new InsertUser(user));
                // assert
                Expect(id)
                    .To.Be.Greater.Than(1);
            }
        }
    }
}