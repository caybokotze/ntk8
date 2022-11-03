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
            IUserPrimitiveProperties primitiveProperties;
            // assert
            Expect(typeof(IUserPrimitiveProperties))
                .To.Have.Property(nameof(primitiveProperties.Title));
            Expect(typeof(IUserPrimitiveProperties))
                .To.Have.Property(nameof(primitiveProperties.Email));
            Expect(typeof(IUserPrimitiveProperties))
                .To.Have.Property(nameof(primitiveProperties.FirstName));
            Expect(typeof(IUserPrimitiveProperties))
                .To.Have.Property(nameof(primitiveProperties.LastName));
            Expect(typeof(IUserPrimitiveProperties))
                .To.Have.Property(nameof(primitiveProperties.TelNumber));
        }
    }
}