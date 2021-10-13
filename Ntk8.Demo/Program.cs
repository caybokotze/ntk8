using Microsoft.AspNetCore.Builder;

namespace Ntk8.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            var _ = new AuthHandler(app);
            app.Run();
        }
    }
}