using System;
using System.Linq;
using AutoMapper;

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

        public static T2 MapFromTo<T1, T2>(this T1 map, T2 instance)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<T1, T2>();
            });

            var mapper = new Mapper(config);

            return mapper.Map(map, instance);
        }
    }
}