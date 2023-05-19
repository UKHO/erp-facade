using System.Diagnostics.CodeAnalysis;

namespace UKHO.SAP.MockAPIService.Filters
{
    [ExcludeFromCodeCoverage]
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder BasicAuthCustomMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthMiddleware>();
        }
    }
}