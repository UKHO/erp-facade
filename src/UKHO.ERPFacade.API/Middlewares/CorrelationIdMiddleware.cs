using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Operations;

namespace UKHO.ERPFacade.API.Middlewares
{
    public class CorrelationIdMiddleware
    {
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

            if (string.IsNullOrWhiteSpace(bodyAsString))
            {
                await _next(httpContext);
                return;
            }

            var correlationId = Extractor.ExtractTokenValue(JObject.Parse(bodyAsString), JsonFields.CorrelationIdKey) ?? Guid.NewGuid().ToString();

            httpContext.Request.Body.Position = 0;

            httpContext.Request.Headers[ApiHeaderKeys.XCorrelationIdHeaderKeyName] = correlationId;
            httpContext.Response.Headers[ApiHeaderKeys.XCorrelationIdHeaderKeyName] = correlationId;

            var state = new Dictionary<string, object>
            {
                [ApiHeaderKeys.XCorrelationIdHeaderKeyName] = correlationId,
            };

            var logger = httpContext.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>();

            using (logger.BeginScope(state))
            {
                await _next(httpContext);
            }
        }
    }
}
