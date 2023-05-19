using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class WebhookScenarios
    {
        private WebhookEndpoint _webhook { get; set; }
        private SAPXmlHelper SapXmlHelper { get; set; }
        private readonly ADAuthTokenProvider _authToken = new();
        public static Boolean noRole = false;
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        [SetUp]
        public void Setup()
        {
            _webhook = new WebhookEndpoint();
            SapXmlHelper = new SAPXmlHelper();
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse"), Order(2)]
        public async Task WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var response = await _webhook.OptionWebhookResponseAsync(await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse"), Order(2)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse()
        {
            string filePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, WebhookEndpoint.config.TestConfig.WebhookPayloadFileName);
            var response = await _webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse"), Order(2)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, WebhookEndpoint.config.TestConfig.WebhookPayloadFileName);

            var response = await _webhook.PostWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(3)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            string filePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, WebhookEndpoint.config.TestConfig.WebhookPayloadFileName);
            var response = await _webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test(Description = "WhenICallMockSapApiWithValidRequestAndValidAuthenticationButSapApiServiceIsDown_ThenItShouldReturn500IntervalServerErrorResponse"), Order(4)]
        public async Task WhenICallMockSapApiWithValidRequestAndValidAuthenticationButSapApiServiceIsDown_ThenItShouldReturn500IntervalServerErrorResponse()
        {
            string filePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, WebhookEndpoint.config.TestConfig.WebhookPayloadFileName);
            string XmlFilePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, WebhookEndpoint.config.TestConfig.SapMockApiPayloadFileName);

            _webhook.PostMockSapResponseAsync(XmlFilePath);
            Thread.Sleep(3500);

            var response = await _webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        [Test(Description = "WhenInvalidEventInNewEncContentPublishedEventReceivedWithXML_ThenWebhookReturns500Response"), Order(2)]
        public async Task WhenInvalidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns500OkResponse()
        {
            string filePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, WebhookEndpoint.config.TestConfig.WebhookInvalidPayloadFileName);

            var response = await _webhook.PostWebhookResponseAsyncForXML(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        [Test, Order(0)]
        //New Cell
        [TestCase("1NewCellScenario.JSON", TestName = "WhenICallTheWebhookWithOneNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("2NewCellScenario.JSON", TestName = "WhenICallTheWebhookWithTwoNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("1NewCellWoNewAVCSUnit.JSON", TestName = "WhenICallTheWebhookWithOneNewCellScenarioWithourAVCSUoS_ThenWebhookReturns200Response")]

        //Cancel & Replace
        [TestCase("2CellsReplace1CellsCancel.JSON", TestName = "WhenICallTheWebhookWithTwoCellsReplacesOneCellScenario_ThenWebhookReturns200Response")]
        [TestCase("1CellCancel.JSON", TestName = "WhenICallTheWebhookWithOneCancelCellScenario_ThenWebhookReturns200Response")]

        //Metadata Change
        //[TestCase("2CellMetadataChange.JSON", TestName = "WhenICallTheWebhookWith2CellMetadataChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("MetadataChange.JSON", TestName = "WhenICallTheWebhookWithMetadataChangeScenario_ThenWebhookReturns200Response")]

        //Update
        [TestCase("UpdateSimple.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellWithStatusNameAsUpdate_ThenWebhookReturns200Response")]
        [TestCase("updateOneCellWithNewEditionStatus.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellWithStatusNameAsNewEdition_ThenWebhookReturns200Response")]
        [TestCase("updateOneCellWithReIssueStatus.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellStatusNameAsReIssue_ThenWebhookReturns200Response")]
        [TestCase("updateTwoCellsWithDifferentStatusName.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingTwoCellsWithDifferentStatusName_ThenWebhookReturns200Response")]

        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse1(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.GeneratedXMLFolder);
            //string traceID = SapXmlHelper.getTraceID(filePath);

            var response = await _webhook.PostWebhookResponseAsyncForXML(filePath, generatedXMLFolder, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}