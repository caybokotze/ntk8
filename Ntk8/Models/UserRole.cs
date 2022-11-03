using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Ntk8.Models
{
    public class UserRole
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [JsonIgnore]
        public virtual IUserEntity? User { get; set; }
        
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        
        [JsonIgnore]
        public virtual Role? Role { get; set; }
    }
}