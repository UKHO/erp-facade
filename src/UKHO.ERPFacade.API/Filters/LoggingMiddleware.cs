using System.Net;
using UKHO.ERPFacade.API.Controllers;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Filters
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception exception)
            {
                var exceptionType = exception.GetType();
                var correlationId = httpContext!.Request.Headers[CorrelationIdMiddleware.XCorrelationIdHeaderKey].FirstOrDefault()!;

                if (exceptionType == typeof(ERPFacadeException))
                {
                    EventIds eventId = (EventIds)((ERPFacadeException)exception).EventId.Id;
                    _logger.LogError(eventId.ToEventId(), exception, eventId.ToString() + ". | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);
                }
                else
                {
                    _logger.LogError(EventIds.UnhandledException.ToEventId(), exception, "Exception occured while processing ErpFacade API." + " | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);
                }

                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}