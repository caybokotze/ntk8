using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;
using PeanutButter.Utils.Dictionaries;

namespace Ntk8.Tests
{
    public static class AppSettingProvider
    {
        public static AppSettings CreateAppSettings()
        {
            return GetSettingsFrom(
                CreateConfig()
            );
        }

        public static IConfigurationRoot CreateConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            const string deployConfig = "appsettings.deploy.json";
            if (CanLoad(deployConfig)) builder.AddJsonFile(deployConfig);

            return builder.Build();
        }

        private static bool CanLoad(string deployConfig)
        {
            if (!File.Exists(deployConfig)) return false;

            var lines = File.ReadAllLines(deployConfig);
            var re = new Regex("#{.+}");
            return lines.All(l => !re.Match(l).Success);
        }

        private static AppSettings GetSettingsFrom(
            IConfigurationRoot config)
        {
            var defaultConfig = new Dictionary<string, string>();
            var providedConfig = config.GetSection("Settings")
                ?.GetChildren()
                .ToDictionary(s => s.Key, s => s.Value) ?? new Dictionary<string, string>();
            var merged = new MergeDictionary<string, string>(providedConfig, defaultConfig);
            try
            {
                return merged.FuzzyDuckAs<AppSettings>(true);
            }
            catch (UnDuckableException ex)
            {
                Console.WriteLine(ex.Errors.JoinWith("\n"));
                throw;
            }
        }
    }
}