using System;
using System.ComponentModel.DataAnnotations;
using Ntk8.Dto.Interfaces;
using Ntk8.Models;

namespace Ntk8.Dto
{
    public class UpdateRequest : IUserPrimitiveProperties
    {
        public UpdateRequest()
        {
            Roles = Array.Empty<Role>();
        }

        public int Id { get; set; }
        
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        public Role[] Roles { get; set; }

        [EmailAddress] 
        public string? Email { get; set; }
        public string? TelNumber { get; set; }

    }
}