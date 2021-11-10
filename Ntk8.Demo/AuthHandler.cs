using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ntk8.Dto;
using Ntk8.Services;
using static Ntk8.Demo.GlobalHelpers;

namespace Ntk8.Demo
{
    public class AuthHandler
    {
        private readonly IAccountService _accountService;
        private readonly IAuthenticationContextService _authenticationContextService;

        public AuthHandler(
            IEndpointRouteBuilder builder,
            IAccountService accountService,
            IAuthenticationContextService authenticationContextService)
        {
            _accountService = accountService;
            _authenticationContextService = authenticationContextService;
            builder.MapPost("/login", Login);
            builder.MapPost("/register", Register);
            builder.MapGet("/verify", VerifyByUrl);
            builder.MapPost("/verify", Verify);
        }

        public async Task Verify(HttpContext context)
        {
            var verifyRequest = await context
                .DeserializeRequestBody<VerifyEmailRequest>();
            
            var user = _accountService
                .GetUserByEmail(verifyRequest.Email);
            
            _accountService
                .VerifyEmailByVerificationToken(user.VerificationToken);
        }

        public Task VerifyByUrl(HttpContext context)
        {
            var verifyRequest =  context
                .Request
                .Query[nameof(VerifyEmailByTokenRequest.Token)];

            if (!string.IsNullOrEmpty(verifyRequest))
            {
                _accountService
                    .VerifyEmailByVerificationToken(verifyRequest);
            }
            
            return Task.CompletedTask;
        }

        public async Task Register(HttpContext context)
        {
            var registerRequest = await context
                .DeserializeRequestBody<RegisterRequest>();
            
            ValidateModel(registerRequest);

            _accountService
                .Register(registerRequest, _authenticationContextService.GetIpAddress());
            
            // stuff to send an email here.
        }

        public async Task Login(HttpContext context)
        {
            var authRequest = await context
                .DeserializeRequestBody<AuthenticateRequest>();
            
            ValidateModel(authRequest);
            
            var response = _accountService
                .Authenticate(authRequest, _authenticationContextService.GetIpAddress());
            
            _authenticationContextService.SetTokenCookie(response.RefreshToken);
            await context.SerialiseResponseBody(response);
        }
    }
}