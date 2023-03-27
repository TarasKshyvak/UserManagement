namespace UserManagement.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public int GenderId { get; set; }
        public Guid? RoleId { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
