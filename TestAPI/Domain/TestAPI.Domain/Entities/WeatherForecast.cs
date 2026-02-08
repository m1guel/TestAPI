namespace TestAPI.Domain.Entities
{
    public class WeatherForecast : DomainEntity
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF { get; set; }
        public string Summary { get; set; } = string.Empty;

        public override string EntityType => nameof(WeatherForecast);
    }
}
