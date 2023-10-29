using Microsoft.AspNetCore.Mvc;

namespace ExampleRESTfulService.Controllers;

/// <summary>
/// Represents a controller for managing weather forecast resources in the RESTful API.
/// </summary>
/// <remarks>
/// This controller provides endpoints for retrieving, creating, and updating weather forecasts.
/// It inherits common functionality from the <see cref="BaseController"/> class.
/// </remarks>
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : BaseController
{

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherForecastController"/> class.
    /// </summary>
    /// <param name="configuration">The configuration to be used by the controller.</param>
    public WeatherForecastController(IConfiguration configuration) : base(configuration)
    {
    }

    /// <summary>
    /// Updates a Weather Forecast resource using the PUT method.
    /// This method replaces the entire WeatherForecast entity with the provided data.
    /// </summary>
    /// <param name="forecast">The updated WeatherForecast entity.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
    [HttpPut]
    public IActionResult UpdateWeather(WeatherForecast forecast)
    {
        // Call the generic UpdateResource method to handle the common logic. Passes in the create and validate methods
        return UpdateResource(() => UpdateWeatherForecastMethod(forecast), wf => ValidateWeatherForecast(forecast));
    }

    /// <summary>
    /// Creates a new weather forecast.
    /// This endpoint is accessible via an HTTP POST request and allows you to create a new weather forecast by providing the necessary data in the request body.
    /// </summary>
    /// <param name="forecast">The data for the new weather forecast to be created.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the creation operation.</returns>
    [HttpPost]
    public IActionResult CreateWeatherForecast(WeatherForecast forecast)
    {
        // Call the generic CreateResource method to handle the common logic. Passes in the create and validate methods
        return CreateResource(() => CreateWeatherForecastMethod(forecast), wf => ValidateWeatherForecast(forecast));
    }

    /// <summary>
    /// Updates the summary of a Weather Forecast resource using the PATCH method.
    /// This method allows partial updates by modifying only the 'summary' field of the WeatherForecast entity.
    /// </summary>
    /// <param name="id">The ID of the WeatherForecast entity to be updated.</param>
    /// <param name="summary">The new summary to be applied to the WeatherForecast entity.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
    [HttpPatch]
    public IActionResult UpdateWeatherSummary(int id,  string summary)
    {
        // Call the generic UpdateResource method to handle the common logic. Passes in the create and validate methods
        return UpdateResource(() => UpdateWeatherForecastMethod(id, summary), wf => IsValidSummary(id, summary));
    }

    /// <summary>
    /// Retrieves all weather forecasts.
    /// This endpoint is accessible via an HTTP GET request and returns a collection of all available weather forecasts.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing a collection of weather forecasts.</returns>
    [HttpGet]
    [Route("all-weather")]
    public IActionResult GetAllWeatherForecasts()
    {
        return GetResource(GetWeatherForecastsData);
    }

    /// <summary>
    /// Retrieves custom weather forecasts based on a search request.
    /// This endpoint is accessible via an HTTP GET request and allows you to search for weather forecasts using a custom search request.
    /// </summary>
    /// <param name="request">A custom search request for weather forecasts.</param>
    /// <returns>An <see cref="IActionResult"/> containing weather forecasts that match the search request.</returns>
    [HttpGet]
    [Route("search-weather")]
    public IActionResult GetWeatherForecasts(string request)
    {
        // Create a delegate function using a lambda expression
        Func<IEnumerable<WeatherForecast>> getCustomForecastsFunc = () => GetCustomWeatherForecastsData(request);

        return GetResource(getCustomForecastsFunc);

    }

    private IEnumerable<WeatherForecast> GetCustomWeatherForecastsData(string request)
    {
        return new List<WeatherForecast>();
    }

    private static (bool Pass, string Message) ValidateWeatherForecast(WeatherForecast forecast)
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


    /// <summary>
    /// Deletes a specific resource by its unique identifier.
    /// This endpoint is accessible via an HTTP DELETE request and allows you to delete a resource identified by its unique identifier (ID).
    /// </summary>
    /// <param name="id">The unique identifier of the resource to be deleted.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the deletion operation.</returns>
    [HttpDelete("{id}")]
    public IActionResult DeleteResource(int id)
    {
        if (DeleteResourceById(id))
        {
            return NoContent();
        }
        return Accepted();
    }

    private bool DeleteResourceById(int id)
    {
        //delete the item and return result
        return true;
    }

    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private IEnumerable<WeatherForecast> GetWeatherForecastsData()
    {
        return Enumerable.Range(1, 50).Select(index => new WeatherForecast
        {
            ID = index,
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
    .ToArray();
    }

    private WeatherForecast CreateWeatherForecastMethod(WeatherForecast weatherForecast)
    {
        // Add forecast to the database and return
        return weatherForecast;
    }

    private WeatherForecast UpdateWeatherForecastMethod(WeatherForecast forecast)
    {
        //Add update to database
        return forecast;
    }

    private WeatherForecast UpdateWeatherForecastMethod(int id, string summary)
    {
        // update summary and return the model
        return new WeatherForecast(); //replace with update logic
    }

    /// <summary>
    /// Validates a summary based on its ID and content.
    /// </summary>
    /// <param name="id">The ID of the summary to validate.</param>
    /// <param name="summary">The summary content to validate.</param>
    /// <returns>
    /// A tuple with a Boolean indicating whether the summary is valid (Pass) and an optional validation message (Message).
    /// </returns>
    /// <remarks>
    /// This method can be used to define custom validation logic for summary records.
    /// You can implement your specific validation rules and return a validation result.
    /// </remarks>
    private (bool Pass, string Message) IsValidSummary(int id, string summary)
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

}