﻿using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.SAP.MockAPIService.Filters
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<SapConfiguration> _sapConfiguration;

        public BasicAuthMiddleware(RequestDelegate next, IOptions<SapConfiguration> sapConfiguration)
        {
            _next = next;
            _sapConfiguration = sapConfiguration ?? throw new ArgumentNullException(nameof(sapConfiguration));
        }

        public async Task Invoke(HttpContext context)
        {            
            var request = context.Request;
            var authHeader = request.Headers["Authorization"];
            if (!Microsoft.Extensions.Primitives.StringValues.IsNullOrEmpty(authHeader))
            {
                var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);
                                
                if (authHeaderVal.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) &&
                    authHeaderVal.Parameter != null)
                {
                    AuthenticateUser(context, authHeaderVal.Parameter);
                }
                if (context.Response.StatusCode != 401)
                    await _next.Invoke(context);
            }            
        }

        private void AuthenticateUser(HttpContext context, string credentials)
        {
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String(credentials));

                int separator = credentials.IndexOf(':');
                string name = credentials.Substring(0, separator);
                string password = credentials.Substring(separator + 1);

                if (!CheckPassword(name, password))
                {                    
                    context.Response.StatusCode = 401;                    
                }
            }
            catch (FormatException)
            {                
                context.Response.StatusCode = 401;
            }
        }
        
        private bool CheckPassword(string username, string password)
        {       

            return username == _sapConfiguration.Value.Username && password == _sapConfiguration.Value.Password;
        }
    }

    public static class MyMiddlewareExtensions
    {
        public static IApplicationBuilder BasicAuthCustomMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthMiddleware>();
        }
    }
}
