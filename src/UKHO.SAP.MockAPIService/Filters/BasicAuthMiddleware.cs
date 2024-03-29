﻿using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.SAP.MockAPIService.Filters
{
    [ExcludeFromCodeCoverage]
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<SapConfiguration> _sapConfiguration;        
        private readonly IConfiguration _configuration;

        public BasicAuthMiddleware(RequestDelegate next, IOptions<SapConfiguration> sapConfiguration, IConfiguration configuration)
        {
            _next = next;
            _sapConfiguration = sapConfiguration ?? throw new ArgumentNullException(nameof(sapConfiguration));           
            _configuration = configuration ?? throw new ArgumentNullException(nameof(_configuration));
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
                string contextReqPath = context.Request.Path;

                if (!IsUserAuthenticated(name, password, contextReqPath))
                {
                    context.Response.StatusCode = 401;
                }               
            }
            catch (FormatException)
            {
                context.Response.StatusCode = 401;
            }
        }

        private bool IsUserAuthenticated(string username, string password, string contextReqPath)
        {
            string sapEndpointForEncEvent = _sapConfiguration.Value.SapEndpointForEncEvent;
            string splitSapEndpointForEncEvent = sapEndpointForEncEvent.Substring(sapEndpointForEncEvent.LastIndexOf("/", StringComparison.Ordinal));

            string sapEndpointForRecordOfSale = _sapConfiguration.Value.SapEndpointForRecordOfSale;
            string splitSapEndpointForRecordOfSale = sapEndpointForRecordOfSale.Substring(sapEndpointForRecordOfSale.LastIndexOf("/", StringComparison.Ordinal));

            if (contextReqPath == splitSapEndpointForEncEvent)
            {
                return username == _sapConfiguration.Value.SapUsernameForEncEvent && password == _sapConfiguration.Value.SapPasswordForEncEvent;
            }

            if (contextReqPath == splitSapEndpointForRecordOfSale)
            {
                return username == _sapConfiguration.Value.SapUsernameForRecordOfSale && password == _sapConfiguration.Value.SapPasswordForRecordOfSale;
            }

            return false;
        }
    }
}
