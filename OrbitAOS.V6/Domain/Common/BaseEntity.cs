// ============================================================
// Domain/Common/BaseEntity.cs
// Clean Architecture – Domain Layer
// Base class for all domain entities.
// All domain entities inherit from this to get audit fields
// and a strongly-typed primary key.
// ============================================================
namespace OrbitAOS.V6.Domain.Common;

/// <summary>
/// Base entity providing common audit fields for all domain entities.
/// Replaces the ad-hoc audit columns that were scattered across
/// legacy EF6 models.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Primary key – integer identity column.</summary>
    public int Id { get; set; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Username or user ID that created the record.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>UTC timestamp of the last modification (null if never modified).</summary>
    public DateTime? LastModifiedAt { get; set; }

    /// <summary>Username or user ID that last modified the record.</summary>
    public string? LastModifiedBy { get; set; }
}
