using System.Collections.Generic;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.Common.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FluentAssertions;

namespace UKHO.ERPFacade.API.UnitTests.Filters
{
    [TestFixture]
    public class SharedKeyAuthFilterTests
    {
        private IOptions<SharedKeyConfiguration> _fakeSharedKeyConfig;
        private SharedKeyAuthFilter _fakeSharedKeyAuthFilter;
        private AuthorizationFilterContext _fakeAuthorizationFilter;


        [SetUp]
        public void SetUp()
        {
            _fakeSharedKeyConfig = Options.Create(new SharedKeyConfiguration()
            {
                Key = "abc-123"
            });
            _fakeSharedKeyAuthFilter = new SharedKeyAuthFilter(_fakeSharedKeyConfig);
        }

        [Test]
        public void WhenSharedKeyIsInvalidInRequestBody_ThenThrowUnauthorizedObjectResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString("?key=abc-123-xyz");
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new());

            _fakeAuthorizationFilter = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
            _fakeSharedKeyAuthFilter.OnAuthorization(_fakeAuthorizationFilter);

            _fakeAuthorizationFilter.Result.Should().BeOfType<UnauthorizedObjectResult>();
            ((UnauthorizedObjectResult)_fakeAuthorizationFilter.Result).Value.Should().Be("Invalid Shared Key.");
        }

        [Test]
        public void WhenSharedKeyIsMissingInRequestBody_ThenThrowUnauthorizedObjectResult()
        {
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new());

            _fakeAuthorizationFilter = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
            _fakeSharedKeyAuthFilter.OnAuthorization(_fakeAuthorizationFilter);

            _fakeAuthorizationFilter.Result.Should().BeOfType<UnauthorizedObjectResult>();
            ((UnauthorizedObjectResult)_fakeAuthorizationFilter.Result).Value.Should().Be("Shared key is missing in request.");
        }

        [Test]
        public void WhenSharedKeyIsValidInRequestBody_ThenResultShouldBeNull()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString("?key=abc-123");
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new());

            _fakeAuthorizationFilter = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
            _fakeSharedKeyAuthFilter.OnAuthorization(_fakeAuthorizationFilter);

            _fakeAuthorizationFilter.Result.Should().BeNull();
        }
    }
}
