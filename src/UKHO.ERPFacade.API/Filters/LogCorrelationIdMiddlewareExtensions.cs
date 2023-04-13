using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Filters
{
    [ExcludeFromCodeCoverage]
    public static class LogCorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogCorrelationIdMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogCorrelationIdMiddleware>();
        }
    }
}