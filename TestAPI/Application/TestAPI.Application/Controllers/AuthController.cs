using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestAPI.Application.DTOs;
using TestAPI.Domain.Auth.Interfaces;

namespace TestAPI.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            IMapper mapper,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginRequest)
        {
            _logger.LogInformation("Login attempt for email: {Email}", loginRequest.Email);

            var (user, token) = await _authService.LoginAsync(loginRequest.Email, loginRequest.Password);

            var userDto = _mapper.Map<UserDto>(user);
            var response = new LoginResponseDto
            {
                Token = token,
                User = userDto
            };

            _logger.LogInformation("User {UserId} logged in successfully", user.EntityKey);

            return Ok(response);
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequestDto registerRequest)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", registerRequest.Email);

            var user = await _authService.RegisterAsync(
                registerRequest.Email,
                registerRequest.Password,
                registerRequest.FirstName,
                registerRequest.LastName);

            var userDto = _mapper.Map<UserDto>(user);

            return CreatedAtAction(nameof(GetCurrentUser), new { id = user.EntityKey }, userDto);
        }

        /// <summary>
        /// Get current authenticated user
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("Invalid user ID in token");
                return Unauthorized();
            }

            _logger.LogDebug("Retrieving current user with ID: {UserId}", userId);

            // For now, return user info from claims
            var userDto = new UserDto
            {
                EntityKey = userId,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty,
                FirstName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty,
                LastName = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? string.Empty
            };

            return Ok(userDto);
        }
    }
}
