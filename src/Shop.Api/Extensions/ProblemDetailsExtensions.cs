using Microsoft.AspNetCore.Mvc;
using Shop.Api.Middleware;

namespace Shop.Api.Extensions;

public static class ProblemDetailsExtensions
{
    public static ProblemDetails WithTraceId(this ProblemDetails problemDetails, HttpContext httpContext)
    {
        problemDetails.Extensions["traceId"] =
            httpContext.Items[RequestTraceMiddleware.ItemKey] as string ?? httpContext.TraceIdentifier;

        return problemDetails;
    }
}