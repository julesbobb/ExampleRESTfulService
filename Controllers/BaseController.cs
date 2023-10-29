using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ExampleRESTfulService.Controllers;

/// <summary>
/// The base controller for your RESTful API, providing common functionality for resource retrieval, creation, updating and authentication.
/// </summary>
/// <remarks>
/// This base controller class is designed to be used as a foundation for building RESTful APIs in your application.
/// It includes methods for generating request IDs, creating resource locations, authenticating clients, and handling resource retrieval and creation.
/// You can inherit from this class to implement your API controllers, leveraging the common functionality it provides.
/// <para>
/// PLEASE NOTE - The is an example class. Add the required logic for managing the request ID and authentification.
/// </para>
/// </remarks>
public class BaseController : ControllerBase
{

    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseController"/> class with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration to be used by the controller.</param>
    public BaseController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Generates a unique request ID and adds it to the response headers.
    /// </summary>
    /// <remarks>
    /// This method should be customized with your logic to generate a request ID.
    /// For example, you might store request IDs in a database for tracking purposes.
    /// </remarks>
    private void GenerateRequestId()
    {
        // Your logic to generate a request ID.
        // Replace this with your actual request ID generation code e.g. store in a database.
        var requestID = Guid.NewGuid().ToString();
        Response.Headers.Add("Request-Id", requestID);
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
        string resourceLocation = HttpContext.Request.Path;
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
        // Check if the client is authenticated
        if (!IsAuthorised())
        {
            // Return a 401 Unauthorized response with the required authentication method.
            return UnauthorizedWithAuthenticateHeader("Bearer", "YourRealm");
        }


        return null;
    }

    /// <summary>
    /// Checks if the client is authorized and enforces authorization requirements.
    /// </summary>
    /// <returns>
    /// Returns a Boolean value indicating whether the client is authorized.
    /// </returns>
    /// <remarks>
    /// This method verifies if the client is authorized to access the requested resource.
    /// Implement your security checks here, such as authorization or resource access control.
    /// <example>
    /// Example of usage:
    /// <code>
    /// bool isAuthorized = IsAuthorised();
    /// if (isAuthorized)
    /// {
    ///     // Perform authorized actions.
    /// }
    /// else
    /// {
    ///     // Handle unauthorized access.
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    private bool IsAuthorised()
    {
        // Perform security checks here, e.g., authorization or resource access control.
        return true;
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
    protected IActionResult GetResource<T>(Func<T> getResourceFunc)
    {
        try
        {
            var authentication = Authenticate();
            if (authentication != null)
            {
                return authentication;
            }
            T resource = getResourceFunc();
            var payloadCheck = CheckPayloadSize(resource);
            if (payloadCheck != null)
            {
                return payloadCheck;
            }
            GenerateLocation();
            GenerateRequestId();
            return Ok(resource);
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
    /// Validates a resource using a custom validation function and returns an Unprocessable Entity response if validation fails.
    /// </summary>
    /// <typeparam name="T">The type of the resource to be validated.</typeparam>
    /// <param name="resource">The resource to validate.</param>
    /// <param name="validateResourceFunc">A function that performs validation and returns a validation result.</param>
    /// <returns>
    /// Returns an Unprocessable Entity (422 Unprocessable Entity) response with the provided validation message if validation fails.
    /// Returns null if validation passes.
    /// </returns>
    /// <remarks>
    /// This method allows you to perform custom validation on the provided resource using the validation function.
    /// If the validation function returns a result indicating failure, this method returns an Unprocessable Entity response with the specified validation message.
    /// <example>
    /// Example of usage:
    /// <code>
    /// var validationResult = Validate(resource, resource => ValidateResource(resource));
    /// if (validationResult != null)
    /// {
    ///     return validationResult;
    /// }
    /// // Continue with processing the validated resource.
    /// </code>
    /// </example>
    /// </remarks>
    private IActionResult? Validate<T>(T resource,  Func<T, (bool Pass, string Message)> validateResourceFunc)
    {
        if (validateResourceFunc != null)
        {
            var validationResult = validateResourceFunc(resource);
            if (!validationResult.Pass)
            {
                // Return a 422 Unprocessable Entity response with the validation message.
                return UnprocessableEntity(validationResult.Message);
            }
        }
        return null;
    }

    /// <summary>
    /// A generic PUT or PATCH request that updates a resource using a specified resource update function and returns a response.
    /// </summary>
    /// <typeparam name="T">The type of the resource to be updated.</typeparam>
    /// <param name="updateResourceFunc">A function that updates the resource.</param>
    /// <param name="validateResourceFunc">An optional function that validates the updated resource.</param>
    /// <returns>
    /// Returns a successful response (200 OK) with the updated resource.
    /// Returns a BadRequest response (400 Bad Request) with an error message if an exception occurs or validation fails.
    /// Returns an Unprocessable Entity response (422 Unprocessable Entity) with a validation message if validation fails.
    /// </returns>
    /// <remarks>
    /// This method authenticates the client, updates a resource using the provided function,
    /// and returns a response that complies with RESTful API conventions.
    /// If an exception occurs during the process, it returns a BadRequest response with an error message.
    /// If validation is provided and fails, it returns an Unprocessable Entity response with the validation message.
    /// <example>
    /// Example of usage:
    /// <code>
    /// var resourceResponse = UpdateResource(() => UpdateResourceInDatabase(), resource => ValidateResource(resource));
    /// return resourceResponse;
    /// </code>
    /// </example>
    /// </remarks>    
    protected IActionResult UpdateResource<T>(Func<T> updateResourceFunc, Func<T, (bool Pass, string Message)> validateResourceFunc = null)
    {
        try
        {
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

            var validation = Validate(resource, validateResourceFunc);
            if (validation != null)
            {
                return validation;
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
    /// A generic PUT request that creates a resource using a specified resource creation function and returns a response.
    /// </summary>
    /// <typeparam name="T">The type of the resource to be created.</typeparam>
    /// <param name="createResourceFunc">A function that creates the resource.</param>
    /// <param name="validateResourceFunc">An optional function that validates the created resource.</param>
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
    /// var resourceResponse = CreateResource(() => CreateResourceInDatabase(), resource => ValidateResource(resource));
    /// return resourceResponse;
    /// </code>
    /// </example>
    /// </remarks>    
    protected IActionResult CreateResource<T>(Func<T> createResourceFunc, Func<T, (bool Pass, string Message)> validateResourceFunc = null)
    {
        try
        {
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

            var validation = Validate(resource, validateResourceFunc);
            if (validation != null)
            {
                return validation;
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
    /// It also adds a unique request ID to the response headers using the <see cref="GenerateRequestId"/> method.
    /// The response complies with RESTful API conventions and includes the newly created resource's location.
    /// <example>
    /// Example of usage:
    /// <code>
    /// var resourceResponse = CreateResult(newlyCreatedResource);
    /// return resourceResponse;
    /// </code>
    /// </example>
    /// </remarks>
    private IActionResult CreateResult<T>(T resource)
    {
        string relativePath = GenerateLocation();
        GenerateRequestId();
        return Created(new Uri(relativePath, UriKind.Relative), resource);
    }

    /// <summary>
    /// Returns a 401 Unauthorized response with the specified authentication method and realm.
    /// </summary>
    /// <param name="authenticationMethod">The authentication method required for access.</param>
    /// <param name="realm">The realm for which authentication is required.</param>
    /// <returns>
    /// Returns a 401 Unauthorized response with the specified authentication method and realm in the WWW-Authenticate header.
    /// </returns>
    /// <remarks>
    /// This method is used to enforce authentication requirements by returning a 401 Unauthorized response
    /// with the specified authentication method and realm in the WWW-Authenticate header.
    /// Clients can use this information to authenticate themselves for access.
    /// <example>
    /// Example of usage:
    /// <code>
    /// var unauthorizedResponse = UnauthorizedWithAuthenticateHeader("Bearer", "YourRealm");
    /// return unauthorizedResponse;
    /// </code>
    /// </example>
    /// </remarks>
    private IActionResult UnauthorizedWithAuthenticateHeader(string authenticationMethod, string realm)
    {
        // Return a 401 Unauthorized response with the required authentication method.
        Response.Headers.Add("WWW-Authenticate", $"{authenticationMethod} realm=\"{realm}\"");
        return Unauthorized();
    }
}
