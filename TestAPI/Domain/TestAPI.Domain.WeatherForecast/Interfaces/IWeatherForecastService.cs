using TestAPI.Domain.Entities;

namespace TestAPI.Domain.DataExample.Interfaces
{
    public interface IWeatherForecastService
    {
        Task<IEnumerable<WeatherForecast>> GetAllWeatherForecastsAsync();
        Task<WeatherForecast?> GetWeatherForecastByIdAsync(long entityKey);
        Task<WeatherForecast> CreateWeatherForecastAsync(WeatherForecast weatherForecast);
        Task<bool> UpdateWeatherForecastAsync(long entityKey, WeatherForecast weatherForecast);
        Task<bool> DeleteWeatherForecastAsync(long entityKey);
    }
}
