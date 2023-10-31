using ExampleRESTfulService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExampleRESTfulService.Services;

public class AuthentificationRepository : IAuthentificationRepository
{
    public bool IsAuthenticated(HttpRequest request)
    {
        // TODO Perform security checks here, e.g., authorization or resource access control.
        return true;
    }
}
