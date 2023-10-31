using ExampleRESTfulService.Interfaces;

namespace ExampleRESTfulService.Services;

public class WeatherForecastRepository : IWeatherForecastRepository
{
    public bool DeleteForecastById(int id)
    {
        // delete the item and return result
        return true;
    }

    public IEnumerable<WeatherForecast> GetSummaryWeatherForecastsData(string request)
    {
        // add logic
        return GetAllWeatherForecasts().Where(x => x.Summary == request);
    }

    private static readonly string[] Summaries = new[]
{
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public IEnumerable<WeatherForecast> GetAllWeatherForecasts()
    {
        return Enumerable.Range(1, 50).Select(index => new WeatherForecast
        {
            ID = index,
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
    .ToArray();
    }

    public (bool Pass, string Message) IsValidSummary(int id, string summary)
    {
        // Implement your custom validation logic here.
        // Replace this with your validation rules.

        if (id < 0)
        {
            return (false, "ID must be a non-negative value.");
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            return (false, "Summary content cannot be empty or whitespace.");
        }

        // Additional validation checks...

        return (true, ""); // Adjust the validation result as needed.
    }

    public WeatherForecast UpdateWeatherForecastMethod(int id, string summary)
    {
        // update summary and return the model
        return new WeatherForecast(); //replace with update logic
    }

    public WeatherForecast UpdateWeatherForecastMethod(WeatherForecast forecast)
    {
        //Add update to database
        return forecast;
    }

    public (bool Pass, string Message) ValidateWeatherForecast(WeatherForecast forecast)
    {
        if (forecast.TemperatureC < -30)
        {
            return (false, "Temperature is too low.");
        }
        else if (forecast.TemperatureC > 40)
        {
            return (false, "Temperature is too high.");
        }

        return (true, "Validation passed.");
    }

    public WeatherForecast CreateWeatherForecastMethod(WeatherForecast weatherForecast)
    {
        // Add forecast to the database and return
        return weatherForecast;
    }

    public bool PrimaryKeyExist(int id)
    {
        return GetAllWeatherForecasts().Any(x => x.ID == id);
    }
}
