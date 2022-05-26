using System;

namespace Ntk8.Tests.TestHelpers
{
    public class NonExpectationBasedTestingHelpers
    {
        public static void Fail(string message = null)
        {
            throw new TestFailedException(message);
        }
    }
    
    public class TestFailedException : Exception
    {
        public TestFailedException(string message = null) : base(message ?? "The test has failed")
        {
                
        }
    }
}