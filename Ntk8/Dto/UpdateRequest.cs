using System.ComponentModel.DataAnnotations;
using Ntk8.Dto.Interfaces;
using Ntk8.Models;

namespace Ntk8.Dto
{
    public class UpdateRequest : IUserPrimaryProperties
    {
        
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public Role[] Roles { get; set; }

        [EmailAddress] 
        public string Email { get; set; }

        [MinLength(6)] 
        public string Password { get; set; }

        [Compare("Password")] 
        public string ConfirmPassword { get; set; }

    }
}