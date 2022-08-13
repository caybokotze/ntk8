using System;
using System.ComponentModel.DataAnnotations;

namespace Ntk8.Models
{
    public class RefreshToken
    {
        /// <summary>
        /// Required for AutoMapper
        /// </summary>
        public RefreshToken()
        {
            DateCreated = DateTime.UtcNow;
        }
        
        public RefreshToken(string token)
        {
            Token = token;
            DateCreated = DateTime.UtcNow;
        }
        
        [Key]
        public long Id { get; set; }
        public int UserId { get; set; }
        public string? Token { get; }
        public DateTime? Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime DateCreated { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime? DateRevoked { get; set; }
        public string? RevokedByIp { get; set; }
        public bool IsActive => DateRevoked == null && !IsExpired;
    }
}