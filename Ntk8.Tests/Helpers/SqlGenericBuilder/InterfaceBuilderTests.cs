using System;
using GenericSqlBuilder;
using Ntk8.Models;
using NUnit.Framework;

namespace Ntk8.Tests.Helpers.SqlGenericBuilder;

[TestFixture]
public class InterfaceBuilderTests
{
    public class Implementation<T> where T : new()
    {
        
    }
    
    [TestFixture]
    public class WhenBuildingSQLQueries
    {
        [Test]
        public void ShouldBuildUpSqlQueryForImplementedInterface()
        {
            // arrange
            
            // act
            // assert
        }

        [Test]
        public void ShouldBuildUpQueryForInterface()
        {
            // arrange
            
            // act
            // assert
        }
    }
}