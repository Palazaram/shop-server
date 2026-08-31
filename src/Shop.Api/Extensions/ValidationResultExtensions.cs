using Microsoft.AspNetCore.Mvc;
using FluentValidation.Results;

namespace Shop.Api.Extensions;

public static class ValidationResultExtensions
{
    public static ProblemDetails ToProblemDetails(this ValidationResult validationResult)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed"
        };

        problemDetails.Extensions["errors"] = validationResult.Errors
            .Select(failure => new
            {
                field = ToCamelCase(failure.PropertyName),
                code = failure.ErrorCode,
                message = failure.ErrorMessage
            })
            .ToArray();

        return problemDetails;
    }

    private static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return propertyName;

        return string.Join('.', propertyName
            .Split('.')
            .Select(segment => segment.Length == 0
                ? segment
                : char.ToLowerInvariant(segment[0]) + segment[1..]));
    }
}