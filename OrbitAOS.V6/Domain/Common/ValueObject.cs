// ============================================================
// Domain/Common/ValueObject.cs
// Clean Architecture – Domain Layer
// Base class for Value Objects (DDD pattern).
// Value objects are immutable and compared by value, not identity.
// ============================================================
namespace OrbitAOS.V6.Domain.Common;

/// <summary>
/// Abstract base class for DDD Value Objects.
/// Provides structural equality based on component values
/// rather than reference identity.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Returns the atomic components that define equality for this value object.
    /// Derived classes must implement this to list all fields that participate
    /// in equality comparison.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);

    /// <summary>Equality operator.</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
        => Equals(left, right);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !Equals(left, right);
}
