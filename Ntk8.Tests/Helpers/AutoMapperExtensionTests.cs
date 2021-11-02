using NExpect;
using Ntk8.Dto;
using Ntk8.Helpers;
using Ntk8.Models;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Helpers
{
    [TestFixture]
    public class AutoMapperExtensionTests
    {
        [TestFixture]
        public class MapFromTo
        {
            [Test]
            public void ShouldMapAllAvailableProperties()
            {
                // arrange
                var user = GetRandom<BaseUser>();
                // act
                var request = user.MapFromTo<BaseUser, RegisterRequest>();
                // assert
                Expect(request.FirstName).To.Equal(user.FirstName);
                Expect(request.LastName).To.Equal(user.LastName);
                Expect(request.Email).To.Equal(user.Email);
                Expect(request.Title).To.Equal(user.Title);
                Expect(request.Password).To.Equal(null);
                Expect(request.AcceptedTerms).To.Equal(user.AcceptedTerms);
                Expect(request.TelNumber).To.Equal(user.TelNumber);
                Expect(request.UserRoles).To.Equal(user.UserRoles);
            }

            [Test]
            public void ShouldOverrideInstanceProperties()
            {
                // arrange
                var user = GetRandom<BaseUser>();
                var request = GetRandom<RegisterRequest>();
                var requestPassword = request.Password;
                // act
                request = user.MapFromTo(request);
                // assert
                Expect(request.FirstName).To.Equal(user.FirstName);
                Expect(request.LastName).To.Equal(user.LastName);
                Expect(request.Email).To.Equal(user.Email);
                Expect(request.Title).To.Equal(user.Title);
                Expect(request.Password).To.Equal(requestPassword);
                Expect(request.AcceptedTerms).To.Equal(user.AcceptedTerms);
                Expect(request.TelNumber).To.Equal(user.TelNumber);
                Expect(request.UserRoles).To.Equal(user.UserRoles);
            }
        }
    }
}