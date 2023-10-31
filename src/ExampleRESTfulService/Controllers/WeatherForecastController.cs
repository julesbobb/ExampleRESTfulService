using ExampleRESTfulService.Interfaces;
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

    private readonly IWeatherForecastRepository _repo;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherForecastController"/> class.
    /// </summary>
    /// <param name="configuration">The configuration to be used by the controller.</param>
    public WeatherForecastController(IConfiguration configuration, IAuthentificationRepository authentificationRepository, IWeatherForecastRepository weatherForecastRepository) : base(configuration, authentificationRepository)
    {
        _repo = weatherForecastRepository;
    }

    /// <summary>
    /// Updates a Weather Forecast resource using the PUT method.
    /// This method replaces the entire WeatherForecast entity with the provided data.
    /// </summary>
    /// <param name="forecast">The updated WeatherForecast entity.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
    [HttpPut()]
    public IActionResult UpdateWeather([FromBody] WeatherForecast forecast)
    {
        try
        {
            var (Pass, Message) = _repo.ValidateWeatherForecast(forecast);
            if (!Pass)
            {
                return CustomUnprocessableEntity(Message);
            }
        }
        catch (Exception ex) { return CustomInternalServerError(ex.Message); }

        // Call the generic UpdateResource method to handle the common logic. 
        return UpdateResource(()=> _repo.UpdateWeatherForecastMethod(forecast));
    }

    /// <summary>
    /// Creates a new weather forecast.
    /// This endpoint is accessible via an HTTP POST request and allows you to create a new weather forecast by providing the necessary data in the request body.
    /// </summary>
    /// <param name="forecast">The data for the new weather forecast to be created.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the creation operation.</returns>
    [HttpPost()]
    public IActionResult CreateWeatherForecast(WeatherForecast forecast)
    {
        try
        {
            var (Pass, Message) = _repo.ValidateWeatherForecast(forecast);
            if (!Pass)
            {
                // Return a 422 Unprocessable Entity response with the validation message.
                return CustomUnprocessableEntity(Message);
            }
        }
        catch (Exception ex) { return CustomInternalServerError(ex.Message); }

        // Call the generic CreateResource method to handle the common logic. Passes in the create method
        return CreateResource(() =>  _repo.CreateWeatherForecastMethod(forecast));
    }

    /// <summary>
    /// Updates the summary of a Weather Forecast resource using the PATCH method.
    /// This method allows partial updates by modifying only the 'summary' field of the WeatherForecast entity.
    /// </summary>
    /// <param name="id">The ID of the WeatherForecast entity to be updated.</param>
    /// <param name="summary">The new summary to be applied to the WeatherForecast entity.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
    [HttpPatch("UpdateWeatherSummary/{id}/{summary}")]
    public IActionResult UpdateWeatherSummary(int id,  string summary)
    {
        try
        {
            var (Pass, Message) = _repo.IsValidSummary(id, summary);
            if (!Pass)
            {
                // Return a 422 Unprocessable Entity response with the validation message.
                return CustomUnprocessableEntity(Message);
            }
        }
        catch (Exception ex) { return CustomInternalServerError(ex.Message); }

        // Call the generic UpdateResource method to handle the common logic. 
        return UpdateResource(() => _repo.UpdateWeatherForecastMethod(id, summary));
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
        return GetResource(_repo.GetAllWeatherForecasts);
    }

    /// <summary>
    /// Retrieves custom weather forecasts based on a search request.
    /// This endpoint is accessible via an HTTP GET request and allows you to search for weather forecasts using a custom search request.
    /// </summary>
    /// <param name="summary">A custom search request for weather forecasts.</param>
    /// <returns>An <see cref="IActionResult"/> containing weather forecasts that match the search request.</returns>
    [HttpGet]
    [Route("GetSummaryForecasts/{summary}")]
    public IActionResult GetSummaryForecasts(string summary)
    {
        return GetResource(() => _repo.GetSummaryWeatherForecastsData(summary));
    }

    /// <summary>
    /// Deletes a specific resource by its unique identifier.
    /// This endpoint is accessible via an HTTP DELETE request and allows you to delete a resource identified by its unique identifier (ID).
    /// </summary>
    /// <param name="id">The unique identifier of the resource to be deleted.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the deletion operation.</returns>
    [HttpDelete("DeleteForecast/{id}")]
    public IActionResult DeleteForecast(int id)
    {
        try
        {
            if (!_repo.PrimaryKeyExist(id))
            {
                // Return a 422 Unprocessable Entity response with the validation message.
                return CustomUnprocessableEntity($"Cannot process. ID {id} does not exist");
            }
        }
        catch (Exception ex) { return CustomInternalServerError(ex.Message); }

        return UpdateResource(() => _repo.DeleteForecastById(id));
    }

}