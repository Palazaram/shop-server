using Microsoft.AspNetCore.Mvc;
using Shop.Domain.Errors;

namespace Shop.Api.Extensions;

public static class ErrorExtensions
{
    public static ProblemDetails ToProblemDetails(this Error error)
    {
        var (status, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation failed"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Server error")
        };

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = error.Message
        };

        problemDetails.Extensions["errors"] = new[]
        {
            new { field = (string?)null, code = error.Code, message = error.Message }
        };

        return problemDetails;
    }
}