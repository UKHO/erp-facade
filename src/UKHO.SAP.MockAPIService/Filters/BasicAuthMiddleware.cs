using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.SAP.MockAPIService.Enums;
using UKHO.SAP.MockAPIService.Services;

namespace UKHO.SAP.MockAPIService.Filters
{
    [ExcludeFromCodeCoverage]
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<SapConfiguration> _sapConfiguration;
        private readonly MockService _mockService;
        private readonly IConfiguration _configuration;

        public BasicAuthMiddleware(RequestDelegate next, IOptions<SapConfiguration> sapConfiguration, MockService mockService, IConfiguration configuration)
        {
            _next = next;
            _sapConfiguration = sapConfiguration ?? throw new ArgumentNullException(nameof(sapConfiguration));
            _mockService = mockService ?? throw new ArgumentNullException(nameof(_mockService));
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

                if (!IsUserAuthenticated(name, password))
                {
                    context.Response.StatusCode = 401;
                }

                if (bool.Parse(_configuration["IsFTRunning"]))
                {
                    string currentTestCase = _mockService.GetCurrentTestCase();

                    if (currentTestCase == TestCase.SAPInternalServerError500.ToString())
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    }
                }
            }
            catch (FormatException)
            {
                context.Response.StatusCode = 401;
            }
        }

        private bool IsUserAuthenticated(string username, string password)
        {
            return username == _sapConfiguration.Value.Username && password == _sapConfiguration.Value.Password;
        }
    }
}
