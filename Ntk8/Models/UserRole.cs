﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Ntk8.Model;

namespace Ntk8.Models
{
    public class UserRole
    {
        public int Id { get; set; }
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        
        [JsonIgnore]
        public virtual User User { get; set; }
        
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        
        [JsonIgnore]
        public virtual Role Role { get; set; }
    }
}