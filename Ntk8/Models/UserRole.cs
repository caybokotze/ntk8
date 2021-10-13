using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Ntk8.Models
{
    public class UserRole
    {
        public int Id { get; set; }
        [ForeignKey(nameof(BaseUser))]
        public int UserId { get; set; }

        [JsonIgnore]
        public virtual BaseUser BaseUser { get; set; }
        
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        
        [JsonIgnore]
        public virtual Role Role { get; set; }
    }
}