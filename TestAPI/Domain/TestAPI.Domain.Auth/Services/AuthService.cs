using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TestAPI.Domain.Auth.Interfaces;
using TestAPI.Domain.Entities;
using TestAPI.Domain.Exceptions;
using TestAPI.Infrastructure;
using TestAPI.Infrastructure.Interfaces;

namespace TestAPI.Domain.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IUserRepository userRepository, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<(User User, string Token)> LoginAsync(string email, string password)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(email))
                throw new UnauthorizedException("Email is required.");

            if (string.IsNullOrWhiteSpace(password))
                throw new UnauthorizedException("Password is required.");

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(email.ToLower().Trim());
            
            if (user == null)
                throw new UnauthorizedException("Invalid email or password.");

            // Check if user is active
            if (!user.IsActive)
                throw new UnauthorizedException("User account is inactive.");

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
                throw new UnauthorizedException("Invalid email or password.");

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return (user, token);
        }

        public async Task<User> RegisterAsync(string email, string password, string firstName, string lastName)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(email))
                throw new ErrorCodeFaultException(Types.ErrorCodeType.EmailRequired, "Email is required.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ErrorCodeFaultException(Types.ErrorCodeType.PasswordRequired, "Password is required.");

            if (password.Length < 6)
                throw new ErrorCodeFaultException(Types.ErrorCodeType.ShortPassword, "Password must be at least 6 characters long.");

            if (string.IsNullOrWhiteSpace(firstName))
                throw new ErrorCodeFaultException(Types.ErrorCodeType.FirstNameRequired, "First name is required.");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ErrorCodeFaultException(Types.ErrorCodeType.LastNameRequired, "Last name is required.");

            // Check if email already exists
            var emailExists = await _userRepository.EmailExistsAsync(email.ToLower().Trim());
            if (emailExists)
                throw new ErrorCodeFaultException(Types.ErrorCodeType.EmailAlreadyExists, "Email already exists.");

            // Create user
            var user = new User
            {
                Email = email.ToLower().Trim(),
                PasswordHash = HashPassword(password),
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdUser = await _userRepository.CreateAsync(user);
            await _unitOfWork.CommitAsync();
            
            return createdUser;
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? "TestAPI";
            var audience = jwtSettings["Audience"] ?? "TestAPI";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.EntityKey.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.EntityKey.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
