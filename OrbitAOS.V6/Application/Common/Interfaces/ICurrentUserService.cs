// ============================================================
// Application/Common/Interfaces/ICurrentUserService.cs
// Clean Architecture – Application Layer
// Provides the current authenticated user's identity to
// Application layer handlers without a direct dependency on
// HttpContext (which belongs to the Web/Infrastructure layer).
// ============================================================
namespace OrbitAOS.V6.Application.Common.Interfaces;

/// <summary>
/// Provides information about the currently authenticated user.
/// Replaces direct access to HttpContext.User in legacy controllers.
/// The Infrastructure layer implementation uses IHttpContextAccessor.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's identifier (claim sub / NameIdentifier).
    /// Returns null when the request is unauthenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the current user's display name.
    /// Returns null when the request is unauthenticated.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Returns true when the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
