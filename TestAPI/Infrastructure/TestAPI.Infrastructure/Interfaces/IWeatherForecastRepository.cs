using TestAPI.Domain.Entities;

namespace TestAPI.Infrastructure.Interfaces
{
    public interface IWeatherForecastRepository
    {
        Task<IEnumerable<WeatherForecast>> GetAllAsync();
        Task<WeatherForecast?> GetByIdAsync(long entityKey);
        Task<WeatherForecast> AddAsync(WeatherForecast weatherForecast);
        Task UpdateAsync(WeatherForecast weatherForecast);
        Task DeleteAsync(long entityKey);
    }
}
