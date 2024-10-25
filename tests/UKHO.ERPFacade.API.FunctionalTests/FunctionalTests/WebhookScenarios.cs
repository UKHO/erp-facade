using FluentAssertions;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Service;

namespace UKHO.ERPFacade.API.FunctionalTests.FunctionalTests
{
    [TestFixture]
    public class WebhookScenarios
    {
        private WebhookEndpoint WebhookEndpoint { get; set; }
        private readonly ADAuthTokenProvider _authToken = new();

        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        [SetUp]
        public void Setup()
        {
            WebhookEndpoint = new WebhookEndpoint();
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var response = await WebhookEndpoint.OptionWebhookResponseAsync(await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            var response = await WebhookEndpoint.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse"), Order(2)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            var response = await WebhookEndpoint.PostWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(4)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            var response = await WebhookEndpoint.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(true));
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

        //AIO-cell scenarios
        [TestCase("AIOUpdateCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithAIOUpdateCellScenario_ThenWebhookReturns200Response")]
        [TestCase("AIONewCell.JSON", "PermitWithSameKey", TestName = "WhenICallTheWebhookWithAIONewCellScenario_ThenWebhookReturns200Response")]

        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse(string payloadJsonFileName, string permitState)
        {
            Console.WriteLine("Scenario:" + payloadJsonFileName + "\n");
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedXmlFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXmlFolder);
            RestResponse response = await WebhookEndpoint.PostWebhookResponseAsyncForXml(filePath, generatedXmlFolder, await _authToken.GetAzureADToken(false), permitState );
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
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
        public async Task WhenMandatoryAttributeIsEmptyOrNullInPayload_ThenWebhookReturnsInternalServerErrorResponse(string attributeName, int index)
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "MandatoryAttributeValidation.JSON");
            RestResponse response = await WebhookEndpoint.PostWebhookResponseForMandatoryAttributeValidation(filePath, await _authToken.GetAzureADToken(false), attributeName, index, "Remove");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);

            response = await WebhookEndpoint.PostWebhookResponseForMandatoryAttributeValidation(filePath, await _authToken.GetAzureADToken(false), attributeName, index, "EmptyOrNull");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task WhenPermitDecryptionFails_ThenWebhookReturnsInternalServerErrorResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "NewCell.JSON");
            const string permitString = "wwslkC9oG3rNcT4ZrgqX39pg9DuC9oSkBsl4kqiwr5h3nW6t0HUmlSaYhpdLEpO1";  //Invalid permit string to test permit decryption failure.
            RestResponse response = await WebhookEndpoint.PostWebhookResponseAsyncForXml(filePath, "", await _authToken.GetAzureADToken(false), permitString);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }
    }
}
