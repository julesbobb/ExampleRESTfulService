using ExampleRESTfulService.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;

namespace ExampleRESTfulService.Tests;

/// <summary>
/// Unit tests for authentication scenarios.
/// </summary>
internal class AuthentificationTests
{

    /// <summary>
    /// Verifies that an unauthorized request contains the "WWW-Authenticate" header
    /// and has the correct HTTP status code (401 Unauthorized).
    /// </summary>
    [Test]
    public async Task NotAuthorised_ContainsAuthenticateaderAndHasCorrectStatusCode()
    {
        using HttpClient client = TestHttpClient(false);

        // Act
        var response = await client.GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that an authorized request does not contain the "WWW-Authenticate" header
    /// and has the correct HTTP status code (200 OK).
    /// </summary>
    [Test]
    public async Task IsAuthorised_DoesNotContainAuthenticateaderAndHasCorrectStatusCode()
    {
       using HttpClient client = TestHttpClient(true);

        // Act
        var response = await client.GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }


    /// <summary>
    /// Creates an <see cref="HttpClient"/> with a mocked authentication repository for testing.
    /// </summary>
    /// <param name="isAuthorised">A boolean indicating whether the request should be authorized.</param>
    /// <returns>An <see cref="HttpClient"/> instance for testing.</returns>
    private static HttpClient TestHttpClient(bool isAuthorised)
    {
        // Arrange
        var mockRepository = new Mock<IAuthentificationRepository>();
        mockRepository.Setup(r => r.IsAuthenticated(It.IsAny<HttpRequest>())).Returns(isAuthorised);

        var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Additional Setup for testing services
                services.AddSingleton(mockRepository.Object);
            });
        });
        var client = application.CreateClient();
        return client;
    }
}

