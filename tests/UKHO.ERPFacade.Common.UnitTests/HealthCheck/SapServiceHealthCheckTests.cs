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
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HealthCheck;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Operations;
using UKHO.ERPFacade.Common.Operations.IO;

namespace UKHO.ERPFacade.Common.UnitTests.HealthCheck
{
    [TestFixture]
    public class SapServiceHealthCheckTests
    {
        private ISapClient _fakeSapClient;
        private IOptions<SapConfiguration> _fakeSapConfig;
        private IXmlOperations _fakeXmlOperations;
        private IFileOperations _fakeFileOperations;
        private ILogger<SapServiceHealthCheck> _fakeLogger;
        private SapServiceHealthCheck _fakeSapServiceHealthCheck;

        private const string SapBaseUrl = "http://sap.ukho.gov.uk";
        private const string SapEndpointForEncEvent = "sap1";
        private const string SapServiceOperationForEncEvent = "Z_ADDS_MAT_INFO";

        private readonly string _fakeHealthySapXmlFile = @$"<?xml version=""1.0"" encoding=""utf-8""?>
                                                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                                  <soap:Body>
                                                    <{SapServiceOperationForEncEvent} xmlns=""urn:sap-com:document:sap:rfc:functions"">
                                                      <IM_MATINFO xmlns="""">
                                                        <CORRID>HEALTHCHECK</CORRID>
                                                        <NOOFACTIONS></NOOFACTIONS>
                                                        <RECDATE></RECDATE>
                                                        <RECTIME></RECTIME>
                                                        <ORG>UKHO</ORG>
                                                        <ACTIONITEMS>
                                                        </ACTIONITEMS>
                                                      </IM_MATINFO>
                                                    </{SapServiceOperationForEncEvent}>
                                                  </soap:Body>
                                                </soap:Envelope>";

        private readonly string _fakeUnhealthySapXmlFile = @$"<?xml version=""1.0"" encoding=""utf-8""?>
                                                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                                  <soap:Body>
                                                    <{SapServiceOperationForEncEvent} xmlns=""urn:sap-com:document:sap:rfc:functions"">
                                                      <IM_MATINFO xmlns="""">
                                                        <CORRID></CORRID>
                                                        <NOOFACTIONS></NOOFACTIONS>
                                                        <RECDATE></RECDATE>
                                                        <RECTIME></RECTIME>
                                                        <ORG>UKHO</ORG>
                                                        <ACTIONITEMS>
                                                        </ACTIONITEMS>
                                                      </IM_MATINFO>
                                                    </{SapServiceOperationForEncEvent}>
                                                  </soap:Body>
                                                </soap:Envelope>";

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<SapServiceHealthCheck>>();
            _fakeSapClient = A.Fake<ISapClient>();
            A.CallTo(() => _fakeSapClient.Uri).Returns(new Uri(SapBaseUrl));
            _fakeXmlOperations = A.Fake<IXmlOperations>();
            _fakeFileOperations = A.Fake<IFileOperations>();
            _fakeSapConfig = Options.Create(new SapConfiguration
            {
                SapEndpointForEncEvent = SapEndpointForEncEvent,
                SapServiceOperationForEncEvent = SapServiceOperationForEncEvent
            });

            _fakeSapServiceHealthCheck = new SapServiceHealthCheck(_fakeSapClient, _fakeSapConfig, _fakeXmlOperations, _fakeFileOperations, _fakeLogger);
        }

        [Test]
        public async Task WhenSapReturnsResponseHealthy_ThenSapServiceHealthIsHealthy()
        {
            CancellationToken fakeCancellationToken = default;

            XmlDocument fakeSapXmlPayload = new();
            fakeSapXmlPayload.LoadXml(_fakeHealthySapXmlFile);

            A.CallTo(() => _fakeFileOperations.IsFileExists(A<string>.Ignored)).Returns(true);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(fakeSapXmlPayload);

            A.CallTo(() => _fakeSapClient.SendUpdateAsync(fakeSapXmlPayload, A<string>.Ignored, SapServiceOperationForEncEvent, A<string>.Ignored, A<string>.Ignored))
                .Returns(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, RequestMessage = new HttpRequestMessage { RequestUri = new Uri("http://abc.com") }, Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("ServiceUnavailable"))) });

            var result = await _fakeSapServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
                Assert.That(result.Description, Is.EqualTo("Reports unhealthy if SAP endpoint is not reachable, or does not return 200 status"));
            }

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
        public async Task WhenSapReturnsResponseUnhealthy_ThenSapServiceHealthIsUnhealthy()
        {
            CancellationToken fakeCancellationToken = default;

            XmlDocument fakeSapXmlPayload = new();
            fakeSapXmlPayload.LoadXml(_fakeUnhealthySapXmlFile);

            A.CallTo(() => _fakeFileOperations.IsFileExists(A<string>.Ignored)).Returns(true);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(fakeSapXmlPayload);

            A.CallTo(() => _fakeSapClient.SendUpdateAsync(fakeSapXmlPayload, A<string>.Ignored, SapServiceOperationForEncEvent, A<string>.Ignored, A<string>.Ignored))
                .Returns(new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable, RequestMessage = new HttpRequestMessage { RequestUri = new Uri("http://abc.com") }, Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("ServiceUnavailable"))) });

            var result = await _fakeSapServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
                Assert.That(result.Description, Is.EqualTo("Reports unhealthy if SAP endpoint is not reachable, or does not return 200 status"));
            }

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                && call.GetArgument<EventId>(1) == EventIds.SapHealthCheckRequestSentToSap.ToEventId()
                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP Health Check request has been sent to SAP successfully. | {StatusCode}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                && call.GetArgument<EventId>(1) == EventIds.SAPIsUnhealthy.ToEventId()
                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP is Unhealthy").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenSapXmlTemplateFileNotExist_ThenThrowFileNotFoundException()
        {
            CancellationToken fakeCancellationToken = default;

            A.CallTo(() => _fakeFileOperations.IsFileExists(A<string>.Ignored)).Returns(false);
            A.CallTo(() => _fakeSapClient.SendUpdateAsync(A<XmlDocument>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Throws<FileNotFoundException>();

            var result = await _fakeSapServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
                Assert.That(result.Description, Is.EqualTo("Reports unhealthy if SAP endpoint is not reachable, or does not return 200 status"));
            }

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Warning
                && call.GetArgument<EventId>(1) == EventIds.SapHealthCheckXmlTemplateNotFound.ToEventId()
                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The SAP Health Check xml template does not exist.").MustHaveHappenedOnceExactly();
        }
    }
}
