using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;

namespace ExampleRESTfulService.Tests;

/// <summary>
/// Unit tests for checking payload size handling in a REST service.
/// These tests verify that payloads are correctly processed and that
/// size limitations are enforced.
/// </summary>
internal class PayloadSizeTests
{
    /// <summary>
    /// Verifies that a valid payload of an appropriate size returns an HTTP 200 (OK) response.
    /// </summary>
    [Test]
    public async Task CheckPayloadSize_ValidPayload__ReturnsOk()
    {
        // Act
        var response = await TestHttpClient(10485760).GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Deserialize the response content to check if it's a WeatherForecast list
        var deserializedForecast = JsonConvert.DeserializeObject<List<WeatherForecast>>(responseContent);

        // Now you can assert properties of the deserialized object
        deserializedForecast.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that a payload exceeding the size limit returns an HTTP 413 (Request Entity Too Large) response.
    /// </summary>
    [Test]
    public async Task CheckPayloadSize_PayloadTooLarge_ReturnsStatusCode413()
    {
        // Act
        int maxPayload = 1000;
        var response = await TestHttpClient(maxPayload).GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);

        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Be($"Payload size exceeds the allowed limit of {maxPayload} bytes.");
    }

    /// <summary>
    /// Creates an HttpClient configured with different payload size settings for testing.
    /// </summary>
    private HttpClient TestHttpClient(int maxPayloadSize)
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string> {
            {"MaxPayloadSizeBytes", $"{maxPayloadSize}"}
        };
        IConfiguration _configuration;

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Additional Setup for testing services
                services.AddSingleton(_configuration);
            });
        });
        var client = application.CreateClient();
        return client;
    }

}

