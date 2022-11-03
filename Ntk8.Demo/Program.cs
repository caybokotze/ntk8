using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Ntk8.DatabaseServices;
using Ntk8.Dto;
using Ntk8.Infrastructure;
using Ntk8.Models;
using Ntk8.Services;
using ScopeFunction.Utils;
using static ScopeFunction.Utils.AppSettingsBuilder;

namespace Ntk8.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseDefaultServiceProvider(options => options.ValidateScopes = true);
            ConfigureDependencies(builder);

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var app = builder.Build();
            app.UseRouting();
            app.UseNtk8JwtMiddleware<UserEntity>();
            app.UseNtk8ExceptionMiddleware();

            app.MapPost("/login", (AuthenticateRequest request, IAccountService accountService) => 
                accountService.AuthenticateUser(request));
            
            app.MapPost("/register", (RegisterRequest request, IAccountService accountService) => 
                accountService.RegisterUser(request));
            
            app.MapPost("/verify-by-email", (VerifyUserByEmailRequest request, IAccountService accountService) => 
                accountService.VerifyUserByEmail(request));

            app.MapPost("/verify-by-verification-token",
                (VerifyUserByVerificationTokenRequest request, IAccountService accountService) => accountService.VerifyUserByVerificationToken(request));

            app.MapPost("/update-user",
                (UpdateRequest request, IAccountQueries accountQueries, IAccountCommands accountCommands) =>
                {
                    var dbUser = accountQueries.FetchUserById<UserEntity>(request.Id);
                    request.MapTo(dbUser);
                    return accountCommands.UpdateUser(dbUser);
                });

            app.MapPost("/assign-roles", (IAccountService accountService, IAccountCommands accountCommands, IAccountQueries accountQueries) =>
            {
                var currentUser = accountService.CurrentUser;
                accountCommands.InsertOrUpdateUserRole(new UserRole
                {
                    UserId = accountService.CurrentUser!.Id,
                    RoleId = 1
                });
                
                accountCommands.InsertOrUpdateUserRole(new UserRole
                {
                    UserId = accountService.CurrentUser!.Id,
                    RoleId = 1
                });
                
                accountCommands.InsertOrUpdateUserRole(new UserRole
                {
                    UserId = accountService.CurrentUser!.Id,
                    RoleId = 2
                });

                return accountQueries.FetchUserById<UserEntity>(currentUser!.Id).MapTo(new UserAccountResponse());
            });

            app.MapPost("/request-password-reset", (ForgotPasswordRequest forgot, IAccountService accountService) =>
            {
                var res = accountService.ResetUserPassword(forgot);
                return res.resetToken;
            });

            await app.RunAsync();
        }
        
        // TODO: When fetching users, isActive results set to false, should be ignored.
        // TODO: Salt the password before storing.
        // TODO: Fix the authenticated response to not contain dates.

        private static void ConfigureDependencies(WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IExecutable, Executable>();
            builder.Services.AddTransient<IQueryable, Queryable>();
            builder.Services.AddTransient<IQueryExecutor, QueryExecutor>();
            builder.Services.AddTransient<ICommandExecutor, CommandExecutor>();
            builder.Services.AddTransient<IDbConnection, DbConnection>(_ => 
                new MySqlConnection(GetConnectionString()));
            
            builder.Services.AddHttpContextAccessor();
            
            builder.Services.ConfigureNtk8<UserEntity>(o =>
            {
                o.UseJwt = true;
                o.ConfigureAuthSettings(a =>
                {
                    a.RefreshTokenSecret = builder.Configuration["AuthSettings:RefreshTokenSecret"];
                    a.JwtTTL = 30_000;
                    a.RefreshTokenTTL = 300_000_000;
                    a.UserVerificationTokenTTL = 10_000;
                });
            });
        }

        public static string GetConnectionString()
        {
            var config = CreateConfigurationRoot()
                .GetConnectionString("DefaultConnection");

            return config;
        }
    }
}