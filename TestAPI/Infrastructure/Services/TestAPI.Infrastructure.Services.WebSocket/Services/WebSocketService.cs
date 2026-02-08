using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TestAPI.Domain;
using TestAPI.Domain.Entities;
using TestAPI.Infrastructure.WebSockets.Interfaces;

namespace TestAPI.Infrastructure.WebSockets.Services
{
    public class WebSocketService : IWebSocketService
    {
        private readonly IWebSocketManager _webSocketManager;
        private readonly ILogger<WebSocketService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public WebSocketService(ILogger<WebSocketService> logger, IWebSocketManager webSocketManager)
        {
            _webSocketManager = webSocketManager;   
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task SendToUserAsync<T>(string userId, T data) where T : DomainEntity
        {
            var message = new
            {
                Type = data.EntityType,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            await SendMessageToUserAsync(userId, message);
        }

        public async Task SendToAllAsync<T>(T data) where T : DomainEntity
        {
            var message = new
            {
                Type = data.EntityType,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            await SendMessageToAllAsync(message);
            _logger.LogInformation("Weather update broadcasted to all connected users");

        }

        private async Task SendMessageToUserAsync(string userId, object data)
        {
            var connections = _webSocketManager.GetAllConnections();
            var userConnection = connections.FirstOrDefault(c => c.Key == userId);

            if (userConnection.Value != null && userConnection.Value.State == WebSocketState.Open)
            {
                await SendMessageAsync(userConnection.Value, data);
                _logger.LogDebug("Message sent to user {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("User {UserId} not connected or socket not open", userId);
            }
        }

        private async Task SendMessageToAllAsync(object data)
        {
            var connections = _webSocketManager.GetAllConnections();
            var tasks = connections.Select(kvp => SendMessageAsync(kvp.Value, data));

            await Task.WhenAll(tasks);

            _logger.LogInformation("Broadcasted message to {Count} connected users", connections.Count());
        }

        private async Task SendMessageAsync(WebSocket socket, object data)
        {
            if (socket.State != WebSocketState.Open)
            {
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var bytes = Encoding.UTF8.GetBytes(json);

                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WebSocket message");
            }
        }
    }
}