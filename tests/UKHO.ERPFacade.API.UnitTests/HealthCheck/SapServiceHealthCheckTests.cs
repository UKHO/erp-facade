using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using NUnit.Framework;
using UKHO.ERPFacade.Common.HealthCheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http;
using System.Net;
using System.Xml;
using UKHO.ERPFacade.Common.Logging;
using System.Threading;
using FluentAssertions;

namespace UKHO.ERPFacade.API.UnitTests.HealthCheck
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
                                                </soap:Envelope>
                                                ";

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
                                                </soap:Envelope>
                                                ";


        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<SapServiceHealthCheck>>();
            _fakeSapClient = A.Fake<ISapClient>();
            _fakeXmlHelper = A.Fake<IXmlHelper>();
            _fakeFileSystemHelper = A.Fake<IFileSystemHelper>();
            _fakeSapConfig = Options.Create(new SapConfiguration()
            {
                SapServiceOperation = "Z_ADDS_MAT_INFO"
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
         
            //fake xml Document
            XmlDocument fakeSapXmlPayload = new();
            fakeSapXmlPayload.LoadXml(fakeHealthySapXmlFile);

            //file exist returns true
            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);

            //returns fake xml Document
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(fakeSapXmlPayload);

            //post event data to sap which returns response OK
            A.CallTo(() => _fakeSapClient.PostEventData(fakeSapXmlPayload, "Z_ADDS_MAT_INFO"))
               .Returns(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK
               });

            //Assert
            HealthCheckResult result = await _fakeSapServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);
            result.Status.Should().Be(HealthStatus.Healthy);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapHealthCheckRequestSentToSap.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP Health Check request has been sent to SAP successfully. | {StatusCode}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenSapReturnsResponseUnHealthy_ThenSapServiceHealthIsUnHealthy()
        {
            CancellationToken fakeCancellationToken = default;

            //fake xml Document
            XmlDocument fakeSapXmlPayload = new();
            fakeSapXmlPayload.LoadXml(fakeUnHealthySapXmlFile);

            //file exist returns true
            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);

            //returns fake xml Document
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(fakeSapXmlPayload);

            //post event data to sap which returns response OK
            A.CallTo(() => _fakeSapClient.PostEventData(fakeSapXmlPayload, "Z_ADDS_MAT_INFO"))
               .Returns(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.BadRequest
               }) ;

            //Assert
            HealthCheckResult result = await _fakeSapServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);
            result.Status.Should().Be(HealthStatus.Unhealthy);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapHealthCheckRequestSentToSap.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP Health Check request has been sent to SAP successfully. | {StatusCode}").MustHaveHappenedOnceExactly();
        }     
    }
}
