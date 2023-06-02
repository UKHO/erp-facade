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
        public static bool noRole = false;
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        [SetUp]
        public void Setup()
        {
            _webhook = new WebhookEndpoint();
            SapXmlHelper = new SAPXmlHelper();
        }

        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test(Description = "WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var response = await _webhook.OptionWebhookResponseAsync(await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse()
        {
            string filePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, WebhookEndpoint.config.TestConfig.WebhookPayloadFileName);
            var response = await _webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse"), Order(2)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, WebhookEndpoint.config.TestConfig.WebhookPayloadFileName);
            var response = await _webhook.PostWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(4)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            string filePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, WebhookEndpoint.config.TestConfig.WebhookPayloadFileName);
            var response = await _webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Category("DevEnvFT")]
        [Category("QAEnvFT")]
        [Test(Description = "WhenInvalidEventInNewEncContentPublishedEventReceivedWithXML_ThenWebhookReturns500Response"), Order(1)]
        public async Task WhenInvalidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns500OkResponse()
        {
            string filePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, WebhookEndpoint.config.TestConfig.WebhookInvalidPayloadFileName);

            var response = await _webhook.PostWebhookResponseAsyncForXML(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        [Category("DevEnvFT")]
        [Test, Order(1)]
        //New Cell
        [TestCase("ID3_1NewCellScenario.JSON", TestName = "WhenICallTheWebhookWithOneNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("ID4_2NewCellScenario.JSON", TestName = "WhenICallTheWebhookWithTwoNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("ID5_1NewCellWoNewAVCSUnit.JSON", TestName = "WhenICallTheWebhookWithOneNewCellScenarioWithourAVCSUoS_ThenWebhookReturns200Response")]

        //Cancel & Replace
        [TestCase("ID6_2CellsReplace1CellsCancel.JSON", TestName = "WhenICallTheWebhookWithTwoCellsReplacesOneCellScenario_ThenWebhookReturns200Response")]
        [TestCase("ID7_1CellCancel.JSON", TestName = "WhenICallTheWebhookWithOneCancelCellScenario_ThenWebhookReturns200Response")]

        //Metadata Change
        [TestCase("ID8_2CellMetadataChange.JSON", TestName = "WhenICallTheWebhookWith2CellMetadataChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("ID9_MetadataChange.JSON", TestName = "WhenICallTheWebhookWithMetadataChangeScenario_ThenWebhookReturns200Response")]

        //Update
        [TestCase("ID10_UpdateSimple.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellWithStatusNameAsUpdate_ThenWebhookReturns200Response")]
        [TestCase("ID11_updateOneCellWithNewEditionStatus.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellWithStatusNameAsNewEdition_ThenWebhookReturns200Response")]
        [TestCase("ID12_updateOneCellWithReIssueStatus.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellStatusNameAsReIssue_ThenWebhookReturns200Response")]
        [TestCase("ID13_updateTwoCellsWithDifferentStatusName.JSON", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingTwoCellsWithDifferentStatusName_ThenWebhookReturns200Response")]

        //Move Cell
        [TestCase("ID14_moveOneCell.JSON", TestName = "WhenICallTheWebhookWithSimpleMoveCellScenario_ThenWebhookReturns200Response")]
        [TestCase("ID15_oneNewCellAndOneMoveOneCell.JSON", TestName = "WhenICallTheWebhookWithOneNewCellAndOneMoveOneCellScenario_ThenWebhookReturns200Response")]

        //Mixed
        [TestCase("ID16_newCell_updateCell_metadataChange.JSON", TestName = "WhenICallTheWebhookWithMixScenarioHavingNewCellAndUpdateCellAndMetadataChange_ThenWebhookReturns200Response")]
        [TestCase("ID17_newCell_and_CancelReplace.JSON", TestName = "WhenICallTheWebhookWithMixScenarioHavingOneNewCellAndOneCancel&ReplaceCell_ThenWebhookReturns200Response")]
        [TestCase("ID18_CancelReplace_UpdateCell.JSON", TestName = "WhenICallTheWebhookWithMixScenarioHavingCancel&Replace_UpdateCell_ThenWebhookReturns200Response")]
        [TestCase("ID19_CR_metadata_move.JSON", TestName = "WhenICallTheWebhookWithMixScenarioHavingCancel&ReplaceAndMetadataChangeAndMoveCell_ThenWebhookReturns200Response")]

        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse1(string payloadJsonFileName)
        {
            string filePath = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, WebhookEndpoint.config.TestConfig.GeneratedXMLFolder);

            var response = await _webhook.PostWebhookResponseAsyncForXML(filePath, generatedXMLFolder, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}