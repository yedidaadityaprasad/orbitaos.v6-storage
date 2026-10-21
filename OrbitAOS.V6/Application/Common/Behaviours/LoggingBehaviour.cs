// ============================================================
// Application/Common/Behaviours/LoggingBehaviour.cs
// Clean Architecture – Application Layer
// MediatR pipeline behaviour that logs every request and
// its execution time. Replaces legacy action filters used
// for cross-cutting logging concerns.
// ============================================================
using MediatR;
using Microsoft.Extensions.Logging;

namespace OrbitAOS.V6.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that logs the start and
/// completion of every request, including elapsed time.
/// Replaces legacy <c>ActionFilterAttribute</c> logging
/// in the legacy MVC application.
/// </summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The MediatR response type.</typeparam>
public sealed class LoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="LoggingBehaviour{TRequest, TResponse}"/>.
    /// </summary>
    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "Handling {RequestName}: {@Request}", requestName, request);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        _logger.LogInformation(
            "Handled {RequestName} in {ElapsedMilliseconds}ms",
            requestName,
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}
