using Microsoft.EntityFrameworkCore;
using TestAPI.Domain.Entities;
using TestAPI.Infrastructure.Interfaces;
using TestAPI.Infrastructure.Repositories.SqlServer;

namespace TestAPI.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(long entityKey)
        {
            return await _context.Users.FindAsync(entityKey);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower());
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            return await Task.FromResult(user);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email.ToLower());
        }
    }
}
