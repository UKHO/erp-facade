﻿using FluentAssertions;
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
        private WebhookEndpoint _webhook { get; set; }
        private readonly ADAuthTokenProvider _authToken = new();
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));
        
        [SetUp]
        public void Setup()
        {
            _webhook = new WebhookEndpoint();
        }
        //added comment to check git access
        [Test(Description = "WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var response = await _webhook.OptionWebhookResponseAsync(await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse"), Order(0)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            var response = await _webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse"), Order(2)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithInvalidToken_ThenWebhookReturns401UnAuthorizedResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            var response = await _webhook.PostWebhookResponseAsync(filePath, "invalidToken_abcd");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Test(Description = "WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse"), Order(4)]
        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithTokenHavingNoRole_ThenWebhookReturns403ForbiddenResponse()
        {
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.WebhookPayloadFileName);
            var response = await _webhook.PostWebhookResponseAsync(filePath, await _authToken.GetAzureADToken(true));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

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

        //New Edition
        [TestCase("ID20_newEditionAdditionalCoverageV_01.JSON", TestName = "WhenICallTheWebhookWithNewEditionAdditionalCoverageV01PayloadFile_ThenWebhookReturns200Response")]
        
        //V0.3S
        [TestCase("ID21_cancelAndReplaceV_03.JSON", TestName = "WhenICallTheWebhookWithCancelAndReplaceV03PayloadFile_ThenWebhookReturns200Response")]
        [TestCase("ID22_Cell_Moves_Unit_and_New_CellV_03.JSON", TestName = "WhenICallTheWebhookWithCellMoveAndNewCellV03PayloadFile_ThenWebhookReturns200Response")]
        [TestCase("ID23_Cell_MoveV_03.JSON", TestName = "WhenICallTheWebhookWithCellMoveV03PayloadFile_ThenWebhookReturns200Response")]
        [TestCase("ID24_Metadata_ChangeV_03.JSON", TestName = "WhenICallTheWebhookWithMetadataChangeV03PayloadFile_ThenWebhookReturns200Response")]
        [TestCase("ID25_Mixed_scenario1V_03.JSON", TestName = "WhenICallTheWebhookWithMixedScenario1V03PayloadFile_ThenWebhookReturns200Response")]
        [TestCase("ID26_New_CellV_03.JSON", TestName = "WhenICallTheWebhookWithNewCellV03PayloadFile_ThenWebhookReturns200Response")]

        //Supplier Defined Releasability Set v0.1
        [TestCase("ID27_supplier_Defined_ReleasabilitySet_V_01.JSON", TestName = "WhenICallTheWebhookWithSupplierDefinedReleasabilitySetV01_ThenWebhookReturns200Response")]

        //Suspended & Withdrawn
        [TestCase("ID28_simpleSuspendedScenario.JSON", TestName = "WhenICallTheWebhookWithSimpleSuspendedScenario_ThenWebhookReturns200Response")]
        [TestCase("ID29_simpleWithdrawnScenario.JSON", TestName = "WhenICallTheWebhookWithSimpleWithdrawnScenario_ThenWebhookReturns200Response")]
        [TestCase("ID30_Suspend_and_WithdrawV01.JSON", TestName = "WhenICallTheWebhookWithSuspendedAndWithdrawnScenario_ThenWebhookReturns200Response")]
        [TestCase("ID31_metadataAndSuspended.JSON", TestName = "WhenICallTheWebhookWithMetadataAndSuspendedMixScenario_ThenWebhookReturns200Response")]
        [TestCase("ID32_moveAndSuspended.JSON", TestName = "WhenICallTheWebhookWithMovedataAndSuspendedMixScenario_ThenWebhookReturns200Response")]

        //Rule change unitType & addProducts
        [TestCase("ID33_NewCell_With2UoS_But_only1_having_addProduct.JSON", TestName = "WhenICallTheWebhookWithNewCellScenarioWithMultipleUoSHavingUnitOfSalesTypeUnitButOnly1HavingValueInAddProducts_ThenWebhookReturns200Response")]
        [TestCase("ID34_Cancel&Replace_With_NewCells_having_2UoS_With_addProductValue.JSON", TestName = "WhenICallTheWebhookWithCancel&ReplaceScenarioHavingMultipleUnitOfSalesTypeUnitAndValueInAddProducts_ThenWebhookReturns200Response")]
        [TestCase("ID35_Cancel&Replace_With_CancelCell_having_2UoS_but_onlyOneAsTypeUnit.JSON", TestName = "WhenICallTheWebhookWithCancel&ReplaceWithCancelledCellHaving2UoSButOnly1IsOfTypeUnit_ThenWebhookReturns200Response")]
        [TestCase("ID36_MoveAndSuspended_With_2UoS_But1_Having_addProductsValue.JSON", TestName = "WhenICallTheWebhookWithMoveAndSuspendedWith2UoSButOnly1HavingValueInAddProduct_ThenWebhookReturns200Response")]

        // Rule change for create avcs unit of sale (having multiple products in addProducts)
        [TestCase("ID37_CreateUoSHavingMultipleItemsInAddProducts.JSON", TestName = "WhenICallTheWebhookWithMoveAndNewCellScenarioWhereUoSHasMultipleValuesInAddProducts_ThenWebhookReturns200Response")]

        public async Task WhenValidEventInNewEncContentPublishedEventReceivedWithValidToken_ThenWebhookReturns200OkResponse1(string payloadJsonFileName)
        {
            Console.WriteLine("Scenario:" + payloadJsonFileName + "\n");
            string filePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, payloadJsonFileName);
            string generatedXMLFolder = Path.Combine(_projectDir, Config.TestConfig.GeneratedXMLFolder);
            RestResponse response = await _webhook.PostWebhookResponseAsyncForXML(filePath, generatedXMLFolder, await _authToken.GetAzureADToken(false));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
