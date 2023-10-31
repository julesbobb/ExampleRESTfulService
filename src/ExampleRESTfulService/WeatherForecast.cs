namespace ExampleRESTfulService;

public class WeatherForecast
{
    public int ID { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}
