using System.Text.Json.Serialization;

namespace UM.DAL.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = null!;
        public int GenderId { get; set; }
        public Gender? Gender { get; set; } = null!;
        public Guid? RoleId { get; set; }
        public Role? Role { get; set; } = null!;
        [JsonIgnore] public string PasswordHash { get; set; } = string.Empty;
        [JsonIgnore] public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
