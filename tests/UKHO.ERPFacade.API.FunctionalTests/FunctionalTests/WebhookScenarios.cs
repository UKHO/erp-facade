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
        [TestCase("ID3_1NewCellScenario.JSON", "Y", "SamePermitKey", TestName = "WhenICallTheWebhookWithOneNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("ID4_2NewCellScenario.JSON", "N", "DifferentPermitKey", TestName = "WhenICallTheWebhookWithTwoNewCellScenario_ThenWebhookReturns200Response")]
        [TestCase("ID5_1NewCellWoNewAVCSUnit.JSON", "Y", "SamePermitKey", TestName = "WhenICallTheWebhookWithOneNewCellScenarioWithourAVCSUoS_ThenWebhookReturns200Response")]

        //Cancel & Replace
        [TestCase("ID6_2CellsReplace1CellsCancel.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithTwoCellsReplacesOneCellScenario_ThenWebhookReturns200Response")]
        [TestCase("ID7_1CellCancel.JSON", "N", TestName = "WhenICallTheWebhookWithOneCancelCellScenario_ThenWebhookReturns200Response")]

        //Metadata Change
        [TestCase("ID8_2CellMetadataChange.JSON", "Y", TestName = "WhenICallTheWebhookWith2CellMetadataChangeScenario_ThenWebhookReturns200Response")]
        [TestCase("ID9_MetadataChange.JSON", "N", TestName = "WhenICallTheWebhookWithMetadataChangeScenario_ThenWebhookReturns200Response")]

        //Update
        [TestCase("ID10_UpdateSimple.JSON", "Y", "SamePermitKey", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellWithStatusNameAsUpdate_ThenWebhookReturns200Response")]
        [TestCase("ID11_updateOneCellWithNewEditionStatus.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellWithStatusNameAsNewEdition_ThenWebhookReturns200Response")]
        [TestCase("ID12_updateOneCellWithReIssueStatus.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingOneCellStatusNameAsReIssue_ThenWebhookReturns200Response")]
        [TestCase("ID13_updateTwoCellsWithDifferentStatusName.JSON", "N", "DifferentPermitKey", TestName = "WhenICallTheWebhookWithSimpleUpdateScenarioHavingTwoCellsWithDifferentStatusName_ThenWebhookReturns200Response")]

        //Move Cell
        [TestCase("ID14_moveOneCell.JSON", "Y", TestName = "WhenICallTheWebhookWithSimpleMoveCellScenario_ThenWebhookReturns200Response")]
        [TestCase("ID15_oneNewCellAndOneMoveOneCell.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithOneNewCellAndOneMoveOneCellScenario_ThenWebhookReturns200Response")]

        //Mixed
        [TestCase("ID16_newCell_updateCell_metadataChange.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithMixScenarioHavingNewCellAndUpdateCellAndMetadataChange_ThenWebhookReturns200Response")]
        [TestCase("ID17_newCell_and_CancelReplace.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithMixScenarioHavingOneNewCellAndOneCancel&ReplaceCell_ThenWebhookReturns200Response")]
        [TestCase("ID18_CancelReplace_UpdateCell.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithMixScenarioHavingCancel&Replace_UpdateCell_ThenWebhookReturns200Response")]
        [TestCase("ID19_CR_metadata_move.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithMixScenarioHavingCancel&ReplaceAndMetadataChangeAndMoveCell_ThenWebhookReturns200Response")]

        //New Edition
        [TestCase("ID20_newEditionAdditionalCoverageV_01.JSON", "N", "SamePermitKey",TestName = "WhenICallTheWebhookWithNewEditionAdditionalCoverageV01PayloadFile_ThenWebhookReturns200Response")]

        //V0.3S
        [TestCase("ID21_cancelAndReplaceV_03.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithCancelAndReplaceV03PayloadFile_ThenWebhookReturns200Response")]
        [TestCase("ID22_Cell_Moves_Unit_and_New_CellV_03.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithCellMoveAndNewCellV03PayloadFile_ThenWebhookReturns200Response")]
        [TestCase("ID23_Cell_MoveV_03.JSON", "N", TestName = "WhenICallTheWebhookWithCellMoveV03PayloadFile_ThenWebhookReturns200Response")]
        [TestCase("ID24_Metadata_ChangeV_03.JSON", "N", TestName = "WhenICallTheWebhookWithMetadataChangeV03PayloadFile_ThenWebhookReturns200Response")]
        [TestCase("ID25_Mixed_scenario1V_03.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithMixedScenario1V03PayloadFile_ThenWebhookReturns200Response")]
        [TestCase("ID26_New_CellV_03.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithNewCellV03PayloadFile_ThenWebhookReturns200Response")]

        //Supplier Defined Releasability Set v0.1
        [TestCase("ID27_supplier_Defined_ReleasabilitySet_V_01.JSON", "Y", "SamePermitKe", TestName = "WhenICallTheWebhookWithSupplierDefinedReleasabilitySetV01_ThenWebhookReturns200Response")]

        //Suspended & Withdrawn
        [TestCase("ID28_simpleSuspendedScenario.JSON", "N", "DifferentPermitKey", TestName = "WhenICallTheWebhookWithSimpleSuspendedScenario_ThenWebhookReturns200Response")]
        [TestCase("ID29_simpleWithdrawnScenario.JSON", "N", TestName = "WhenICallTheWebhookWithSimpleWithdrawnScenario_ThenWebhookReturns200Response")]
        [TestCase("ID30_Suspend_and_WithdrawV01.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithSuspendedAndWithdrawnScenario_ThenWebhookReturns200Response")]
        [TestCase("ID31_metadataAndSuspended.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithMetadataAndSuspendedMixScenario_ThenWebhookReturns200Response")]
        [TestCase("ID32_moveAndSuspended.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithMoveDataAndSuspendedMixScenario_ThenWebhookReturns200Response")]

        //Rule change unitType & addProducts
        [TestCase("ID33_NewCell_With2UoS_But_only1_having_addProduct.JSON", "N", "SamePermitKey",TestName = "WhenICallTheWebhookWithNewCellScenarioWithMultipleUoSHavingUnitOfSalesTypeUnitButOnly1HavingValueInAddProducts_ThenWebhookReturns200Response")]
        [TestCase("ID34_Cancel&Replace_With_NewCells_having_2UoS_With_addProductValue.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithCancel&ReplaceScenarioHavingMultipleUnitOfSalesTypeUnitAndValueInAddProducts_ThenWebhookReturns200Response")]
        [TestCase("ID35_Cancel&Replace_With_CancelCell_having_2UoS_but_onlyOneAsTypeUnit.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithCancel&ReplaceWithCancelledCellHaving2UoSButOnly1IsOfTypeUnit_ThenWebhookReturns200Response")]
        [TestCase("ID36_MoveAndSuspended_With_2UoS_But1_Having_addProductsValue.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithMoveAndSuspendedWith2UoSButOnly1HavingValueInAddProduct_ThenWebhookReturns200Response")]

        // Rule change for create avcs unit of sale (having multiple products in addProducts)
        [TestCase("ID37_CreateUoSHavingMultipleItemsInAddProducts.JSON", "N", "SamePermitKey", TestName = "WhenICallTheWebhookWithMoveAndNewCellScenarioWhereUoSHasMultipleValuesInAddProducts_ThenWebhookReturns200Response")]
        
        [TestCase("ID38_3AdditionalCoverageCell.JSON", "N","SamePermitKey", TestName = "WhenICallTheWebhookWithThreeAdditionalCoverageCellScenario_ThenWebhookReturns200Response")]
        [TestCase("ID39_1AdditionalCoverageWith1CancelledCell.JSON", "N","SamePermitKey", TestName = "WhenICallTheWebhookWithOneAdditionalCoverageWithCancelledCellScenario_ThenWebhookReturns200Response")]
        [TestCase("ID40_1AdditionalCoverageWithReplacedCellAnd1CancelledCell.JSON", "N", "SamePermitKey",TestName = "WhenICallTheWebhookWithOneAdditionalCoverageWithReplacedAndOneCancelledCellScenario_ThenWebhookReturns200Response")]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse1(string payloadJsonFileName, string correctionTag, string permitState = "permitString")
        {
            Console.WriteLine("Scenario:" + payloadJsonFileName + "\n");
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedXmlFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXmlFolder);
            RestResponse response = await WebhookEndpoint.PostWebhookResponseAsyncForXml(filePath, generatedXmlFolder, await _authToken.GetAzureADToken(false), correctionTag, permitState);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
