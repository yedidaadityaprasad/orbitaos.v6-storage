// ============================================================
// Application/Common/Models/Result.cs
// Clean Architecture – Application Layer
// Generic result wrapper used by all Application layer
// commands and queries to communicate success/failure
// without throwing exceptions for expected business errors.
// ============================================================
namespace OrbitAOS.V6.Application.Common.Models;

/// <summary>
/// Represents the outcome of an Application layer operation.
/// Use this instead of throwing exceptions for expected
/// business-rule violations.
/// </summary>
public class Result
{
    /// <summary>Initializes a new instance of <see cref="Result"/>.</summary>
    protected Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool Succeeded { get; }

    /// <summary>Gets the collection of error messages (empty on success).</summary>
    public string[] Errors { get; }

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, Array.Empty<string>());

    /// <summary>Creates a failed result with one or more error messages.</summary>
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    /// <summary>Creates a failed result with a single error message.</summary>
    public static Result Failure(string error) => new(false, new[] { error });
}

/// <summary>
/// Represents the outcome of an Application layer operation
/// that returns a value on success.
/// </summary>
/// <typeparam name="T">The type of the returned value.</typeparam>
public class Result<T> : Result
{
    private Result(bool succeeded, T? value, IEnumerable<string> errors)
        : base(succeeded, errors)
    {
        Value = value;
    }

    /// <summary>Gets the value returned by the operation (null on failure).</summary>
    public T? Value { get; }

    /// <summary>Creates a successful result with a value.</summary>
    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());

    /// <summary>Creates a failed result with one or more error messages.</summary>
    public new static Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);

    /// <summary>Creates a failed result with a single error message.</summary>
    public new static Result<T> Failure(string error) => new(false, default, new[] { error });
}
