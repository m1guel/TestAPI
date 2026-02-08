using AutoMapper;
using TestAPI.Application.DTOs;
using TestAPI.Domain.Entities;

namespace TestAPI.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // WeatherForecast mappings
            CreateMap<WeatherForecast, WeatherForecastDto>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.Date)))
                .ReverseMap()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToDateTime(TimeOnly.MinValue)));

            CreateMap<CreateWeatherForecastDto, WeatherForecast>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToDateTime(TimeOnly.MinValue)))
                .ForMember(dest => dest.EntityKey, opt => opt.Ignore())
                .ForMember(dest => dest.TemperatureF, opt => opt.Ignore());

            CreateMap<UpdateWeatherForecastDto, WeatherForecast>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToDateTime(TimeOnly.MinValue)))
                .ForMember(dest => dest.TemperatureF, opt => opt.Ignore());

            // User mappings
            CreateMap<User, UserDto>();
            
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.EntityKey, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
        }
    }
}
