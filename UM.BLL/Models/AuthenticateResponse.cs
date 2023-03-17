using System.Text.Json.Serialization;
using UM.DAL.Entities;

namespace UM.BLL.Models
{
    public class AuthenticateResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string JwtToken { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
        public string RefreshToken { get; set; }

        public AuthenticateResponse(User user, string jwtToken, string refreshToken)
        {
            Id = user.Id;
            Username = user.Username;
            Role = user.Role!.RoleName;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }
}
