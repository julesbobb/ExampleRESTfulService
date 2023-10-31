namespace ExampleRESTfulService.Interfaces;

/// <summary>
/// An interface used within the BaseController. This is so unit tests can be performed
/// </summary>
public interface IAuthentificationRepository
{
    /// <summary>
    /// Checks if the client is authorized and enforces authorization requirements.
    /// </summary>
    bool IsAuthenticated(HttpRequest request);
}
