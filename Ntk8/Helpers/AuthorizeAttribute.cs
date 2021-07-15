using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.CQRS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ntk8.Constants;
using Ntk8.Models;

namespace Ntk8.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public AuthorizeAttribute()
        {
            _roles = null;
        }
        
        public IQueryExecutor QueryExecutor { get; }

        public AuthorizeAttribute(IQueryExecutor queryExecutor)
        {
            QueryExecutor = queryExecutor;
        }
        
        private readonly IList<string> _roles;

        public AuthorizeAttribute(params UserRole[] roles)
        {
            _roles = GetRolesAsStrings(roles);
        }

        public string[] GetRolesAsStrings(UserRole[] roles)
        {
            var length = roles.Length;
            var userRoles = new string[length];
            for (int i = 0; i <= length-1; i++)
            {
                userRoles[i] = roles[i].ToString();
            }

            return userRoles;
        }
        
        public AuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }
        
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = (User) context.HttpContext.Items[AuthenticationConstants.ContextAccount];
            
            // todo: needs to check against all of the roles that the user has in the db.
            
            if (user == null || (_roles.Any() && !_roles.Contains("")))
            {
                // not logged in or role not authorized
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}