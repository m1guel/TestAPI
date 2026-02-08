using TestAPI.Domain.DataExample.Interfaces;
using TestAPI.Domain.Entities;
using TestAPI.Infrastructure;
using TestAPI.Infrastructure.Interfaces;

namespace TestAPI.Domain.DataExample.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly IWeatherForecastRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public WeatherForecastService(IUnitOfWork unitOfWork, IWeatherForecastRepository repository )
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<IEnumerable<WeatherForecast>> GetAllWeatherForecastsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<WeatherForecast?> GetWeatherForecastByIdAsync(long entityKey)
        {
            return await _repository.GetByIdAsync(entityKey);
        }

        public async Task<WeatherForecast> CreateWeatherForecastAsync(WeatherForecast weatherForecast)
        {
            // Calculate TemperatureF based on TemperatureC
            weatherForecast.TemperatureF = 32 + (int)(weatherForecast.TemperatureC / 0.5556);
            
            var created = await _repository.AddAsync(weatherForecast);
            await _unitOfWork.CommitAsync();
            
            return created;
        }

        public async Task<bool> UpdateWeatherForecastAsync(long entityKey, WeatherForecast weatherForecast)
        {
            var existingWeatherForecast = await _repository.GetByIdAsync(entityKey);
            if (existingWeatherForecast == null)
            {
                return false;
            }

            // Calculate TemperatureF based on TemperatureC
            weatherForecast.TemperatureF = 32 + (int)(weatherForecast.TemperatureC / 0.5556);
            weatherForecast.EntityKey = entityKey;

            await _repository.UpdateAsync(weatherForecast);
            await _unitOfWork.CommitAsync();
            
            return true;
        }

        public async Task<bool> DeleteWeatherForecastAsync(long entityKey)
        {
            var weatherForecast = await _repository.GetByIdAsync(entityKey);
            if (weatherForecast == null)
            {
                return false;
            }

            await _repository.DeleteAsync(entityKey);
            await _unitOfWork.CommitAsync();
            
            return true;
        }
    }
}
