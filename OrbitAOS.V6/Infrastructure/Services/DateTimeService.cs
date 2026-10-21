// ============================================================
// Infrastructure/Services/DateTimeService.cs
// Clean Architecture – Infrastructure Layer
// Concrete implementation of IDateTimeService.
// Wraps DateTime.UtcNow so Application layer code is
// fully unit-testable without time-dependent side effects.
// ============================================================
using OrbitAOS.V6.Application.Common.Interfaces;

namespace OrbitAOS.V6.Infrastructure.Services;

/// <summary>
/// Provides the current UTC date and time via the system clock.
/// In unit tests, replace this with a mock or stub that returns
/// a fixed date to make tests deterministic.
/// </summary>
public sealed class DateTimeService : IDateTimeService
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
