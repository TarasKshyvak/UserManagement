namespace UserManagement.Entities
{
    public class Gender
    {
        public int Id { get; set; }
        public string GenderName { get; set; } = null!;
        public IEnumerable<User>? Users{ get; set; } = null!;
    }
}
