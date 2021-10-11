using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ntk8.Models;
using PeanutButter.DuckTyping;
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
            return GetRandom<TestUser>();
        }
    }
}