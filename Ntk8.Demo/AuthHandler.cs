using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Ntk8.ActionFilters;
using Ntk8.Data.Commands;
using Ntk8.Data.Queries;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Services;
using static Ntk8.Demo.GlobalHelpers;

namespace Ntk8.Demo
{
    public class AuthHandler
    {
        private readonly IAccountService _accountService;
        private readonly IQueryExecutor _queryExecutor;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ITokenService _tokenService;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthHandler(
            IEndpointRouteBuilder builder,
            IAccountService accountService,
            IQueryExecutor queryExecutor,
            ICommandExecutor commandExecutor,
            ITokenService tokenService,
            IHttpContextAccessor contextAccessor)
        {
            _accountService = accountService;
            _queryExecutor = queryExecutor;
            _commandExecutor = commandExecutor;
            _tokenService = tokenService;
            _contextAccessor = contextAccessor;
            builder.MapPost("/login", Login);
            builder.MapPost("/register", Register);
            // builder.MapGet("/verify", VerifyByUrl);
            builder.MapPost("/verify", Verify);
            builder.MapPost("/secure", SecureEndpoint);
            builder.MapPost("/new-token", NewToken);
            builder.MapPost("/update", Update);
        }

        public async Task Update(HttpContext context)
        {

            var updateRequest = await context.DeserializeRequestBody<UpdateRequest>();

            var user = _queryExecutor.Execute(new FetchUserByEmailAddress<User>(updateRequest.Email));
            user.TelNumber = updateRequest.TelNumber;

            _commandExecutor.Execute(new UpdateUser(user));
        }

        public async Task Verify(HttpContext context)
        {
            var verifyRequest = await context
                .DeserializeRequestBody<VerifyEmailRequest>();
            
            var user = _queryExecutor
                .Execute(new FetchUserByEmailAddress<User>(verifyRequest.Email));
            
            _accountService
                .VerifyUserByVerificationToken(user.VerificationToken);
        }

        public Task VerifyByUrl(HttpContext context)
        {
            var verifyRequest =  context
                .Request
                .Query[nameof(VerifyEmailByTokenRequest.Token)];

            if (!string.IsNullOrEmpty(verifyRequest))
            {
                _accountService
                    .VerifyUserByVerificationToken(verifyRequest);
            }
            
            return Task.CompletedTask;
        }

        public async Task Register(HttpContext context)
        {
            var registerRequest = await context
                .DeserializeRequestBody<RegisterRequest>();
            
            ValidateModel(registerRequest);

            try
            {
                _accountService
                    .RegisterUser(registerRequest);
            }
            catch (Exception e)
            {
                // ignore
            }

            // todo: send email or something...
        }
        
        public async Task Login(HttpContext context)
        {
            var authRequest = await context
                .DeserializeRequestBody<AuthenticateRequest>();
            
            ValidateModel(authRequest);
            
            var response = _accountService
                .AuthenticateUser(authRequest);
            
            await context.SerialiseResponseBody(response);
        }

        public async Task NewToken(HttpContext context)
        {
            var resetTokenRequest = await context
                .DeserializeRequestBody<ResetTokenRequest>();
            
            ValidateModel(resetTokenRequest);

            var user = _queryExecutor.Execute(new FetchUserByRefreshToken<User>(resetTokenRequest.Token));
            var response = _tokenService.GenerateJwtToken(user.Id, user.Roles.ToArray());
            await context.SerialiseResponseBody(response);
        }
        
        public async Task SecureEndpoint(HttpContext context)
        {
            new AuthoriseAttribute("administrator").OnAuthorization(
                new AuthorizationFilterContext(new ActionContext(_contextAccessor.HttpContext, new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>()));
            
            if (!_accountService.IsUserAuthenticated)
            {
                throw new UserNotAuthenticatedException();
            }
            
            await context.SerialiseResponseBody($"Hi there {_accountService.CurrentUser?.FirstName}. You have these roles: {string.Join(",",_accountService.CurrentUser?.Roles?.Select(s => s.RoleName))}");
        }
    }
}