﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Ntk8.Model
{
    public class RefreshToken
    {
        [Key]
        public long Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }
        
        public bool IsActive => Revoked == null && !IsExpired;
        
        public User User { get; set; }
    }
}