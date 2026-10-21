// ============================================================
// Middleware/GlobalExceptionHandlerMiddleware.cs
// Clean Architecture – Web Layer (Infrastructure/Middleware)
// Replaces: Global.asax Application_Error event handler
//           and legacy custom IHttpModule error handlers.
// .NET 10 pattern: IMiddleware with structured error responses.
// ============================================================
using System.Net;
using System.Text.Json;

namespace OrbitAOS.V6.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches all unhandled exceptions in the request pipeline,
/// logs them with structured logging, and returns a consistent
/// JSON error response for API requests or redirects to the
/// error page for browser requests.
///
/// Replaces:
///   - Global.asax Application_Error event
///   - Legacy custom IHttpModule error handlers
///   - Web.config customErrors section
///
/// Registration in Program.cs:
///   app.UseMiddleware&lt;GlobalExceptionHandlerMiddleware&gt;();
///   (Register before all other middleware)
/// </summary>
public sealed class GlobalExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="GlobalExceptionHandlerMiddleware"/>.
    /// </summary>
    public GlobalExceptionHandlerMiddleware(
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception for request {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Return JSON for API/AJAX requests, redirect for browser requests
        if (context.Request.Headers.Accept.ToString().Contains("application/json"))
        {
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An internal server error occurred.",
                // Only expose details in Development
                Detail = (string?)null
            };

            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
        else
        {
            context.Response.Redirect("/Home/Error");
        }
    }
}
