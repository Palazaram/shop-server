using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shop.Persistence.Exceptions;

namespace Shop.Api.ExceptionHandling;

internal sealed class DuplicateKeyExceptionHandler(ILogger<DuplicateKeyExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DuplicateKeyException duplicateKey)
            return false;

        logger.LogWarning("Unique constraint violated: {Constraint}", duplicateKey.ConstraintName);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Detail = "A record with the same unique value already exists."
        };

        problemDetails.Extensions["errors"] = new[]
        {
            new { field = (string?)null, code = "resource.duplicate", message = problemDetails.Detail }
        };

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}