using ExampleRESTfulService.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ExampleRESTfulService.Tests;

/// <summary>
/// Unit tests for middleware functionality related to HTTP response headers in a REST service.
/// These headers are an essential part of securing and optimizing RESTful communication.
/// </summary>
internal class MiddlewareTests : TestBase
{
    public MiddlewareTests() : base()
    {
    }

    /// <summary>
    /// Verifies that the response contains the "Content-Security-Policy" header, which specifies
    /// which websites are allowed to access a resource through a web browser.
    /// For example, "frame-ancestors 'none'" indicates that frames are not allowed.
    /// </summary>
    [Test]
    public async Task Should_Return_ContentSecurityPolicyHeader()
    {
        // Arrange 
        var response = await Client.GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.EnsureSuccessStatusCode();

        // Check the response headers here as needed.
        response.Headers.Should().ContainKey("Content-Security-Policy");
        response.Headers.GetValues("Content-Security-Policy").FirstOrDefault().Should().Be("frame-ancestors 'none'");
    }

    /// <summary>
    /// Verifies that the response contains the "X-Content-Type-Options" header, which helps
    /// prevent browsers from MIME-sniffing a response away from the declared content type.
    /// "nosniff" indicates that browsers should not perform MIME-type detection.
    /// </summary>
    [Test]
    public async Task Should_Return_XContentTypeOptionsHeader()
    {
        // Arrange 
        var response = await Client.GetAsync("/WeatherForecast/all-weather"); 

        // Assert
        response.EnsureSuccessStatusCode();

        // Check the response headers here as needed.
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").FirstOrDefault().Should().Be("nosniff");
    }

    /// <summary>
    /// Verifies that the response contains the "X-Frame-Options" header, which controls
    /// whether a browser should be allowed to render a page in a <frame>, <iframe>, <embed>, or <object>.
    /// "SAMEORIGIN" indicates that the page can only be displayed in a frame on the same origin as the page.
    /// </summary>
    [Test]
    public async Task Should_Return_XFrameOptionsHeader()
    {
        // Arrange 
        var response = await Client.GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.EnsureSuccessStatusCode();

        // Check the response headers here as needed.
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").FirstOrDefault().Should().Be("SAMEORIGIN");
    }

    /// <summary>
    /// Verifies that the response contains the "Strict-Transport-Security" header, which instructs
    /// browsers to only interact with the web service using secure HTTPS connections.
    /// For example, "max-age=31536000; includeSubDomains" indicates that the policy is in effect for a year
    /// and includes all subdomains.
    /// </summary>
    [Test]
    public async Task Should_Return_StrictTransportSecurityHeader()
    {
        // Arrange 
        var response = await Client.GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.EnsureSuccessStatusCode();

        // Check the response headers here as needed.
        response.Headers.Should().ContainKey("Strict-Transport-Security");
        response.Headers.GetValues("Strict-Transport-Security").FirstOrDefault().Should().Be("max-age=31536000; includeSubDomains");
    }

    /// <summary>
    /// Verifies that the response contains the "Cache-Control" header, which controls
    /// how web content is cached and request/responses are related to cache.
    /// For example, "no-store, max-age=3600, private" indicates not to store the response in cache,
    /// allow the response to be cached for a maximum of 3600 seconds, and make the cache entry private.
    /// </summary>
    [Test]
    public async Task Should_Return_CacheControlHeader()
    {
        // Arrange 
        var response = await Client.GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.EnsureSuccessStatusCode();

        // Check the response headers here as needed.
        response.Headers.Should().ContainKey("Cache-Control");
        response.Headers.GetValues("Cache-Control").FirstOrDefault().Should().Be("no-store, max-age=3600, private");
    }

}

