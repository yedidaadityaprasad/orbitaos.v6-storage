// ============================================================
// Infrastructure/Services/CurrentUserService.cs
// Clean Architecture – Infrastructure Layer
// Implements ICurrentUserService using IHttpContextAccessor.
// This is the only place in the application that directly
// accesses HttpContext for user identity information.
// Replaces direct HttpContext.User access in legacy controllers.
// ============================================================
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OrbitAOS.V6.Application.Common.Interfaces;

namespace OrbitAOS.V6.Infrastructure.Services;

/// <summary>
/// Provides the current authenticated user's identity by
/// reading claims from the ASP.NET Core HTTP context.
/// Registered as Scoped in Program.cs.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of <see cref="CurrentUserService"/>.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// Injected by the DI container; requires
    /// <c>builder.Services.AddHttpContextAccessor()</c> in Program.cs.
    /// </param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);

    /// <inheritdoc />
    public string? UserName =>
        _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.Name);

    /// <inheritdoc />
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
