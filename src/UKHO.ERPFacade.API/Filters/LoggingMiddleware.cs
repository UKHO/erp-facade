using System.Net;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Filters
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ILoggerFactory _loggerFactory { get; }

        public LoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _loggerFactory = loggerFactory;
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
                    _loggerFactory
                        .CreateLogger(httpContext.Request.Path)
                        .LogError(eventId.ToEventId(), exception, eventId.ToString() + ". | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);
                }
                else
                {
                    _loggerFactory
                    .CreateLogger(httpContext.Request.Path)
                    .LogError(EventIds.UnhandledException.ToEventId(), exception, "Exception occured while processing ErpFacade API." + " | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);
                }

                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}