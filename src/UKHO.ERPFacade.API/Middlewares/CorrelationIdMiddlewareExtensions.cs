using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Middlewares
{
    [ExcludeFromCodeCoverage]
    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationIdMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}