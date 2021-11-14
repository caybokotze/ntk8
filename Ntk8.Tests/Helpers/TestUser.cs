using Ntk8.Models;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace Ntk8.Tests.Helpers
{
    public class TestUser : BaseUser
    {
        public TestUser()
        {
            
        }
        
        public TestUser(IBaseUser baseUser) : base(baseUser)
        {
            
        }

        public static TestUser Create()
        {
            var user = GetRandom<TestUser>();
            user.Roles = CreateRandomRoles();
            return user;
        }

        public static Role[] CreateRandomRoles()
        {
            return GetRandomArray<Role>();
        }
    }
}