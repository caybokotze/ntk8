using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;
using PeanutButter.Utils.Dictionaries;

namespace Ntk8.Demo
{
    public static class GlobalExtensions
    {
        public static T Resolve<T>(this WebApplication webApplication)
        {
            return webApplication.Services.GetRequiredService<T>();
        }
        
        public static T Resolve<T>(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            return endpointRouteBuilder
                .ServiceProvider
                .GetRequiredService<T>();
        }
        
        public static T Objectify<T>(this IDictionary<string, string> source)
            where T : class, new()
        {
            var defaultConfig = new Dictionary<string, string>();
            var providedConfig = source ?? new Dictionary<string, string>();
            var merged = new MergeDictionary<string, string>(providedConfig, defaultConfig);
            try
            {
                return merged.FuzzyDuckAs<T>(true);
            }
            catch (UnDuckableException ex)
            {
                Console.WriteLine(ex.Errors.JoinWith("\n"));
                throw;
            }
        }
    }
}