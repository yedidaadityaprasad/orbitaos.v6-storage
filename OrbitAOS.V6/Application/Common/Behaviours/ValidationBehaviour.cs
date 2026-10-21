// ============================================================
// Application/Common/Behaviours/ValidationBehaviour.cs
// Clean Architecture – Application Layer
// MediatR pipeline behaviour that runs FluentValidation
// validators before every command/query handler executes.
// Replaces ad-hoc ModelState.IsValid checks in legacy controllers.
// ============================================================
using FluentValidation;
using MediatR;

namespace OrbitAOS.V6.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that validates every request
/// using registered FluentValidation validators before the
/// handler is invoked.
/// Throws <see cref="ValidationException"/> when validation fails,
/// which is caught by the global exception handler middleware.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The MediatR response type.</typeparam>
public sealed class ValidationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of <see cref="ValidationBehaviour{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="validators">All validators registered for <typeparamref name="TRequest"/>.</param>
    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Count > 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
