namespace TestAPI.Application.DTOs
{
    public class UserDto
    {
        public long EntityKey { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
    }
}
