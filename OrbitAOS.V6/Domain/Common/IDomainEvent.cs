// ============================================================
// Domain/Common/IDomainEvent.cs
// Clean Architecture – Domain Layer
// Marker interface for domain events.
// Domain events decouple side-effects from core business logic.
// ============================================================
using MediatR;

namespace OrbitAOS.V6.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Implement this interface on any class that represents
/// a significant occurrence within the domain.
/// Domain events are dispatched via MediatR after the
/// unit of work is committed.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>UTC timestamp when the event occurred.</summary>
    DateTime OccurredOn { get; }
}
