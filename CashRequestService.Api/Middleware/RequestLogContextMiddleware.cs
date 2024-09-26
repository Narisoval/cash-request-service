using Serilog.Context;

namespace CashRequestService.Api.Middleware;

public class RequestLogContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLogContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        {
            return _next(context);
        }
    }
}