using System;
using System.Linq;
using HigLabo.Core;

namespace Ntk8.Helpers
{
    public static class Utils
    {
        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static T MapTo<T>(this object map) where T : class, new()
        {
            return map.Map(new T());
        }
    }
}