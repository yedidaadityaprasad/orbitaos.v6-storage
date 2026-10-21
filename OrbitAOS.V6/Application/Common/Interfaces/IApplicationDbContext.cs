// ============================================================
// Application/Common/Interfaces/IApplicationDbContext.cs
// Clean Architecture – Application Layer
// Defines the database context contract used by Application
// layer handlers. The Infrastructure layer provides the
// concrete EF Core implementation.
// ============================================================
using Microsoft.EntityFrameworkCore;

namespace OrbitAOS.V6.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext used by Application
/// layer command/query handlers.
/// Decouples Application logic from EF Core infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>Saves all pending changes to the database.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
