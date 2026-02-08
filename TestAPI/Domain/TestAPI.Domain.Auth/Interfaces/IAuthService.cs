using TestAPI.Domain.Entities;

namespace TestAPI.Domain.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<(User User, string Token)> LoginAsync(string email, string password);
        Task<User> RegisterAsync(string email, string password, string firstName, string lastName);
        string GenerateJwtToken(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
