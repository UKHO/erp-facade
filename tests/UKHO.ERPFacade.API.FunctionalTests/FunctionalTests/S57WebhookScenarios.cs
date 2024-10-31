using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Auth;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Modifiers;
using UKHO.ERPFacade.API.FunctionalTests.Operations;
using UKHO.ERPFacade.API.FunctionalTests.Service;
using UKHO.ERPFacade.API.FunctionalTests.Validators;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class S57WebhookScenarios : TestFixtureBase
    {
        private readonly ErpFacadeConfiguration _erpFacadeConfiguration;

        private AuthTokenProvider _authTokenProvider;
        private WebhookEndpoint _webhookEndpoint;
        private AzureBlobReaderWriter _azureBlobReaderWriter;

        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        public S57WebhookScenarios()
        {
            var serviceProvider = GetServiceProvider();
            _erpFacadeConfiguration = serviceProvider!.GetRequiredService<IOptions<ErpFacadeConfiguration>>().Value;
        }

        [SetUp]
        public void Setup()
        {
            _authTokenProvider = new AuthTokenProvider();
            _webhookEndpoint = new WebhookEndpoint();
            _azureBlobReaderWriter = new AzureBlobReaderWriter();
        }

        [Test(Description = "WhenWebhookOptionsEndpointRequestedWithValidToken_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenWebhookOptionsEndpointRequestedWithValidToken_ThenWebhookReturns200OkResponse()
        {
            var token = await _authTokenProvider.GetAzureADToken(false);
            var response = await _webhookEndpoint.OptionWebhookResponseAsync(token);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenWebhookOptionsEndpointRequestedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse"), Order(0)]
        public async Task WhenWebhookOptionsEndpointRequestedWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse()
        {
            var response = await _webhookEndpoint.OptionWebhookResponseAsync("InvalidToken");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenWebhookOptionsEndpointRequestedWithValidTokenWithNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(0)]
        public async Task WhenWebhookOptionsEndpointRequestedWithValidTokenWithNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            var response = await _webhookEndpoint.OptionWebhookResponseAsync(await _authTokenProvider.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test(Description = "WhenWebhookPostEndpointReceivesEventWithValidToken_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenWebhookPostEndpointReceivesEventWithValidToken_ThenWebhookReturns200OkResponse()
        {
            string requestPayload = await File.ReadAllTextAsync(Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S57PayloadFolder, EventPayloadFiles.WebhookPayloadFileName));
            var response = await _webhookEndpoint.PostWebhookResponseAsync(requestPayload, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenWebhookPostEndpointReceivesEventWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse"), Order(2)]
        public async Task WhenWebhookPostEndpointReceivesEventWithInvalidToken_ThenWebhookReturns401UnauthorizedResponse()
        {
            string requestPayload = await File.ReadAllTextAsync(Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S57PayloadFolder, EventPayloadFiles.WebhookPayloadFileName));
            var response = await _webhookEndpoint.PostWebhookResponseAsync(requestPayload, "InvalidToken");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenWebhookPostEndpointReceivesEventWithValidTokenWithNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(4)]
        public async Task WhenWebhookPostEndpointReceivesEventWithValidTokenWithNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            string requestPayload = await File.ReadAllTextAsync(Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S57PayloadFolder, EventPayloadFiles.WebhookPayloadFileName));
            var response = await _webhookEndpoint.PostWebhookResponseAsync(requestPayload, await _authTokenProvider.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Test, Order(1)]
        //New Cell
        [TestCase("NewCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("2NewCells.JSON", "PermitWithDifferentKey", TestName = "WhenICallTheWebhookWith2NewCellsScenario_ThenWebhookReturns200Response")]

        //Additional Coverage
        [TestCase("3AdditionalCoverageCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWith3AdditionalCoverageCellScenario_ThenWebhookReturns200Response")]
        [TestCase("3AdditionalCoverageCell_AND_CancelCellWithNewCellReplacement.JSON", "PermitWithDifferentKey", TestName = "WhenICallTheWebhookWith3AdditionalCoverageCell_AND_CancelCellWithNewCellReplacementScenario_ThenWebhookReturns200Response")]
        [TestCase("AdditionalCoverageWithNewEdition.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithAdditionalCoverageWithNewEditionScenario_ThenWebhookReturns200Response")]

        //Cancel and Replace
        [TestCase("CancelCellWithExistingCellReplacement.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithCancelCellWithExistingCellReplacementScenario_ThenWebhookReturns200Response")]
        [TestCase("CancelCellWithNewCellReplacement.JSON", "PermitWithDifferentKey", TestName = "WhenICallTheWebhookWithCancelCellWithNewCellReplacementScenario_ThenWebhookReturns200Response")]
        [TestCase("CancelCellWithNewCellReplacement_AND_CellMetadataChange_AND_MoveCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithCancelCellWithNewCellReplacement_AND_CellMetadataChange_AND_MoveCellScenario_ThenWebhookReturns200Response")]
        [TestCase("CancelCellWithNewCellReplacement_AND_UpdateCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithCancelCellWithNewCellReplacement_AND_UpdateCellScenario_ThenWebhookReturns200Response")]
        [TestCase("CancelCellWithoutCellReplacement.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithCancelCellWithoutCellReplacementScenario_ThenWebhookReturns200Response")]

        //Metadata Change
        [TestCase("CellMetadataChange.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithCellMetadataChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("UoSMetadataChange.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithUoSMetadataChangeScenario_ThenWebhookReturns200Response")]

        //Cell Movement
        [TestCase("MoveCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithMoveCellScenario_ThenWebhookReturns200Response")]
        [TestCase("MultipleCellsInSingleUnit.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithMultipleCellsInSingleUnitScenario_ThenWebhookReturns200Response")]

        //Mixed Scenarios
        [TestCase("NewCell_AND_CancelCellWithNewCellReplacement.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithNewCell_AND_CancelCellWithNewCellReplacementScenario_ThenWebhookReturns200Response")]
        [TestCase("NewCell_AND_MoveCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithNewCell_AND_MoveCellScenario_ThenWebhookReturns200Response")]
        [TestCase("NewCell_AND_UoSMetadataChange.JSON", "PermitWithDifferentKey", TestName = "WhenICallTheWebhookWithNewCell_AND_UoSMetadataChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("NewCell_AND_UpdateCell_AND_CellMetadataChange.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithNewCell_AND_UpdateCell_AND_CellMetadataChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("CellMetadataChange_AND_SuspendCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithCellMetadataChange_AND_SuspendCellScenario_ThenWebhookReturns200Response")]
        [TestCase("CellMetadataChangeWithNewCell.JSON", "PermitWithDifferentKey", TestName = "WhenICallTheWebhookWithCellMetadataChangeWithNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("MoveCell_AND_SuspendCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithMoveCell_AND_SuspendCellScenario_ThenWebhookReturns200Response")]

        //Suspend and Withdraw Scenarios
        [TestCase("SuspendCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithSuspendCellScenario_ThenWebhookReturns200Response")]
        [TestCase("SuspendCell_AND_WithdrawCell.JSON", "PermitWithDifferentKey", TestName = "WhenICallTheWebhookWithSuspendCell_AND_WithdrawCellScenario_ThenWebhookReturns200Response")]
        [TestCase("WithdrawCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithWithdrawCellScenario_ThenWebhookReturns200Response")]

        //Update Scenarios
        [TestCase("UpdateCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithUpdateCellScenario_ThenWebhookReturns200Response")]
        [TestCase("UpdateCellsWithDifferentStatusName.JSON", "PermitWithDifferentKey", TestName = "WhenICallTheWebhookWithUpdateCellsWithDifferentStatusNameScenario_ThenWebhookReturns200Response")]
        [TestCase("UpdateCellWithNewEdition.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithUpdateCellWithNewEditionScenario_ThenWebhookReturns200Response")]

        //Re-issue scenario
        [TestCase("Re-issue.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithReIssueScenario_ThenWebhookReturns200Response")]
        public async Task WhenWebhookPostEndpointReceivesEventWithValidScenario_ThenWebhookReturns200OkResponse(string jsonPayloadFileName, string permitState)
        {
            PermitWithSameKey permitWithSameKey = null;
            PermitWithDifferentKey permitWithDifferentKey = null;
            string correlationId = null;

            Console.WriteLine("Scenario:" + jsonPayloadFileName + "\n");

            string jsonPayloadFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S57PayloadFolder, jsonPayloadFileName);
            string xmlPayloadFilePath = jsonPayloadFilePath.Replace(EventPayloadFiles.PayloadFolder, EventPayloadFiles.ErpFacadeExpectedXmlFolder)
                                                           .Replace(EventPayloadFiles.S57PayloadFolder, EventPayloadFiles.S57ExpectedXmlFolder)
                                                           .Replace(".JSON", ".xml");

            string requestBody = await File.ReadAllTextAsync(jsonPayloadFilePath);

            if (permitState == JsonFields.PermitWithSameKey)
            {
                permitWithSameKey = new PermitWithSameKey() { Permit = _erpFacadeConfiguration.PermitWithSameKey.Permit, ActiveKey = _erpFacadeConfiguration.PermitWithSameKey.ActiveKey, NextKey = _erpFacadeConfiguration.PermitWithSameKey.NextKey };
            }
            else
            {
                permitWithDifferentKey = new PermitWithDifferentKey() { Permit = _erpFacadeConfiguration.PermitWithDifferentKey.Permit, ActiveKey = _erpFacadeConfiguration.PermitWithDifferentKey.ActiveKey, NextKey = _erpFacadeConfiguration.PermitWithDifferentKey.NextKey };
            }

            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, correlationId) = JsonModifier.UpdateCorrelationId(requestBody);

            requestBody = JsonModifier.UpdatePermitField(requestBody, permitState == JsonFields.PermitWithSameKey ? permitWithSameKey.Permit : permitWithDifferentKey.Permit);

            RestResponse response = await _webhookEndpoint.PostWebhookResponseAsync(requestBody, await _authTokenProvider.GetAzureADToken(false));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            string generatedXmlFilePath = _azureBlobReaderWriter.DownloadContainerFile(Path.Combine(_projectDir, EventPayloadFiles.GeneratedXmlFolder), correlationId, ".xml");

            Assert.That(S57XmlValidator.VerifyXmlAttributes(generatedXmlFilePath, xmlPayloadFilePath, permitState == JsonFields.PermitWithSameKey ? permitWithSameKey.ActiveKey : permitWithDifferentKey.ActiveKey, permitState == JsonFields.PermitWithSameKey ? permitWithSameKey.NextKey : permitWithDifferentKey.NextKey));
        }

        //AIO-cell scenarios
        [TestCase("AIOUpdateCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithAIOUpdateCellScenario_ThenWebhookReturns200Response")]
        [TestCase("AIONewCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithAIONewCellScenario_ThenWebhookReturns200Response")]

        public async Task WhenWebhookPostEndpointReceivesEventWithValidAioCellScenario_ThenWebhookReturns200OkResponse(string jsonPayloadFileName, string permitState)
        {
            string correlationId = null;

            Console.WriteLine("Scenario:" + jsonPayloadFileName + "\n");

            string requestPayloadFilePath = Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S57PayloadFolder, jsonPayloadFileName);

            string requestBody = await File.ReadAllTextAsync(requestPayloadFilePath);

            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, correlationId) = JsonModifier.UpdateCorrelationId(requestBody);

            RestResponse response = await _webhookEndpoint.PostWebhookResponseAsync(requestBody, await _authTokenProvider.GetAzureADToken(false));

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            string generatedXmlFilePath = _azureBlobReaderWriter.DownloadContainerFile(Path.Combine(_projectDir, EventPayloadFiles.GeneratedXmlFolder), correlationId, ".xml");

            generatedXmlFilePath.Should().BeNullOrEmpty();
        }

        [Test]
        //Mandatory attribute validation scenarios
        [TestCase("products.productName", 1, TestName = "WhenProductNameIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("unitsOfSale.unitName", 1, TestName = "WhenUnitNameIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("products.providerCode", 1, TestName = "WhenProductProviderCodeIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("unitsOfSale.providerCode", 1, TestName = "WhenUnitProviderCodeIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("products.size", 1, TestName = "WhenProductSizeIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("unitsOfSale.unitSize", 1, TestName = "WhenUnitSizeIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("products.title", 1, TestName = "WhenProductTitleIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("unitsOfSale.title", 1, TestName = "WhenUnitTitleIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("products.editionNumber", 1, TestName = "WhenProductEditionNumberIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("products.updateNumber", 1, TestName = "WhenProductUpdateNumberIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("unitsOfSale.unitType", 1, TestName = "WhenUnitTypeIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("year", 0, TestName = "WhenYearIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("week", 0, TestName = "WhenWeekIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("currentWeekAlphaCorrection", 0, TestName = "WhenAlphaCorrectionFlagIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        [TestCase("products.permit", 1, TestName = "WhenProductPermitIsEmptyOrNull_ThenWebhookShouldReturn500ResponseCode")]
        public async Task WhenWebhookPostEndpointReceivesEventWithRequiredDataMissing_ThenWebhookReturnsInternalServerErrorResponse(string attributeName, int index)
        {
            RestResponse response;
            string correlationId = null;

            string requestPayload = await File.ReadAllTextAsync(Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S57PayloadFolder, "MandatoryAttributeValidation.JSON"));

            requestPayload = JsonModifier.UpdateTime(requestPayload);
            (requestPayload, correlationId) = JsonModifier.UpdateCorrelationId(requestPayload);
            requestPayload = JsonModifier.UpdatePermitField(requestPayload, _erpFacadeConfiguration.PermitWithSameKey.Permit);

            var requestPayloadWithMissingProperty = JsonModifier.UpdateMandatoryAttribute(requestPayload, attributeName, index, "Remove");

            response = await _webhookEndpoint.PostWebhookResponseAsync(requestPayloadWithMissingProperty, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);

            var requestPayloadWithNullProperty = JsonModifier.UpdateMandatoryAttribute(requestPayload, attributeName, index, "Null");

            response = await _webhookEndpoint.PostWebhookResponseAsync(requestPayloadWithNullProperty, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task WhenPermitDecryptionFails_ThenWebhookReturnsInternalServerErrorResponse()
        {
            string requestPayload = await File.ReadAllTextAsync(Path.Combine(_projectDir, EventPayloadFiles.PayloadFolder, EventPayloadFiles.S57PayloadFolder, EventPayloadFiles.WebhookPayloadFileName));

            RestResponse response = await _webhookEndpoint.PostWebhookResponseAsync(requestPayload, await _authTokenProvider.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }
    }
}
