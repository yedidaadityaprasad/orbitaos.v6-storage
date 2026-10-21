// ============================================================
// Models/ErrorViewModel.cs
// Clean Architecture – Web Layer (Presentation / ViewModels)
// No breaking changes from net6 to net10 for this model.
// Kept in Models/ folder as a ViewModel (not a domain entity).
// ============================================================
namespace OrbitAOS.V6.Models;

/// <summary>
/// ViewModel for the Error view.
/// Carries the ASP.NET Core request trace identifier so that
/// support teams can correlate browser errors with server logs.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Gets or sets the ASP.NET Core request trace identifier.
    /// Set from <c>Activity.Current?.Id ?? HttpContext.TraceIdentifier</c>
    /// in the Error action of HomeController.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="RequestId"/>
    /// should be displayed to the user.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
