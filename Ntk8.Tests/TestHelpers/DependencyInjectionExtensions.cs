using Microsoft.Extensions.Configuration;

namespace Ntk8.Tests.Helpers
{
    public static class DependencyInjectionExtensions
    {
        public static string GetDefaultConnection(this IConfigurationRoot configurationRoot)
        {
            return configurationRoot.GetConnectionString("DefaultConnection");
        }
    }
}