namespace TestAPI.Application.DTOs
{
    public class WeatherForecastDto
    {
        public long EntityKey { get; set; }
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
