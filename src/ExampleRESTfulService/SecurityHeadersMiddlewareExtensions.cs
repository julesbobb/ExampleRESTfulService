using Microsoft.AspNetCore.Builder;
namespace ExampleRESTfulService;

/// <summary>
/// Provides extension methods for configuring security headers middleware to enhance application security.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Configure security headers middleware to enhance application security.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to which the middleware will be applied.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> with security headers middleware configured.</returns>
    public static IApplicationBuilder ApplySecurityHeaders(this IApplicationBuilder app)
    {
        return app
            .UseContentSecurityPolicy()
            .UseXContentTypeOptions()
            .UseXFrameOptions()
            .UseStrictTransportSecurity()
            .UseCacheControl();
    }

    /// <summary>
    /// Apply Content-Security-Policy middleware.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to which the middleware will be applied.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> with Content-Security-Policy middleware configured.</returns>
    private static IApplicationBuilder UseContentSecurityPolicy(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors 'none'");
            await next();
        });
    }

    /// <summary>
    /// Apply X-Content-Type-Options middleware.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to which the middleware will be applied.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> with X-Content-Type-Options middleware configured.</returns>
    private static IApplicationBuilder UseXContentTypeOptions(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            await next();
        });
    }

    /// <summary>
    /// Apply X-Frame-Options middleware.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to which the middleware will be applied.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> with X-Frame-Options middleware configured.</returns>
    private static IApplicationBuilder UseXFrameOptions(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
            await next();
        });
    }

    /// <summary>
    /// Apply Strict-Transport-Security middleware.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to which the middleware will be applied.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> with Strict-Transport-Security middleware configured.</returns>
    private static IApplicationBuilder UseStrictTransportSecurity(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            await next();
        });
    }

    /// <summary>
    /// Apply Cache-Control middleware.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to which the middleware will be applied.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> with Cache-Control middleware configured.</returns>
    private static IApplicationBuilder UseCacheControl(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("Cache-Control", "no-store, private, max-age=3600");
            await next();
        });
    }
}