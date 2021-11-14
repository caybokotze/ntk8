using System.Collections.Generic;
using System.Linq;
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
            user.UserRoles = CreateRandomUserRoles(user);
            user.Roles = user.UserRoles.Select(s => s.Role).ToArray();
            return user;
        }

        public static UserRole[] CreateRandomUserRoles(BaseUser baseUser)
        {
            var userRoles = new List<UserRole>();
            for (var i = 0; i < 5; i++)
            {
                userRoles.Add(new UserRole()
                {
                    Role = GetRandom<Role>(),
                    Id = GetRandomInt(),
                    BaseUser = baseUser,
                    UserId = baseUser.Id
                });
            }

            return userRoles.ToArray();
        }
        
        public static Role[] CreateRandomRoles()
        {
            return GetRandomArray<Role>();
        }
    }
}