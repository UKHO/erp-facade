using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HealthCheck;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.UnitTests.HealthCheck
{
    [TestFixture]
    public class SapServiceHealthCheckTests
    {
        private ISapClient _fakeSapClient;
        private IOptions<SapConfiguration> _fakeSapConfig;
        private IXmlHelper _fakeXmlHelper;
        private IFileSystemHelper _fakeFileSystemHelper;
        private ILogger<SapServiceHealthCheck> _fakeLogger;
        private SapServiceHealthCheck _fakeSapServiceHealthCheck;

        private readonly string fakeHealthySapXmlFile = @"<?xml version=""1.0"" encoding=""utf-8""?>
                                                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                                  <soap:Body>
                                                    <Z_ADDS_MAT_INFO xmlns=""urn:sap-com:document:sap:rfc:functions"">
                                                      <IM_MATINFO xmlns="""">
                                                        <CORRID>HEALTHCHECK</CORRID>
                                                        <NOOFACTIONS></NOOFACTIONS>
                                                        <RECDATE></RECDATE>
                                                        <RECTIME></RECTIME>
                                                        <ORG>UKHO</ORG>
                                                        <ACTIONITEMS>
                                                        </ACTIONITEMS>
                                                      </IM_MATINFO>
                                                    </Z_ADDS_MAT_INFO>
                                                  </soap:Body>
                                                </soap:Envelope>";

        private readonly string fakeUnHealthySapXmlFile = @"<?xml version=""1.0"" encoding=""utf-8""?>
                                                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                                  <soap:Body>
                                                    <Z_ADDS_MAT_INFO xmlns=""urn:sap-com:document:sap:rfc:functions"">
                                                      <IM_MATINFO xmlns="""">
                                                        <CORRID></CORRID>
                                                        <NOOFACTIONS></NOOFACTIONS>
                                                        <RECDATE></RECDATE>
                                                        <RECTIME></RECTIME>
                                                        <ORG>UKHO</ORG>
                                                        <ACTIONITEMS>
                                                        </ACTIONITEMS>
                                                      </IM_MATINFO>
                                                    </Z_ADDS_MAT_INFO>
                                                  </soap:Body>
                                                </soap:Envelope>";


        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<SapServiceHealthCheck>>();
            _fakeSapClient = A.Fake<ISapClient>();
            _fakeXmlHelper = A.Fake<IXmlHelper>();
            _fakeFileSystemHelper = A.Fake<IFileSystemHelper>();
            _fakeSapConfig = Options.Create(new SapConfiguration()
            {
                SapServiceOperationForEncEvent = "Z_ADDS_MAT_INFO"
            });

            _fakeSapServiceHealthCheck = new SapServiceHealthCheck(_fakeSapClient,
                                                                   _fakeSapConfig,
                                                                   _fakeXmlHelper,
                                                                   _fakeFileSystemHelper,
                                                                   _fakeLogger);
        }

        [Test]
        public async Task WhenSapReturnsResponseHealthy_ThenSapServiceHealthIsHealthy()
        {
            CancellationToken fakeCancellationToken = default;

            XmlDocument fakeSapXmlPayload = new();
            fakeSapXmlPayload.LoadXml(fakeHealthySapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);

            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(fakeSapXmlPayload);

            A.CallTo(() => _fakeSapClient.PostEventData(fakeSapXmlPayload, "Z_ADDS_MAT_INFO"))
               .Returns(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, RequestMessage = new HttpRequestMessage() { RequestUri = new Uri("http://abc.com") }, Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("ServiceUnavailable"))) });

            HealthCheckResult result = await _fakeSapServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

            result.Status.Should().Be(HealthStatus.Healthy);
            result.Description.Should().Be("SAP is Healthy !!!");

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapHealthCheckRequestSentToSap.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP Health Check request has been sent to SAP successfully. | {StatusCode}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Debug
            && call.GetArgument<EventId>(1) == EventIds.SAPIsHealthy.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP is Healthy").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenSapReturnsResponseUnHealthy_ThenSapServiceHealthIsUnHealthy()
        {
            CancellationToken fakeCancellationToken = default;

            XmlDocument fakeSapXmlPayload = new();
            fakeSapXmlPayload.LoadXml(fakeUnHealthySapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);

            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(fakeSapXmlPayload);

            A.CallTo(() => _fakeSapClient.PostEventData(fakeSapXmlPayload, "Z_ADDS_MAT_INFO"))
              .Returns(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable, RequestMessage = new HttpRequestMessage() { RequestUri = new Uri("http://abc.com") }, Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("ServiceUnavailable"))) });

            HealthCheckResult result = await _fakeSapServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Description.Should().Be("SAP is Unhealthy");

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapHealthCheckRequestSentToSap.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP Health Check request has been sent to SAP successfully. | {StatusCode}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.SAPIsUnhealthy.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP is Unhealthy !!!").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenSapXmlTemplateFileNotExist_ThenThrowFileNotFoundException()
        {
            CancellationToken fakeCancellationToken = default;

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored)).Throws<FileNotFoundException>();

            HealthCheckResult result = await _fakeSapServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Description.Should().Contain("SAP is Unhealthy");

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                    && call.GetArgument<LogLevel>(0) == LogLevel.Warning
                                    && call.GetArgument<EventId>(1) == EventIds.SapHealthCheckXmlTemplateNotFound.ToEventId()
                                    && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The SAP Health Check xml template does not exist.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                    && call.GetArgument<LogLevel>(0) == LogLevel.Error
                                    && call.GetArgument<EventId>(1) == EventIds.ErrorOccuredInSap.ToEventId()
                                    && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "An error occured while processing your request in SAP. | {Message}").MustHaveHappened();
        }
    }
}
