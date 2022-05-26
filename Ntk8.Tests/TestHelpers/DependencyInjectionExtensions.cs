using Microsoft.Extensions.Configuration;

namespace Ntk8.Tests.TestHelpers
{
    public static class DependencyInjectionExtensions
    {
        public static string GetDefaultConnection(this IConfigurationRoot configurationRoot)
        {
            return configurationRoot.GetConnectionString("DefaultConnection");
        }
    }
}