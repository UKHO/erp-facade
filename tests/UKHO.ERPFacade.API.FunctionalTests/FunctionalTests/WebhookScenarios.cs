using FluentAssertions;
using NUnit.Framework;
using System.Security.Cryptography.X509Certificates;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class WebhookScenarios
    {
        private WebhookEndpoint Webhook { get; set; }
        private SAPXmlHelper SapXmlHelper{ get; set; }
        private DirectoryInfo _dir;
        private readonly ADAuthTokenProvider _authToken = new();
        public static Boolean noRole = false;

        [SetUp]
        public void Setup()
        {
            Webhook = new WebhookEndpoint();
            SapXmlHelper = new SAPXmlHelper();
            _dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent;
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var response = await Webhook.OptionWebhookResponseAsync(await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookPayloadFileName);

            var response = await Webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse"), Order(2)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookPayloadFileName);

            var response = await Webhook.PostWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(2)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookPayloadFileName);
            var response = await Webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithXML_ThenWebhookReturns500OkResponse"), Order(1)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithXML_ThenWebhookReturns500OkResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookInvalidPayloadFileName);

            var response = await Webhook.PostWebhookResponseAsyncForXML(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        [Test, Order(0)]
        [TestCase("1NewCellScenario.JSON", "1NewCellScenario.xml", TestName = "WhenICallTheWebhookWithOneNewCellScenario")]        
        //[TestCase("1NewCellWoNewAVCSUnit.JSON", "1NewCellWoNewAVCSUnit.xml", TestName = "WhenICallTheWebhookWithOneNewCellScenario")]
        //[TestCase("2NewCellScenario.JSON", "2NewCellScenario.xml", TestName = "WhenICallTheWebhookWithOneNewCellScenario")]
        //[TestCase("1CellCancel.JSON", "1CellCancel.xml", TestName = "WhenICallTheWebhookWithOneNewCellScenario")]
        [TestCase("2CellsReplace1CellsCancel.JSON", "2CellsReplace1CellsCancel.xml", TestName = "WhenICallTheWebhookWithTwoNewCellsScenario")]
        //[TestCase("3CellsReplace2CellsCancel.JSON", "3CellsReplace2CellsCancel.xml", TestName = "WhenICallTheWebhookCancel1Replace2CellsScenario")]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse1(string payloadFileName, string expectedXmlFileName)
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, payloadFileName);
            string expectedXMLfilePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.ExpectedXMLFolder, expectedXmlFileName);
            //string traceID = SapXmlHelper.getTraceID(filePath);
            var response = await Webhook.PostWebhookResponseAsyncForXML(filePath, expectedXMLfilePath,  await _authToken.GetAzureADToken(false));

            // download XML file by passing traceID
            // currently we have given hardcoded traceID otherwise use above commented string
            //string generatedXMLFilePath = SapXmlHelper.downloadGeneratedXML(expectedXMLfilePath,"367ce4a4-1d62-4f56-b359-59e178d77100"); // string path will be returned
            
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
