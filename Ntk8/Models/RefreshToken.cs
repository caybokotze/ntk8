using System;
using System.ComponentModel.DataAnnotations;

namespace Ntk8.Models
{
    public class RefreshToken
    {
        public RefreshToken()
        {
            DateCreated = DateTime.UtcNow;
        }
        
        [Key]
        public long Id { get; set; }
        public int UserId { get; set; }
        public string? Token { get; set; }
        public DateTime? Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime DateCreated { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime? DateRevoked { get; set; }
        public string? RevokedByIp { get; set; }
        public bool IsActive => DateRevoked == null && !IsExpired;
    }
}