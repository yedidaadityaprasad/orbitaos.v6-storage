// ============================================================
// Data/ApplicationDbContext.cs
// Clean Architecture – Infrastructure / Data Layer
// EF Core 10 DbContext.
// Upgraded from EF Core 6.0.19 to EF Core 10.0.0.
// Implements IApplicationDbContext so Application layer
// handlers depend on the abstraction, not the concrete class.
// Includes automatic audit field population via SaveChangesAsync.
// ============================================================
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrbitAOS.V6.Application.Common.Interfaces;
using OrbitAOS.V6.Domain.Common;

namespace OrbitAOS.V6.Data;

/// <summary>
/// EF Core 10 application database context.
/// Inherits from <see cref="IdentityDbContext"/> to include
/// ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, etc.).
/// Implements <see cref="IApplicationDbContext"/> for Clean Architecture
/// dependency inversion.
/// </summary>
public class ApplicationDbContext : IdentityDbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly IDateTimeService? _dateTimeService;

    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationDbContext"/>
    /// with optional Clean Architecture service dependencies.
    /// </summary>
    /// <param name="options">EF Core DbContext options (connection string, provider).</param>
    /// <param name="currentUserService">
    /// Optional: provides the current user for audit fields.
    /// Null during design-time scaffolding / migrations.
    /// </param>
    /// <param name="dateTimeService">
    /// Optional: provides UTC now for audit timestamps.
    /// Null during design-time scaffolding / migrations.
    /// </param>
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null,
        IDateTimeService? dateTimeService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    // -------------------------------------------------------
    // Add domain entity DbSets here as the application grows.
    // Example:
    //   public DbSet<Product> Products => Set<Product>();
    // -------------------------------------------------------

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration<T> classes found
        // in this assembly automatically (Clean Architecture pattern).
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    /// <summary>
    /// Overrides SaveChangesAsync to automatically populate
    /// audit fields (CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy)
    /// on all <see cref="BaseEntity"/> instances before saving.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var utcNow = _dateTimeService?.UtcNow ?? DateTime.UtcNow;
        var currentUser = _currentUserService?.UserId ?? "system";

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.CreatedBy = currentUser;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = utcNow;
                    entry.Entity.LastModifiedBy = currentUser;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
