// ============================================================
// Filters/GlobalExceptionFilter.cs
// Clean Architecture – Web Layer (Filters)
// Replaces: App_Start/FilterConfig.cs RegisterGlobalFilters
//           and legacy HandleErrorAttribute.
// .NET 10 pattern: IExceptionFilter / IAsyncExceptionFilter
// ============================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OrbitAOS.V6.Filters;

/// <summary>
/// Global MVC exception filter that handles unhandled exceptions
/// thrown by controller actions.
///
/// Replaces:
///   - Legacy <c>HandleErrorAttribute</c> registered in FilterConfig.cs
///   - <c>filters.Add(new HandleErrorAttribute())</c> pattern
///
/// Registration in Program.cs:
///   builder.Services.AddControllersWithViews(options =>
///       options.Filters.Add&lt;GlobalExceptionFilter&gt;());
/// </summary>
public sealed class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    private readonly IModelMetadataProvider _modelMetadataProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="GlobalExceptionFilter"/>.
    /// </summary>
    public GlobalExceptionFilter(
        ILogger<GlobalExceptionFilter> logger,
        IModelMetadataProvider modelMetadataProvider)
    {
        _logger = logger;
        _modelMetadataProvider = modelMetadataProvider;
    }

    /// <inheritdoc />
    public void OnException(ExceptionContext context)
    {
        _logger.LogError(
            context.Exception,
            "Unhandled MVC exception in {Controller}.{Action}",
            context.RouteData.Values["controller"],
            context.RouteData.Values["action"]);

        // Mark exception as handled to prevent propagation
        context.ExceptionHandled = true;

        // Return the Error view with a 500 status code
        var result = new ViewResult
        {
            ViewName = "~/Views/Shared/Error.cshtml",
            StatusCode = StatusCodes.Status500InternalServerError,
            ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState)
            {
                Model = new OrbitAOS.V6.Models.ErrorViewModel
                {
                    RequestId = context.HttpContext.TraceIdentifier
                }
            }
        };

        context.Result = result;
    }
}
