using System;
using System.ComponentModel.DataAnnotations;

namespace Ntk8.Dto
{
    public class ResetTokenRequest
    {
        [Required]
        public string? Token { get; set; }
    }

    public class ResetTokenResponse : ResetTokenRequest
    {
        public DateTime ExpiryDate { get; set; }
    }
}