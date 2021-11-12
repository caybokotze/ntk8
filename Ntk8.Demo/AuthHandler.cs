using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ntk8.Dto;
using Ntk8.Exceptions;
using Ntk8.Services;
using static Ntk8.Demo.GlobalHelpers;

namespace Ntk8.Demo
{
    public class AuthHandler
    {
        private readonly IUserAccountService _userAccountService;
        private readonly IAuthenticationContextService _authenticationContextService;

        public AuthHandler(
            IEndpointRouteBuilder builder,
            IUserAccountService userAccountService,
            IAuthenticationContextService authenticationContextService)
        {
            _userAccountService = userAccountService;
            _authenticationContextService = authenticationContextService;
            builder.MapPost("/login", Login);
            builder.MapPost("/register", Register);
            builder.MapGet("/verify", VerifyByUrl);
            builder.MapPost("/verify", Verify);
            builder.MapPost("/secure", SecureEndpoint);
            builder.MapPost("/new-token", NewToken);
        }

        public async Task Verify(HttpContext context)
        {
            var verifyRequest = await context
                .DeserializeRequestBody<VerifyEmailRequest>();
            
            var user = _userAccountService
                .GetUserByEmail(verifyRequest.Email);
            
            _userAccountService
                .VerifyUserByVerificationToken(user.VerificationToken);
        }

        public Task VerifyByUrl(HttpContext context)
        {
            var verifyRequest =  context
                .Request
                .Query[nameof(VerifyEmailByTokenRequest.Token)];

            if (!string.IsNullOrEmpty(verifyRequest))
            {
                _userAccountService
                    .VerifyUserByVerificationToken(verifyRequest);
            }
            
            return Task.CompletedTask;
        }

        public async Task Register(HttpContext context)
        {
            var registerRequest = await context
                .DeserializeRequestBody<RegisterRequest>();
            
            ValidateModel(registerRequest);

            _userAccountService
                .RegisterUser(registerRequest);
            
            // todo: send email or something...
        }

        public async Task Login(HttpContext context)
        {
            var authRequest = await context
                .DeserializeRequestBody<AuthenticateRequest>();
            
            ValidateModel(authRequest);
            
            var response = _userAccountService
                .AuthenticateUser(authRequest);
            
            await context.SerialiseResponseBody(response);
        }

        public async Task NewToken(HttpContext context)
        {
            var resetTokenRequest = await context
                .DeserializeRequestBody<ResetTokenRequest>();
            
            ValidateModel(resetTokenRequest);

            var response = _userAccountService
                .GenerateNewJwtToken(resetTokenRequest.Token);
            await context.SerialiseResponseBody(response);
        }
        
        public async Task SecureEndpoint(HttpContext context)
        {
            if (_authenticationContextService
                .IsUserAuthenticated())
            {
                await context.SerialiseResponseBody("Hi there jonny.");
            }

            throw new UserNotAuthenticatedException();
        }
    }
}