using Microsoft.EntityFrameworkCore;
using TestAPI.Domain.Entities;
using TestAPI.Infrastructure.Interfaces;
using TestAPI.Infrastructure.Repositories.SqlServer;
using TestAPI.Infrastructure.WebSockets.Interfaces;

namespace TestAPI.Infrastructure.Repositories
{
    public class WeatherForecastRepository : IWeatherForecastRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebSocketService _webSocketService;

        public WeatherForecastRepository(ApplicationDbContext context, IWebSocketService webSocketService)
        {
            _context = context;
            _webSocketService = webSocketService;
        }

        public async Task<IEnumerable<WeatherForecast>> GetAllAsync()
        {
            return await _context.WeatherForecasts.ToListAsync();
        }

        public async Task<WeatherForecast?> GetByIdAsync(long entityKey)
        {
            return await _context.WeatherForecasts.FindAsync(entityKey);
        }

        public async Task<WeatherForecast> AddAsync(WeatherForecast weatherForecast)
        {
            _context.WeatherForecasts.Add(weatherForecast);
            await _webSocketService.SendToAllAsync(weatherForecast);
            return await Task.FromResult(weatherForecast);
        }

        public async Task UpdateAsync(WeatherForecast weatherForecast)
        {
            _context.Entry(weatherForecast).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(long entityKey)
        {
            var weatherForecast = await _context.WeatherForecasts.FindAsync(entityKey);
            if (weatherForecast != null)
            {
                _context.WeatherForecasts.Remove(weatherForecast);
            }
        }
    }
}
