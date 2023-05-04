using Newtonsoft.Json;
using NUnit.Framework;
using System.Xml;
using System.Xml.Serialization;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class SAPXmlHelper
    {
        static int actionCounter = 1;
        static bool actionAttributesValue;
        static List<string> AttrNotMatched = new List<string>();
        public static Config config;
        private static WebhookPayload jsonPayload { get; set; }
        private static SAPXmlPayload xmlPayload { get; set; }

        public static async Task<bool> CheckXMLAttributes(string requestBody)
        {
            //Below line added for testing purpose once container is ready then will download xml from there using traceid
            string XMLFilePath = "C:\\Users\\Sadha1501493\\GitHubRepo\\erp-facade\\tests\\UKHO.ERPFacade.API.FunctionalTests\\ERPFacadePayloadTestData\\SAPNewCell.xml";
            
            //Deserialize JSOn and XML payloads
            jsonPayload = JsonConvert.DeserializeObject<WebhookPayload>(requestBody);
            XmlSerializer serializer = new XmlSerializer(typeof(SAPXmlPayload));

            using (Stream reader = new FileStream(XMLFilePath, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                xmlPayload = (SAPXmlPayload)serializer.Deserialize(reader);
            }

            XmlDocument xDoc = new XmlDocument();
            //load up the xml from the location 
            xDoc.Load(XMLFilePath);
            XmlNodeList nodeList = xDoc.SelectNodes("/Z_ADDS_MAT_INFO/IM_MATINFO/ACTIONITEMS/item");
            Assert.True(VerifyPresenseOfMandatoryXMLAtrributes(nodeList).Result);

            //verification of action atrribute's value
            foreach (Item item in xmlPayload.IM_MATINFO.ACTIONITEMS.Item)
            {

                if (item.ACTION == "CREATE ENC CELL")
                    Assert.True(verifyCreateENCCell(item.CHILDCELL, item));
                else if (item.ACTION == "CREATE AVCS UNIT OF SALE")
                    Assert.True(verifyCreateAVCSUnitOfSale(item.PRODUCTNAME, item));
                else if (item.ACTION == "ASSIGN CELL TO AVCS UNIT OF SALE")
                    Assert.True(verifyAssignCellToAVCSUnitOfSale(item.CHILDCELL, item.PRODUCTNAME, item));
                else if (item.ACTION == "REPLACE WITH ENC CELL")
                    Assert.True(verifyReplaceWithENCCell(item.CHILDCELL, item.PRODUCTNAME, item));
                else if (item.ACTION == "REMOVE ENC CELL FROM AVCS UNIT OF SALE")
                    Assert.True(verifyRemoveENCCellFromAVCSUnitOFSale(item.CHILDCELL, item.PRODUCTNAME, item));
                else if (item.ACTION == "CANCEL ENC CELL")
                    Assert.True(verifyCancelENCCell(item.CHILDCELL, item.PRODUCTNAME, item));
                else if (item.ACTION == "CANCEL AVCS UNIT OF SALE")
                    Assert.True(verifyCancelToAVCSUnitOfSale(item.PRODUCTNAME, item));
                actionCounter++;

            }
            Console.WriteLine("Total verified Actions:" + --actionCounter);
            await Task.CompletedTask;
            return true;
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
                    if (!item.CANCELLED.Equals(""))
                        AttrNotMatched.Add(nameof(item.CANCELLED));
                    if (!item.REPLACEDBY.Equals(""))
                        AttrNotMatched.Add(nameof(item.REPLACEDBY));
                    if (!item.AGENCY.Equals(""))
                        AttrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(""))
                        AttrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(""))
                        AttrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(""))
                        AttrNotMatched.Add(nameof(item.TITLE));                    
                    if (!item.EDITIONNO.Equals(""))
                        AttrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(""))
                        AttrNotMatched.Add(nameof(item.UPDATENO));
                    if(!item.UNITTYPE.Equals(""))
                        AttrNotMatched.Add(nameof(item.UNITTYPE));

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
            return false;
        }

        private static bool? verifyCancelENCCell(string childCell, string productName, Item item)
        {
            Console.WriteLine("Action#:" + actionCounter + ".ENC Cell:" + childCell);
            foreach (Product product in jsonPayload.Data.Products)
            {

                if ((childCell == product.ProductName) && (product.Status.Equals("Cancellation update")) && (product.InUnitsOfSale.Contains(productName)) )
                {
                    AttrNotMatched.Clear();
                    if (!item.ACTIONNUMBER.Equals(actionCounter.ToString()))
                        AttrNotMatched.Add(nameof(item.ACTIONNUMBER));
                    //xmlAttributes[1] is skipped as already checked
                    if (!item.PRODUCT.Equals("ENC CELL"))
                        AttrNotMatched.Add(nameof(item.PRODUCT));
                    if (!item.PRODTYPE.Equals(product.ProductType))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if (!item.CANCELLED.Equals(""))
                        AttrNotMatched.Add(nameof(item.CANCELLED));
                    if (!item.REPLACEDBY.Equals(""))
                        AttrNotMatched.Add(nameof(item.REPLACEDBY));
                    if (!item.AGENCY.Equals(""))
                        AttrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(""))
                        AttrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(""))
                        AttrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(""))
                        AttrNotMatched.Add(nameof(item.TITLE));
                    if (!item.EDITIONNO.Equals(""))
                        AttrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(""))
                        AttrNotMatched.Add(nameof(item.UPDATENO));
                    if (!item.UNITTYPE.Equals(""))
                        AttrNotMatched.Add(nameof(item.UNITTYPE));

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
                        if (!item.PRODTYPE.Equals((getProductInfo(unitOfSale.CompositionChanges.AddProducts)).ProductType))
                            AttrNotMatched.Add(nameof(item.PRODTYPE));
                        //xmlAttributes[4] & [5] are skipped as already checked
                        //Below code to check rest all attributes are blank
                        if (!item.CANCELLED.Equals(""))
                            AttrNotMatched.Add(nameof(item.CANCELLED));
                        if (!item.REPLACEDBY.Equals(""))
                            AttrNotMatched.Add(nameof(item.REPLACEDBY));
                        if (!item.AGENCY.Equals(""))
                            AttrNotMatched.Add(nameof(item.AGENCY));
                        if (!item.PROVIDER.Equals(""))
                            AttrNotMatched.Add(nameof(item.PROVIDER));
                        if (!item.ENCSIZE.Equals(""))
                            AttrNotMatched.Add(nameof(item.ENCSIZE));
                        if (!item.TITLE.Equals(""))
                            AttrNotMatched.Add(nameof(item.TITLE));
                        if (!item.EDITIONNO.Equals(""))
                            AttrNotMatched.Add(nameof(item.EDITIONNO));
                        if (!item.UPDATENO.Equals(""))
                            AttrNotMatched.Add(nameof(item.UPDATENO));
                        if (!item.UNITTYPE.Equals(""))
                            AttrNotMatched.Add(nameof(item.UNITTYPE));
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
                    if (!item.PRODTYPE.Equals(product.ProductType))
                        AttrNotMatched.Add(nameof(item.PRODTYPE));
                    if (!product.InUnitsOfSale.Contains(item.PRODUCTNAME))
                        AttrNotMatched.Add(nameof(item.PRODUCTNAME));
                    if (!item.CANCELLED.Equals(""))
                        AttrNotMatched.Add(nameof(item.CANCELLED));                    
                    if (!item.AGENCY.Equals(""))
                        AttrNotMatched.Add(nameof(item.AGENCY));
                    if (!item.PROVIDER.Equals(""))
                        AttrNotMatched.Add(nameof(item.PROVIDER));
                    if (!item.ENCSIZE.Equals(""))
                        AttrNotMatched.Add(nameof(item.ENCSIZE));
                    if (!item.TITLE.Equals(""))
                        AttrNotMatched.Add(nameof(item.TITLE));
                    if (!item.EDITIONNO.Equals(""))
                        AttrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(""))
                        AttrNotMatched.Add(nameof(item.UPDATENO));
                    if (!item.UNITTYPE.Equals(""))
                        AttrNotMatched.Add(nameof(item.UNITTYPE));

                    if (AttrNotMatched.Count == 0)
                    {
                        Console.WriteLine("REPLACE WITH ENC CELL Action's Data is correct");
                        return true;
                    }

                    else
                    {
                        Console.WriteLine("REPLACE WITH ENC CELL Action's Data is incorrect");
                        Console.WriteLine("Not matching attributes are:");
                        foreach (string attribute in AttrNotMatched)
                        { Console.WriteLine(attribute); }
                        return false;
                    }
                }

            }
            return false;
        }

        public static async Task<bool> VerifyPresenseOfMandatoryXMLAtrributes(XmlNodeList nodeList)
        {
            List<string> ActionAttributesSeq = formActionAtrributes();
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
            await Task.CompletedTask;
            return true;
        }

        private static bool verifyAssignCellToAVCSUnitOfSale(string childCell, string productName, Item item)
        {

            Console.WriteLine("Action#:" + actionCounter + ".AVCSUnitOfSale:" + productName);
            foreach (UnitOfSale unitOfSale in jsonPayload.Data.UnitsOfSales)
            {
                //bool flagMatchProduct = false;
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
                        //Below code to check rest all attributes are blank
                        if (!item.CANCELLED.Equals(""))
                            AttrNotMatched.Add(nameof(item.CANCELLED));
                        if (!item.REPLACEDBY.Equals(""))
                            AttrNotMatched.Add(nameof(item.REPLACEDBY));
                        if (!item.AGENCY.Equals(""))
                            AttrNotMatched.Add(nameof(item.AGENCY));
                        if (!item.PROVIDER.Equals(""))
                            AttrNotMatched.Add(nameof(item.PROVIDER));
                        if (!item.ENCSIZE.Equals(""))
                            AttrNotMatched.Add(nameof(item.ENCSIZE));
                        if (!item.TITLE.Equals(""))
                            AttrNotMatched.Add(nameof(item.TITLE));
                        if (!item.EDITIONNO.Equals(""))
                            AttrNotMatched.Add(nameof(item.EDITIONNO));
                        if (!item.UPDATENO.Equals(""))
                            AttrNotMatched.Add(nameof(item.UPDATENO));
                        if (!item.UNITTYPE.Equals(""))
                            AttrNotMatched.Add(nameof(item.UNITTYPE));
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
                    if (!item.CANCELLED.Equals(""))
                        AttrNotMatched.Add(nameof(item.CANCELLED));
                    if (!item.REPLACEDBY.Equals(""))
                        AttrNotMatched.Add(nameof(item.REPLACEDBY));
                    if (!item.EDITIONNO.Equals(""))
                        AttrNotMatched.Add(nameof(item.EDITIONNO));
                    if (!item.UPDATENO.Equals(""))
                        AttrNotMatched.Add(nameof(item.UPDATENO));
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
                    if (!item.PRODUCTNAME.Equals(product.ProductName))
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
                    if (!item.CANCELLED.Equals(""))
                        AttrNotMatched.Add(nameof(item.CANCELLED));
                    if (!item.REPLACEDBY.Equals(""))
                        AttrNotMatched.Add(nameof(item.REPLACEDBY));
                    if (!item.UNITTYPE.Equals(""))
                        AttrNotMatched.Add(nameof(item.UNITTYPE));

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
            return false;


        }
        private static UoSProductInfo getProductInfo(List<string> addProducts)
        {
            UoSProductInfo productInfo = new UoSProductInfo();
            foreach (string pdt in addProducts)
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

        private static List<string> formActionAtrributes()
        {

            List<string> ActionAttributesSeq = new List<string>();
            ActionAttributesSeq.Add("ACTIONNUMBER");
            ActionAttributesSeq.Add("ACTION");
            ActionAttributesSeq.Add("PRODUCT");
            ActionAttributesSeq.Add("PRODTYPE");
            ActionAttributesSeq.Add("CHILDCELL");
            ActionAttributesSeq.Add("PRODUCTNAME");
            ActionAttributesSeq.Add("CANCELLED");
            ActionAttributesSeq.Add("REPLACEDBY");
            ActionAttributesSeq.Add("AGENCY");
            ActionAttributesSeq.Add("PROVIDER");
            ActionAttributesSeq.Add("ENCSIZE");
            ActionAttributesSeq.Add("TITLE");
            ActionAttributesSeq.Add("EDITIONNO");
            ActionAttributesSeq.Add("UPDATENO");
            ActionAttributesSeq.Add("UNITTYPE");
            return ActionAttributesSeq;
        }

    }
}
