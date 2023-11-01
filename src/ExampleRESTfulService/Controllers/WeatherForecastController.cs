using ExampleRESTfulService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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
            if (forecast.ID<0 || !_repo.PrimaryKeyExist(forecast.ID))
            {
                // Return a 422 Unprocessable Entity response with the validation message.
                return CustomBadRequest($"Cannot process. ID {forecast.ID} does not exist");
            }

            var (Pass, Message) = _repo.ValidateWeatherForecast(forecast);
            if (!Pass)
            {
                return CustomUnprocessableEntity(Message);
            }
        }
        catch (Exception ex) { return CustomInternalServerError(ex.Message); }

        // Call the generic UpdateResource method to handle the common logic. 
        return UpdateResource(()=> _repo.UpdateWeatherForecastMethod(forecast), "forecast");
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
            if (id < 0 || !_repo.PrimaryKeyExist(id))
            {
                // Return a 422 Unprocessable Entity response with the validation message.
                return CustomBadRequest($"Cannot process. ID {id} does not exist");
            }
        }
        catch (Exception ex) { return CustomInternalServerError(ex.Message); }

        // Call the generic UpdateResource method to handle the common logic. 
        return UpdateResource(() => _repo.UpdateWeatherForecastMethod(id, summary), "forecast");
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
        return GetResource(_repo.GetAllWeatherForecasts,  "forecasts");
    }

    /// <summary>
    /// Retrieves all weather forecasts.
    /// This endpoint is accessible via an HTTP GET request and returns a collection of all available weather forecasts.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing a collection of weather forecasts.</returns>
    [HttpGet]
    [Route("{id}")]
    public IActionResult Get(int id)
    {
        return GetResource(() => _repo.GetForecast(id), "forecast");
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
        return GetResource(() => _repo.GetSummaryWeatherForecastsData(summary), "forecasts");
    }

    /// <summary>
    /// Deletes a specific resource by its unique identifier.
    /// This endpoint is accessible via an HTTP DELETE request and allows you to delete a resource identified by its unique identifier (ID).
    /// </summary>
    /// <param name="id">The unique identifier of the resource to be deleted.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the deletion operation.</returns>
    [HttpDelete("{id}")]
    public IActionResult DeleteForecast(int id)
    {
        try
        {
            if (!_repo.PrimaryKeyExist(id))
            {
                // Return a 422 Unprocessable Entity response with the validation message.
                return CustomBadRequest($"Cannot process. ID {id} does not exist");
            }
        }
        catch (Exception ex) { return CustomInternalServerError(ex.Message); }

        return DeleteResource(() => _repo.DeleteForecastById(id));
    }


    #region Token-based paging 

    /// <summary>
    /// Get the first page of weather forecasts.
    /// </summary>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>The first page of weather forecasts.</returns>
    /// <remarks>
    /// <para>Example call:</para>
    /// <example><code>GET /api/WeatherForecast/initial?pageSize=10</code></example>
    /// </remarks>
    [HttpGet("initial")]
    public ActionResult<PagedResult<WeatherForecast>> GetInitialWeatherForecasts([FromQuery] int pageSize = 10)
    {
        try
        {
            if (pageSize <= 0 || pageSize > DefaultMaxPageSize) 
            {
                return (ActionResult)CustomBadRequest("Invalid pageSize value.");
            }

            int startIndex = 0; // Default to start from the beginning

            var pageWeatherForecasts = _repo.GetAllWeatherForecasts().Skip(startIndex).Take(pageSize).ToList();
            var nextToken = CreateToken(startIndex + pageWeatherForecasts.Count);

            return (ActionResult)CustomOKResult(BuildResponse(pageWeatherForecasts, startIndex / pageSize, pageSize, pageWeatherForecasts.Count, nextToken));
        }
        catch (Exception ex)
        {
            return CustomStatusCode(500, "An error occurred: " + ex.Message);
        }
    }


    /// <summary>
    /// Get the next page of weather forecasts.
    /// </summary>
    /// <param name="token">The token for paging.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>The next page of weather forecasts.</returns>
    /// <remarks>
    /// <para>Example call:</para>
    /// <example><code>GET /api/WeatherForecast/next?token=base64encodedtoken&amp;pageSize=10</code></example>
    /// </remarks>    
    [HttpGet("next")]
    public ActionResult<PagedResult<WeatherForecast>> GetNextWeatherForecasts([FromQuery] string token, int pageSize = 10, int pageOffset = 1)
    {
        try
        {
            if (pageSize <= 0 || pageSize > DefaultMaxPageSize)
            {
                return (ActionResult)CustomBadRequest("Invalid pageSize value.");
            }

            int startIndex = ParseToken(token);
            IEnumerable<WeatherForecast> forecast = _repo.GetAllWeatherForecasts();
            startIndex = (pageOffset - 1) * pageSize;

            if (startIndex >= forecast.Count())
            {
                return (ActionResult)CustomOKResult(new PagedResult<WeatherForecast>());
            }

            var pageWeatherForecasts = forecast.Skip(startIndex).Take(pageSize).ToList();
            var nextToken = CreateToken(startIndex + pageWeatherForecasts.Count);

            return (ActionResult)CustomOKResult(BuildResponse(pageWeatherForecasts, pageOffset, pageSize, pageWeatherForecasts.Count, nextToken));
        }
        catch (FormatException fex)
        {
            return (ActionResult)CustomBadRequest("Invalid token: " + fex.Message);
        }
        catch (Exception ex)
        {
            return CustomStatusCode(500, "An error occurred: " + ex.Message);
        }
    }

    private int DefaultMaxPageSize => _configuration.GetValue<int>("maxPageSize");

    private string CreateToken(int startIndex)
    {
        // Create a token that represents the current position
        return Convert.ToBase64String(BitConverter.GetBytes(startIndex));
    }

    private int ParseToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return 0; // Start from the beginning if no token is provided
        }

        byte[] bytes = Convert.FromBase64String(token);
        return BitConverter.ToInt32(bytes, 0);
    }

    private object BuildResponse(List<WeatherForecast> data, int pageOffset, int pageSize, int total, string nextToken)
    {
        Dictionary<string, object> dataNode = new();
        dataNode["forecasts"] = data;

        var meta = new
        {
            pageOffset,
            pageSize,
            total,
        };

        var links = new List<LinkInfo>
    {
        new LinkInfo
        {
            Href = Url.Action("GetNextWeatherForecasts", "WeatherForecast", new { token = nextToken, pageSize }, Request.Scheme),
            Rel = "next"
        }
    };

        return new
        {
            data = dataNode,
            meta,
            links
        };
    }


    #endregion

}

public class PagedResult<T>
{
    public List<T> Data { get; set; }
    public MetaInfo Meta { get; set; }

    public PagedResult()
    {
        Data = new List<T>();
        Meta = new MetaInfo();
    }
}
public class LinkInfo
{
    public string Href { get; set; }
    public string Rel { get; set; }
}

public class MetaInfo
{
    public int PageOffset { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}