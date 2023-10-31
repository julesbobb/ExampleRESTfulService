# RESTful API Example

An example RESTful APIs with a robust foundation that includes common functionality provided by the BaseController, featuring request ID generation, authentication handling, and security headers middleware.

## Overview

The base controller includes methods for generating request IDs, creating resource locations, authenticating clients, and handling resource retrieval and creation. It's designed to be used as a foundation for building RESTful APIs in your application.

### Prerequisites

- .NET Core SDK [Install .NET Core](https://dotnet.microsoft.com/download)

## Getting Started


1. Add the BaseController.cs to your project.
2. Inherit from BaseController in your API controllers and implement the necessary logic specific to your application.

```
// Example Usage
public class WeatherForecastController : BaseController
{
    // Controller Logic
}
```
## Features

- **Generate Request ID**: Automatically generates a unique request ID for each API request.
- **Generate Resource Location**: Dynamically generates a resource location based on the current request's path.
- **Authentication Handling**: Checks if the client is authenticated and enforces authentication requirements.
- **Security Headers Middleware**: Includes middleware for setting security headers like Content-Security-Policy, X-Content-Type-Options, X-Frame-Options, Strict-Transport-Security, and Cache-Control.
- **Resource Retrieval and Creation**: Provides methods for retrieving and creating resources with error handling.

- ## Usage

1. **Generate Request IDs and Authentication**:
   - Implement your own logic for generating request IDs and handling authentication. Customize the `GenerateRequestId` and `Authenticate` methods in your controllers to fit your application's needs.

2. **Inherit from BaseController**:
   - Inherit from the `BaseController` class to leverage the common functionality provided for resource retrieval, creation, and authentication. Customize and extend the base controller to implement your API's specific business logic.

3. **Customize Security Headers Middleware**:
   - Customize the security headers middleware to meet your application's security requirements. The middleware provided sets headers like Content-Security-Policy, X-Content-Type-Options, X-Frame-Options, Strict-Transport-Security, and Cache-Control. Adjust the middleware settings in the `Configure` method of your Startup class to match your security needs.

Example header
```
access-control-allow-origin: www.example-allowed-origin.com 
cache-control: no-store,private,max-age=3600 
content-security-policy: frame-ancestors 'none' 
content-type: application/json; charset=utf-8 
date: Tue,31 Oct 2023 16:01:15 GMT 
expires: Tue,30 Apr 2024 16:01:16 GMT 
feature-policy: Test Feature Policy 
referrer-policy: Test Referrer Policy 
request-id: 82ca3e12-ac01-41bd-b9a5-3e36cb4ef691 
server: Kestrel 
strict-transport-security: max-age=31536000; includeSubDomains 
www-authenticate: Basic YWxhZGRpbjpvcGVuc2VzYW1l 
x-content-type-options: nosniff 
x-frame-options: SAMEORIGIN 
```
