namespace UM.DAL.Entities
{
    public class Role : BaseEntity
    {
        public string RoleName { get; set; } = string.Empty;
        public IEnumerable<User>? Users { get; set; } = null!;
    }
}
