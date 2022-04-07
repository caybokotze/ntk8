using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ntk8.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string? RoleName { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<UserRole>? UserRoles { get; set; }
    }
}