using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace ExampleRESTfulService.Tests;

internal class TokenBasedPagingTests : TestBase
{
    [Test]
    public async Task GetInitialWeatherForecasts_ReturnsStatus200Ok()
    {
        // Act
        var response = await Client.GetAsync("WeatherForecast/initial?pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var forecastList = await ExtractWeatherForecastsFromResponse(response);

            // Check if forecastList contains WeatherForecast objects and perform your assertions
            forecastList.Should().NotBeNull();
            forecastList.Should().NotBeEmpty();
            forecastList.Should().AllBeAssignableTo<WeatherForecast>();
            forecastList.Count.Should().Be(10);
        }
    }

    [Test]
    public async Task GetNextWeatherForecasts_ReturnsStatus200Ok()
    {
        // Act
        var response = await Client.GetAsync("/WeatherForecast/next?token=CgAAAA%3D%3D&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var forecastList = await ExtractWeatherForecastsFromResponse(response);
            // Check if forecastList contains WeatherForecast objects and perform your assertions
            forecastList.Should().AllBeAssignableTo<WeatherForecast>();
            forecastList.Should().NotBeNull();
            forecastList.Should().NotBeEmpty();
            forecastList.Count.Should().Be(10);
        }
    }

    [Test]
    public async Task GetNextWeatherForecasts_WithPageOffset_ReturnsStatus200Ok()
    {
        // Act
        var response = await Client.GetAsync($"WeatherForecast/next?token=CgAAAA%3D%3D&pageSize=10&pageOffset={3}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var forecastList = await ExtractWeatherForecastsFromResponse(response);
            // Check if forecastList contains WeatherForecast objects and perform your assertions
            forecastList.Should().AllBeAssignableTo<WeatherForecast>();
            forecastList.Should().NotBeNull();
            forecastList.Should().NotBeEmpty();
            forecastList.Count.Should().Be(10);
        }
    }

    [Test]
    public async Task GetInitialWeatherForecasts_WithLinkInResponse_ReturnsStatus200Ok()
    {
        // Act - Request the initial page
        var response = await Client.GetAsync("WeatherForecast/initial?pageSize=10");

        // Assert - Ensure the request is successful
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            // Check if the response contains a "next" link in the "links" array
            var hasNextLink = ContainsLinkInResponse(response, "next");

            hasNextLink.Should().BeTrue();
        }
    }

    [Test]
    public async Task GetInitialWeatherForecasts_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Act - Request the initial page with an invalid page size
        var response = await Client.GetAsync("WeatherForecast/initial?pageSize=10000");

        // Assert - Ensure the request results in a bad request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetNextWeatherForecasts_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Act - Request the next page with an invalid page size
        var response = await Client.GetAsync("WeatherForecast/next?token=CgAAAA%3D%3D&pageSize=1000");

        // Assert - Ensure the request results in a bad request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private bool ContainsLinkInResponse(HttpResponseMessage response, string rel)
    {
        var responseContent = response.Content.ReadAsStringAsync().Result;
        using (JsonDocument doc = JsonDocument.Parse(responseContent))
        {
            var root = doc.RootElement;
            if (root.TryGetProperty("links", out var links))
            {
                foreach (var link in links.EnumerateArray())
                {
                    if (link.TryGetProperty("rel", out var relProperty))
                    {
                        if (relProperty.GetString() == rel)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    private async Task<List<WeatherForecast>> ExtractWeatherForecastsFromResponse(HttpResponseMessage response)
    {
        var forecastList = new List<WeatherForecast>();

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;
            if (root.TryGetProperty("data", out var data) && data.TryGetProperty("forecasts", out var forecasts))
            {
                foreach (var forecastElement in forecasts.EnumerateArray())
                {
                    var forecast = new WeatherForecast
                    {
                        // Map properties from forecastElement to the WeatherForecast object
                        ID = forecastElement.GetProperty("id").GetInt32(),
                        Date = forecastElement.GetProperty("date").GetDateTime(),
                        TemperatureC = forecastElement.GetProperty("temperatureC").GetInt32(),
                        Summary = forecastElement.GetProperty("summary").GetString(),
                    };

                    forecastList.Add(forecast);
                }
            }
        }

        return forecastList;
    }

}
