using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace ExampleRESTfulService.Tests;

/// <summary>
/// Unit tests for verifying responses and actions of the WeatherForecastController.
/// These tests check the behavior of actions that create, update, pathch and delete methods.
/// </summary>
internal class WeatherControllerReponseTests : TestBase
{

    /// <summary>
    /// Verifies that creating a valid weather forecast returns an HTTP 201 (Created) response.
    /// </summary>
    [Test]
    public async Task CreateWeatherForecast_ReturnsStatus201Created()
    {
        // Arrange
        var forecast = new WeatherForecast { TemperatureC = -10 };
        string json = JsonConvert.SerializeObject(forecast);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/WeatherForecast", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Deserialize the response content to check if it's a WeatherForecast
        var deserializedForecast = JsonConvert.DeserializeObject<WeatherForecast>(responseContent);

        // Now you can assert properties of the deserialized object
        deserializedForecast.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that attempting to update a weather forecast with an illegal ID returns an HTTP 422 (Unprocessable Entity) response.
    /// </summary>
    [Test]
    public async Task UpdateWeatherSummary_IllegalID_ReturnsUnprocessableEntity()
    {
        // Act
        var response = await Client.PatchAsync("/WeatherForecast/UpdateWeatherSummary/-23/summary", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Be("ID must be a non-negative value.");
    }

    /// <summary>
    /// Verifies that deleting a weather forecast with a valid ID returns an HTTP 200 (OK) response.
    /// </summary>
    [Test]
    public async Task DeleteForecast_ValidID_ReturnsOk()
    {
        // Act
        var response = await Client.DeleteAsync("/WeatherForecast/DeleteForecast/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var expectedContent = "true"; // Replace with your expected content
        responseContent.Should().Be(expectedContent);
    }

    /// <summary>
    /// Verifies that attempting to delete a weather forecast with an invalid ID returns an HTTP 422 (Unprocessable Entity) response.
    /// </summary>
    [Test]
    public async Task DeleteForecast_InvalidID_ReturnsUnprocessableEntity()
    {
        // Act
        int id = 1000;
        var response = await Client.DeleteAsync($"/WeatherForecast/DeleteForecast/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Be($"Cannot process. ID {id} does not exist");
    }

    /// <summary>
    /// Verifies that checking a weather forecast with an excessively low temperature returns an HTTP 422 (Unprocessable Entity) response.
    /// </summary>
    [Test]
    public async Task ValidateWeatherForecast_TemperatureTooLow_ReturnsUnprocessableEntity()
    {
        // Arrange
        var forecast = new WeatherForecast { TemperatureC = -100};
        var content = new StringContent(JsonConvert.SerializeObject(forecast), Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync("/WeatherForecast", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Be("Temperature is too low.");
    }

    /// <summary>
    /// Verifies that checking a weather forecast with an excessively high temperature returns an HTTP 422 (Unprocessable Entity) response.
    /// </summary>
    [Test]
    public async Task ValidateWeatherForecast_TemperatureTooHigh_ReturnsUnprocessableEntity()
    {
        // Arrange
        var forecast = new WeatherForecast { TemperatureC = 100 };
        var content = new StringContent(JsonConvert.SerializeObject(forecast), Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync("WeatherForecast/", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Be("Temperature is too high.");
    }

    /// <summary>
    /// Verifies that updating a weather forecast with a valid temperature returns an HTTP 200 (OK) response.
    /// </summary>
    [Test]
    public async Task UpdateWeather_ValidTemperature_ReturnsOk()
    {
        // Arrange
        var forecast = new WeatherForecast { TemperatureC = 25 };
        var content = new StringContent(JsonConvert.SerializeObject(forecast), Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync("/WeatherForecast", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Deserialize the response content to check if it's a WeatherForecast
        var deserializedForecast = JsonConvert.DeserializeObject<WeatherForecast>(responseContent);

        // Now you can assert properties of the deserialized object
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        deserializedForecast.Should().NotBeNull();
    }
}