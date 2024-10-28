using System.Net;
using Microsoft.AspNetCore.Mvc;
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
                // Proceed with the next middleware in the pipeline
                await _next(httpContext);
            }
            catch (ERPFacadeException exception)
            {
                await HandleExceptionAsync(httpContext, exception, exception.EventId, exception.Message, exception.MessageArguments);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(httpContext, exception, EventIds.UnhandledException.ToEventId(), exception.Message);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception, EventId eventId, string message, params object[] messageArgs)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            _logger.LogError(eventId, exception, message, messageArgs);

            await Task.CompletedTask;
        }
    }
}
