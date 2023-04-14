using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Filters
{
    [ExcludeFromCodeCoverage]
    public class CorrelationIdMiddleware
    {
        public const string XCorrelationIdHeaderKey = "_X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string? correlationId = string.Empty;

            if (httpContext.Request.ContentLength > 0)
            {
                httpContext.Request.EnableBuffering();

                var bodyAsText = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
                JObject bodyAsJson = JObject.Parse(bodyAsText);
                correlationId = bodyAsJson.SelectToken("data.traceId")?.Value<string>();

                httpContext.Request.Headers.Add(XCorrelationIdHeaderKey, correlationId);
                httpContext.Request.Body.Position = 0;  //rewinding the stream to 0
            }

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                httpContext.Request.Headers.Add(XCorrelationIdHeaderKey, correlationId);
            }

            httpContext.Response.Headers.Add(XCorrelationIdHeaderKey, correlationId);

            var state = new Dictionary<string, object>
            {
                [XCorrelationIdHeaderKey] = correlationId,
            };

            var logger = httpContext.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>();
            using (logger.BeginScope(state))
            {
                await _next(httpContext);
            }
        }
    }
}