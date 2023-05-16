﻿using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class WebhookScenarios
    {
        private WebhookEndpoint Webhook { get; set; }
        private SAPXmlHelper SapXmlHelper { get; set; }
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

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse"), Order(1)]
        public async Task WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var response = await Webhook.OptionWebhookResponseAsync(await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse"), Order(1)]
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

        [Test(Description = "WhenICallMockSapApiWithValidRequestAndValidAuthenticationButSapApiServiceIsDown_ThenItShouldReturn500IntervalServerErrorResponse"), Order(0)]
        public async Task WhenICallMockSapApiWithValidRequestAndValidAuthenticationButSapApiServiceIsDown_ThenItShouldReturn500IntervalServerErrorResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookPayloadFileName);
            string XmlFilePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.SapMockApiPayloadFileName);
           
            Webhook.PostMockSapResponseAsync(XmlFilePath);
            
            var response = await Webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithXML_ThenWebhookReturns500OkResponse"), Order(1)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithXML_ThenWebhookReturns500OkResponse()
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, WebhookEndpoint.config.testConfig.WebhookInvalidPayloadFileName);

            var response = await Webhook.PostWebhookResponseAsyncForXML(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        [Test, Order(0)]
        //New Cell
        [TestCase("1NewCellScenario.JSON", "1NewCellScenario.xml", TestName = "WhenICallTheWebhookWithOneNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("2NewCellScenario.JSON", "2NewCellScenario.xml", TestName = "WhenICallTheWebhookWithTwoNewCellScenario_ThenWebhookReturns200Response")]

        //Cancel & Replace
        [TestCase("2CellsReplace1CellsCancel.JSON", "2CellsReplace1CellsCancel.xml", TestName = "WhenICallTheWebhookWithTwoNewCellsScenario_ThenWebhookReturns200Response")]
        [TestCase("1CellCancel.JSON", "1CellCancel.xml", TestName = "WhenICallTheWebhookWithOneCancelCellScenario_ThenWebhookReturns200Response")]

        //Move Cell
        [TestCase("cellMove.JSON", "cellMove.xml", TestName = "WhenICallTheWebhookWithCellMoveScenario_ThenWebhookReturns200Response")]
        
        //Metadata Change
        [TestCase("MetadataChange.JSON", "MetadataChange.xml", TestName = "WhenICallTheWebhookWithMetadataChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("2CellMetadataChange.JSON", "2CellMetadataChange.xml", TestName = "WhenICallTheWebhookWith2CellMetadataChangeScenario_ThenWebhookReturns200Response")]
        
        //Update
        [TestCase("UpdateSimple.JSON", "UpdateSimple.xml", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellWithStatusNameAsUpdate_ThenWebhookReturns200Response")]
        [TestCase("updateOneCellWithNewEditionStatus.JSON", "UpdateSimple.xml", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellWithStatusNameAsNewEdition_ThenWebhookReturns200Response")]
        [TestCase("updateOneCellWithReIssueStatus.JSON", "UpdateSimple.xml", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellStatusNameAsReIssue_ThenWebhookReturns200Response")]
        [TestCase("updateTwoCellsWithDifferentStatusName.JSON", "UpdateSimple.xml", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingTwoCellsWithDifferentStatusName_ThenWebhookReturns200Response")]

        //[TestCase("3CellsReplace2CellsCancel.JSON", "3CellsReplace2CellsCancel.xml", TestName = "WhenICallTheWebhookCancel1Replace2CellsScenario")]
        //[TestCase("1NewCellWoNewAVCSUnit.JSON", "1NewCellWoNewAVCSUnit.xml", TestName = "WhenICallTheWebhookWithOneNewCellScenario")]
        

        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse1(string payloadFileName, string expectedXmlFileName)
        {
            string filePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.PayloadFolder, payloadFileName);
            string expectedXMLfilePath = Path.Combine(_dir.FullName, WebhookEndpoint.config.testConfig.GeneratedXMLFolder);
            //string traceID = SapXmlHelper.getTraceID(filePath);
            var response = await Webhook.PostWebhookResponseAsyncForXML(filePath, expectedXMLfilePath, await _authToken.GetAzureADToken(false));
    
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}