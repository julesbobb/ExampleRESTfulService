using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace ExampleRESTfulService.Tests;

/// <summary>
/// The base class for unit tests that require an HTTP client for testing APIs.
/// </summary>
public class TestBase
{
    private readonly TestServer _server;
    private readonly HttpClient _client;


    /// <summary>
    /// Gets the HTTP client for making requests to the API during testing.
    /// </summary>
    protected HttpClient Client { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class.
    /// </summary>
    public TestBase()
    {
        // Create a test WebApplicationFactory with additional setup.
        var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => {
            builder.ConfigureTestServices(services => {
                // Additional Setup for testing services
            });
        });

        // Create an HTTP client for making requests to the API.
        Client = application.CreateClient();
    }
}
