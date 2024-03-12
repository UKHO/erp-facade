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
        private static int s_actionCounter;
        private static readonly List<string> s_attrNotMatched = new();
        private static List<string> s_changeAvcsUoS = new();
        private static readonly Dictionary<string, List<string>> s_changeEncCell = new();
        private static JsonPayloadHelper JsonPayload { get; set; }
        private static JsonPayloadHelper UpdatedJsonPayload { get; set; }
        public static List<string> ListFromJson = new();
        public static List<string> ActionsListFromXml = new();
        private static readonly string s_weekNoTag = Config.TestConfig.WeekNoTag;
        private static readonly string s_validFromTag = Config.TestConfig.ValidFromTag;

        public static async Task<bool> CheckXmlAttributes(JsonPayloadHelper jsonPayload, string xmlFilePath, string updatedRequestBody, string correctionTag, string permitState)
        {
            SapXmlHelper.JsonPayload = jsonPayload;
            UpdatedJsonPayload = JsonConvert.DeserializeObject<JsonPayloadHelper>(updatedRequestBody);

            XmlDocument xmlDoc = new XmlDocument();
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

            s_actionCounter = 1;
            s_changeEncCell.Clear();
            foreach (ZMAT_ACTIONITEMS item in result.IM_MATINFO.ACTIONITEMS)
            {
                if (item.ACTION == "CREATE ENC CELL")
                    Assert.That(VerifyCreateEncCell(item.CHILDCELL, item, correctionTag, permitState));
                else if (item.ACTION == "CREATE AVCS UNIT OF SALE")
                    Assert.That(VerifyCreateAVCSUnitOfSale(item.PRODUCTNAME, item, correctionTag));
                else if (item.ACTION == "ASSIGN CELL TO AVCS UNIT OF SALE")
                    Assert.That(VerifyAssignCellToAVCSUnitOfSale(item.CHILDCELL, item.PRODUCTNAME, item, correctionTag));
                else if (item.ACTION == "REPLACED WITH ENC CELL")
                    Assert.That(VerifyReplaceWithEncCell(item.CHILDCELL, item.REPLACEDBY, item) ?? false);
                else if (item.ACTION == "REMOVE ENC CELL FROM AVCS UNIT OF SALE")
                    Assert.That(VerifyRemoveENCCellFromAVCSUnitOFSale(item.CHILDCELL, item.PRODUCTNAME, item) ?? false);
                else if (item.ACTION == "CANCEL ENC CELL")
                    Assert.That(VerifyCancelEncCell(item.CHILDCELL, item.PRODUCTNAME, item) ?? false);
                else if (item.ACTION == "CANCEL AVCS UNIT OF SALE")
                    Assert.That(VerifyCancelToAVCSUnitOfSale(item.PRODUCTNAME, item) ?? false);
                else if (item.ACTION == "CHANGE ENC CELL")
                    Assert.That(VerifyChangeEncCell(item.CHILDCELL, item) ?? false);
                else if (item.ACTION == "CHANGE AVCS UNIT OF SALE")
                    Assert.That(VerifyChangeAVCSUnitOfSale(item.PRODUCTNAME, item) ?? false);
                else if (item.ACTION == "UPDATE ENC CELL EDITION UPDATE NUMBER")
                    Assert.That(VerifyUpdateAVCSUnitOfSale(item.CHILDCELL, item, permitState) ?? false);
                s_actionCounter++;
            }

            Console.WriteLine("Total verified Actions:" + --s_actionCounter);
            await Task.CompletedTask;
            Console.WriteLine("XML has correct data");
            return true;
        }

        private static bool? VerifyChangeAVCSUnitOfSale(string productName, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + s_actionCounter + ".UnitOfSale:" + productName);
            foreach (KeyValuePair<string, List<string>> ele2 in s_changeEncCell)
            {
                s_changeAvcsUoS = ele2.Value;

                if (s_changeAvcsUoS.Contains(productName))
                {
                    s_attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(s_actionCounter.ToString()))
                        s_attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        s_attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(GetProductInfo(ele2.Key).ProductType))
                        s_attrNotMatched.Add(nameof(item.PRODTYPE));
                    if (!item.AGENCY.Equals((GetProductInfo(ele2.Key)).Agency))
                        s_attrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(GetProductInfo(ele2.Key).ProviderCode))
                        s_attrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(GetUoSInfo(productName).UnitSize))
                        s_attrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals((GetUoSInfo(productName)).Title))
                        s_attrNotMatched.Add(nameof(item.TITLE));
                    if (!item.UNITTYPE.Equals(GetUoSInfo(productName).UnitType))
                        s_attrNotMatched.Add(nameof(item.UNITTYPE));
                    //Checking blanks
                    List<string> blankFieldNames = new List<string> { "CANCELLED", "REPLACEDBY", "EDITIONNO", "UPDATENO", "ActiveKey", "NextKey" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (s_attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CHANGE AVCS UNIT OF SALE Action's Data is correct");
                        int valueIndex = ele2.Value.IndexOf(productName);
                        s_changeAvcsUoS[valueIndex] = s_changeAvcsUoS[valueIndex].Replace(productName, "skip");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CHANGE AVCS UNIT OF SALE Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in s_attrNotMatched)
                        {
                            Console.WriteLine(attribute);
                        }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }

        private static bool? VerifyUpdateAVCSUnitOfSale(string childCell, ZMAT_ACTIONITEMS item, string permitState)
        {
            Console.WriteLine("Action#:" + s_actionCounter + ".Childcell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) &&
                    (product.Status.StatusName.Contains("Update") || product.Status.StatusName.Contains("New Edition") || product.Status.StatusName.Contains("Re-issue")) &&
                    (!product.Status.IsNewCell) &&
                    (product.ContentChange))
                {
                    s_attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(s_actionCounter.ToString()))
                        s_attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        s_attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        s_attrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        s_attrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.AGENCY.Equals(product.Agency))
                        s_attrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(product.ProviderCode))
                        s_attrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(product.Size))
                        s_attrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(product.Title))
                        s_attrNotMatched.Add(nameof(item.TITLE));
                    if (!item.EDITIONNO.Equals(product.EditionNumber))
                        s_attrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(product.UpdateNumber))
                        s_attrNotMatched.Add(nameof(item.UPDATENO));
                    List<string> blankFieldNames = new List<string> { "CANCELLED", "REPLACEDBY", "UNITTYPE" };
                    if (product.Status.StatusName.Contains("New Edition"))
                    {
                        Assert.That(VerifyDecryptedPermit(item.CHILDCELL, item, permitState));
                    }
                    else if (product.Status.StatusName.Contains("Update") || product.Status.StatusName.Contains("Re-issue"))
                    {
                        blankFieldNames.Add("ActiveKey");
                        blankFieldNames.Add("NextKey");
                    }
                    //Checking blanks
                    VerifyBlankFields(item, blankFieldNames);

                    if (s_attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in s_attrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
                else if ((childCell == product.ProductName) && (product.Status.StatusName.Contains("Suspended")))
                {
                    s_attrNotMatched.Clear();
                    Console.WriteLine("The UoS name for " + childCell + " calculated is: " + GetUoSName(childCell));
                    if (!item.ACTIONNUMBER.Equals(s_actionCounter.ToString()))
                        s_attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        s_attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        s_attrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        s_attrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.AGENCY.Equals(product.Agency))
                        s_attrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(product.ProviderCode))
                        s_attrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(product.Size))
                        s_attrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(product.Title))
                        s_attrNotMatched.Add(nameof(item.TITLE));
                    if (!item.EDITIONNO.Equals(product.EditionNumber))
                        s_attrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(product.UpdateNumber))
                        s_attrNotMatched.Add(nameof(item.UPDATENO));
                    //Checking blanks
                    List<string> blankFieldNames = new List<string> { "CANCELLED", "REPLACEDBY", "UNITTYPE" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (s_attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in s_attrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool? VerifyChangeEncCell(string childCell, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + s_actionCounter + ".Childcell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (!product.ContentChange))
                {
                    s_attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(s_actionCounter.ToString()))
                        s_attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        s_attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        s_attrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        s_attrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.AGENCY.Equals(product.Agency))
                        s_attrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(product.ProviderCode))
                        s_attrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(product.Size))
                        s_attrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(product.Title))
                        s_attrNotMatched.Add(nameof(item.TITLE));
                    if (!item.EDITIONNO.Equals(product.EditionNumber))
                        s_attrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(product.UpdateNumber))
                        s_attrNotMatched.Add(nameof(item.UPDATENO));
                    //Checking blanks
                    List<string> blankFieldNames = new List<string> { "CANCELLED", "REPLACEDBY", "UNITTYPE", "ActiveKey", "NextKey" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (s_attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CHANGE ENC CELL Action's Data is correct");
                        s_changeEncCell.Add(childCell, product.InUnitsOfSale);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CHANGE ENC CELL Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in s_attrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool? VerifyCancelToAVCSUnitOfSale(string productName, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + s_actionCounter + ".UnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                if ((productName == unitOfSale.UnitName) && (unitOfSale.Status.Equals("NotForSale")))
                {
                    s_attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(s_actionCounter.ToString()))
                        s_attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        s_attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals((GetProductInfo(unitOfSale.CompositionChanges.RemoveProducts)).ProductType))
                        s_attrNotMatched.Add(nameof(item.PRODTYPE));

                    //Checking blanks
                    List<string> blankFieldNames = new List<string> { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE", "ActiveKey", "NextKey" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (s_attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CANCEL AVCS UNIT OF SALE Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CANCEL AVCS UNIT OF SALE Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in s_attrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }

        private static bool? VerifyCancelEncCell(string childCell, string productName, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + s_actionCounter + ".ENC Cell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.Status.StatusName.Equals("Cancellation Update")) && (product.InUnitsOfSale.Contains(productName)))
                {
                    s_attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(s_actionCounter.ToString()))
                        s_attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        s_attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        s_attrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        s_attrNotMatched.Add(nameof(item.PRODUCTNAME));

                    //Checking blanks
                    List<string> blankFieldNames = new List<string> { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE", "ActiveKey", "NextKey" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (s_attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CANCEL ENC CELL Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CANCEL ENC CELL Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in s_attrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool? VerifyRemoveENCCellFromAVCSUnitOFSale(string childCell, string productName, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + s_actionCounter + ".AVCSUnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                List<string> pdts = unitOfSale.CompositionChanges.RemoveProducts;
                foreach (string pdt in pdts)
                {
                    if ((childCell == pdt) && (productName == unitOfSale.UnitName))
                    {
                        s_attrNotMatched.Clear();
                        if (!item.ACTIONNUMBER.Equals(s_actionCounter.ToString()))
                            s_attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                        if (!item.PRODUCT.Equals("AVCS UNIT"))
                            s_attrNotMatched.Add(nameof(item.PRODUCT));
                        if (!item.PRODTYPE.Equals((GetProductInfo(unitOfSale.CompositionChanges.RemoveProducts)).ProductType))
                            s_attrNotMatched.Add(nameof(item.PRODTYPE));
                        //xmlAttributes[4] & [5] are skipped as already checked
                        //Checking blanks
                        List<string> blankFieldNames = new List<string> { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE", "ActiveKey", "NextKey" };
                        VerifyBlankFields(item, blankFieldNames);

                        if (s_attrNotMatched.Count == 0)
                        {
                            Console.WriteLine("REMOVE ENC CELL FROM AVCS UNIT OF SALE Action's Data is correct");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("REMOVE ENC CELL FROM AVCS UNIT OF SALE Action's Data is incorrect");
                            Console.WriteLine("Not matching attributes are:");
                            foreach (string attribute in s_attrNotMatched)
                            { Console.WriteLine(attribute); }
                            return false;
                        }
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }

        private static bool? VerifyReplaceWithEncCell(string childCell, string replaceBy, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + s_actionCounter + ".ENC Cell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.ReplacedBy.Contains(replaceBy)))
                {
                    s_attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(s_actionCounter.ToString()))
                        s_attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        s_attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        s_attrNotMatched.Add(nameof(item.PRODTYPE));
                    //if (!product.InUnitsOfSale.Contains(item.PRODUCTNAME))
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        s_attrNotMatched.Add(nameof(item.PRODUCTNAME));
                    //Checking blanks
                    List<string> blankFieldNames = new List<string> { "CANCELLED", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE", "ActiveKey", "NextKey" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (s_attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("REPLACED WITH ENC CELL Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("REPLACED WITH ENC CELL Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in s_attrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool VerifyAssignCellToAVCSUnitOfSale(string childCell, string productName, ZMAT_ACTIONITEMS item, string correctionTag)
        {
            Console.WriteLine("Action#:" + s_actionCounter + ".AVCSUnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                List<string> pdts = unitOfSale.CompositionChanges.AddProducts;
                foreach (string pdt in pdts)
                {
                    if ((childCell == pdt) && (productName == unitOfSale.UnitName))
                    {
                        s_attrNotMatched.Clear();
                        if (!item.ACTIONNUMBER.Equals(s_actionCounter.ToString()))
                            s_attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                        if (!item.PRODUCT.Equals("AVCS UNIT"))
                            s_attrNotMatched.Add(nameof(item.PRODUCT));
                        if (!item.PRODTYPE.Equals((GetProductInfo(unitOfSale.CompositionChanges.AddProducts)).ProductType))
                            s_attrNotMatched.Add(nameof(item.PRODTYPE));
                        //xmlAttributes[4] & [5] are skipped as already checked
                        //Checking blanks
                        if (!item.WEEKNO.Equals(s_weekNoTag))
                            s_attrNotMatched.Add(nameof(item.WEEKNO));
                        if (!item.VALIDFROM.Equals(s_validFromTag))
                            s_attrNotMatched.Add(nameof(item.VALIDFROM));
                        if (!item.CORRECTION.Equals(correctionTag))
                            s_attrNotMatched.Add(nameof(item.CORRECTION));
                        List<string> blankFieldNames = new List<string> { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE", "ActiveKey", "NextKey" };
                        VerifyBlankFields(item, blankFieldNames);

                        if (s_attrNotMatched.Count == 0)
                        {
                            Console.WriteLine("ASSIGN CELL TO AVCS UNIT OF SALE Action's Data is correct");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("ASSIGN CELL TO AVCS UNIT OF SALE Action's Data is incorrect");
                            Console.WriteLine("Not matching attributes are:");
                            foreach (string attribute in s_attrNotMatched)
                            { Console.WriteLine(attribute); }
                            return false;
                        }
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Product/Unit of Sale.");
            return false;
        }

        private static bool VerifyCreateAVCSUnitOfSale(string productName, ZMAT_ACTIONITEMS item, string correctionTag)
        {
            Console.WriteLine("Action#:" + s_actionCounter + ".UnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                if ((productName == unitOfSale.UnitName) && (unitOfSale.IsNewUnitOfSale))
                {
                    s_attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(s_actionCounter.ToString()))
                        s_attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        s_attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals((GetFirstProductsInfoHavingUoS(productName)).ProductType))
                        s_attrNotMatched.Add(nameof(item.PRODTYPE));
                    if (!item.AGENCY.Equals((GetFirstProductsInfoHavingUoS(productName)).Agency))
                        s_attrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals((GetFirstProductsInfoHavingUoS(productName)).ProviderCode))
                        s_attrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(unitOfSale.UnitSize))
                        s_attrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(unitOfSale.Title))
                        s_attrNotMatched.Add(nameof(item.TITLE));
                    if (!item.UNITTYPE.Equals(unitOfSale.UnitType))
                        s_attrNotMatched.Add(nameof(item.UNITTYPE));
                    if (!item.WEEKNO.Equals(s_weekNoTag))
                        s_attrNotMatched.Add(nameof(item.WEEKNO));
                    if (!item.VALIDFROM.Equals(s_validFromTag))
                        s_attrNotMatched.Add(nameof(item.VALIDFROM));
                    if (!item.CORRECTION.Equals(correctionTag))
                        s_attrNotMatched.Add(nameof(item.CORRECTION));

                    //Checking blanks
                    List<string> blankFieldNames = new List<string> { "CANCELLED", "REPLACEDBY", "EDITIONNO", "UPDATENO", "ActiveKey", "NextKey" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (s_attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CREATE AVCS UNIT OF SALE Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CREATE AVCS UNIT OF SALE Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in s_attrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }

        private static bool VerifyCreateEncCell(string childCell, ZMAT_ACTIONITEMS item, string correctionTag, string permitState)
        {
            Console.WriteLine("Action#:" + s_actionCounter + ".Childcell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.Status.IsNewCell))
                {
                    s_attrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(s_actionCounter.ToString()))
                        s_attrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        s_attrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        s_attrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        s_attrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.AGENCY.Equals(product.Agency))
                        s_attrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(product.ProviderCode))
                        s_attrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(product.Size))
                        s_attrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(product.Title))
                        s_attrNotMatched.Add(nameof(item.TITLE));
                    if (!item.EDITIONNO.Equals(product.EditionNumber))
                        s_attrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(product.UpdateNumber))
                        s_attrNotMatched.Add(nameof(item.UPDATENO));
                    if (!item.WEEKNO.Equals(s_weekNoTag))
                        s_attrNotMatched.Add(nameof(item.WEEKNO));
                    if (!item.VALIDFROM.Equals(s_validFromTag))
                        s_attrNotMatched.Add(nameof(item.VALIDFROM));
                    if (!item.CORRECTION.Equals(correctionTag))
                        s_attrNotMatched.Add(nameof(item.CORRECTION));
                    Assert.That(VerifyDecryptedPermit(item.CHILDCELL, item, permitState));
                    //Checking blanks
                    List<string> blankFieldNames = new List<string> { "CANCELLED", "REPLACEDBY", "UNITTYPE" };
                    VerifyBlankFields(item, blankFieldNames);

                    if (s_attrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CREATE ENC CELL Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CREATE ENC CELL Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in s_attrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return true;
        }

        private static bool VerifyDecryptedPermit(string childCell, ZMAT_ACTIONITEMS item, string permitState)
        {
            Console.WriteLine("Action#:" + s_actionCounter + ".Childcell:" + childCell);

            if (permitState.Contains("Same"))
            {

                if (!item.ACTIVEKEY.Equals(Config.TestConfig.PermitWithSameKey.ActiveKey))
                    s_attrNotMatched.Add(nameof(item.ACTIVEKEY));
                if (!item.NEXTKEY.Equals(Config.TestConfig.PermitWithSameKey.NextKey))
                    s_attrNotMatched.Add(nameof(item.NEXTKEY));
            }
            else if (permitState.Contains("Different"))
            {
                if (!item.ACTIVEKEY.Equals(Config.TestConfig.PermitWithDifferentKey.ActiveKey))
                    s_attrNotMatched.Add(nameof(item.ACTIVEKEY));
                if (!item.NEXTKEY.Equals(Config.TestConfig.PermitWithDifferentKey.NextKey))
                    s_attrNotMatched.Add(nameof(item.NEXTKEY));
            }
            return true;
        }

        private static bool VerifyBlankFields(ZMAT_ACTIONITEMS item, List<string> fieldNames)
        {
            bool allBlanks = true;

            foreach (string field in fieldNames)
            {
                if (!typeof(ZMAT_ACTIONITEMS).GetProperty(field).GetValue(item, null).Equals(""))
                    s_attrNotMatched.Add(typeof(ZMAT_ACTIONITEMS).GetProperty(field).Name);
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
                for (int i = 0; i < 15; i++)
                {
                    if (currentActionAttributes[i] != actionAttributesSeq[i])
                    {
                        Console.WriteLine("First missed Attribute is:" + actionAttributesSeq[i] +
                                          " for action number:" + item.ACTIONNUMBER);
                        return false;
                    }
                }
            }
            if (ZMAT_ACTIONITEMS.Length > 0)
            {
                Console.WriteLine("Mandatory atrributes are present in all XML actions");
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
            else
            {
                Console.WriteLine("XML has incorrect action sequence");
                return false;
            }
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
            else
            {
                Console.WriteLine("XML headers are incorrect");
                return false;
            }
        }

        public static bool VerifyRECTIMEHeader(JsonPayloadHelper jsonPayload, string generatedXmlFilePath)
        {
            string time = jsonPayload.Time;
            DateTime dt = DateTime.Parse(time);
            string timeFromJson = dt.ToString("yyyyMMdd");

            string timeFromXml = GetRequiredXmlText(generatedXmlFilePath, "RECDATE");

            if (timeFromJson == timeFromXml)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            else
            {
                Console.WriteLine("XML has incorrect number of actions");
                return false;
            }
        }

        public static bool VerifyCORRIDHeader(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {
            string correlationId = jsonPayload.Data.correlationId;
            string corrId = GetRequiredXmlText(generatedXMLFilePath, "CORRID");

            if (correlationId == corrId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool VerifyORGHeader(JsonPayloadHelper jsonPayload, string generatedXmlFilePath)
        {
            string orgValueFromXMl = GetRequiredXmlText(generatedXmlFilePath, "ORG");

            if (orgValueFromXMl == "UKHO")
            {
                return true;
            }
            else
            {
                Console.WriteLine("ORG Header failed to match");
                return false;
            }
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

        public static int CalculateChangeEncCellActionCount(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.Products.Count(product => product.ContentChange == false);
            if (count > 0)
            {
                UpdateActionList(count, "5.  CHANGE ENC CELL");
                Console.WriteLine("Total No. of Change ENC Cell: " + count);
            }
            return count;
        }

        public static int CalculateChangeUoSActionCount(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.Products.Where(product => product.ContentChange == false).Aggregate(0, (current, product) => current + product.InUnitsOfSale.Count);
            if (count > 0)
            {
                UpdateActionList(count, "6.  CHANGE AVCS UNIT OF SALE");
                Console.WriteLine("Total No. of Change AVCS UoS: " + count);
            }
            return count;
        }

        public static int CalculateUpdateEncCellEditionUpdateNumber(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.Products.Count(product => product.ContentChange == true && product.Status.IsNewCell == false && (product.Status.StatusName == "Update" || product.Status.StatusName == "New Edition" || product.Status.StatusName == "Re-issue"));
            if (count > 0)
            {
                UpdateActionList(count, "7.  UPDATE ENC CELL EDITION UPDATE NUMBER");
                Console.WriteLine("Total no. of ENC Cell Edition Update Number: " + count);
            }
            return count;
        }

        public static int CalculateUpdateEncCellEditionUpdateNumberForSuspendedStatus(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.Products.Count(product => product.Status.StatusName == "Suspended");
            if (count > 0)
            {
                UpdateActionList(count, "7.  UPDATE ENC CELL EDITION UPDATE NUMBER");
                Console.WriteLine("Total no. of ENC Cell Edition Update Number: " + count);
            }
            return count;
        }

        public static int CalculateRemoveCellFromUoSActionCount(JsonPayloadHelper jsonPayload)
        {
            int count = jsonPayload.Data.UnitsOfSales.Where(unitOfSale => unitOfSale.CompositionChanges.RemoveProducts.Count > 0).Aggregate(0, (current, unitOfSale) => current + unitOfSale.CompositionChanges.RemoveProducts.Count);
            if (count > 0)
            {
                UpdateActionList(count, "8.  REMOVE ENC CELL FROM AVCS UNIT OF SALE");
                Console.WriteLine("Total no. of Remove Cell from UoS: " + count);
            }
            return count;
        }

        public static int CalculateCancelledCellCount(JsonPayloadHelper jsonPayload)
        {
            int cancelledCellCount = jsonPayload.Data.Products.Count(product => product.Status.StatusName == "Cancellation Update");
            if (cancelledCellCount > 0)
            {
                UpdateActionList(cancelledCellCount, "9.  CANCEL ENC CELL");
                Console.WriteLine("Total No. of Cancel ENC Cell: " + cancelledCellCount);
            }
            return cancelledCellCount;
        }

        public static int CalculateCancelUnitOfSalesActionCount(JsonPayloadHelper jsonPayload)
        {
            int cancelledUoSCount = jsonPayload.Data.UnitsOfSales.Count(unitOfSale => unitOfSale.Status == "NotForSale");
            if (cancelledUoSCount > 0)
            {
                UpdateActionList(cancelledUoSCount, "99. CANCEL AVCS UNIT OF SALE");
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
                    else
                    {
                        Console.WriteLine("Item not found");
                        return "";
                    }
                }
                else
                {
                    return matchingUosItems.FirstOrDefault().UnitName;
                }
            }
            else
            {
                Console.WriteLine("Product object is null");
                return null;
            }
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
