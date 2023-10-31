namespace ExampleRESTfulService.Interfaces;

public interface IWeatherForecastRepository
{
    WeatherForecast UpdateWeatherForecastMethod(int id, string summary);
    WeatherForecast UpdateWeatherForecastMethod(WeatherForecast forecast);
    (bool Pass, string Message) IsValidSummary(int id, string summary);

    IEnumerable<WeatherForecast> GetSummaryWeatherForecastsData(string request);
    (bool Pass, string Message) ValidateWeatherForecast(WeatherForecast forecast);
    bool DeleteForecastById(int id);
    bool PrimaryKeyExist(int id);
    IEnumerable<WeatherForecast> GetAllWeatherForecasts();
    WeatherForecast CreateWeatherForecastMethod(WeatherForecast weatherForecast);
}
