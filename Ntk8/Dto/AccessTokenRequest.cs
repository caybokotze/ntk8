using System;
using System.ComponentModel.DataAnnotations;

namespace Ntk8.Dto
{
    public class AccessTokenRequest
    {
        [Required]
        public string? Token { get; set; }
    }

    public class AccessTokenResponse : AccessTokenRequest
    {
        public DateTime ExpiryDate { get; set; }
    }
}