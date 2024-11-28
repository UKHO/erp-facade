using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Middlewares
{
    [ExcludeFromCodeCoverage]
    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}