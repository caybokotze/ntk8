using System;

namespace Ntk8.Tests.TestHelpers
{
    public static class NumericExtensions
    {
        public static void Times(this int count, Action action)
        {
            for (int i = 0; i < count; i++)
            {
                action();
            }
        }
    }
}