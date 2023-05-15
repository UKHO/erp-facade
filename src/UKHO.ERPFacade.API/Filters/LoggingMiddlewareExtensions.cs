using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Filters
{
    [ExcludeFromCodeCoverage]
    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder, ILoggerFactory loggerFactory)
        {
            return builder.UseMiddleware<LoggingMiddleware>(loggerFactory);
        }
    }
}