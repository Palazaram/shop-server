using System.Diagnostics;
using Serilog.Context;

namespace Shop.Api.Middleware;

/// <summary>
/// Считает идентификатор запроса один раз: кладёт его в HttpContext — для ответов,
/// и в контекст логирования — для всех строк, записанных во время этого запроса.
/// </summary>
internal sealed class RequestTraceMiddleware(RequestDelegate next)
{
    public const string ItemKey = "TraceId";

    public async Task InvokeAsync(HttpContext context)
    {
        string traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        context.Items[ItemKey] = traceId;

        using (LogContext.PushProperty("TraceId", traceId))
            await next(context);
    }
}