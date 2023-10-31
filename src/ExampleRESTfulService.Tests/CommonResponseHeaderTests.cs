using FluentAssertions;

namespace ExampleRESTfulService.Tests;

/// <summary>
/// This class contains unit tests for common response headers added by the controller.
/// </summary>
internal class CommonResponseHeaderTests : TestBase
{

    /// <summary>
    /// Tests whether a GET request response contains the "WWW-Authenticate" header.
    /// </summary>
    [Test]
    public async Task GetRequest_ContainsAuthenticateHeader()
    {
        // Act
        var response = await Client.GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.Headers.Should().ContainKey("WWW-Authenticate");
    }

    /// <summary>
    /// Tests whether a GET request response contains the "Access-Control-Allow-Origin" header.
    /// </summary>
    [Test]
    public async Task GetRequest_ContainsAccessControlAllowOriginHeader()
    {
        // Act
        var response = await Client.GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin");

    }

    /// <summary>
    /// Tests whether a GET request response contains the "Expires" header.
    /// Note: A different methodology is used to extract and validate the "Expires" header value due to the way it's handled in the response.
    /// </summary>    
    [Test]
    public async Task GetRequest_ContainsExpiresHeader()
    {
        // Act
        var response = await Client.GetAsync("/WeatherForecast/all-weather");
        IEnumerable<string> expiresHeaders;

        object expiresHeaderValue = null;
        if (response.Content.Headers.TryGetValues("Expires", out expiresHeaders))
        {
            // Perform assertions on the "Expires" header value here
            expiresHeaderValue = expiresHeaders?.FirstOrDefault();
        }

        // Assert
        expiresHeaderValue.Should().NotBeNull();
    }

    /// <summary>
    /// Tests whether a GET request response contains the "Feature-Policy" header.
    /// </summary>
    [Test]
    public async Task GetRequest_ContainsFeaturePolicyHeader()
    {
        // Act
        var response = await Client.GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.Headers.Should().ContainKey("Feature-Policy");

    }

    /// <summary>
    /// Tests whether a GET request response contains the "Referrer-Policy" header.
    /// </summary>
    [Test]
    public async Task GetRequest_ContainsReferrerPolicyHeader()
    {
        // Act
        var response = await Client.GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.Headers.Should().ContainKey("Referrer-Policy");

    }

}

