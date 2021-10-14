using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Ntk8.Models;
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

    public class GlobalHelpers
    {
        public static void ValidateModel(object model)
        {
            var vc = new ValidationContext(model);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, vc, results, true);

            if (!isValid)
            {
                throw new ValidationException(results[0].ErrorMessage);
            }
        }
    }
}