using NExpect;
using Ntk8.Dto.Interfaces;
using NUnit.Framework;
using static NExpect.Expectations;

namespace Ntk8.Tests
{
    [TestFixture]
    public class UserPrimaryPropertiesTests
    {
        [Test]
        public void ShouldContainAllRequiredProperties()
        {
            // arrange
            // act
            // ReSharper disable once EntityNameCapturedOnly.Local
            IUserPrimaryProperties primaryProperties;
            // assert
            Expect(typeof(IUserPrimaryProperties))
                .To.Have.Property(nameof(primaryProperties.Title));
            Expect(typeof(IUserPrimaryProperties))
                .To.Have.Property(nameof(primaryProperties.Email));
            Expect(typeof(IUserPrimaryProperties))
                .To.Have.Property(nameof(primaryProperties.FirstName));
            Expect(typeof(IUserPrimaryProperties))
                .To.Have.Property(nameof(primaryProperties.LastName));
        }
    }
}