// ============================================================
// Controllers/HomeController.cs
// Clean Architecture – Web Layer (Presentation)
// Upgraded from ASP.NET Core 6.0 to .NET 10.
// Key changes:
//   - Added IMediator injection (Clean Architecture CQRS pattern)
//   - Added ICurrentUserService injection (replaces HttpContext.User)
//   - Error action uses IExceptionHandlerPathFeature (.NET 10 pattern)
//   - All actions remain synchronous where no async work is needed
//   - ResponseCache attribute preserved (uses .NET 10 middleware)
// ============================================================
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrbitAOS.V6.Application.Common.Interfaces;
using OrbitAOS.V6.Models;
using System.Diagnostics;

namespace OrbitAOS.V6.Controllers;

/// <summary>
/// Home controller providing the application's landing page,
/// privacy policy page, and error handling endpoint.
/// Follows Clean Architecture: thin controller that delegates
/// business logic to Application layer via MediatR.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of <see cref="HomeController"/>.
    /// </summary>
    /// <param name="logger">Structured logger injected by DI.</param>
    /// <param name="mediator">MediatR mediator for CQRS dispatch.</param>
    /// <param name="currentUserService">
    /// Current user service (Clean Architecture abstraction over HttpContext.User).
    /// </param>
    public HomeController(
        ILogger<HomeController> logger,
        IMediator mediator,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// GET /  or  GET /Home/Index
    /// Application landing page.
    /// </summary>
    public IActionResult Index()
    {
        _logger.LogInformation(
            "Home/Index accessed by user: {UserId}",
            _currentUserService.UserId ?? "anonymous");

        return View();
    }

    /// <summary>
    /// GET /Home/Privacy
    /// Privacy policy page.
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// GET /Home/Error
    /// Global error handler endpoint.
    /// Uses IExceptionHandlerPathFeature (.NET 10 pattern) to
    /// retrieve exception details in non-development environments.
    /// The [ResponseCache] attribute uses the .NET 10 Response
    /// Caching middleware registered in Program.cs.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // .NET 10: Use IExceptionHandlerPathFeature to get exception details
        // This replaces the legacy Server.GetLastError() pattern from Global.asax
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionFeature?.Error is not null)
        {
            _logger.LogError(
                exceptionFeature.Error,
                "Unhandled exception on path: {Path}",
                exceptionFeature.Path);
        }

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
