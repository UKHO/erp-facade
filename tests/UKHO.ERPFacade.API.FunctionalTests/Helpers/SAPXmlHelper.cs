using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Xml;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class SAPXmlHelper
    {
        private JsonPayloadHelper jsonPayloadHelper { get; set; }
        private string _storageAccount_connectionString = "";
        static int actionCounter = 1;
        static bool actionAttributesValue;
        static List<string> AttrNotMatched = new List<string>();
        public static List<string> listFromJson = new List<string>();
        public static List<string> actionsListFromXml = new List<string>();
        public static Config config;
        private static JsonPayloadHelper jsonPayload { get; set; }
        private static SAPXmlPayload xmlPayload { get; set; }


        public static async Task<bool> CheckXMLAttributes(string requestBody)
        {
            //Below line added for testing purpose once container is ready then will download xml from there using traceid
            string XMLFilePath = "C:\\Users\\Sadha1501493\\GitHubRepo\\erp-facade\\tests\\UKHO.ERPFacade.API.FunctionalTests\\ERPFacadePayloadTestData\\SAPNewCell.xml";
            
            //Deserialize JSOn and XML payloads
            jsonPayload = JsonConvert.DeserializeObject<JsonPayloadHelper>(requestBody);
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
        public string downloadGeneratedXML(string generatedXMlFilePath, string traceID)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_storageAccount_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(traceID);
            BlobClient blobClient = containerClient.GetBlobClient(traceID + ".xml");

            BlobDownloadInfo blobDownload = blobClient.Download();
            using (FileStream downloadFileStream = new FileStream((generatedXMlFilePath + ".xml"), FileMode.Create))
            {
                blobDownload.Content.CopyTo(downloadFileStream);
            }

            return (generatedXMlFilePath + ".xml");
        }

        public List<string> curateListOfActionsFromXmlFile(string downloadedXMLFilePath)
        {

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(downloadedXMLFilePath + ".xml");
            XmlNodeList nodeList = xDoc.SelectNodes("//ACTION");

            foreach (XmlNode node in nodeList)
            {
                actionsListFromXml.Add(node.InnerText);
            }

            //actionsListFromXml.Sort();
            return actionsListFromXml;
        }

        //  ================  JSON Related Code =======================
        public string getTraceID(string jsonFilePath)
        {

            using (StreamReader r = new StreamReader(jsonFilePath))
            {
                string jsonOutput = r.ReadToEnd();
                jsonPayloadHelper = JsonConvert.DeserializeObject<JsonPayloadHelper>(jsonOutput);
            }

            return jsonPayloadHelper.Id;
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

        // methods which gets called by action calculator methods to push action data
        public void updateActionList(int n, string actionMessage)
        {
            for (int i = 0; i < n; i++)
            {
                listFromJson.Add(actionMessage);
            }
            listFromJson.Sort();
            //returnTrimmedActionsList(list);
        }

        public List<string> getFinalActionsListFromJson(List<string> actionsList)
        {
            //actionsList.Sort();
            for (int i = 0; i < actionsList.Count; i++)
            {
                actionsList[i] = actionsList[i].Substring(4);
                // Console.WriteLine("update list index "+i+" : "+actionsList[i]);
            }
            return actionsList;
        }

        // ====== Calculation Logic Starts ======

        public int calculateTotalNumberOfActions()
        {
            int totalNumberOfActions = 0;
            totalNumberOfActions = calculateNewCellCount()
                                 + calculateNewUnitOfSalesCount()
                                 + calculateCancelledCellCount()
                                 + calculateCancelUnitOfSalesActionCount()
                                 + calculateReplaceCellActionCount()
                                 + calculateAssignCellToUoSActionCount()
                                 + calculateRemoveCellFromUoSActionCount()
                                 + calculateUpdateEncCellEditionUpdateNumber();

            Console.WriteLine("Total No. of Actions = " + totalNumberOfActions);
            return totalNumberOfActions;
        }

        public int calculateNewCellCount()
        {   
            var obj = jsonPayloadHelper;
            int count = 0;

            foreach (Product product in obj.Data.Products)
            {
                if (product.Status.IsNewCell == true)
                {
                    count++;
                }
            }

            updateActionList(count, "1.  CREATE ENC CELL");
            Console.WriteLine("New Cells: " + count);
            return count;
        }

        public int calculateNewUnitOfSalesCount()
        {
            var obj = jsonPayloadHelper;
            int newUoSCount = 0;

            foreach (UnitOfSale unitOfSale in obj.Data.UnitsOfSales)
            {
                if (unitOfSale.IsNewUnitOfSale == true)
                {
                    newUoSCount++;
                }
            }

            updateActionList(newUoSCount, "2.  CREATE AVCS UNIT OF SALE");
            Console.WriteLine("New UoS Count: " + newUoSCount);
            return newUoSCount;
        }
        public int calculateAssignCellToUoSActionCount()
        {
            var obj = jsonPayloadHelper;
            int count = 0;
            foreach (UnitOfSale unitOfSale in obj.Data.UnitsOfSales)
            {
                count = count + unitOfSale.CompositionChanges.AddProducts.Count;
            }
            updateActionList(count, "3.  ASSIGN CELL TO AVCS UNIT OF SALE");
            Console.WriteLine("Total Assign Cell to UoS action: " + count);
            return count;
        }
        public int calculateReplaceCellActionCount()
        {
            var obj = jsonPayloadHelper;
            int count = 0;
            foreach (Product product in obj.Data.Products)
            {
                if (product.Status.IsNewCell == false && ((product.ReplacedBy.Count) > 0))
                {
                    count = count + product.ReplacedBy.Count;
                }
            }
            updateActionList(count, "4.  REPLACE WITH NEW ENC CELL");
            Console.WriteLine("Total Replace ENC Cell Action: " + count);
            return count;
        }

        public int calculateUpdateEncCellEditionUpdateNumber()
        {
            var obj = jsonPayloadHelper;
            int count = 0;
            foreach (Product product in obj.Data.Products)
            {
                if (product.ContentChanged == true && product.Status.IsNewCell == false
                    && (product.Status.StatusName == "Update" || product.Status.StatusName == "New Edition"))
                {
                    count = count++;
                }
            }
            updateActionList(count, "7.  UPDATE ENC CELL EDITION UPDATE NUMBER");
            Console.WriteLine("Total no. of ENC Cell Edition Update: " + count);
            return count;
        }

        public int calculateRemoveCellFromUoSActionCount()
        {
            var obj = jsonPayloadHelper;
            int count = 0;
            foreach (UnitOfSale unitOfSale in obj.Data.UnitsOfSales)
            {
                if (unitOfSale.CompositionChanges.RemoveProducts.Count > 0)
                {
                    count = count + unitOfSale.CompositionChanges.RemoveProducts.Count;
                }
            }
            updateActionList(count, "8.  REMOVE CELL FROM AVCS UNIT OF SALE");
            Console.WriteLine("Remove Cell from UoS Action Count: " + count);
            return count;
        }

        public int calculateCancelledCellCount()
        {
            var obj = jsonPayloadHelper;
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
            updateActionList(cancelledCellCount, "9.  CANCEL ENC CELL");
            Console.WriteLine("Total No. of Cancelled Cells: " + cancelledCellCount);
            return cancelledCellCount;
        }

        public int calculateCancelUnitOfSalesActionCount()
        {
            var obj = jsonPayloadHelper;
            int cancelledUoSCount = 0;

            foreach (UnitOfSale unitOfSale in obj.Data.UnitsOfSales)
            {
                if (unitOfSale.Status == "NotForSale")
                {
                    cancelledUoSCount++;
                }
            }
            updateActionList(cancelledUoSCount, "99. CANCEL AVCS UNIT OF SALE");
            Console.WriteLine("Total No. of Cancelled UoS: " + cancelledUoSCount);
            return cancelledUoSCount;
        }


    }
}
