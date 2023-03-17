using System.ComponentModel.DataAnnotations;

namespace UM.BLL.Models
{
    public class AuthenticateRequest
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
