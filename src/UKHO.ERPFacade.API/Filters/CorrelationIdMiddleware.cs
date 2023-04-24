using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Filters
{
    public class CorrelationIdMiddleware
    {
        public const string XCORRELATIONIDHEADERKEY = "_X-Correlation-ID";
        public const string TRACEIDKEY = "data.traceId";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            var correlationId = Guid.NewGuid().ToString();

            using var streamReader = new StreamReader(httpContext.Request.Body);
            var bodyAsText = await streamReader.ReadToEndAsync();

            if (!string.IsNullOrWhiteSpace(bodyAsText))
            {
                JObject bodyAsJson = JObject.Parse(bodyAsText);
                correlationId = bodyAsJson.SelectToken(TRACEIDKEY)?.Value<string>();
            }

            httpContext.Request.Body.Position = 0;

            httpContext.Request.Headers.Add(XCORRELATIONIDHEADERKEY, correlationId);
            httpContext.Response.Headers.Add(XCORRELATIONIDHEADERKEY, correlationId);

            var state = new Dictionary<string, object>
            {
                [XCORRELATIONIDHEADERKEY] = correlationId!,
            };

            var logger = httpContext.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>();
            using (logger.BeginScope(state))
            {
                await _next(httpContext);
            }
        }
    }
}