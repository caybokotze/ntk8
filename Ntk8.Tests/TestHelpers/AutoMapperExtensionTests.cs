using NExpect;
using Ntk8.Dto;
using Ntk8.Models;
using Ntk8.Utilities;
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
                var user = GetRandom<IBaseUser>();
                // act
                var request = user.MapFromTo(new RegisterRequest());
                // assert
                Expect(request.FirstName).To.Equal(user.FirstName);
                Expect(request.LastName).To.Equal(user.LastName);
                Expect(request.Email).To.Equal(user.Email);
                Expect(request.Title).To.Equal(user.Title);
                Expect(request.Password).To.Equal(null);
                Expect(request.AcceptedTerms).To.Equal(user.AcceptedTerms);
                Expect(request.TelNumber).To.Equal(user.TelNumber);
            }

            [Test]
            public void ShouldOverrideInstanceProperties()
            {
                // arrange
                var user = GetRandom<IBaseUser>();
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
            }

            [TestFixture]
            public class WhenMappingInterfaces
            {
                [Test]
                public void ShouldMapFromInterface()
                {
                    var user = GetRandom<IBaseUser>();
                    var registerRequest = new RegisterRequest();
                    var result = user.MapFromTo(registerRequest);
                    Expect(result.Email)
                        .To.Equal(registerRequest.Email)
                        .And.To.Equal(user.Email);
                }

                [Test]
                public void ShouldMapToInterface()
                {
                    var userRegistration = GetRandom<RegisterRequest>();
                    var user = GetRandom<IBaseUser>();
                    var result = userRegistration.MapFromTo(user);

                    Expect(userRegistration.Email)
                        .To.Equal(result.Email)
                        .And.To.Equal(user.Email);
                    
                    Expect(userRegistration.FirstName)
                        .To.Equal(result.FirstName)
                        .And.To.Equal(user.FirstName);
                }
            }

            [TestFixture]
            public class WhenMappingImplementedInterfaces
            {
                [Test]
                public void ShouldMapFromImplementedInterface()
                {
                    // arrange
                    var user = TestUser.Create();
                    // act
                    // assert
                }

                [Test]
                public void ShouldMapToImplementedInterface()
                {
                    // arrange
                    
                    // act
                    // assert
                }
            }
        }
    }
}