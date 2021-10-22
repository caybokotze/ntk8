﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            builder.MapGet("/authenticate", GetAuthenticate);
            builder.MapPost("/authenticate", PostAuthenticate);
        }

        public async Task GetAuthenticate(HttpContext context)
        {
            var reader = new StreamReader(context.Request.Body);
            var data = await reader.ReadToEndAsync();
        }

        public async Task PostAuthenticate(HttpContext context)
        {
            var authRequest = await context.DeserializeRequestBody<AuthenticateRequest>();
            
            ValidateModel(authRequest);
            
            var response = _accountService
                .Authenticate(authRequest, _authenticationContextService.GetIpAddress());
            
            _authenticationContextService.SetTokenCookie(response.RefreshToken);
            await context.SerialiseResponseBody(response);
        }
    }
}