using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Ntk8.Demo
{
    public class AuthHandler
    {
        public AuthHandler(IEndpointRouteBuilder builder)
        {
            builder.MapGet("/auth", context =>
            {
                context.PrintToWeb("we are having a good time...");
                return Task.CompletedTask;
            });
        }
    }
}