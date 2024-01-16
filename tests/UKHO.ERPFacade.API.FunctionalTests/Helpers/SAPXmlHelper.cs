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
    public class SAPXmlHelper
    {
        private static int ActionCounter;
        private static readonly List<string> AttrNotMatched = new();
        private static List<string> ChangeAVCSUoS = new();
        private static readonly Dictionary<string, List<string>> ChangeENCCell = new();
        private static JsonPayloadHelper JsonPayload { get; set; }
        private static JsonPayloadHelper UpdatedJsonPayload { get; set; }
        public static List<string> ListFromJson = new();
        public static List<string> ActionsListFromXml = new();

        public static async Task<bool> CheckXMLAttributes(JsonPayloadHelper jsonPayload, string XMLFilePath, string updatedRequestBody)
        {
            SAPXmlHelper.JsonPayload = jsonPayload;
            UpdatedJsonPayload = JsonConvert.DeserializeObject<JsonPayloadHelper>(updatedRequestBody);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(File.ReadAllText(XMLFilePath));

            while (xmlDoc.DocumentElement.Name == "soap:Envelope" || xmlDoc.DocumentElement.Name == "soap:Body")
            {
                string tempXmlString = xmlDoc.DocumentElement.InnerXml;
                xmlDoc.LoadXml(tempXmlString);
            }

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlDoc.InnerXml));
            var reader = new XmlTextReader(ms) { Namespaces = false };
            var serializer = new XmlSerializer(typeof(Z_ADDS_MAT_INFO));
            var result = (Z_ADDS_MAT_INFO)serializer.Deserialize(reader);

            Assert.That(VerifyPresenseOfMandatoryXMLAtrributes(result.IM_MATINFO.ACTIONITEMS).Result);

            ActionCounter = 1;
            ChangeENCCell.Clear();
            foreach (ZMAT_ACTIONITEMS item in result.IM_MATINFO.ACTIONITEMS)
            {
                if (item.ACTION == "CREATE ENC CELL")
                    Assert.That(VerifyCreateENCCell(item.CHILDCELL, item));
                else if (item.ACTION == "CREATE AVCS UNIT OF SALE")
                    Assert.That(VerifyCreateAVCSUnitOfSale(item.PRODUCTNAME, item));
                else if (item.ACTION == "ASSIGN CELL TO AVCS UNIT OF SALE")
                    Assert.That(VerifyAssignCellToAVCSUnitOfSale(item.CHILDCELL, item.PRODUCTNAME, item));
                else if (item.ACTION == "REPLACED WITH ENC CELL")
                    Assert.That(VerifyReplaceWithENCCell(item.CHILDCELL, item.REPLACEDBY, item) ?? false);
                else if (item.ACTION == "REMOVE ENC CELL FROM AVCS UNIT OF SALE")
                    Assert.That(VerifyRemoveENCCellFromAVCSUnitOFSale(item.CHILDCELL, item.PRODUCTNAME, item) ?? false);
                else if (item.ACTION == "CANCEL ENC CELL")
                    Assert.That(VerifyCancelENCCell(item.CHILDCELL, item.PRODUCTNAME, item) ?? false);
                else if (item.ACTION == "CANCEL AVCS UNIT OF SALE")
                    Assert.That(VerifyCancelToAVCSUnitOfSale(item.PRODUCTNAME, item) ?? false);
                else if (item.ACTION == "CHANGE ENC CELL")
                    Assert.That(VerifyChangeENCCell(item.CHILDCELL, item) ?? false);
                else if (item.ACTION == "CHANGE AVCS UNIT OF SALE")
                    Assert.That(VerifyChangeAVCSUnitOfSale(item.PRODUCTNAME, item) ?? false);
                else if (item.ACTION == "UPDATE ENC CELL EDITION UPDATE NUMBER")
                    Assert.That(VerifyUpdateAVCSUnitOfSale(item.CHILDCELL, item) ?? false);
                ActionCounter++;
            }

            Console.WriteLine("Total verified Actions:" + --ActionCounter);
            await Task.CompletedTask;
            Console.WriteLine("XML has correct data");
            return true;
        }

        private static bool? VerifyChangeAVCSUnitOfSale(string productName, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + ActionCounter + ".UnitOfSale:" + productName);
            foreach (KeyValuePair<string, List<string>> ele2 in ChangeENCCell)
            {
                ChangeAVCSUoS = ele2.Value;

                if (ChangeAVCSUoS.Contains(productName))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(ActionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(GetProductInfo(ele2.Key).ProductType))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if (!item.AGENCY.Equals((GetProductInfo(ele2.Key)).Agency))
                        AttrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(GetProductInfo(ele2.Key).ProviderCode))
                        AttrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(GetUoSInfo(productName).UnitSize))
                        AttrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals((GetUoSInfo(productName)).Title))
                        AttrNotMatched.Add(nameof(item.TITLE));
                    if (!item.UNITTYPE.Equals(GetUoSInfo(productName).UnitType))
                        AttrNotMatched.Add(nameof(item.UNITTYPE));
                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "EDITIONNO", "UPDATENO" };
                    VerifyBlankFields(item, fieldNames);

                    if (AttrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CHANGE AVCS UNIT OF SALE Action's Data is correct");
                        var valueindex = ele2.Value.IndexOf(productName);
                        ChangeAVCSUoS[valueindex] = ChangeAVCSUoS[valueindex].Replace(productName, "skip");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CHANGE AVCS UNIT OF SALE Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in AttrNotMatched)
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

        private static bool? VerifyUpdateAVCSUnitOfSale(string childCell, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + ActionCounter + ".Childcell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) &&
                    (product.Status.StatusName.Contains("Update") || product.Status.StatusName.Contains("New Edition") || product.Status.StatusName.Contains("Re-issue")) &&
                    (!product.Status.IsNewCell) &&
                    (product.ContentChange))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(ActionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        AttrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.AGENCY.Equals(product.Agency))
                        AttrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(product.ProviderCode))
                        AttrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(product.Size))
                        AttrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(product.Title))
                        AttrNotMatched.Add(nameof(item.TITLE));
                    if (!item.EDITIONNO.Equals(product.EditionNumber))
                        AttrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(product.UpdateNumber))
                        AttrNotMatched.Add(nameof(item.UPDATENO));
                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "UNITTYPE" };
                    VerifyBlankFields(item, fieldNames);

                    if (AttrNotMatched.Count == 0)
                    {
                        Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in AttrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
                else if ((childCell == product.ProductName) && (product.Status.StatusName.Contains("Suspended")))
                {
                    AttrNotMatched.Clear();
                    Console.WriteLine("The UoS name for " + childCell + " calculated is: " + GetUoSName(childCell));
                    if (!item.ACTIONNUMBER.Equals(ActionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        AttrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.AGENCY.Equals(product.Agency))
                        AttrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(product.ProviderCode))
                        AttrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(product.Size))
                        AttrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(product.Title))
                        AttrNotMatched.Add(nameof(item.TITLE));
                    if (!item.EDITIONNO.Equals(product.EditionNumber))
                        AttrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(product.UpdateNumber))
                        AttrNotMatched.Add(nameof(item.UPDATENO));
                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "UNITTYPE" };
                    VerifyBlankFields(item, fieldNames);

                    if (AttrNotMatched.Count == 0)
                    {
                        Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("UPDATE ENC CELL EDITION UPDATE NUMBER Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in AttrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool? VerifyChangeENCCell(string childCell, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + ActionCounter + ".Childcell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (!product.ContentChange))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(ActionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        AttrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.AGENCY.Equals(product.Agency))
                        AttrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(product.ProviderCode))
                        AttrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(product.Size))
                        AttrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(product.Title))
                        AttrNotMatched.Add(nameof(item.TITLE));
                    if (!item.EDITIONNO.Equals(product.EditionNumber))
                        AttrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(product.UpdateNumber))
                        AttrNotMatched.Add(nameof(item.UPDATENO));
                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "UNITTYPE" };
                    VerifyBlankFields(item, fieldNames);

                    if (AttrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CHANGE ENC CELL Action's Data is correct");
                        ChangeENCCell.Add(childCell, product.InUnitsOfSale);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CHANGE ENC CELL Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in AttrNotMatched)
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
            Console.WriteLine("Action#:" + ActionCounter + ".UnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                if ((productName == unitOfSale.UnitName) && (unitOfSale.Status.Equals("NotForSale")))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(ActionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals((GetProductInfo(unitOfSale.CompositionChanges.RemoveProducts)).ProductType))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));

                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE" };
                    VerifyBlankFields(item, fieldNames);

                    if (AttrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CANCEL AVCS UNIT OF SALE Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CANCEL AVCS UNIT OF SALE Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in AttrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }

        private static bool? VerifyCancelENCCell(string childCell, string productName, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + ActionCounter + ".ENC Cell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.Status.StatusName.Equals("Cancellation Update")) && (product.InUnitsOfSale.Contains(productName)))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(ActionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        AttrNotMatched.Add(nameof(item.PRODUCTNAME));

                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE" };
                    VerifyBlankFields(item, fieldNames);

                    if (AttrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CANCEL ENC CELL Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CANCEL ENC CELL Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in AttrNotMatched)
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
            Console.WriteLine("Action#:" + ActionCounter + ".AVCSUnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                List<string> pdts = unitOfSale.CompositionChanges.RemoveProducts;
                foreach (string pdt in pdts)
                {
                    if ((childCell == pdt) && (productName == unitOfSale.UnitName))
                    {
                        AttrNotMatched.Clear();
                        if (!item.ACTIONNUMBER.Equals(ActionCounter.ToString()))
                            AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                        if (!item.PRODUCT.Equals("AVCS UNIT"))
                            AttrNotMatched.Add(nameof(item.PRODUCT));
                        if (!item.PRODTYPE.Equals((GetProductInfo(unitOfSale.CompositionChanges.RemoveProducts)).ProductType))
                            AttrNotMatched.Add(nameof(item.PRODTYPE));
                        //xmlAttributes[4] & [5] are skipped as already checked
                        //Checking blanks
                        string[] fieldNames = { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE" };
                        VerifyBlankFields(item, fieldNames);

                        if (AttrNotMatched.Count == 0)
                        {
                            Console.WriteLine("REMOVE ENC CELL FROM AVCS UNIT OF SALE Action's Data is correct");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("REMOVE ENC CELL FROM AVCS UNIT OF SALE Action's Data is incorrect");
                            Console.WriteLine("Not matching attributes are:");
                            foreach (string attribute in AttrNotMatched)
                            { Console.WriteLine(attribute); }
                            return false;
                        }
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }

        private static bool? VerifyReplaceWithENCCell(string childCell, string replaceBy, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + ActionCounter + ".ENC Cell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.ReplacedBy.Contains(replaceBy)))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(ActionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    //if (!product.InUnitsOfSale.Contains(item.PRODUCTNAME))
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        AttrNotMatched.Add(nameof(item.PRODUCTNAME));
                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE" };
                    VerifyBlankFields(item, fieldNames);

                    if (AttrNotMatched.Count == 0)
                    {
                        Console.WriteLine("REPLACED WITH ENC CELL Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("REPLACED WITH ENC CELL Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in AttrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool VerifyAssignCellToAVCSUnitOfSale(string childCell, string productName, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + ActionCounter + ".AVCSUnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                List<string> pdts = unitOfSale.CompositionChanges.AddProducts;
                foreach (string pdt in pdts)
                {
                    if ((childCell == pdt) && (productName == unitOfSale.UnitName))
                    {
                        AttrNotMatched.Clear();
                        if (!item.ACTIONNUMBER.Equals(ActionCounter.ToString()))
                            AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                        if (!item.PRODUCT.Equals("AVCS UNIT"))
                            AttrNotMatched.Add(nameof(item.PRODUCT));
                        if (!item.PRODTYPE.Equals((GetProductInfo(unitOfSale.CompositionChanges.AddProducts)).ProductType))
                            AttrNotMatched.Add(nameof(item.PRODTYPE));
                        //xmlAttributes[4] & [5] are skipped as already checked
                        //Checking blanks
                        string[] fieldNames = { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE" };
                        VerifyBlankFields(item, fieldNames);

                        if (AttrNotMatched.Count == 0)
                        {
                            Console.WriteLine("ASSIGN CELL TO AVCS UNIT OF SALE Action's Data is correct");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("ASSIGN CELL TO AVCS UNIT OF SALE Action's Data is incorrect");
                            Console.WriteLine("Not matching attributes are:");
                            foreach (string attribute in AttrNotMatched)
                            { Console.WriteLine(attribute); }
                            return false;
                        }
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Product/Unit of Sale.");
            return false;
        }

        private static bool VerifyCreateAVCSUnitOfSale(string productName, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + ActionCounter + ".UnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in JsonPayload.Data.UnitsOfSales)
            {
                if ((productName == unitOfSale.UnitName) && (unitOfSale.IsNewUnitOfSale))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(ActionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals((GetFirstProductsInfoHavingUoS(productName)).ProductType))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if (!item.AGENCY.Equals((GetFirstProductsInfoHavingUoS(productName)).Agency))
                        AttrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals((GetFirstProductsInfoHavingUoS(productName)).ProviderCode))
                        AttrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(unitOfSale.UnitSize))
                        AttrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(unitOfSale.Title))
                        AttrNotMatched.Add(nameof(item.TITLE));
                    if (!item.UNITTYPE.Equals(unitOfSale.UnitType))
                        AttrNotMatched.Add(nameof(item.UNITTYPE));

                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "EDITIONNO", "UPDATENO" };
                    VerifyBlankFields(item, fieldNames);

                    if (AttrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CREATE AVCS UNIT OF SALE Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CREATE AVCS UNIT OF SALE Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in AttrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }

        private static bool VerifyCreateENCCell(string childCell, ZMAT_ACTIONITEMS item)
        {
            Console.WriteLine("Action#:" + ActionCounter + ".Childcell:" + childCell);
            foreach (Product product in JsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.Status.IsNewCell))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(ActionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!item.PRODUCTNAME.Equals(GetUoSName(childCell))))
                        AttrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.AGENCY.Equals(product.Agency))
                        AttrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(product.ProviderCode))
                        AttrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(product.Size))
                        AttrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(product.Title))
                        AttrNotMatched.Add(nameof(item.TITLE));
                    if (!item.EDITIONNO.Equals(product.EditionNumber))
                        AttrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(product.UpdateNumber))
                        AttrNotMatched.Add(nameof(item.UPDATENO));
                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "UNITTYPE" };
                    VerifyBlankFields(item, fieldNames);

                    if (AttrNotMatched.Count == 0)
                    {
                        Console.WriteLine("CREATE ENC CELL Action's Data is correct");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("CREATE ENC CELL Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in AttrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }
            }
            Console.WriteLine("JSON doesn't have corresponding product.");
            return false;
        }

        private static bool VerifyBlankFields(ZMAT_ACTIONITEMS item, string[] fieldNames)
        {
            bool allBlanks = true;

            foreach (string field in fieldNames)
            {
                if (!typeof(ZMAT_ACTIONITEMS).GetProperty(field).GetValue(item, null).Equals(""))
                    AttrNotMatched.Add(typeof(ZMAT_ACTIONITEMS).GetProperty(field).Name);
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

        public static async Task<bool> VerifyPresenseOfMandatoryXMLAtrributes(ZMAT_ACTIONITEMS[] ZMAT_ACTIONITEMS)
        {
            List<string> ActionAttributesSeq = new List<string>();
            ActionAttributesSeq = Config.TestConfig.XMLActionList.ToList<string>();
            List<string> CurrentActionAttributes = new List<string>();

            foreach (ZMAT_ACTIONITEMS item in ZMAT_ACTIONITEMS)
            {
                CurrentActionAttributes.Clear();
                Type arrayType = item.GetType();
                var properties = arrayType.GetProperties();

                foreach (var property in properties)
                {
                    CurrentActionAttributes.Add(property.Name);
                }


                for (int i = 0; i < 15; i++)
                {
                    if (CurrentActionAttributes[i] != ActionAttributesSeq[i])
                    {
                        Console.WriteLine("First missed Attribute is:" + ActionAttributesSeq[i] + " for action number:" + item.ACTIONNUMBER);
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
            else
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
            ProductUoSInfo UoSInfo = new ProductUoSInfo();
            foreach (UnitOfSale uos in JsonPayload.Data.UnitsOfSales)
            {
                if (productName.Equals(uos.UnitName))
                {
                    UoSInfo.UnitType = uos.UnitType;
                    UoSInfo.UnitSize = uos.UnitSize;
                    UoSInfo.Title = uos.Title;
                    UoSInfo.UnitOfSaleType = uos.UnitOfSaleType;
                }
            }
            return UoSInfo;
        }

        private static UoSProductInfo GetProductInfo(string products)
        {
            UoSProductInfo productInfo = new UoSProductInfo();
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

        public static string GetRequiredXMLText(string generatedXMLFilePath, string tagName)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(File.ReadAllText(generatedXMLFilePath));
            XmlNode node = xDoc.SelectSingleNode("//" + tagName);
            return node.InnerText;
        }

        public static bool VerifyOrderOfActions(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {
            bool areEqual = GetFinalActionsListFromJson(ListFromJson).SequenceEqual(CurateListOfActionsFromXmlFile(generatedXMLFilePath));
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

        public static bool VerifyInitialXMLHeaders(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
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

        public static bool VerifyRECTIMEHeader(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {
            string time = jsonPayload.Time;
            DateTime dt = DateTime.Parse(time);
            string timeFromJSON = dt.ToString("yyyyMMdd");

            string timeFromXML = GetRequiredXMLText(generatedXMLFilePath, "RECDATE");

            if (timeFromJSON == timeFromXML)
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
            int totalNumberofActions = CalculateTotalNumberOfActions(jsonPayload);
            int noofActions = int.Parse(GetRequiredXMLText(generatedXMLFilePath, "NOOFACTIONS"));

            if (totalNumberofActions == noofActions)
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
            string correlationID = jsonPayload.Data.correlationId;
            string corrID = GetRequiredXMLText(generatedXMLFilePath, "CORRID");

            if (correlationID == corrID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool VerifyORGHeader(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {
            string orgValueFromXMl = GetRequiredXMLText(generatedXMLFilePath, "ORG");

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

        public static List<string> CurateListOfActionsFromXmlFile(string downloadedXMLFilePath)
        {
            ActionsListFromXml.Clear();
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(File.ReadAllText(downloadedXMLFilePath));
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
            var obj = jsonPayload;
            int count = 0;

            foreach (Product product in obj.Data.Products)
            {
                if (product.Status.IsNewCell == true)
                {
                    count++;
                }
            }

            if (count > 0)
            {
                UpdateActionList(count, "1.  CREATE ENC CELL");
                Console.WriteLine("Total no. of Create ENC Cell: " + count);
            }

            return count;
        }

        public static int CalculateNewUnitOfSalesCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int newUoSCount = 0;

            foreach (UnitOfSale unitOfSale in obj.Data.UnitsOfSales)
            {
                if (unitOfSale.IsNewUnitOfSale == true)
                {
                    newUoSCount++;
                }
            }

            if (newUoSCount > 0)
            {
                UpdateActionList(newUoSCount, "2.  CREATE AVCS UNIT OF SALE");
                Console.WriteLine("Total no. of Create AVCS Unit of Sale: " + newUoSCount);
            }

            return newUoSCount;
        }

        public static int CalculateAssignCellToUoSActionCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int count = 0;
            foreach (UnitOfSale unitOfSale in obj.Data.UnitsOfSales)
            {
                count = count + unitOfSale.CompositionChanges.AddProducts.Count;
            }

            if (count > 0)
            {
                UpdateActionList(count, "3.  ASSIGN CELL TO AVCS UNIT OF SALE");
                Console.WriteLine("Total no. of Assign Cell to AVCS UoS: " + count);
            }

            return count;
        }

        public static int CalculateReplaceCellActionCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int count = 0;
            foreach (Product product in obj.Data.Products)
            {
                if (product.Status.IsNewCell == false && ((product.ReplacedBy.Count) > 0))
                {
                    count = count + product.ReplacedBy.Count;
                }
            }

            if (count > 0)
            {
                UpdateActionList(count, "4.  REPLACED WITH ENC CELL");
                Console.WriteLine("Total no. of ReplaceD With ENC Cell: " + count);
            }

            return count;
        }

        public static int CalculateChangeEncCellActionCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int count = 0;

            foreach (Product product in obj.Data.Products)
            {
                if (product.ContentChange == false)
                {
                    count++;
                }
            }

            if (count > 0)
            {
                UpdateActionList(count, "5.  CHANGE ENC CELL");
                Console.WriteLine("Total No. of Change ENC Cell: " + count);
            }

            return count;
        }

        public static int CalculateChangeUoSActionCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int count = 0;

            foreach (Product product in obj.Data.Products)
            {
                if (product.ContentChange == false)
                {
                    count = count + product.InUnitsOfSale.Count;
                }
            }

            if (count > 0)
            {
                UpdateActionList(count, "6.  CHANGE AVCS UNIT OF SALE");
                Console.WriteLine("Total No. of Change AVCS UoS: " + count);
            }

            return count;
        }

        public static int CalculateUpdateEncCellEditionUpdateNumber(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int count = 0;
            foreach (Product product in obj.Data.Products)
            {
                if (product.ContentChange == true && product.Status.IsNewCell == false
                    && (product.Status.StatusName == "Update" || product.Status.StatusName == "New Edition" || product.Status.StatusName == "Re-issue"))
                {
                    count++;
                }
            }

            if (count > 0)
            {
                UpdateActionList(count, "7.  UPDATE ENC CELL EDITION UPDATE NUMBER");
                Console.WriteLine("Total no. of ENC Cell Edition Update Number: " + count);
            }

            return count;
        }

        public static int CalculateUpdateEncCellEditionUpdateNumberForSuspendedStatus(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int count = 0;
            foreach (Product product in obj.Data.Products)
            {
                if (product.Status.StatusName == "Suspended")
                {
                    count++;
                }
            }

            if (count > 0)
            {
                UpdateActionList(count, "7.  UPDATE ENC CELL EDITION UPDATE NUMBER");
                Console.WriteLine("Total no. of ENC Cell Edition Update Number: " + count);
            }

            return count;
        }

        public static int CalculateRemoveCellFromUoSActionCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int count = 0;
            foreach (UnitOfSale unitOfSale in obj.Data.UnitsOfSales)
            {
                if (unitOfSale.CompositionChanges.RemoveProducts.Count > 0)
                {
                    count = count + unitOfSale.CompositionChanges.RemoveProducts.Count;
                }
            }

            if (count > 0)
            {
                UpdateActionList(count, "8.  REMOVE ENC CELL FROM AVCS UNIT OF SALE");
                Console.WriteLine("Total no. of Remove Cell from UoS: " + count);
            }

            return count;
        }

        public static int CalculateCancelledCellCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int cancelledCellCount = 0;

            foreach (Product product in obj.Data.Products)
            {
                if (product.Status.StatusName == "Cancellation Update")
                {
                    cancelledCellCount++;
                }
            }

            if (cancelledCellCount > 0)
            {
                UpdateActionList(cancelledCellCount, "9.  CANCEL ENC CELL");
                Console.WriteLine("Total No. of Cancel ENC Cell: " + cancelledCellCount);
            }

            return cancelledCellCount;
        }

        public static int CalculateCancelUnitOfSalesActionCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int cancelledUoSCount = 0;

            foreach (UnitOfSale unitOfSale in obj.Data.UnitsOfSales)
            {
                if (unitOfSale.Status == "NotForSale")
                {
                    cancelledUoSCount++;
                }
            }

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
            string randomCorrID = guid.ToString("N").Substring(0, 21);
            randomCorrID = randomCorrID.Insert(5, "-");
            randomCorrID = randomCorrID.Insert(11, "-");
            randomCorrID = randomCorrID.Insert(16, "-");
            string currentTimeStamp = DateTime.Now.ToString("yyyyMMdd");
            randomCorrID = "ft-" + currentTimeStamp + "-" + randomCorrID;
            Console.WriteLine("Generated CorrelationId = " + randomCorrID);
            return randomCorrID;
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
    }
}
