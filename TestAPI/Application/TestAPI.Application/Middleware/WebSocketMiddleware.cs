using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TestAPI.Infrastructure.WebSockets.Interfaces;

namespace TestAPI.Application.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly IWebSocketManager _connectionManager;
        private readonly IConfiguration _configuration;
        private readonly RequestDelegate _next;
        private readonly ILogger<WebSocketMiddleware> _logger;

        public WebSocketMiddleware(RequestDelegate next, 
            ILogger<WebSocketMiddleware> logger, 
            IWebSocketManager connectionManager,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _connectionManager = connectionManager;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if request is for WebSocket endpoint
            if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
            {
                _logger.LogInformation("WebSocket connection request received from {RemoteIp}",
                    context.Connection.RemoteIpAddress);

                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await HandleWebSocketConnectionAsync(context, webSocket);
            }
            else if (context.Request.Path == "/ws" && !context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("WebSocket connection required");
            }
            else
            {
                await _next(context);
            }
        }

        public async Task HandleWebSocketConnectionAsync(HttpContext context, WebSocket webSocket)
        {
            // Authenticate using JWT token
            var userId = await AuthenticateWebSocketAsync(context);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("WebSocket connection rejected: Invalid or missing JWT token");
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.PolicyViolation,
                    "Authentication required",
                    CancellationToken.None);
                return;
            }

            // Add connection to manager
            await _connectionManager.AddConnectionAsync(userId, webSocket);

            // Send welcome message
            //await SendWelcomeMessageAsync(webSocket, userId);

            _logger.LogInformation("User {UserId} connected via WebSocket", userId);

            // Keep connection alive (one-way: server sends, client receives only)
            await KeepConnectionAliveAsync(webSocket, userId);
        }

        private async Task<string?> AuthenticateWebSocketAsync(HttpContext context)
        {
            try
            {
                // Get token from query string (WebSocket can't send custom headers during handshake)
                var token = context.Request.Query["access_token"].FirstOrDefault();

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No access_token provided in WebSocket connection");
                    return null;
                }

                // Validate JWT token using the same settings as API
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];

                if (string.IsNullOrEmpty(secretKey))
                {
                    _logger.LogError("JWT SecretKey not configured");
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                _logger.LogDebug("JWT token validated successfully for user {UserId}", userId);

                return userId;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("JWT token expired");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JWT token for WebSocket");
                return null;
            }
        }

        private async Task SendWelcomeMessageAsync(WebSocket socket, string userId)
        {
            var welcomeMessage = new
            {
                Type = "Connected",
                Message = "Successfully connected to WebSocket server",
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(welcomeMessage, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var bytes = Encoding.UTF8.GetBytes(json);

            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None);
            }
        }

        private async Task KeepConnectionAliveAsync(WebSocket webSocket, string userId)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                // Keep the connection open and listen for close requests
                // Since this is one-way, we don't process any messages from clients
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("User {UserId} requested to close WebSocket", userId);
                        await _connectionManager.RemoveConnectionAsync(userId);
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Connection closed by client",
                            CancellationToken.None);
                        break;
                    }

                    // Ignore any text/binary messages (one-way communication: server -> client only)
                    if (result.MessageType == WebSocketMessageType.Text ||
                        result.MessageType == WebSocketMessageType.Binary)
                    {
                        _logger.LogDebug("Ignoring message from client {UserId} (one-way communication)", userId);
                    }
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, "WebSocket error for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in WebSocket connection for user {UserId}", userId);
            }
            finally
            {
                await _connectionManager.RemoveConnectionAsync(userId);
                _logger.LogInformation("User {UserId} disconnected from WebSocket", userId);
            }
        }
    }

    public static class WebSocketMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketMiddleware>();
        }
    }
}