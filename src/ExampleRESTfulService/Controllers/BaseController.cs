using ExampleRESTfulService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExampleRESTfulService.Controllers;

/// <summary>
/// The base controller for your RESTful API, providing common functionality for resource retrieval, creation, updating and authentication.
/// </summary>
/// <remarks>
/// This base controller class is designed to be used as a foundation for building RESTful APIs in your application.
/// It includes methods for generating request IDs, creating resource locations, authenticating clients, and handling resource retrieval and creation.
/// You can inherit from this class to implement your API controllers, leveraging the common functionality it provides.
/// <para>
/// PLEASE NOTE - The is an example class. Add the required logic for managing the request ID, location and authentification (see TODO tasks).
/// </para>
/// </remarks>
public class BaseController : ControllerBase
{

    protected readonly IConfiguration _configuration;
    private readonly IAuthentificationRepository _authentificationRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseController"/> class with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration to be used by the controller.</param>
    public BaseController(IConfiguration configuration, IAuthentificationRepository authentificationRepository)
    {
        _configuration = configuration;
        _authentificationRepository = authentificationRepository;
    }

    /// <summary>
    /// Extracts the Request-ID and adds it to the header
    /// </summary>
    private void AddRequestId()
    {
        if (HttpContext.Request.Headers.TryGetValue("Request-Id", out var requestIdValues))
        {
            string requestId = requestIdValues.FirstOrDefault();
            Response.Headers.Add("Request-Id", requestId);
        }
    }

    /// <summary>
    /// Generates a resource location dynamically based on the request context and sets it in the response headers.
    /// </summary>
    /// <returns>The generated resource location.</returns>
    /// <remarks>
    /// This method is used to create a resource location based on the current request's path.
    /// The generated resource location is used in the Location header of the response.
    /// </remarks>
    private string GenerateLocation()
    {
        // Generate the resourceLocation dynamically, for example, based on the request context.
        // TODO Create location containing the relative path of the newly created resource. 
        // example:
        var locationID = Guid.NewGuid().ToString();

        string resourceLocation = $"{HttpContext.Request.Path}/{locationID}";
        // Set the Location header with the provided resourceLocation.
        Response.Headers.Add("Location", resourceLocation);
        return resourceLocation;
    }

    /// <summary>
    /// Checks if the client is authenticated and enforces authentication requirements.
    /// </summary>
    /// <returns>
    /// Returns a 401 Unauthorized response with the required authentication method if the client is not authenticated.
    /// Returns null if the client is authenticated, indicating successful authentication.
    /// </returns>
    /// <remarks>
    /// This method verifies if the client is authenticated and enforces authentication requirements.
    /// If the client is not authenticated, it returns a 401 Unauthorized response with the specified authentication method.
    /// Additional security checks, such as authorization or resource access control, can be performed here.
    /// If these checks fail, a 403 Forbidden response can be returned.
    /// </remarks>
    private IActionResult? Authenticate()
    {
        if (!_authentificationRepository.IsAuthenticated(Request))
        {
            // Return a 401 Unauthorized response with the required authentication method.
            return Unauthorized();

        }
        return null;
    }

    /// <summary>
    /// A generic GET request that retrieves a resource using a specified resource retrieval function.
    /// </summary>
    /// <typeparam name="T">The type of the resource to be retrieved.</typeparam>
    /// <param name="getResourceFunc">A function that retrieves the resource.</param>
    /// <returns>
    /// Returns a successful response (200 OK) with the retrieved resource.
    /// Returns a BadRequest response (400 Bad Request) with an error message if an exception occurs.
    /// </returns>
    /// <remarks>
    /// This method authenticates the client, retrieves a resource using the provided function,
    /// generates the resource's location, and adds a request ID to the response headers.
    /// If an exception occurs during the process, it returns a BadRequest response with an error message.
    /// <example>
    /// Example of usage:
    /// <code>
    /// var resource = GetResource(() => RetrieveResourceFromDatabase());
    /// return resource;
    /// </code>
    /// </example>
    /// </remarks>
    protected IActionResult GetResource<T>(Func<T> getResourceFunc, string nodeName)
    {
        try
        {
            ApplyCommonResponseHeaders();
            var authentication = Authenticate();
            if (authentication != null)
            {
                return authentication;
            }
            T resource = getResourceFunc();

            if (resource == null)
            {
                return NoContent();
            }

            var payloadCheck = CheckPayloadSize(resource);
            if (payloadCheck != null)
            {
                return payloadCheck;
            }

            Dictionary<string, object> dataNode = new();

            if (resource is Array resourceArray)
            {
                dataNode[nodeName] = resourceArray;
            }
            else if (resource is T singleResource)
            {
                dataNode[nodeName] = singleResource;
            }
            else
            {
                // Handle other resource types as needed
                return NotFound();
            }

            var response = new
            {
                data = dataNode
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Handle the exception and return a BadRequest response with an error message.
            return StatusCode(500, "An error occurred: " + ex.Message);
        }
    }

    /// <summary>
    /// Checks if the payload size of the resource exceeds the maximum allowed size.
    /// </summary>
    /// <param name="resource">The resource to check the payload size for.</param>
    /// <returns>
    /// Returns a 500 Internal Server Error response if the payload size cannot be determined due to serialization issues.
    /// Returns a 413 Payload Too Large response if the payload size exceeds the allowed limit.
    /// Returns null if the payload size is within the allowed limit.
    /// </returns>
    /// <remarks>
    /// This method checks if the object is serializable and calculates the payload size.
    /// If serialization fails, it returns a 500 Internal Server Error response.
    /// If the payload size exceeds the allowed limit, it returns a 413 Payload Too Large response.
    /// </remarks>
    private IActionResult? CheckPayloadSize(object? resource)
    {
        if (resource == null) { return null; }

        // Check if the object is serializable
        if (!IsSerializable(resource, out string? jsonString))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to determine payload size.");
        }

        // Check payload size
        long payloadSize = Encoding.UTF8.GetByteCount(jsonString);
        long maxPayloadSizeBytes = _configuration.GetValue<long>("MaxPayloadSizeBytes");

        if (payloadSize > maxPayloadSizeBytes)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, $"Payload size exceeds the allowed limit of {maxPayloadSizeBytes} bytes.");
        }
        return null;
    }

    /// <summary>
    /// Checks if an object is serializable and provides the serialized JSON string.
    /// </summary>
    /// <param name="data">The object to check for serializability.</param>
    /// <param name="jsonString">The JSON string resulting from serialization if successful; otherwise, null.</param>
    /// <returns>
    /// Returns true if the object is serializable; otherwise, false.
    /// </returns>
    /// <remarks>
    /// This method checks if the provided object can be successfully serialized to a JSON string.
    /// If successful, it provides the serialized JSON string via the jsonString output parameter.
    /// </remarks>
    private bool IsSerializable(object? data, out string? jsonString)
    {
        jsonString = null;

        if (data == null)
        {
            return false;
        }

        try
        {
            // Serialize the object to a JSON string and provide the result
            jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,  // Set to true if you want indented JSON
            });
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns an Bad Request (404) response with a custom error message.
    /// An "Authenticate" header is added to the response, and a unique request ID is generated.
    /// </summary>
    /// <param name="message">The custom error message to include in the response.</param>
    /// <returns>An <see cref="IActionResult"/> representing an BadRequest response.</returns>
    internal IActionResult CustomBadRequest(string message)
    {
        ApplyCommonResponseHeaders();
        return BadRequest(message);
    }

    internal ObjectResult CustomStatusCode(int code, string message)
    {
        ApplyCommonResponseHeaders();
        return StatusCode(code, "An error occurred: " + message);
    }


    /// <summary>
    /// Returns an Bad Request (404) response with a custom error message.
    /// An "Authenticate" header is added to the response, and a unique request ID is generated.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing an BadRequest response.</returns>
    internal IActionResult CustomOKResult(object resource)
    {
        ApplyCommonResponseHeaders();
        return Ok(resource);
    }


    /// <summary>
    /// Returns an Unprocessable Entity (422) response with a custom error message.
    /// An "Authenticate" header is added to the response, and a unique request ID is generated.
    /// </summary>
    /// <param name="message">The custom error message to include in the response.</param>
    /// <returns>An <see cref="IActionResult"/> representing an Unprocessable Entity response.</returns>
    internal IActionResult CustomUnprocessableEntity(string message)
    {
        ApplyCommonResponseHeaders();
        return UnprocessableEntity(message);
    }

    /// <summary>
    /// Returns a 500 Internal Server Error response with a custom error message.
    /// An "Authenticate" header is added to the response, and a unique request ID is generated.
    /// </summary>
    /// <param name="message">The custom error message to include in the response.</param>
    /// <returns>An <see cref="IActionResult"/> representing an Unprocessable Entity response.</returns>
    internal IActionResult CustomInternalServerError(string message)
    {
        ApplyCommonResponseHeaders();
        return StatusCode(500, "An error occurred: " + message);
    }

    /// <summary>
    /// Deletes a resource based on the provided delete function and returns an appropriate response.
    /// </summary>
    /// <typeparam name="T">The type of the resource to be deleted.</typeparam>
    /// <param name="deleteResourceFunc">A function that performs the resource deletion.</param>
    /// <param name="nodeName">The name of the node for serialization purposes.</param>
    /// <returns>
    /// Returns an Accepted (202 Accepted) response if the resource is successfully deleted.
    /// Returns a BadRequest (400 Bad Request) response with an error message if an exception occurs.
    /// Returns a response with common headers applied, such as authentication and other response headers.
    /// </returns>
    protected IActionResult DeleteResource<T>(Func<T> deleteResourceFunc)
    {
        try
        {
            ApplyCommonResponseHeaders();
            var authentication = Authenticate();
            if (authentication != null)
            {
                return authentication;
            }

            T resource = deleteResourceFunc();

            return Accepted(resource);

        }
        catch (Exception ex)
        {
            // Handle the exception and return a BadRequest response with an error message.
            return StatusCode(500, "An error occurred: " + ex.Message);
        }
    }


    /// <summary>
    /// A generic PUT, PATCH, or DELETE request that updates a resource using a specified resource update function and returns a response.
    /// </summary>
    /// <typeparam name="T">The type of the resource to be updated.</typeparam>
    /// <returns>
    /// Returns a successful response (200 OK) with the updated resource.
    /// Returns a BadRequest response (400 Bad Request) with an error message if an exception occurs or validation fails.
    /// </returns>
    /// <remarks>
    /// This method authenticates the client, updates a resource using the provided function,
    /// and returns a response that complies with RESTful API conventions.
    /// If an exception occurs during the process, it returns a BadRequest response with an error message.
    /// If validation is provided and fails, it returns an Unprocessable Entity response with the validation message.
    /// <example>
    /// Example of usage:
    /// <code>
    /// return UpdateResource(() => _repo.UpdateWeatherForecastMethod(forecast));
    /// </code>
    /// </example>
    /// </remarks>    
    protected IActionResult UpdateResource<T>(Func<T> updateResourceFunc, string nodeName)
    {
        try
        {
            ApplyCommonResponseHeaders();
            var authentication = Authenticate();
            if (authentication != null)
            {
                return authentication;
            }

            T resource = updateResourceFunc();

            if (resource == null)
            {
                // Return a 400 Bad Request response when the resource creation fails.
                return BadRequest("Failed to update the resource.");
            }

            Dictionary<string, object> dataNode = new();

            if (resource is Array resourceArray)
            {
                dataNode[nodeName] = resourceArray;
            }
            else if (resource is T singleResource)
            {
                dataNode[nodeName] = singleResource;
            }
            else
            {
                // Handle other resource types as needed
                return NotFound();
            }

            var response = new
            {
                data = dataNode
            };

            return Accepted(response);

        }
        catch (Exception ex)
        {
            // Handle the exception and return a BadRequest response with an error message.
            return StatusCode(500, "An error occurred: " + ex.Message);
        }
    }

    /// <summary>
    /// A generic POST request that creates a resource using a specified resource creation function and returns a response.
    /// </summary>
    /// <typeparam name="T">The type of the resource to be created.</typeparam>
    /// <param name="createResourceFunc">A function that creates the resource.</param>
    /// <returns>
    /// Returns a successful response (201 Created) with the newly created resource and its location.
    /// Returns a BadRequest response (400 Bad Request) with an error message if an exception occurs or validation fails.
    /// Returns an Unprocessable Entity response (422 Unprocessable Entity) with a validation message if validation fails.
    /// </returns>
    /// <remarks>
    /// This method authenticates the client, creates a resource using the provided function,
    /// and returns a response that complies with RESTful API conventions.
    /// If an exception occurs during the process, it returns a BadRequest response with an error message.
    /// If validation is provided and fails, it returns an Unprocessable Entity response with the validation message.
    /// <example>
    /// Example of usage:
    /// <code>
    /// return CreateResource(() =>  _repo.CreateWeatherForecastMethod(forecast));
    /// </code>
    /// </example>
    /// </remarks>    
    protected IActionResult CreateResource<T>(Func<T> createResourceFunc)
    {
        try
        {
            ApplyCommonResponseHeaders();
            var authentication = Authenticate();
            if (authentication != null)
            {
                return authentication;
            }

            T resource = createResourceFunc();

            if (resource == null)
            {
                // Return a 400 Bad Request response when the resource creation fails.
                return BadRequest("Failed to create the resource.");
            }

            return CreateResult(resource);
        }
        catch (Exception ex)
        {
            // Handle the exception and return a BadRequest response with an error message.
            return StatusCode(500, "An error occurred: " + ex.Message);
        }
    }

    /// <summary>
    /// Creates an `Created` response for a newly created resource with its location.
    /// </summary>
    /// <typeparam name="T">The type of the newly created resource.</typeparam>
    /// <param name="resource">The newly created resource.</param>
    /// <returns>
    /// Returns a successful response (201 Created) with the newly created resource and its location.
    /// </returns>
    /// <remarks>
    /// This method constructs a relative path to the newly created resource's location using the <see cref="GenerateLocation"/> method.
    /// It also adds a unique request ID to the response headers using the <see cref="AddRequestId"/> method.
    /// The response complies with RESTful API conventions and includes the newly created resource's location.
    /// </remarks>
    private IActionResult CreateResult<T>(T resource)
    {
        string relativePath = GenerateLocation();
        AddRequestId();
        return Created(new Uri(relativePath, UriKind.Relative), resource);
    }


    #region Common Headers

    private void ApplyCommonResponseHeaders()
    {
        AddAuthenticateHeader();
        AddAccessControlAllowOriginHeader();
        AddExpiresHeader();
        AddFeaturePolicyHeader();
        AddReferrerPolicyHeader();
        AddRequestId();
    }

    /// <summary>
    /// Adds an "WWW-Authenticate" header to the HTTP response based on the configuration value.
    /// This method is typically used to inform clients about the required authentication method.
    /// </summary>
    private void AddAuthenticateHeader()
    {
        string authenticateHeader = _configuration.GetValue<string>("AuthenticateHeader");
        Response.Headers.Add("WWW-Authenticate", authenticateHeader);
    }

    /// <summary>
    /// Adds the "Access-Control-Allow-Origin" header to the HTTP response, specifying which websites are allowed to access a resource through a web browser.
    /// </summary>
    private void AddAccessControlAllowOriginHeader()
    {
        string allowedOrigin = _configuration.GetValue<string>("AllowedOrigin");
        Response.Headers.Add("Access-Control-Allow-Origin", allowedOrigin);
    }

    /// <summary>
    /// Adds an "Expires" header to the HTTP response specifying the date and time after which the response is considered stale or no longer valid.
    /// The date and time are expressed in RFC 5322 date and time format.
    /// </summary>
    private void AddExpiresHeader()
    {
        Response.Headers.Add("Expires", ExpiryDate.ToString("r"));
    }

    /// <summary>
    /// Adds a "Feature-Policy" header to the HTTP response to define the features that are either permitted or prohibited for client-side code operating within your API's context.
    /// For example, to deny all features, set the value to 'none'.
    /// </summary>
    private void AddFeaturePolicyHeader()
    {
        string featurePolicy = _configuration.GetValue<string>("FeaturePolicy");
        Response.Headers.Add("Feature-Policy", featurePolicy);
    }

    /// <summary>
    /// Adds a "Referrer-Policy" header to the HTTP response to determine the level of referrer information exposure.
    /// For example, to prevent sending referrer information, set the value to 'no-referrer'.
    /// </summary>
    private void AddReferrerPolicyHeader()
    {
        string referrerPolicy = _configuration.GetValue<string>("ReferrerPolicy");
        Response.Headers.Add("Referrer-Policy", referrerPolicy);
    }

    //TODO Add logic to create expiry data
    public DateTime ExpiryDate => DateTime.Now.AddMonths(6);
    #endregion

}