
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class SAPXmlHelper
    {
        private static JsonPayloadHelper jsonPayloadHelper { get; set; }

        static int actionCounter;

        static readonly List<string> AttrNotMatched = new();
        static List<string> ChangeAVCSUoS = new();
        static readonly Dictionary<string, List<string>> ChangeENCCell = new();
        public static List<string> listFromJson = new();
        public static List<string> actionsListFromXml = new();

        private static JsonPayloadHelper jsonPayload { get; set; }
        private static Z_ADDS_MAT_INFO xmlPayload { get; set; }

        public static async Task<bool> CheckXMLAttributes(JsonPayloadHelper jsonPayload, string XMLFilePath)
        {

            SAPXmlHelper.jsonPayload = jsonPayload;
            XmlDocument xDoc = new XmlDocument();
            //load up the xml from the location 
            xDoc.LoadXml(File.ReadAllText(XMLFilePath));

            var ns = new XmlNamespaceManager(xDoc.NameTable);
            ns.AddNamespace("xsisp", "http://www.w3.org/2001/XMLSchema-instance");
            ns.AddNamespace("xsdsp", "http://www.w3.org/2001/XMLSchema");

            XmlNodeList nodeList = xDoc.SelectNodes("//*[local-name()='IM_MATINFO']/ACTIONITEMS/item", ns);

            XmlSerializer xsw = new XmlSerializer(typeof(Z_ADDS_MAT_INFO));
            FileStream fs = new FileStream(XMLFilePath, FileMode.Open);
            StreamReader stream = new StreamReader(fs, Encoding.UTF8);
            xmlPayload = (Z_ADDS_MAT_INFO)xsw.Deserialize(new XmlTextReader(stream));

            Assert.True(VerifyPresenseOfMandatoryXMLAtrributes(nodeList).Result);

            //verification of action atrribute's value
            actionCounter = 1;
            ChangeENCCell.Clear();

            foreach (Item item in xmlPayload.IM_MATINFO.ACTIONITEMS)

            {

                if (item.ACTION == "CREATE ENC CELL")
                    Assert.True(verifyCreateENCCell(item.CHILDCELL, item));
                else if (item.ACTION == "CREATE AVCS UNIT OF SALE")
                    Assert.True(verifyCreateAVCSUnitOfSale(item.PRODUCTNAME, item));
                else if (item.ACTION == "ASSIGN CELL TO AVCS UNIT OF SALE")
                    Assert.True(verifyAssignCellToAVCSUnitOfSale(item.CHILDCELL, item.PRODUCTNAME, item));
                else if (item.ACTION == "REPLACED WITH ENC CELL")
                    Assert.True(verifyReplaceWithENCCell(item.CHILDCELL, item.REPLACEDBY, item));
                else if (item.ACTION == "REMOVE ENC CELL FROM AVCS UNIT OF SALE")
                    Assert.True(verifyRemoveENCCellFromAVCSUnitOFSale(item.CHILDCELL, item.PRODUCTNAME, item));
                else if (item.ACTION == "CANCEL ENC CELL")
                    Assert.True(verifyCancelENCCell(item.CHILDCELL, item.PRODUCTNAME, item));
                else if (item.ACTION == "CANCEL AVCS UNIT OF SALE")
                    Assert.True(verifyCancelToAVCSUnitOfSale(item.PRODUCTNAME, item));
                else if (item.ACTION == "CANCEL AVCS UNIT OF SALE")
                    Assert.True(verifyCancelToAVCSUnitOfSale(item.PRODUCTNAME, item));
                else if (item.ACTION == "CHANGE ENC CELL")
                    Assert.True(verifyChangeENCCell(item.CHILDCELL, item));
                else if (item.ACTION == "CHANGE AVCS UNIT OF SALE")
                    Assert.True(verifyChangeAVCSUnitOfSale(item.PRODUCTNAME, item));
                else if (item.ACTION == "UPDATE ENC CELL EDITION UPDATE NUMBER")
                    Assert.True(verifyUpdateAVCSUnitOfSale(item.CHILDCELL, item));
                actionCounter++;

            }
            Console.WriteLine("Total verified Actions:" + --actionCounter);
            await Task.CompletedTask;
            Console.WriteLine("XML has correct data");
            return true;
        }

        private static bool? verifyChangeAVCSUnitOfSale(string productName, Item item)
        {
            Console.WriteLine("Action#:" + actionCounter + ".UnitOfSale:" + productName);
            foreach (KeyValuePair<string, List<string>> ele2 in ChangeENCCell)
            {
                ChangeAVCSUoS = ele2.Value;


                if (ChangeAVCSUoS.Contains(productName))
                {

                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(getProductInfo(ele2.Key).ProductType))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    //if (getUoSInfo(productName).UnitOfSaleType.Equals("unit"))
                    if (!item.AGENCY.Equals((getProductInfo(ele2.Key)).Agency))
                        AttrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(getProductInfo(ele2.Key).ProviderCode))
                        AttrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(getUoSInfo(productName).UnitSize))
                        AttrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals((getUoSInfo(productName)).Title))
                        AttrNotMatched.Add(nameof(item.TITLE));
                    if (!item.UNITTYPE.Equals(getUoSInfo(productName).UnitType))
                        AttrNotMatched.Add(nameof(item.UNITTYPE));
                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "EDITIONNO", "UPDATENO" };
                    var v = VerifyBlankFields(item, fieldNames);


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
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }

            }
            Console.WriteLine("JSON doesn't have corresponding Unit of Sale.");
            return false;
        }


        private static bool? verifyUpdateAVCSUnitOfSale(string childCell, Item item)
        {

            Console.WriteLine("Action#:" + actionCounter + ".Childcell:" + childCell);
            foreach (Product product in jsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) &&
                    (product.Status.StatusName.Contains("Update") || product.Status.StatusName.Contains("New Edition") || product.Status.StatusName.Contains("Re-issue")) &&
                    (!product.Status.IsNewCell) &&
                    (product.ContentChanged))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!getUoSInfo(item.PRODUCTNAME).UnitOfSaleType.Contains("unit")))
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
                    var v = VerifyBlankFields(item, fieldNames);


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
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!getUoSInfo(item.PRODUCTNAME).UnitOfSaleType.Contains("unit")))
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
                    var v = VerifyBlankFields(item, fieldNames);


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

        private static bool getUoSUnitType(string pRODUCTNAME)
        {
            throw new NotImplementedException();
        }

        private static bool? verifyChangeENCCell(string childCell, Item item)
        {

            Console.WriteLine("Action#:" + actionCounter + ".Childcell:" + childCell);
            foreach (Product product in jsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (!product.ContentChanged))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!getUoSInfo(item.PRODUCTNAME).UnitOfSaleType.Contains("unit")))
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
                    var v = VerifyBlankFields(item, fieldNames);


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


        private static bool? verifyCancelToAVCSUnitOfSale(string productName, Item item)
        {
            Console.WriteLine("Action#:" + actionCounter + ".UnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in jsonPayload.Data.UnitsOfSales)
            {

                if ((productName == unitOfSale.UnitName) && (unitOfSale.Status.Equals("NotForSale")))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals((getProductInfo(unitOfSale.CompositionChanges.RemoveProducts)).ProductType))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));

                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE" };
                    var v = VerifyBlankFields(item, fieldNames);

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

        private static bool? verifyCancelENCCell(string childCell, string productName, Item item)
        {
            Console.WriteLine("Action#:" + actionCounter + ".ENC Cell:" + childCell);
            foreach (Product product in jsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.Status.StatusName.Equals("Cancellation Update")) && (product.InUnitsOfSale.Contains(productName)))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));

                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE" };
                    var v = VerifyBlankFields(item, fieldNames);


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


        private static bool? verifyRemoveENCCellFromAVCSUnitOFSale(string childCell, string productName, Item item)
        {

            Console.WriteLine("Action#:" + actionCounter + ".AVCSUnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in jsonPayload.Data.UnitsOfSales)
            {
                List<string> pdts = unitOfSale.CompositionChanges.RemoveProducts;
                foreach (string pdt in pdts)
                {
                    if ((childCell == pdt) && (productName == unitOfSale.UnitName))
                    {

                        AttrNotMatched.Clear();
                        if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                            AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                        if (!item.PRODUCT.Equals("AVCS UNIT"))
                            AttrNotMatched.Add(nameof(item.PRODUCT));
                        if (!item.PRODTYPE.Equals((getProductInfo(unitOfSale.CompositionChanges.RemoveProducts)).ProductType))
                            AttrNotMatched.Add(nameof(item.PRODTYPE));
                        //xmlAttributes[4] & [5] are skipped as already checked
                        //Checking blanks
                        string[] fieldNames = { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE" };
                        var v = VerifyBlankFields(item, fieldNames);

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

        private static bool? verifyReplaceWithENCCell(string childCell, string replaceBy, Item item)
        {
            Console.WriteLine("Action#:" + actionCounter + ".ENC Cell:" + childCell);
            foreach (Product product in jsonPayload.Data.Products)
            {

                if ((childCell == product.ProductName) && (product.ReplacedBy.Contains(replaceBy)))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if (!product.InUnitsOfSale.Contains(item.PRODUCTNAME))
                        AttrNotMatched.Add(nameof(item.PRODUCTNAME));
                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE" };
                    var v = VerifyBlankFields(item, fieldNames);


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

        private static bool verifyAssignCellToAVCSUnitOfSale(string childCell, string productName, Item item)
        {

            Console.WriteLine("Action#:" + actionCounter + ".AVCSUnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in jsonPayload.Data.UnitsOfSales)
            {
                List<string> pdts = unitOfSale.CompositionChanges.AddProducts;
                foreach (string pdt in pdts)
                {
                    if ((childCell == pdt) && (productName == unitOfSale.UnitName))
                    {

                        AttrNotMatched.Clear();
                        if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                            AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                        if (!item.PRODUCT.Equals("AVCS UNIT"))
                            AttrNotMatched.Add(nameof(item.PRODUCT));
                        if (!item.PRODTYPE.Equals((getProductInfo(unitOfSale.CompositionChanges.AddProducts)).ProductType))
                            AttrNotMatched.Add(nameof(item.PRODTYPE));
                        //xmlAttributes[4] & [5] are skipped as already checked
                        //Checking blanks
                        string[] fieldNames = { "CANCELLED", "REPLACEDBY", "AGENCY", "PROVIDER", "ENCSIZE", "TITLE", "EDITIONNO", "UPDATENO", "UNITTYPE" };
                        var v = VerifyBlankFields(item, fieldNames);


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

        private static bool verifyCreateAVCSUnitOfSale(string productName, Item item)
        {
            Console.WriteLine("Action#:" + actionCounter + ".UnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in jsonPayload.Data.UnitsOfSales)
            {

                if ((productName == unitOfSale.UnitName) && (unitOfSale.IsNewUnitOfSale))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("AVCS UNIT"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals((getProductInfo(unitOfSale.CompositionChanges.AddProducts)).ProductType))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if (!item.AGENCY.Equals((getProductInfo(unitOfSale.CompositionChanges.AddProducts)).Agency))
                        AttrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals((getProductInfo(unitOfSale.CompositionChanges.AddProducts)).ProviderCode))
                        AttrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(unitOfSale.UnitSize))
                        AttrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(unitOfSale.Title))
                        AttrNotMatched.Add(nameof(item.TITLE));
                    if (!item.UNITTYPE.Equals(unitOfSale.UnitType))
                        AttrNotMatched.Add(nameof(item.UNITTYPE));

                    //Checking blanks
                    string[] fieldNames = { "CANCELLED", "REPLACEDBY", "EDITIONNO", "UPDATENO" };
                    var v = VerifyBlankFields(item, fieldNames);

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


        private static bool verifyCreateENCCell(string childCell, Item item)
        {

            Console.WriteLine("Action#:" + actionCounter + ".Childcell:" + childCell);
            foreach (Product product in jsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.Status.IsNewCell))
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] as already checked
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType[4..]))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if ((!product.InUnitsOfSale.Contains(item.PRODUCTNAME)) && (!getUoSInfo(item.PRODUCTNAME).UnitOfSaleType.Contains("Unit")))
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
                    var v = VerifyBlankFields(item, fieldNames);


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
        private static bool VerifyBlankFields(Item item, string[] fieldNames)
        {
            bool allBlanks = true;
            
            foreach (string field in fieldNames)
            {
                if (!typeof(Item).GetProperty(field).GetValue(item, null).Equals(""))
                    AttrNotMatched.Add(typeof(Item).GetProperty(field).Name);
            }
            return allBlanks;
        }
        private static UoSProductInfo getProductInfo(List<string> products)
        {
            UoSProductInfo productInfo = new UoSProductInfo();
            foreach (string pdt in products)
            {
                foreach (Product product in jsonPayload.Data.Products)
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
        private static ProductUoSInfo getUoSInfo(string productName)
        {
            ProductUoSInfo UoSInfo = new ProductUoSInfo();
            foreach (UnitOfSale uos in jsonPayload.Data.UnitsOfSales)
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

        public static async Task<bool> VerifyPresenseOfMandatoryXMLAtrributes(XmlNodeList nodeList)
        {
            List<string> ActionAttributesSeq = new List<string>();
            ActionAttributesSeq = Config.TestConfig.XMLActionList.ToList<string>();

            foreach (XmlNode node in nodeList)
            {
                XmlNodeList dd = node.ChildNodes;

                for (int i = 0; i < 15; i++)
                {
                    if (dd[i].Name != ActionAttributesSeq[i])
                    {
                        Console.WriteLine("First missed Attribute is:" + ActionAttributesSeq[i] + " for action number:" + dd[0].InnerText);
                        return false;
                    }
                }

            }
            if (nodeList.Count > 0)
            {
                Console.WriteLine("Mandatory atrributes are present in all XML actions");
                await Task.CompletedTask;
                return true;
            }
            else
                return false;
        }

        private static UoSProductInfo getProductInfo(string products)
        {
            UoSProductInfo productInfo = new UoSProductInfo();
            foreach (Product product in jsonPayload.Data.Products)
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
        public string downloadGeneratedXML(string expectedXMLfilePath, string containerAndBlobName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerAndBlobName);
            BlobClient blobClient = containerClient.GetBlobClient(containerAndBlobName + ".xml");

            BlobDownloadInfo blobDownload = blobClient.Download();
            using (FileStream downloadFileStream = new FileStream((expectedXMLfilePath + "\\" + containerAndBlobName + ".xml"), FileMode.Create))
            {
                blobDownload.Content.CopyTo(downloadFileStream);
            }
            return (expectedXMLfilePath + "\\" + containerAndBlobName + ".xml");
        }

        public static string getRequiredXMLText(string generatedXMLFilePath, string tagName)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(File.ReadAllText(generatedXMLFilePath));
            XmlNode node = xDoc.SelectSingleNode("//" + tagName);

            return node.InnerText;
        }


        public static bool verifyOrderOfActions(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {
            bool areEqual = getFinalActionsListFromJson(listFromJson).SequenceEqual(curateListOfActionsFromXmlFile(generatedXMLFilePath));
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

        public static bool verifyInitialXMLHeaders(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {
            bool b = verifyNOOFACTIONSHeader(jsonPayload, generatedXMLFilePath);
            bool a = verifyRECTIMEHeader(jsonPayload, generatedXMLFilePath);
            bool c = verifyCORRIDHeader(jsonPayload, generatedXMLFilePath);
            bool d = verifyORGHeader(jsonPayload, generatedXMLFilePath);

            if (a && b && c && d == true)
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

        public static bool verifyRECTIMEHeader(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {

            string time = jsonPayload.Time;
            DateTime dt = DateTime.Parse(time);
            string timeFromJSON = dt.ToString("yyyyMMdd");

            string timeFromXML = getRequiredXMLText(generatedXMLFilePath, "RECDATE");

            if (timeFromJSON == timeFromXML)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool verifyNOOFACTIONSHeader(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {

            string a = calculateTotalNumberOfActions(jsonPayload).ToString();
            string b = getRequiredXMLText(generatedXMLFilePath, "NOOFACTIONS");

            if (a == b)
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

        public static bool verifyCORRIDHeader(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {

            string traceID = jsonPayload.Data.correlationId;
            string corrID = getRequiredXMLText(generatedXMLFilePath, "CORRID");

            if (traceID == corrID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool verifyORGHeader(JsonPayloadHelper jsonPayload, string generatedXMLFilePath)
        {
            string orgValueFromXMl = getRequiredXMLText(generatedXMLFilePath, "ORG");

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

        public static List<string> curateListOfActionsFromXmlFile(string downloadedXMLFilePath)
        {
            actionsListFromXml.Clear();
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(File.ReadAllText(downloadedXMLFilePath));
            XmlNodeList nodeList = xDoc.SelectNodes("//ACTION");

            foreach (XmlNode node in nodeList)
            {
                actionsListFromXml.Add(node.InnerText);
            }

            return actionsListFromXml;
        }

        public static JsonPayloadHelper getPayloadDeserealized(string jsonPayloadFilePath)
        {
            using (StreamReader r = new StreamReader(jsonPayloadFilePath))
            {
                string jsonOutput = r.ReadToEnd();
                JsonPayloadHelper jsonPayloadHelper = JsonConvert.DeserializeObject<JsonPayloadHelper>(jsonOutput);

                return jsonPayloadHelper;
            }
        }

        public static void updateActionList(int n, string actionMessage)
        {
            for (int i = 0; i < n; i++)
            {
                listFromJson.Add(actionMessage);
            }
            listFromJson.Sort();
        }

        public static List<string> getFinalActionsListFromJson(List<string> actionsList)
        {
            for (int i = 0; i < actionsList.Count; i++)
            {
                actionsList[i] = actionsList[i].Substring(4);
            }
            return actionsList;
        }

        // ====== Calculation Logic Starts ======

        public static int calculateTotalNumberOfActions(JsonPayloadHelper jsonPayload)
        {
            int totalNumberOfActions = 0;
            listFromJson.Clear();
            totalNumberOfActions = calculateNewCellCount(jsonPayload)
                                 + calculateNewUnitOfSalesCount(jsonPayload)
                                 + calculateAssignCellToUoSActionCount(jsonPayload)
                                 + calculateReplaceCellActionCount(jsonPayload)
                                 + calculateChangeEncCellActionCount(jsonPayload)
                                 + calculateChangeUoSActionCount(jsonPayload)
                                 + calculateRemoveCellFromUoSActionCount(jsonPayload)
                                 + calculateUpdateEncCellEditionUpdateNumber(jsonPayload)
                                 + calculateUpdateEncCellEditionUpdateNumberForSuspendedStatus(jsonPayload)
                                 + calculateCancelledCellCount(jsonPayload)
                                 + calculateCancelUnitOfSalesActionCount(jsonPayload);

            Console.WriteLine("Total No. of Actions = " + totalNumberOfActions);
            return totalNumberOfActions;
        }


        public static int calculateNewCellCount(JsonPayloadHelper jsonPayload)
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
                updateActionList(count, "1.  CREATE ENC CELL");
                Console.WriteLine("Total no. of Create ENC Cell: " + count);
            }


            return count;
        }

        public static int calculateNewUnitOfSalesCount(JsonPayloadHelper jsonPayload)
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
                updateActionList(newUoSCount, "2.  CREATE AVCS UNIT OF SALE");
                Console.WriteLine("Total no. of Create AVCS Unit of Sale: " + newUoSCount);
            }


            return newUoSCount;
        }

        public static int calculateAssignCellToUoSActionCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int count = 0;
            foreach (UnitOfSale unitOfSale in obj.Data.UnitsOfSales)
            {
                count = count + unitOfSale.CompositionChanges.AddProducts.Count;
            }

            if (count > 0)
            {
                updateActionList(count, "3.  ASSIGN CELL TO AVCS UNIT OF SALE");
                Console.WriteLine("Total no. of Assign Cell to AVCS UoS: " + count);
            }


            return count;
        }
        public static int calculateReplaceCellActionCount(JsonPayloadHelper jsonPayload)
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
                updateActionList(count, "4.  REPLACED WITH ENC CELL");
                Console.WriteLine("Total no. of ReplaceD With ENC Cell: " + count);
            }

            return count;
        }

        public static int calculateChangeEncCellActionCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int count = 0;

            foreach (Product product in obj.Data.Products)
            {
                if (product.ContentChanged == false)
                {
                    count++;
                }
            }

            if (count > 0)
            {
                updateActionList(count, "5.  CHANGE ENC CELL");
                Console.WriteLine("Total No. of Change ENC Cell: " + count);
            }

            return count;
        }

        public static int calculateChangeUoSActionCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int count = 0;

            foreach (Product product in obj.Data.Products)
            {
                if (product.ContentChanged == false)
                {
                    count = count + product.InUnitsOfSale.Count;
                }
            }

            if (count > 0)
            {
                updateActionList(count, "6.  CHANGE AVCS UNIT OF SALE");
                Console.WriteLine("Total No. of Change AVCS UoS: " + count);
            }


            return count;
        }

        public static int calculateUpdateEncCellEditionUpdateNumber(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int count = 0;
            foreach (Product product in obj.Data.Products)
            {
                if (product.ContentChanged == true && product.Status.IsNewCell == false
                    && (product.Status.StatusName == "Update" || product.Status.StatusName == "New Edition" || product.Status.StatusName == "Re-issue"))
                {
                    count++;
                }
            }

            if (count > 0)
            {
                updateActionList(count, "7.  UPDATE ENC CELL EDITION UPDATE NUMBER");
                Console.WriteLine("Total no. of ENC Cell Edition Update Number: " + count);
            }

            return count;
        }

        public static int calculateUpdateEncCellEditionUpdateNumberForSuspendedStatus(JsonPayloadHelper jsonPayload)
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
                updateActionList(count, "7.  UPDATE ENC CELL EDITION UPDATE NUMBER");
                Console.WriteLine("Total no. of ENC Cell Edition Update Number: " + count);
            }

            return count;
        }

        public static int calculateRemoveCellFromUoSActionCount(JsonPayloadHelper jsonPayload)
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
                updateActionList(count, "8.  REMOVE ENC CELL FROM AVCS UNIT OF SALE");
                Console.WriteLine("Total no. of Remove Cell from UoS: " + count);

            }

            return count;
        }

        public static int calculateCancelledCellCount(JsonPayloadHelper jsonPayload)
        {
            var obj = jsonPayload;
            int cancelledCellCount = 0;

            foreach (Product product in obj.Data.Products)
            {
                /*if (product.Status.IsNewCell == false && ((product.ReplacedBy.Count)>0))
                {
                    cancelledCellCount++;
                }*/

                if (product.Status.StatusName == "Cancellation Update")
                {
                    cancelledCellCount++;
                }
            }

            if (cancelledCellCount > 0)
            {
                updateActionList(cancelledCellCount, "9.  CANCEL ENC CELL");
                Console.WriteLine("Total No. of Cancel ENC Cell: " + cancelledCellCount);
            }

            return cancelledCellCount;
        }

        public static int calculateCancelUnitOfSalesActionCount(JsonPayloadHelper jsonPayload)
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
                updateActionList(cancelledUoSCount, "99. CANCEL AVCS UNIT OF SALE");
                Console.WriteLine("Total No. of Cancel AVCS UoS: " + cancelledUoSCount);
            }

            return cancelledUoSCount;
        }

        public static string generateRandomCorrelationId()
        {
            Guid guid = Guid.NewGuid();

            string randomCorrID = guid.ToString("N");

            randomCorrID = randomCorrID.Insert(8, "-");
            randomCorrID = randomCorrID.Insert(13, "-");
            randomCorrID = randomCorrID.Insert(18, "-");
            randomCorrID = randomCorrID.Insert(23, "-");

            Console.WriteLine("Generated TraceId = " + randomCorrID);
            return randomCorrID;
        }

        public static string updateTimeAndCorrIdField(string requestBody, string generatedCorrelationId)
        {
            var currentTimeStamp = DateTime.Now.ToString("yyyy-MM-dd");
            JObject jsonObj = JObject.Parse(requestBody);
            jsonObj["time"] = currentTimeStamp;
            jsonObj["data"]["correlationId"] = generatedCorrelationId;
            string updatedRequestBody = jsonObj.ToString();
            return updatedRequestBody;
        }

    }
}
