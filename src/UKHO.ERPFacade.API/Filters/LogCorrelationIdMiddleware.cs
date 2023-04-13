using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Filters
{
    [ExcludeFromCodeCoverage]
    public class LogCorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public LogCorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string? correlationId = httpContext.Request.Headers[CorrelationIdMiddleware.XCorrelationIdHeaderKey].FirstOrDefault();
            var logger = httpContext.RequestServices.GetRequiredService<ILogger<LogCorrelationIdMiddleware>>();

            using (logger.BeginScope("{@CorrelationId}", correlationId))
            {
                await _next(httpContext);
            }
        }
    }
}