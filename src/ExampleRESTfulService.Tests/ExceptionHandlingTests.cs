using ExampleRESTfulService.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace ExampleRESTfulService.Tests;

/// <summary>
/// Unit tests for handling exceptions in the WeatherForecastController.
/// These tests check the behavior when exceptions are thrown in controller actions.
/// </summary>
internal class ExceptionHandlingTests
{

    [Test]
    public async Task GetAllWeatherForecasts_ThrowsException_ReturnsInternalServerError()
    {
        // Act
        var response = await TestHttpClient().GetAsync("/WeatherForecast/all-weather");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("An error occurred:");
    }

    [Test]
    public async Task CreateWeatherForecastMethod_ThrowsException_ReturnsInternalServerError()
    {
        // Act
        var response = await TestHttpClient().GetAsync("/WeatherForecast/GetSummaryForecasts/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("An error occurred:");
    }

    [Test]
    public async Task UpdateWeatherForecastMethod_ThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var forecast = new WeatherForecast { TemperatureC = 25, ID = 10 };
        var content = new StringContent(JsonConvert.SerializeObject(forecast), Encoding.UTF8, "application/json");

        // Act
        var response = await TestHttpClient().PutAsync("/WeatherForecast", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("An error occurred:");
    }

    [Test]
    public async Task DeleteForecastById_ThrowsException_ReturnsInternalServerError()
    {
        // Act
        var response = await TestHttpClient().DeleteAsync("/WeatherForecast/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("An error occurred:");
    }

    [Test]
    public async Task PrimaryKeyExist_ThrowsException_ReturnsInternalServerError()
    {
        // Act
        var response = await TestHttpClient().DeleteAsync("/WeatherForecast/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("An error occurred:");
    }

    [Test]
    public async Task PrimaryKeyDoesNotExist_ThrowsException_ReturnsInternalServerError()
    {
        // Act
        var response = await TestHttpClient(false).DeleteAsync("/WeatherForecast/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("An error occurred:");
    }

    /// <summary>
    /// Creates an HttpClient configured with different payload size settings for testing.
    /// </summary>
    /// <param name="primaryKeyExists"></param>
    private HttpClient TestHttpClient(bool primaryKeyExists = true)
    {
        var mockRepository = new Mock<IWeatherForecastRepository>();
        mockRepository.Setup(r => r.GetAllWeatherForecasts())
                .Callback(() => throw new Exception());

        mockRepository.Setup(r => r.CreateWeatherForecastMethod(It.IsAny<WeatherForecast>()))
                .Callback(() => throw new Exception());

        mockRepository.Setup(r => r.UpdateWeatherForecastMethod(It.IsAny<WeatherForecast>()))
                .Callback(() => throw new Exception());

        mockRepository.Setup(r => r.DeleteForecastById(It.IsAny<int>()))
                .Callback(() => throw new Exception());

        mockRepository.Setup(r => r.GetSummaryWeatherForecastsData(It.IsAny<string>()))
            .Callback(() => throw new Exception());

        if (primaryKeyExists)
        {
            mockRepository.Setup(r => r.PrimaryKeyExist(It.IsAny<int>()))
                    .Returns(true);
        }
        else
        {
            mockRepository.Setup(r => r.PrimaryKeyExist(It.IsAny<int>()))
                    .Callback(() => throw new Exception());
        }

        mockRepository.Setup(r => r.ValidateWeatherForecast(It.IsAny<WeatherForecast>()))
            .Returns((true, ""));


        var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Additional Setup for testing services
                services.AddScoped(provider => mockRepository.Object);
            });
        });
        var client = application.CreateClient();
        return client;
    }

}

