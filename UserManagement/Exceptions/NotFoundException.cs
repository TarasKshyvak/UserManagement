namespace UserManagement.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() { }
        public NotFoundException(string message): base(message) { }
        public NotFoundException(Guid id): base($"Entity with id: {id} was not found") { }
    }
}
