using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.UnitTests.Filters
{
    [TestFixture]
    public class SharedApiKeyAuthFilterTests
    {
        private ILogger<SharedApiKeyAuthFilter> _fakeLogger;
        private IOptions<SharedApiKeyConfiguration> _fakeSharedApiKeyConfiguration;
        private SharedApiKeyAuthFilter _fakeSharedApiKeyAuthFilter;
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private AuthorizationFilterContext _fakeAuthorizationFilterContext;
        private HttpContext _fakeHttpContext;
        private HeaderDictionary _fakeHeaderDictionary;
        private RouteData _fakeRouteData;
        private ActionDescriptor _fakeActionDescriptor;

        [SetUp]
        public void SetUp()
        {
            _fakeLogger = A.Fake<ILogger<SharedApiKeyAuthFilter>>();
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeSharedApiKeyConfiguration = Options.Create(new SharedApiKeyConfiguration()
            {
                SharedApiKey = "abc-123"
            });
            _fakeHttpContext = A.Fake<HttpContext>();
            _fakeRouteData = A.Fake<RouteData>();
            _fakeActionDescriptor = A.Fake<ActionDescriptor>();
            _fakeHeaderDictionary = A.Fake<HeaderDictionary>();
            _fakeSharedApiKeyAuthFilter = new SharedApiKeyAuthFilter(_fakeLogger, _fakeSharedApiKeyConfiguration);
            var actionContext = new ActionContext(_fakeHttpContext, _fakeRouteData, _fakeActionDescriptor);
            var filterMetaData = new List<IFilterMetadata> { _fakeSharedApiKeyAuthFilter };
            _fakeAuthorizationFilterContext = new AuthorizationFilterContext(actionContext, filterMetaData);
        }

        [Test]
        public void WhenInValidSharedApiKeyInRequestedHeader_ThenSharedApiKeyAuthFilterUnAuthorizedTheSapCallBack()
        {
            A.CallTo(() => _fakeHttpContext.Response.Headers).Returns(_fakeHeaderDictionary);
            A.CallTo(() => _fakeHttpContextAccessor.HttpContext).Returns(_fakeHttpContext);
            A.CallTo(() => _fakeHttpContext.Request.Headers[ApiHeaderKeys.ApiKeyHeaderKey]).Returns(new[] { "xyz-123" });

            _fakeSharedApiKeyAuthFilter.OnAuthorization(_fakeAuthorizationFilterContext);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Warning
                                                && call.GetArgument<EventId>(1) == EventIds.InvalidSharedApiKey.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Invalid shared key").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenEmptySharedApiKeyInRequestedHeader_ThenSharedApiKeyAuthFilterUnAuthorizedTheSapCallBack()
        {
            A.CallTo(() => _fakeHttpContext.Response.Headers).Returns(_fakeHeaderDictionary);
            A.CallTo(() => _fakeHttpContextAccessor.HttpContext).Returns(_fakeHttpContext);
            A.CallTo(() => _fakeHttpContext.Request.Headers[ApiHeaderKeys.ApiKeyHeaderKey]).Returns(new[] { "" });

            _fakeSharedApiKeyAuthFilter.OnAuthorization(_fakeAuthorizationFilterContext);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Warning
                                                && call.GetArgument<EventId>(1) == EventIds.SharedApiKeyMissingInRequest.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Shared key is missing in request").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenValidSharedApiKeyInRequestedHeader_ThenSharedApiKeyAuthFilterAuthorizedTheSapCallBack()
        {
            A.CallTo(() => _fakeHttpContext.Response.Headers).Returns(_fakeHeaderDictionary);
            A.CallTo(() => _fakeHttpContextAccessor.HttpContext).Returns(_fakeHttpContext);
            A.CallTo(() => _fakeHttpContext.Request.Headers[ApiHeaderKeys.ApiKeyHeaderKey]).Returns(new[] { "abc-123" });

            _fakeSharedApiKeyAuthFilter.OnAuthorization(_fakeAuthorizationFilterContext);

            Assert.That(_fakeAuthorizationFilterContext.Result, Is.EqualTo(null));
        }

        [Test]
        public void WhenSharedAPIKeyConfigurationIsMissing_ThenThrowERPFacadeException()
        {
            _fakeSharedApiKeyConfiguration.Value.SharedApiKey = string.Empty;

            Assert.Throws<ERPFacadeException>(() => new SharedApiKeyAuthFilter(_fakeLogger, _fakeSharedApiKeyConfiguration))
                .Message.Should().Be("Shared API key configuration missing.");
        }
    }
}
