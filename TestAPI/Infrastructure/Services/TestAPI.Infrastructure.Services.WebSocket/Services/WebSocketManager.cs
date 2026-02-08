using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using TestAPI.Infrastructure.WebSockets.Interfaces;

namespace TestAPI.Infrastructure.WebSockets.Services
{
    public class WebSocketManager : IWebSocketManager
    {
        private readonly ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> _connections = new();
        private readonly ILogger<WebSocketManager> _logger;

        public WebSocketManager(ILogger<WebSocketManager> logger)
        {
            _logger = logger;
        }

        public Task AddConnectionAsync(string userId, System.Net.WebSockets.WebSocket socket)
        {
            // Remove old connection if exists
            if (_connections.TryRemove(userId, out var oldSocket))
            {
                if (oldSocket.State == WebSocketState.Open)
                {
                    _logger.LogWarning("Closing old WebSocket for user {UserId}", userId);
                    _ = oldSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "New connection established",
                        CancellationToken.None);
                }
            }

            _connections.TryAdd(userId, socket);
            _logger.LogInformation("WebSocket connection added for user {UserId}. Total connections: {Count}",
                userId, _connections.Count);

            return Task.CompletedTask;
        }

        public Task RemoveConnectionAsync(string userId)
        {
            if (_connections.TryRemove(userId, out var socket))
            {
                if (socket.State == WebSocketState.Open)
                {
                    _ = socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Connection closed",
                        CancellationToken.None);
                }

                _logger.LogInformation("WebSocket connection removed for user {UserId}. Total connections: {Count}",
                    userId, _connections.Count);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetAllConnectedUserIdsAsync()
        {
            var userIds = _connections
                .Where(kvp => kvp.Value.State == WebSocketState.Open)
                .Select(kvp => kvp.Key)
                .ToList();

            return Task.FromResult<IEnumerable<string>>(userIds);
        }

        public Task<int> GetConnectionCountAsync()
        {
            var count = _connections.Count(kvp => kvp.Value.State == WebSocketState.Open);
            return Task.FromResult(count);
        }

        public IEnumerable<KeyValuePair<string, System.Net.WebSockets.WebSocket>> GetAllConnections()
        {
            return _connections.Where(kvp => kvp.Value.State == WebSocketState.Open);
        }

    }
}
