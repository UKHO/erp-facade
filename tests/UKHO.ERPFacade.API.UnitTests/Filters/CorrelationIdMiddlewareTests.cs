using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Middlewares;
using UKHO.ERPFacade.Common.Constants;

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
            _fakeHttpContext = A.Fake<HttpContext>();            
            _fakeHttpContext.RequestServices = new ServiceCollection().AddSingleton(_fakeLogger).BuildServiceProvider();          
        }

        [Test]
        public async Task WhenCorrelationIdKeyExistsInRequestBody_ThenXCorrelationIdHeaderKeyAddedToRequestAndResponseHeaders()
        {
            var correlationId = Guid.NewGuid().ToString();
            var bodyAsJson = new JObject { { "data", new JObject { { "correlationId", correlationId } } } };
            var bodyAsText = bodyAsJson.ToString();
            var responseHeaders = new HeaderDictionary();
            var requestHeaders = new HeaderDictionary();

            _fakeHttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyAsText));
            _fakeHttpContext.Request.ContentLength = bodyAsText.Length;
            _fakeHttpContext.Response.Body = new MemoryStream();

            A.CallTo(() => _fakeHttpContext.Response.Headers).Returns(responseHeaders);
            A.CallTo(() => _fakeHttpContext.Request.Headers).Returns(requestHeaders);

            await _middleware.InvokeAsync(_fakeHttpContext);
           
            A.CallTo(() => _fakeLogger.BeginScope(A<Dictionary<string, object>>._)).MustHaveHappenedOnceExactly();

            Assert.That(_fakeHttpContext.Request.Headers.Count, Is.EqualTo(1));
            Assert.That(_fakeHttpContext.Request.Headers[ApiHeaderKeys.XCorrelationIdHeaderKeyName], Is.EqualTo(correlationId));

            Assert.That(_fakeHttpContext.Response.Headers.Count, Is.EqualTo(1));
            Assert.That(_fakeHttpContext.Response.Headers[ApiHeaderKeys.XCorrelationIdHeaderKeyName], Is.EqualTo(correlationId));

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

            A.CallTo(() => _fakeHttpContext.Request.Headers.ContainsKey(ApiHeaderKeys.XCorrelationIdHeaderKeyName)).Returns(true);
            A.CallTo(() => _fakeHttpContext.Response.Headers.ContainsKey(ApiHeaderKeys.XCorrelationIdHeaderKeyName)).Returns(true);
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
            var bodyAsJson = new JObject { { "correlationId", correlationId } };
            var bodyAsText = bodyAsJson.ToString();
            var responseHeaders =new HeaderDictionary() ;
            var requestHeaders = new HeaderDictionary();

            _fakeHttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyAsText));
            _fakeHttpContext.Request.ContentLength = bodyAsText.Length;
            _fakeHttpContext.Response.Body = new MemoryStream();
            A.CallTo(() => _fakeHttpContext.Response.Headers).Returns(responseHeaders);
            A.CallTo(() => _fakeHttpContext.Request.Headers).Returns(requestHeaders);

            await _middleware.InvokeAsync(_fakeHttpContext);

            Assert.That(_fakeHttpContext.Request.Headers.Count, Is.EqualTo(1));
            Assert.That(_fakeHttpContext.Request.Headers[ApiHeaderKeys.XCorrelationIdHeaderKeyName], Is.EqualTo(correlationId));

            Assert.That(_fakeHttpContext.Response.Headers.Count, Is.EqualTo(1));
            Assert.That(_fakeHttpContext.Response.Headers[ApiHeaderKeys.XCorrelationIdHeaderKeyName], Is.EqualTo(correlationId));
        }
    }
}
