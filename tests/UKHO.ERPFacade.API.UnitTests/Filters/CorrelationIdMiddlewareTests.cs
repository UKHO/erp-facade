using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Filters;

namespace UKHO.ERPFacade.API.UnitTests.Filters
{
    [TestFixture]
    public class CorrelationIdMiddlewareTests
    {
        private RequestDelegate _fakeNextMiddleware;
        private CorrelationIdMiddleware _middleware;
        private HttpContext _fakeHttpContext;
        private ILogger<CorrelationIdMiddleware> _fakeLogger;

        [SetUp]
        public void SetUp()
        {
            _fakeNextMiddleware = A.Fake<RequestDelegate>();
            _middleware = new CorrelationIdMiddleware(_fakeNextMiddleware);
            _fakeLogger = A.Fake<ILogger<CorrelationIdMiddleware>>();
           // var responseHeaders = A.Fake<IHeaderDictionary>();
            _fakeHttpContext = A.Fake<HttpContext>();            
            _fakeHttpContext.RequestServices = new ServiceCollection().AddSingleton(_fakeLogger).BuildServiceProvider();
           // A.CallTo(() => _fakeHttpContext.Response.Headers).Returns(responseHeaders);
        }

        [Test]
        public async Task WhenCorrelationIdKeyExistsInRequestBody_ThenXCorrelationIdHeaderKeyAddedToRequestAndResponseHeaders()
        {
            var correlationId = Guid.NewGuid().ToString();
            var bodyAsJson = new JObject { { "data", new JObject { { "correlationId", correlationId } } } };
            var bodyAsText = bodyAsJson.ToString();

            _fakeHttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyAsText));
            _fakeHttpContext.Request.ContentLength = bodyAsText.Length;
            _fakeHttpContext.Response.Body = new MemoryStream();

            await _middleware.InvokeAsync(_fakeHttpContext);

            A.CallTo(() => _fakeHttpContext.Request.Headers[CorrelationIdMiddleware.XCorrelationIdHeaderKey]).Returns(correlationId);
            A.CallTo(() => _fakeHttpContext.Response.Headers[CorrelationIdMiddleware.XCorrelationIdHeaderKey]).Returns(correlationId);
            A.CallTo(() => _fakeLogger.BeginScope(A<Dictionary<string, object>>._)).MustHaveHappenedOnceExactly();                      
        }

        [Test]
        public async Task WhenCorrelationIdKeyDoesNotExistInRequestBody_ThenGenerateNewCorrelationId()
        {
            var correlationId = Guid.NewGuid().ToString();
            var bodyAsJson = new JObject { { "data", new JObject { { "corId", correlationId } } } };
            var bodyAsText = bodyAsJson.ToString();

            _fakeHttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyAsText));
            _fakeHttpContext.Request.ContentLength = bodyAsText.Length;
            _fakeHttpContext.Response.Body = new MemoryStream();

            await _middleware.InvokeAsync(_fakeHttpContext);

            A.CallTo(() => _fakeHttpContext.Request.Headers.ContainsKey(CorrelationIdMiddleware.XCorrelationIdHeaderKey)).Returns(true);
            A.CallTo(() => _fakeHttpContext.Response.Headers.ContainsKey(CorrelationIdMiddleware.XCorrelationIdHeaderKey)).Returns(true);
            A.CallTo(() => _fakeLogger.BeginScope(A<Dictionary<string, object>>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenInvokeAsyncIsCalled_ThenNextMiddlewareShouldBeInvoked()
        {
            var bodyAsJson = new JObject { { "data", new JObject { } } };
            var bodyAsText = bodyAsJson.ToString();

            _fakeHttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyAsText));
            _fakeHttpContext.Request.ContentLength = bodyAsText.Length;
            _fakeHttpContext.Response.Body = new MemoryStream();

            await _middleware.InvokeAsync(_fakeHttpContext);

            A.CallTo(() => _fakeNextMiddleware(_fakeHttpContext)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenCorrIdKeyExistsInRequestBody_ThenXCorrelationIdHeaderKeyAddedToRequestAndResponseHeaders()
        {
            var correlationId = Guid.NewGuid().ToString();
            var bodyAsJson = new JArray { { new JObject { { "corrid", correlationId } } } };
            var bodyAsText = bodyAsJson.ToString();
            var responseHeaders =new HeaderDictionary() ;
            var requestHeaders = new HeaderDictionary();

            _fakeHttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyAsText));
            _fakeHttpContext.Request.ContentLength = bodyAsText.Length;
            _fakeHttpContext.Response.Body = new MemoryStream();
            A.CallTo(() => _fakeHttpContext.Response.Headers).Returns(responseHeaders);
            A.CallTo(() => _fakeHttpContext.Request.Headers).Returns(requestHeaders);

            await _middleware.InvokeAsync(_fakeHttpContext);

            Assert.That(_fakeHttpContext.Request.Headers.Keys.Contains(CorrelationIdMiddleware.XCorrelationIdHeaderKey), Is.True);
            Assert.That(_fakeHttpContext.Request.Headers.Values.Contains(correlationId), Is.True);
            Assert.That(_fakeHttpContext.Response.Headers.Keys.Contains(CorrelationIdMiddleware.XCorrelationIdHeaderKey), Is.True);
            Assert.That(_fakeHttpContext.Response.Headers.Values.Contains(correlationId), Is.True);
        }
    }
}
