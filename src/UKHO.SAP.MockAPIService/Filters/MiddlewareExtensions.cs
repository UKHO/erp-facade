namespace UKHO.SAP.MockAPIService.Filters
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder BasicAuthCustomMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthMiddleware>();
        }
    }
}
