using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestAPI.Application.DTOs;
using TestAPI.Domain.DataExample.Interfaces;
using TestAPI.Domain.Entities;

namespace TestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require JWT authentication for all endpoints
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherForecastService _service;
        private readonly IMapper _mapper;

        public WeatherForecastController(IWeatherForecastService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeatherForecastDto>>> GetAll()
        {
            var weatherForecasts = await _service.GetAllWeatherForecastsAsync();
            var weatherForecastDtos = _mapper.Map<IEnumerable<WeatherForecastDto>>(weatherForecasts);
            return Ok(weatherForecastDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WeatherForecastDto>> GetById(long entityKey)
        {
            var weatherForecast = await _service.GetWeatherForecastByIdAsync(entityKey);
            if (weatherForecast == null)
            {
                return NotFound();
            }

            var weatherForecastDto = _mapper.Map<WeatherForecastDto>(weatherForecast);
            return Ok(weatherForecastDto);
        }

        [HttpPost]
        public async Task<ActionResult<WeatherForecastDto>> Create(CreateWeatherForecastDto createDto)
        {
            var weatherForecast = _mapper.Map<WeatherForecast>(createDto);
            var createdWeatherForecast = await _service.CreateWeatherForecastAsync(weatherForecast);
            var weatherForecastDto = _mapper.Map<WeatherForecastDto>(createdWeatherForecast);

            return CreatedAtAction(nameof(GetById), new { entityKey = weatherForecastDto.EntityKey }, weatherForecastDto);
        }

        [HttpPut("{entityKey}")]
        public async Task<IActionResult> Update(long entityKey, UpdateWeatherForecastDto updateDto)
        {
            if (entityKey != updateDto.EntityKey)
            {
                return BadRequest();
            }

            var weatherForecast = _mapper.Map<WeatherForecast>(updateDto);
            var result = await _service.UpdateWeatherForecastAsync(entityKey, weatherForecast);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{entityKey}")]
        public async Task<IActionResult> Delete(long entityKey)
        {
            var result = await _service.DeleteWeatherForecastAsync(entityKey);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
