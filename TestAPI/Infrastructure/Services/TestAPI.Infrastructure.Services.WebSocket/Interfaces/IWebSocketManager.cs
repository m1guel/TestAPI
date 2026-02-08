using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace TestAPI.Infrastructure.WebSockets.Interfaces
{
    public interface IWebSocketManager
    {
        Task AddConnectionAsync(string userId, System.Net.WebSockets.WebSocket socket);
        Task RemoveConnectionAsync(string userId);
        Task<IEnumerable<string>> GetAllConnectedUserIdsAsync();
        Task<int> GetConnectionCountAsync();
        IEnumerable<KeyValuePair<string, System.Net.WebSockets.WebSocket>> GetAllConnections();
    }
}
