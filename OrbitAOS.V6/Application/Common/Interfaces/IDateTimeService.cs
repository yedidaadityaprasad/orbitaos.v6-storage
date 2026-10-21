// ============================================================
// Application/Common/Interfaces/IDateTimeService.cs
// Clean Architecture – Application Layer
// Abstracts DateTime.UtcNow to make Application layer code
// fully unit-testable without time-dependent side effects.
// ============================================================
namespace OrbitAOS.V6.Application.Common.Interfaces;

/// <summary>
/// Provides the current UTC date and time.
/// Inject this interface instead of calling DateTime.UtcNow
/// directly so that unit tests can control time.
/// </summary>
public interface IDateTimeService
{
    /// <summary>Gets the current UTC date and time.</summary>
    DateTime UtcNow { get; }
}
