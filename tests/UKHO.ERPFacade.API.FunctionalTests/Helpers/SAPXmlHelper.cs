using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class SapXmlHelper
    {
        private static int actionCounter;
        private static readonly List<string> attrNotMatched = new();
        private static List<string> changeAvcsUoS = new();
        private static readonly Dictionary<string, List<string>> changeEncCell = new();
        private static JsonPayloadHelper JsonPayload { get; set; }
        private static JsonPayloadHelper UpdatedJsonPayload { get; set; }
        public static List<string> ListFromJson = new();
        public static List<string> ActionsListFromXml = new();
        private static readonly string weekNoTag = Config.TestConfig.WeekNoTag;
        private static readonly string validFromTagThursday = Config.TestConfig.ValidFromTagThursday;
        private static readonly string validFromTagFriday = Config.TestConfig.ValidFromTagFriday;
        private static readonly int noOfMandatoryXMLAttribute = 20;

        public static async Task<bool> CheckXmlAttributes(JsonPayloadHelper jsonPayload, string xmlFilePath, string updatedRequestBody, string correctionTag, string permitState)
        {
            SapXmlHelper.JsonPayload = jsonPayload;
            UpdatedJsonPayload = JsonConvert.DeserializeObject<JsonPayloadHelper>(updatedRequestBody);

            XmlDocument xmlDoc = new ();
            xmlDoc.LoadXml(await File.ReadAllTextAsync(xmlFilePath));

            while (xmlDoc.DocumentElement.Name == "soap:Envelope" || xmlDoc.DocumentElement.Name == "soap:Body")
            {
                string tempXmlString = xmlDoc.DocumentElement.InnerXml;
                xmlDoc.LoadXml(tempXmlString);
            }

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlDoc.InnerXml));
            var reader = new XmlTextReader(ms) { Namespaces = false };
            var serializer = new XmlSerializer(typeof(Z_ADDS_MAT_INFO));
            var result = (Z_ADDS_MAT_INFO)serializer.Deserialize(reader);

            Assert.That(VerifyPresenceOfMandatoryXmlAttributes(result.IM_MATINFO.ACTIONITEMS).Result);

            actionCounter = 1;
            changeEncCell.Clear();
            foreach (ZMAT_ACTIONITEMS item in result.IM_MATINFO.ACTIONITEMS)
            {
                if (item.ACTION == "CREATE ENC CELL")
                    Assert.That(VerifyCreateEncCell(item.CHILDCELL, item, correctionTag, permitState));
                else if (item.ACTION == "CREATE AVCS UNIT OF SALE")
                    Assert.That(VerifyCreateAVCSUnitOfSale(item.PRODUCTNAME, item, correctionTag));
                else if (item.ACTION == "ASSIGN CELL TO AVCS UNIT OF SALE")
                    Assert.That(VerifyAssignCellToAVCSUnitOfSale(item.CHILDCELL, item.PRODUCTNAME, item, correctionTag));
                else if (item.ACTION == "REPLACED WITH ENC CELL")
                    Assert.That(VerifyReplaceWithEncCell(item.CHILDCELL, item.REPLACEDBY, item, correctionTag) ?? false);
                else if (item.ACTION == "ADDITIONAL COVERAGE ENC CELL")
                    Assert.That(VerifyAdditionalCoverageWithEncCell(item.CHILDCELL, item.REPLACEDBY, item, correctionTag) ?? false);
                else if (item.ACTION == "REMOVE ENC CELL FROM AVCS UNIT OF SALE")
                    Assert.That(VerifyRemoveENCCellFromAVCSUnitOFSale(item.CHILDCELL, item.PRODUCTNAME, item, correctionTag) ?? false);
                else if (item.ACTION == "CANCEL ENC CELL")
                    Assert.That(VerifyCancelEncCell(item.CHILDCELL, item.PRODUCTNAME, item, correctionTag) ?? false);
                else if (item.ACTION == "CANCEL AVCS UNIT OF SALE")
                    Assert.That(VerifyCancelToAVCSUnitOfSale(item.PRODUCTNAME, item, correctionTag) ?? false);
                else if (item.ACTION == "CHANGE ENC CELL")
                    Assert.That(VerifyChangeEncCell(item.CHILDCELL, item, correctionTag) ?? false);
                else if (item.ACTION == "CHANGE AVCS UNIT OF SALE")
                    Assert.That(VerifyChangeAVCSUnitOfSale(item.PRODUCTNAME, item, correctionTag) ?? false);
                else if (item.ACTION == "UPDATE ENC CELL EDITION UPDATE NUMBER")
                    Assert.That(VerifyUpdateEncCellEditionUpdateNumber(item.CHILDCELL, item, permitState, correctionTag) ?? false);
                else
                    Assert.Fail("Not a required action");
                actionCounter++;
            }

            Console.WriteLine("Total verified Actions:" + --actionCounter);
            await Task.CompletedTask;
            Console.WriteLine("XML has correct data");
            return true;
        }

        private static bool? VerifyChangeAVCSUnitOfSale(string productName, ZMAT_ACTIONITEMS item, string correctionTag)
        {
            Console.WriteLine("Action#:" + actionCounter + ".UnitOfSale:" + productName);
            foreach (KeyValuePair<string, List<string>> ele2 in changeEncCell)
            {
                changeAvcsUoS = ele2.Value;

                if (changeAvcsUoS.Contains(productName))
                {
                    attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(GetProductInfo(ele2.Key).ProductType))
                        attrNotMatched.Add(nameof(item.PRODTYPE));
                    if (!item.AGENCY.Equals((GetProductInfo(ele2.Key)).Agency))
                        attrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(GetProductInfo(ele2.Key).ProviderCode))
                        attrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(GetUoSInfo(productName).UnitSize))
                        attrNotMatched.Add(nameof(item.ENCSIZE));
                    VerifyAdditionalXmlTags(item, correctionTag);
                    //Checking blanks
                    List<string> blankFieldNames = new() { "CANCELLED", "REPLACEDBY", "EDITIONNO", "UPDATENO", "ACTIVEKEY", "NEXTKEY", "TITLE", "UNITTYPE" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CHANGE AVCS UNIT OF SALE Action's Data is correct");
                        int valueIndex = ele2.Value.IndexOf(productName);
                        changeAvcsUoS[valueIndex] = changeAvcsUoS[valueIndex].Replace(productName, "skip");
                        return true;
                    }

                    Console.WriteLine("CHANGE AVCS UNIT OF SALE Action's Data is incorrect");
                    Console.WriteLine("Not matching attributes are:");
                    foreach (string attribute in attrNotMatched)
                    {
                        Console.WriteLine(attribute);
                    }
                    return false;
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }

        private static bool? VerifyUpdateEncCellEditionUpdateNumber(string childCell, ZMAT_ACTIONITEMS item, string permitState, string correctionTag)
        {
            Console.WriteLine("Action#:" + actionCounter + ".Childcell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) &&
                    (product.Status.StatusName.Contains("Update") || product.Status.StatusName.Contains("New Edition") || product.Status.StatusName.Contains("Re-issue")) &&
                    (!product.Status.IsNewCell) &&
                    (product.ContentChange))
                {
                    attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        attrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        attrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.EDITIONNO.Equals(product.EditionNumber))
                        attrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(product.UpdateNumber))
                        attrNotMatched.Add(nameof(item.UPDATENO));
                    VerifyAdditionalXmlTags(item, correctionTag);
                    List<string> blankFieldNames = new() { "CANCELLED", "REPLACEDBY", "UNITTYPE", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE" };
                    VerifyDecryptedPermit(item, permitState);
                        
                    //Checking blanks
                    VerifyBlankFields(item, blankFieldNames);

                    if (attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is correct");
                        return true;
                    }

                    Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is incorrect");
                    Console.WriteLine("Not matching attributes are:");
                    foreach (string attribute in attrNotMatched)
                    { Console.WriteLine(attribute); }
                    return false;
                }
                else if ((childCell == product.ProductName) && (product.Status.StatusName.Contains("Suspended")))
                {
                    attrNotMatched.Clear();
                    Console.WriteLine("The UoS name for " + childCell + " calculated is: " + GetUoSName(childCell));
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        attrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        attrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.EDITIONNO.Equals(product.EditionNumber))
                        attrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(product.UpdateNumber))
                        attrNotMatched.Add(nameof(item.UPDATENO));
                    VerifyDecryptedPermit(item, permitState);
                    //Checking blanks
                    List<string> blankFieldNames = new(){ "CANCELLED", "REPLACEDBY", "UNITTYPE", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is correct");
                        return true;
                    }
                    Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is incorrect");
                    Console.WriteLine("Not matching attributes are:");
                    foreach (string attribute in attrNotMatched)
                    { Console.WriteLine(attribute); }
                    return false;
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool? VerifyChangeEncCell(string childCell, ZMAT_ACTIONITEMS item, string correctionTag)
        {
            Console.WriteLine("Action#:" + actionCounter + ".Childcell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (!product.ContentChange))
                {
                    attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        attrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        attrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.AGENCY.Equals(product.Agency))
                        attrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(product.ProviderCode))
                        attrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(product.Size))
                        attrNotMatched.Add(nameof(item.ENCSIZE));
                    VerifyAdditionalXmlTags(item, correctionTag);
                    //Checking blanks
                    List<string> blankFieldNames = new() { "CANCELLED", "REPLACEDBY", "UNITTYPE", "ACTIVEKEY", "NEXTKEY", "TITLE", "EDITIONNO", "UPDATENO" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CHANGE ENC CELL Action's Data is correct");
                        changeEncCell.Add(childCell, product.InUnitsOfSale);
                        return true;
                    }

                    Console.WriteLine("CHANGE ENC CELL Action's Data is incorrect");
                    Console.WriteLine("Not matching attributes are:");
                    foreach (string attribute in attrNotMatched)
                    { Console.WriteLine(attribute); }
                    return false;
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool? VerifyCancelToAVCSUnitOfSale(string productName, ZMAT_ACTIONITEMS item, string correctionTag)
        {
            Console.WriteLine("Action#:" + actionCounter + ".UnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                if ((productName == unitOfSale.UnitName) && (unitOfSale.Status.Equals("NotForSale")))
                {
                    attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals((GetProductInfo(unitOfSale.CompositionChanges.RemoveProducts)).ProductType))
                        attrNotMatched.Add(nameof(item.PRODTYPE));
                    VerifyAdditionalXmlTags(item, correctionTag);
                    //Checking blanks
                    List<string> blankFieldNames = new() { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE", "ACTIVEKEY", "NEXTKEY" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CANCEL AVCS UNIT OF SALE Action's Data is correct");
                        return true;
                    }

                    Console.WriteLine("CANCEL AVCS UNIT OF SALE Action's Data is incorrect");
                    Console.WriteLine("Not matching attributes are:");
                    foreach (string attribute in attrNotMatched)
                    { Console.WriteLine(attribute); }
                    return false;
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }

        private static bool? VerifyCancelEncCell(string childCell, string productName, ZMAT_ACTIONITEMS item, string correctionTag)
        {
            Console.WriteLine("Action#:" + actionCounter + ".ENC Cell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.Status.StatusName.Equals("Cancellation Update")) && (product.InUnitsOfSale.Contains(productName)))
                {
                    attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        attrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        attrNotMatched.Add(nameof(item.PRODUCTNAME));
                    VerifyAdditionalXmlTags(item, correctionTag);

                    //Checking blanks
                    List<string> blankFieldNames = new() { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE", "ACTIVEKEY", "NEXTKEY" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CANCEL ENC CELL Action's Data is correct");
                        return true;
                    }

                    Console.WriteLine("CANCEL ENC CELL Action's Data is incorrect");
                    Console.WriteLine("Not matching attributes are:");
                    foreach (string attribute in attrNotMatched)
                    { Console.WriteLine(attribute); }
                    return false;
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool? VerifyRemoveENCCellFromAVCSUnitOFSale(string childCell, string productName, ZMAT_ACTIONITEMS item, string correctionTag)
        {
            Console.WriteLine("Action#:" + actionCounter + ".AVCSUnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                List<string> pdts = unitOfSale.CompositionChanges.RemoveProducts;
                foreach (string pdt in pdts)
                {
                    if ((childCell == pdt) && (productName == unitOfSale.UnitName))
                    {
                        attrNotMatched.Clear();
                        if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                            attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                        if (!item.PRODUCT.Equals("AVCS UNIT"))
                            attrNotMatched.Add(nameof(item.PRODUCT));
                        if (!item.PRODTYPE.Equals((GetProductInfo(unitOfSale.CompositionChanges.RemoveProducts)).ProductType))
                            attrNotMatched.Add(nameof(item.PRODTYPE));
                        VerifyAdditionalXmlTags(item, correctionTag);
                        //xmlAttributes[4] & [5] are skipped as already checked
                        //Checking blanks
                        List<string> blankFieldNames = new() { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE", "ACTIVEKEY", "NEXTKEY" };
                        VerifyBlankFields(item, blankFieldNames);

                        if (attrNotMatched.Count == 0)
                        {
                            Console.WriteLine("REMOVE ENC CELL FROM AVCS UNIT OF SALE Action's Data is correct");
                            return true;
                        }

                        Console.WriteLine("REMOVE ENC CELL FROM AVCS UNIT OF SALE Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in attrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }

        private static bool? VerifyReplaceWithEncCell(string childCell, string replaceBy, ZMAT_ACTIONITEMS item, string correctionTag)
        {
            Console.WriteLine("Action#:" + actionCounter + ".ENC Cell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.ReplacedBy.Contains(replaceBy)))
                {
                    attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        attrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        attrNotMatched.Add(nameof(item.PRODUCTNAME));
                    VerifyAdditionalXmlTags(item, correctionTag);
                    //Checking blanks
                    List<string> blankFieldNames = new() { "CANCELLED", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE", "ACTIVEKEY", "NEXTKEY" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("REPLACED WITH ENC CELL Action's Data is correct");
                        int valueIndex = product.ReplacedBy.IndexOf(replaceBy);
                        product.ReplacedBy[valueIndex] = product.ReplacedBy[valueIndex].Replace(replaceBy, "skip");
                        return true;
                    }

                    Console.WriteLine("REPLACED WITH ENC CELL Action's Data is incorrect");
                    Console.WriteLine("Not matching attributes are:");
                    foreach (string attribute in attrNotMatched)
                    { Console.WriteLine(attribute); }
                    return false;
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool? VerifyAdditionalCoverageWithEncCell(string childCell, string replaceBy, ZMAT_ACTIONITEMS item, string correctionTag)
        {
            Console.WriteLine("Action#:" + actionCounter + ".ENC Cell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.AdditionalCoverage.Contains(replaceBy)))
                {
                    attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        attrNotMatched.Add(nameof(item.PRODTYPE));
                    VerifyAdditionalXmlTags(item, correctionTag);
                    //Checking blanks
                    List<string> blankFieldNames = new() { "PRODUCTNAME", "CANCELLED", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE", "ACTIVEKEY", "NEXTKEY" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("ADDITIONAL COVERAGE ENC CELL Action's Data is correct");
                        int valueIndex = product.AdditionalCoverage.IndexOf(replaceBy);
                        product.AdditionalCoverage[valueIndex] = product.AdditionalCoverage[valueIndex].Replace(replaceBy, "skip");
                        return true;
                    }

                    Console.WriteLine("ADDITIONAL COVERAGE ENC CELL Action's Data is incorrect");
                    Console.WriteLine("Not matching attributes are:");
                    foreach (string attribute in attrNotMatched)
                    { Console.WriteLine(attribute); }
                    return false;
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool VerifyAssignCellToAVCSUnitOfSale(string childCell, string productName, ZMAT_ACTIONITEMS item, string correctionTag)
        {
            Console.WriteLine("Action#:" + actionCounter + ".AVCSUnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                List<string> pdts = unitOfSale.CompositionChanges.AddProducts;
                foreach (string pdt in pdts)
                {
                    if ((childCell == pdt) && (productName == unitOfSale.UnitName))
                    {
                        attrNotMatched.Clear();
                        if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                            attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                        if (!item.PRODUCT.Equals("AVCS UNIT"))
                            attrNotMatched.Add(nameof(item.PRODUCT));
                        if (!item.PRODTYPE.Equals((GetProductInfo(unitOfSale.CompositionChanges.AddProducts)).ProductType))
                            attrNotMatched.Add(nameof(item.PRODTYPE));
                        //xmlAttributes[4] & [5] are skipped as already checked
                        //Checking blanks
                        VerifyAdditionalXmlTags(item, correctionTag);
                        List<string> blankFieldNames = new() { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE", "ACTIVEKEY", "NEXTKEY" };
                        VerifyBlankFields(item, blankFieldNames);

                        if (attrNotMatched.Count == 0)
                        {
                            Console.WriteLine("ASSIGN CELL TO AVCS UNIT OF SALE Action's Data is correct");
                            return true;
                        }

                        Console.WriteLine("ASSIGN CELL TO AVCS UNIT OF SALE Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in attrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Product/Unit of Sale.");
            return false;
        }

        private static bool VerifyCreateAVCSUnitOfSale(string productName, ZMAT_ACTIONITEMS item, string correctionTag)
        {
            Console.WriteLine("Action#:" + actionCounter + ".UnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                if ((productName == unitOfSale.UnitName) && (unitOfSale.IsNewUnitOfSale))
                {
                    attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals((GetFirstProductsInfoHavingUoS(productName)).ProductType))
                        attrNotMatched.Add(nameof(item.PRODTYPE));
                    if (!item.AGENCY.Equals((GetFirstProductsInfoHavingUoS(productName)).Agency))
                        attrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals((GetFirstProductsInfoHavingUoS(productName)).ProviderCode))
                        attrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(unitOfSale.UnitSize))
                        attrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(unitOfSale.Title))
                        attrNotMatched.Add(nameof(item.TITLE));
                    if (!item.UNITTYPE.Equals(unitOfSale.UnitType))
                        attrNotMatched.Add(nameof(item.UNITTYPE));
                    VerifyAdditionalXmlTags(item, correctionTag);
                    //Checking blanks
                    List<string> blankFieldNames = new() { "CANCELLED", "REPLACEDBY", "EDITIONNO", "UPDATENO", "ACTIVEKEY", "NEXTKEY" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CREATE AVCS UNIT OF SALE Action's Data is correct");
                        return true;
                    }

                    Console.WriteLine("CREATE AVCS UNIT OF SALE Action's Data is incorrect");
                    Console.WriteLine("Not matching attributes are:");
                    foreach (string attribute in attrNotMatched)
                    { Console.WriteLine(attribute); }
                    return false;
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }

        private static bool VerifyCreateEncCell(string childCell, ZMAT_ACTIONITEMS item, string correctionTag, string permitState)
        {
            Console.WriteLine("Action#:" + actionCounter + ".Childcell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.Status.IsNewCell))
                {
                    attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        attrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        attrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.AGENCY.Equals(product.Agency))
                        attrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(product.ProviderCode))
                        attrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(product.Size))
                        attrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(product.Title))
                        attrNotMatched.Add(nameof(item.TITLE));
                    if (!item.EDITIONNO.Equals(product.EditionNumber))
                        attrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(product.UpdateNumber))
                        attrNotMatched.Add(nameof(item.UPDATENO));
                    VerifyAdditionalXmlTags(item, correctionTag);
                    VerifyDecryptedPermit(item, permitState);
                    //Checking blanks
                    List<string> blankFieldNames = new() { "CANCELLED", "REPLACEDBY", "UNITTYPE" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CREATE ENC CELL Action's Data is correct");
                        return true;
                    }

                    Console.WriteLine("CREATE ENC CELL Action's Data is incorrect");
                    Console.WriteLine("Not matching attributes are:");
                    foreach (string attribute in attrNotMatched)
                    { Console.WriteLine(attribute); }
                    return false;
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return true;
        }

        private static void VerifyAdditionalXmlTags(ZMAT_ACTIONITEMS item, string correctionTag)
        {
            if (!item.WEEKNO.Equals(weekNoTag))
                attrNotMatched.Add(nameof(item.WEEKNO));
            if (correctionTag == "Y")
            {
                if (!item.VALIDFROM.Equals(validFromTagFriday))
                    attrNotMatched.Add(nameof(item.VALIDFROM));
            }
            else
            {
                if (!item.VALIDFROM.Equals(validFromTagThursday))
                    attrNotMatched.Add(nameof(item.VALIDFROM));
            }
            if (!item.CORRECTION.Equals(correctionTag))
                attrNotMatched.Add(nameof(item.CORRECTION));
        }

        private static void VerifyDecryptedPermit(ZMAT_ACTIONITEMS item, string permitState)
        {
            if (permitState.Contains("Same"))
            {

                if (!item.ACTIVEKEY.Equals(Config.TestConfig.PermitWithSameKey.ActiveKey))
                    attrNotMatched.Add(nameof(item.ACTIVEKEY));
                if (!item.NEXTKEY.Equals(Config.TestConfig.PermitWithSameKey.NextKey))
                    attrNotMatched.Add(nameof(item.NEXTKEY));
            }
            else if (permitState.Contains("Different"))
            {
                if (!item.ACTIVEKEY.Equals(Config.TestConfig.PermitWithDifferentKey.ActiveKey))
                    attrNotMatched.Add(nameof(item.ACTIVEKEY));
                if (!item.NEXTKEY.Equals(Config.TestConfig.PermitWithDifferentKey.NextKey))
                    attrNotMatched.Add(nameof(item.NEXTKEY));
            }
        }

        private static bool VerifyBlankFields(ZMAT_ACTIONITEMS item, List<string> fieldNames)
        {
            bool allBlanks = true;

            foreach (string field in fieldNames)
            {
                if (!typeof(ZMAT_ACTIONITEMS).GetProperty(field).GetValue(item, null).Equals(""))
                    attrNotMatched.Add(typeof(ZMAT_ACTIONITEMS).GetProperty(field).Name);
            }
            return allBlanks;
        }

        private static UoSProductInfo GetProductInfo(List<string> products)
        {
            UoSProductInfo productInfo = new UoSProductInfo();
            foreach (string pdt in products)
            {
                foreach (Product product in JsonPayload.Data.Products)
                {
                    if (pdt.Equals(product.ProductName))
                    {
                        productInfo.ProductType = product.ProductType[4..];
                        productInfo.Agency = product.Agency;
                        productInfo.ProviderCode = product.ProviderCode;
                    }
                }
            }
            return productInfo;
        }

        public static async Task<bool> VerifyPresenceOfMandatoryXmlAttributes(ZMAT_ACTIONITEMS[] ZMAT_ACTIONITEMS)
        {
            List<string> actionAttributesSeq = Config.TestConfig.XmlActionList.ToList<string>();
            List<string> currentActionAttributes = new();
            foreach (ZMAT_ACTIONITEMS item in ZMAT_ACTIONITEMS)
            {
                currentActionAttributes.Clear();
                Type arrayType = item.GetType();
                System.Reflection.PropertyInfo[] properties = arrayType.GetProperties();
                currentActionAttributes.AddRange(properties.Select(property => property.Name));
                if (currentActionAttributes.Count == noOfMandatoryXMLAttribute)
                {


                    for (int i = 0; i < noOfMandatoryXMLAttribute; i++)
                    {
                        if (currentActionAttributes[i] != actionAttributesSeq[i])
                        {
                            Console.WriteLine("First missed Attribute is:" + actionAttributesSeq[i] +
                                              " for action number:" + item.ACTIONNUMBER);
                            return false;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Mandatory attributes are more than expected for Action number " + item.ACTIONNUMBER);

                    return false;
                }
            }
            if (ZMAT_ACTIONITEMS.Length > 0)
            {
                Console.WriteLine("Mandatory attributes are present in all XML actions");
                await Task.CompletedTask;
                return true;
            }
            return false;
        }

        private static UoSProductInfo GetFirstProductsInfoHavingUoS(string unitOfSalesName)
        {
            UoSProductInfo firstProductInfo = new();
            Product prodHavingRequiredUoS = UpdatedJsonPayload.Data.Products.FirstOrDefault(p => p.InUnitsOfSale.Contains(unitOfSalesName));

            if (prodHavingRequiredUoS != null)
            {
                firstProductInfo.ProductType = prodHavingRequiredUoS.ProductType[4..];
                firstProductInfo.Agency = prodHavingRequiredUoS.Agency;
                firstProductInfo.ProviderCode = prodHavingRequiredUoS.ProviderCode;
                firstProductInfo.Title = prodHavingRequiredUoS.Title;
            }
            else
            {
                Console.WriteLine(unitOfSalesName + " not found in any Product's inUnitOfSale");
            }
            return firstProductInfo;
        }

        private static ProductUoSInfo GetUoSInfo(string productName)
        {
            ProductUoSInfo uoSInfo = new();
            foreach (UnitOfSale uos in JsonPayload.Data.UnitsOfSales)
            {
                if (productName.Equals(uos.UnitName))
                {
                    uoSInfo.UnitType = uos.UnitType;
                    uoSInfo.UnitSize = uos.UnitSize;
                    uoSInfo.Title = uos.Title;
                    uoSInfo.UnitOfSaleType = uos.UnitOfSaleType;
                }
            }
            return uoSInfo;
        }

        private static UoSProductInfo GetProductInfo(string products)
        {
            UoSProductInfo productInfo = new();
            foreach (Product product in JsonPayload.Data.Products)
            {
                if (products.Equals(product.ProductName))
                {
                    productInfo.ProductType = product.ProductType[4..];
                    productInfo.Agency = product.Agency;
                    productInfo.ProviderCode = product.ProviderCode;
                    productInfo.Title = product.Title;
                }
            }
            return productInfo;
        }

        public static string GetRequiredXmlText(string generatedXmlFilePath, string tagName)
        {
            XmlDocument xDoc = new();
            xDoc.LoadXml(File.ReadAllText(generatedXmlFilePath));
            XmlNode node = xDoc.SelectSingleNode("//" + tagName);
            return node.InnerText;
        }

        public static bool VerifyOrderOfActions(JsonPayloadHelper jsonPayload, string generatedXmlFilePath)
        {
            bool areEqual = GetFinalActionsListFromJson(ListFromJson).SequenceEqual(CurateListOfActionsFromXmlFile(generatedXmlFilePath));
            if (areEqual)
            {
                Console.WriteLine("XML has correct action sequence");
                return true;
            }

            Console.WriteLine("XML has incorrect action sequence");
            return false;
        }

        public static bool VerifyInitialXmlHeaders(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {
            bool isNoOfActionsMatching = VerifyNoOfActionsHeader(jsonPayload, generatedXMLFilePath);
            bool isRecTimeMatching = VerifyRECTIMEHeader(jsonPayload, generatedXMLFilePath);
            bool isCorrIdMatching = VerifyCORRIDHeader(jsonPayload, generatedXMLFilePath);
            bool isOrgMatching = VerifyORGHeader(jsonPayload, generatedXMLFilePath);

            if (isRecTimeMatching && isNoOfActionsMatching && isCorrIdMatching && isOrgMatching)
            {
                Console.WriteLine("XML headers are correct");
                return true;
            }

            Console.WriteLine("XML headers are incorrect");
            return false;
        }

        public static bool VerifyRECTIMEHeader(JsonPayloadHelper jsonPayload, string generatedXmlFilePath)
        {
            string time = jsonPayload.Time;
            DateTime dt = DateTime.Parse(time);
            string timeFromJson = dt.ToString("yyyyMMdd");

            string timeFromXml = GetRequiredXmlText(generatedXmlFilePath, "RECDATE");

            if (timeFromJson == timeFromXml)
                return true;
            return false;
        }

        public static bool VerifyNoOfActionsHeader(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {
            int totalNumberOfActions = CalculateTotalNumberOfActions(jsonPayload);
            int noOfActions = int.Parse(GetRequiredXmlText(generatedXMLFilePath, "NOOFACTIONS"));

            if (totalNumberOfActions == noOfActions)
            {
                Console.WriteLine("XML has correct number of actions");
                return true;
            }

            Console.WriteLine("XML has incorrect number of actions");
            return false;
        }

        public static bool VerifyCORRIDHeader(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {
            string correlationId = jsonPayload.Data.correlationId;
            string corrId = GetRequiredXmlText(generatedXMLFilePath, "CORRID");

            if (correlationId == corrId)
                return true;
            return false;
        }

        public static bool VerifyORGHeader(JsonPayloadHelper jsonPayload, string generatedXmlFilePath)
        {
            string orgValueFromXMl = GetRequiredXmlText(generatedXmlFilePath, "ORG");

            if (orgValueFromXMl == "UKHO")
                return true;

            Console.WriteLine("ORG Header failed to match");
            return false;
        }

        public static List<string> CurateListOfActionsFromXmlFile(string downloadedXmlFilePath)
        {
            ActionsListFromXml.Clear();
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(File.ReadAllText(downloadedXmlFilePath));
            XmlNodeList nodeList = xDoc.SelectNodes("//ACTION");

            foreach (XmlNode node in nodeList)
            {
                ActionsListFromXml.Add(node.InnerText);
            }
            return ActionsListFromXml;
        }

        public static void UpdateActionList(int n, string actionMessage)
        {
            for (int i = 0; i < n; i++)
            {
                ListFromJson.Add(actionMessage);
            }
            ListFromJson.Sort();
        }

        public static List<string> GetFinalActionsListFromJson(List<string> actionsList)
        {
            for (int i = 0; i < actionsList.Count; i++)
            {
                actionsList[i] = actionsList[i].Substring(4);
            }
            return actionsList;
        }

        // ====== Calculation Logic Starts ======

        public static int CalculateTotalNumberOfActions(JsonPayloadHelper jsonPayload)
        {
            int totalNumberOfActions = 0;
            ListFromJson.Clear();
            totalNumberOfActions = CalculateNewCellCount(jsonPayload)
                                 + CalculateNewUnitOfSalesCount(jsonPayload)
                                 + CalculateAssignCellToUoSActionCount(jsonPayload)
                                 + CalculateReplaceCellActionCount(jsonPayload)
                                 + CalculateAdditionalCoverageCellActionCount(jsonPayload)
                                 + CalculateChangeEncCellActionCount(jsonPayload)
                                 + CalculateChangeUoSActionCount(jsonPayload)
                                 + CalculateRemoveCellFromUoSActionCount(jsonPayload)
                                 + CalculateUpdateEncCellEditionUpdateNumber(jsonPayload)
                                 + CalculateUpdateEncCellEditionUpdateNumberForSuspendedStatus(jsonPayload)
                                 + CalculateCancelledCellCount(jsonPayload)
                                 + CalculateCancelUnitOfSalesActionCount(jsonPayload);

            Console.WriteLine("Total No. of Actions = " + totalNumberOfActions);
            return totalNumberOfActions;
        }

        public static int CalculateNewCellCount(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.Products.Count(product => product.Status.IsNewCell == true);
            if (count > 0)
            {
                UpdateActionList(count, "1.  CREATE ENC CELL");
                Console.WriteLine("Total no. of Create ENC Cell: " + count);
            }
            return count;
        }

        public static int CalculateNewUnitOfSalesCount(JsonPayloadHelper jsonPayload)
        {
            int newUoSCount = jsonPayload.Data.UnitsOfSales.Count(unitOfSale => unitOfSale.IsNewUnitOfSale == true);
            if (newUoSCount > 0)
            {
                UpdateActionList(newUoSCount, "2.  CREATE AVCS UNIT OF SALE");
                Console.WriteLine("Total no. of Create AVCS Unit of Sale: " + newUoSCount);
            }
            return newUoSCount;
        }

        public static int CalculateAssignCellToUoSActionCount(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.UnitsOfSales.Aggregate(0, (current, unitOfSale) => current + unitOfSale.CompositionChanges.AddProducts.Count);
            if (count > 0)
            {
                UpdateActionList(count, "3.  ASSIGN CELL TO AVCS UNIT OF SALE");
                Console.WriteLine("Total no. of Assign Cell to AVCS UoS: " + count);
            }
            return count;
        }

        public static int CalculateReplaceCellActionCount(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.Products.Where(product => product.Status.IsNewCell == false && ((product.ReplacedBy.Count) > 0)).Aggregate(0, (current, product) => current + product.ReplacedBy.Count);
            if (count > 0)
            {
                UpdateActionList(count, "4.  REPLACED WITH ENC CELL");
                Console.WriteLine("Total no. of ReplaceD With ENC Cell: " + count);
            }

            return count;
        }

        public static int CalculateAdditionalCoverageCellActionCount(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.Products.Where(product => (product.AdditionalCoverage.Count) > 0).Sum(product => product.AdditionalCoverage.Count);

            if (count <= 0)
                return count;

            UpdateActionList(count, "5.  ADDITIONAL COVERAGE ENC CELL");
            Console.WriteLine("Total no. of Additional coverage ENC Cell: " + count);

            return count;
        }

        public static int CalculateChangeEncCellActionCount(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.Products.Count(product => product.ContentChange == false);
            if (count > 0)
            {
                UpdateActionList(count, "6.  CHANGE ENC CELL");
                Console.WriteLine("Total No. of Change ENC Cell: " + count);
            }
            return count;
        }

        public static int CalculateChangeUoSActionCount(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.Products.Where(product => product.ContentChange == false).Aggregate(0, (current, product) => current + product.InUnitsOfSale.Count);
            if (count > 0)
            {
                UpdateActionList(count, "7.  CHANGE AVCS UNIT OF SALE");
                Console.WriteLine("Total No. of Change AVCS UoS: " + count);
            }
            return count;
        }

        public static int CalculateUpdateEncCellEditionUpdateNumber(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.Products.Count(product => product.ContentChange == true && product.Status.IsNewCell == false && (product.Status.StatusName == "Update" || product.Status.StatusName == "New Edition" || product.Status.StatusName == "Re-issue"));
            if (count > 0)
            {
                UpdateActionList(count, "8.  UPDATE ENC CELL EDITION UPDATE NUMBER");
                Console.WriteLine("Total no. of ENC Cell Edition Update Number: " + count);
            }
            return count;
        }

        public static int CalculateUpdateEncCellEditionUpdateNumberForSuspendedStatus(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.Products.Count(product => product.Status.StatusName == "Suspended");
            if (count > 0)
            {
                UpdateActionList(count, "8.  UPDATE ENC CELL EDITION UPDATE NUMBER");
                Console.WriteLine("Total no. of ENC Cell Edition Update Number: " + count);
            }
            return count;
        }

        public static int CalculateRemoveCellFromUoSActionCount(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.UnitsOfSales.Where(unitOfSale => unitOfSale.CompositionChanges.RemoveProducts.Count > 0).Aggregate(0, (current, unitOfSale) => current + unitOfSale.CompositionChanges.RemoveProducts.Count);
            if (count > 0)
            {
                UpdateActionList(count, "9.  REMOVE ENC CELL FROM AVCS UNIT OF SALE");
                Console.WriteLine("Total no. of Remove Cell from UoS: " + count);
            }
            return count;
        }

        public static int CalculateCancelledCellCount(JsonPayloadHelper jsonPayload)
        {
            int cancelledCellCount = jsonPayload.Data.Products.Count(product => product.Status.StatusName == "Cancellation Update");
            if (cancelledCellCount > 0)
            {
                UpdateActionList(cancelledCellCount, "91. CANCEL ENC CELL");
                Console.WriteLine("Total No. of Cancel ENC Cell: " + cancelledCellCount);
            }
            return cancelledCellCount;
        }

        public static int CalculateCancelUnitOfSalesActionCount(JsonPayloadHelper jsonPayload)
        {
            int cancelledUoSCount = jsonPayload.Data.UnitsOfSales.Count(unitOfSale => unitOfSale.Status == "NotForSale");
            if (cancelledUoSCount > 0)
            {
                UpdateActionList(cancelledUoSCount, "92. CANCEL AVCS UNIT OF SALE");
                Console.WriteLine("Total No. of Cancel AVCS UoS: " + cancelledUoSCount);
            }
            return cancelledUoSCount;
        }

        public static string GenerateRandomCorrelationId()
        {
            Guid guid = Guid.NewGuid();
            string randomCorrId = guid.ToString("N").Substring(0, 21);
            randomCorrId = randomCorrId.Insert(5, "-");
            randomCorrId = randomCorrId.Insert(11, "-");
            randomCorrId = randomCorrId.Insert(16, "-");
            var currentTimeStamp = DateTime.Now.ToString("yyyyMMdd");
            randomCorrId = "ft-" + currentTimeStamp + "-" + randomCorrId;
            Console.WriteLine("Generated CorrelationId = " + randomCorrId);
            return randomCorrId;
        }

        public static string UpdateTimeAndCorrIdField(string requestBody, string generatedCorrelationId)
        {
            var currentTimeStamp = DateTime.Now.ToString("yyyy-MM-dd");
            JObject jsonObj = JObject.Parse(requestBody);
            jsonObj["time"] = currentTimeStamp;
            jsonObj["data"]["correlationId"] = generatedCorrelationId;
            return jsonObj.ToString();
        }

        private static string GetUoSName(string productName)
        {
            Product prod = UpdatedJsonPayload.Data.Products.FirstOrDefault(p => p.ProductName == productName);
            if (prod != null)
            {
                List<string> inUoS = prod.InUnitsOfSale;
                //this will return object of 
                var matchingUosItems = UpdatedJsonPayload.Data.UnitsOfSales
                                        .Where(uos => uos.UnitOfSaleType == "unit" && inUoS.Contains(uos.UnitName))
                                        .ToList();

                if (matchingUosItems.Count > 1)
                {
                    UnitOfSale uosObj = matchingUosItems
                                        .FirstOrDefault(x => x.CompositionChanges.AddProducts.Contains(productName));

                    if (uosObj != null)
                    {
                        string name = uosObj.UnitName;
                        return name;
                    }
                    Console.WriteLine("Item not found");
                    return "";
                }
                return matchingUosItems.FirstOrDefault()?.UnitName;
            }
            Console.WriteLine("Product object is null");
            return null;
        }

        public static string UpdatePermitField(string requestBody, string permitState)
        {
            JObject jsonObj = JObject.Parse(requestBody);
            if (permitState.Contains("Same"))
            {
                var products = jsonObj["data"]["products"];
                foreach (var product in products)
                {
                    Assert.That(Config.TestConfig.PermitWithSameKey.Permit, Is.Not.EqualTo(""), "Permit String is empty");
                    product["permit"] = Config.TestConfig.PermitWithSameKey.Permit;
                }
            }
            else if (permitState.Contains("Different"))
            {
                var products = jsonObj["data"]["products"];
                foreach (var product in products)
                {
                    Assert.That(Config.TestConfig.PermitWithDifferentKey.Permit, Is.Not.EqualTo(""), "Permit String is empty");
                    product["permit"] = Config.TestConfig.PermitWithDifferentKey.Permit;
                }
            }
            else
            {
                var products = jsonObj["data"]["products"];
                foreach (var product in products)
                {
                    product["permit"] = "permitString";
                }
            }
            return jsonObj.ToString();
        }
    }
}
