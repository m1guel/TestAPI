namespace TestAPI.Domain.Entities
{
    public class User : DomainEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        public override string EntityType => nameof(User);
    }
}
