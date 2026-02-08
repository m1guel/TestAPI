namespace TestAPI.Application.DTOs
{
    public class UpdateWeatherForecastDto
    {
        public long EntityKey { get; set; }
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
