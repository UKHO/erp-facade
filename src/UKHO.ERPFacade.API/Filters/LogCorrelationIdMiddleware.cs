using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azure.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Filters
{
    [ExcludeFromCodeCoverage]
    public class LogCorrelationIdMiddleware
    {
        public const string XCorrelationIdHeaderKey = "_X-Correlation-ID";
        private readonly RequestDelegate _next;

        public LogCorrelationIdMiddleware(RequestDelegate next)
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
                JObject json = JObject.Parse(bodyAsText);
                correlationId = json.SelectToken("data.traceId")?.Value<string>();
                httpContext.Request.Body.Position = 0;  //rewinding the stream to 0
            }

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                httpContext.Request.Headers.Add(XCorrelationIdHeaderKey, correlationId);
            }

            httpContext.Response.Headers.Add(XCorrelationIdHeaderKey, correlationId);

            var logger = httpContext.RequestServices.GetRequiredService<ILogger<LogCorrelationIdMiddleware>>();

            var state = new Dictionary<string, object>
            {
                [CorrelationIdMiddleware.XCorrelationIdHeaderKey] = correlationId,
                //["CorrelationId"] = correlationId,
            };

            using (logger.BeginScope(state))
            {
                await _next(httpContext);
            }
        }

        
    }
}