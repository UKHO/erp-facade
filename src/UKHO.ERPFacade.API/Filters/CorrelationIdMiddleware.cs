using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Filters
{
    public class CorrelationIdMiddleware
    {
        public const string XCorrelationIdHeaderKey = "_X-Correlation-ID";
        public const string CorrelationIdKey = "correlationId";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();

            using var streamReader = new StreamReader(httpContext.Request.Body);
            var bodyAsString = await streamReader.ReadToEndAsync();

            var correlationId = ExtractCorrelationId(bodyAsString) ?? Guid.NewGuid().ToString();

            httpContext.Request.Body.Position = 0;

            httpContext.Request.Headers[XCorrelationIdHeaderKey] = correlationId;
            httpContext.Response.Headers[XCorrelationIdHeaderKey] = correlationId;

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

        private string? ExtractCorrelationId(string bodyAsString)
        {
            if (string.IsNullOrWhiteSpace(bodyAsString))
            {
                return null;
            }

            if (JToken.Parse(bodyAsString) is JObject bodyAsJson)
            {
                var token = bodyAsJson.SelectToken($"..{CorrelationIdKey}");
                return token?.ToString() ?? null;
            }

            return null;
        }
    }
}
