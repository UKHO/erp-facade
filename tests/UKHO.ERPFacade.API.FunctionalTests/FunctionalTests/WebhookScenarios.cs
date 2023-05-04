using FluentAssertions;
using NUnit.Framework;
using System.Diagnostics;
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
        private readonly ADAuthTokenProvider _authToken = new ADAuthTokenProvider();
        public static Boolean noRole = false;
        public List<string> actionsListBasedOnJSON = new List<string>();
        public List<string> actionsListBasedOnXML = new List<string>();

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

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse"), Order(1)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookPayloadFileName);

            var response = await Webhook.PostWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(1)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookPayloadFileName);
            var response = await Webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }
        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithXML_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithXML_ThenWebhookReturns200OkResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookPayloadFileName);

            var response = await Webhook.PostWebhookResponseAsyncForXML(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithXML_ThenWebhookReturns500OkResponse"), Order(1)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithXML_ThenWebhookReturns500OkResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookInvalidPayloadFileName);

            var response = await Webhook.PostWebhookResponseAsyncForXML(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        // ====== Under Maintenance =======

        [Test]
        [TestCase("1NewCellScenario.json", "1NewCellScenario.xml", TestName = "WhenICallTheWebhookWithOneNewCellScenario")]
        [TestCase("2NewCellScenario.json", "2NewCellScenario.xml", TestName = "WhenICallTheWebhookWithTwoNewCellsScenario")]
        [TestCase("3CellsReplace2CellsCancel.json", "3CellsReplace2CellsCancel.xml", TestName = "WhenICallTheWebhookWithThreeReplaceAndTwoCancelCellsScenario")]
        [TestCase("1CellCancel.json", "1CellCancel.xml", TestName = "WhenICallTheWebhookWithOneCancelCellScenario")]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse1(string payloadFileName, string expectedXmlFileName)
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, payloadFileName);
            string expectedXMLfilePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.ExpectedXMLFolder, expectedXmlFileName);
            string traceID = SapXmlHelper.getTraceID(filePath);
            string generatedXMLfilePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.GeneratedXMLFolderName, traceID);

            // Actions based on JSON Payload
            int totalCalculatedNumberOfAtions =  SapXmlHelper.calculateTotalNumberOfActions();
            actionsListBasedOnJSON = SapXmlHelper.getFinalActionsListFromJson(SAPXmlHelper.listFromJson);

            var response = await Webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));

            //working on generated XML
            string generatedXMLFilePath = SapXmlHelper.downloadGeneratedXML(generatedXMLfilePath, traceID); // string path will be returned
            actionsListBasedOnXML = SapXmlHelper.curateListOfActionsFromXmlFile(generatedXMLFilePath);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }


    }
}
