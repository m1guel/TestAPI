using System.Net.WebSockets;
using TestAPI.Domain;
using TestAPI.Domain.Entities;

namespace TestAPI.Infrastructure.WebSockets.Interfaces
{
    public interface IWebSocketService
    {
        Task SendToUserAsync<T>(string userId, T data) where T : DomainEntity;
        Task SendToAllAsync<T>(T data) where T : DomainEntity;
    }
}