using FluentAssertions.Equivalency;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class SAPXmlHelper
    {
        static int actionCounter = 1;
        private static WebhookPayload jsonPayload { get; set; }
        public void CheckXMLAttributes(string requestBody)
        {
            //string requestBody;
            //string filePath = "C:\\Users\\Sadha1501493\\source\\repos\\TestProject1\\TestProject1\\ERPFacadePayloadTestData\\WebhookPayload.JSON";
            string XMLFilePath = "C:\\Users\\Sadha1501493\\source\\repos\\TestProject1\\TestProject1\\ERPFacadePayloadTestData\\SAPNewCell.xml";

            /*using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }*/


            Console.WriteLine("//deserialization");
            jsonPayload = JsonConvert.DeserializeObject<WebhookPayload>(requestBody);
            
            XmlDocument xDoc = new XmlDocument();

            //load up the xml from the location 
            xDoc.Load(XMLFilePath);
            XmlNodeList nodeList = xDoc.SelectNodes("/Z_ADDS_MAT_INFO/IM_MATINFO/ACTIONITEMS/item");


            foreach (XmlNode node in nodeList)
            {


                XmlNodeList dd = node.ChildNodes;
                if (dd[1].InnerText == "CREATE ENC CELL")
                    verifyCreateENCCell(dd[4].InnerText,  dd);  

                else if (dd[1].InnerText == "CREATE AVCS UNIT OF SALE")
                    verifyCreateAVCSUnitOfSale(dd[5].InnerText,  dd);
                else if (dd[1].InnerText == "ASSIGN CELL TO AVCS UNIT OF SALE")
                    verifyAssignCellToAVCSUnitOfSale(dd[4].InnerText, dd[5].InnerText,  dd);
                actionCounter++;
            }
        }

        private static void verifyAssignCellToAVCSUnitOfSale(string childCell, string productName,  XmlNodeList xmlAttributes)
        {

            Console.WriteLine("Childcell:" + childCell);
            foreach (UnitOfSale unitOfSale in jsonPayload.Data.UnitsOfSales)
            {
                bool flagMatchProduct = false;
                List<string> pdts = unitOfSale.CompositionChanges.AddProducts;
                foreach (string pdt in pdts)
                {
                    if ((childCell == pdt) && (productName == unitOfSale.UnitName))
                    {
                        flagMatchProduct = true;
                        if (!xmlAttributes[0].InnerText.Equals(actionCounter.ToString()))
                            flagMatchProduct = false;
                        //xmlAttributes[1] is skipped as already checked
                        if (!xmlAttributes[2].InnerText.Equals("AVCS UNIT"))
                            flagMatchProduct = false;
                        if (!xmlAttributes[3].InnerText.Equals((getProductInfo(unitOfSale.CompositionChanges.AddProducts)).ProductType))
                            flagMatchProduct = false;
                        //xmlAttributes[4] & [5] are skipped as already checked
                        //Below code to check rest all attributes are blank
                        for (int i = 6; i <= 14; i++)
                        {
                            if (!xmlAttributes[i].InnerText.Equals(""))
                                flagMatchProduct = false;
                        }

                    }
                    if (flagMatchProduct)
                    {
                        Console.WriteLine("ASSIGN CELL TO AVCS UNIT OF SALE Action's Data is correct");
                        return;
                    }

                }
                //if (!flagMatchProduct)
                //Console.WriteLine("ASSIGN CELL TO AVCS UNIT OF SALE Action's Data is incorrect");

            }
        }

        private static void verifyCreateAVCSUnitOfSale(string productName,  XmlNodeList xmlAttributes)
        {
            Console.WriteLine("UnitOfSale:" + productName);
            bool flagMatchProduct = false;
            foreach (UnitOfSale unitOfSale in jsonPayload.Data.UnitsOfSales)
            {

                if ((productName == unitOfSale.UnitName) && (unitOfSale.IsNewUnitOfSale))
                {
                    flagMatchProduct = true;
                    if (!xmlAttributes[0].InnerText.Equals(actionCounter.ToString()))
                        flagMatchProduct = false;
                    //xmlAttributes[1] is skipped as already checked
                    if (!xmlAttributes[2].InnerText.Equals("AVCS UNIT"))
                        flagMatchProduct = false;
                    if (!xmlAttributes[3].InnerText.Equals((getProductInfo(unitOfSale.CompositionChanges.AddProducts )).ProductType))
                        flagMatchProduct = false;
                    if (!xmlAttributes[8].InnerText.Equals((getProductInfo(unitOfSale.CompositionChanges.AddProducts )).Agency))
                        flagMatchProduct = false;
                    if (!xmlAttributes[9].InnerText.Equals((getProductInfo(unitOfSale.CompositionChanges.AddProducts )).ProviderCode))
                        flagMatchProduct = false;
                    if (!xmlAttributes[10].InnerText.Equals(unitOfSale.UnitSize))
                        flagMatchProduct = false;
                    if (!xmlAttributes[11].InnerText.Equals(unitOfSale.Title))
                        flagMatchProduct = false;
                    if (!xmlAttributes[14].InnerText.Equals(unitOfSale.UnitType))
                        flagMatchProduct = false;
                    int[] ints = new int[] { 4, 6, 7, 12, 13 };
                    foreach (int i in ints)
                    {
                        if (!xmlAttributes[i].InnerText.Equals(""))
                            flagMatchProduct = false;
                    }
                }
                if (flagMatchProduct)
                {
                    Console.WriteLine("CREATE AVCS UNIT OF SALE Action's Data is correct");
                    return;
                }
            }
            //if (!flagMatchProduct)
            //    Console.WriteLine("CREATE AVCS UNIT OF SALE Action's Data is incorrect");
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
        private static void verifyCreateENCCell(string childCell, XmlNodeList xmlAttributes)
        {

            bool flagMatchProduct = false;
            Console.WriteLine("Childcell:" + childCell);
            foreach (Product product in jsonPayload.Data.Products)
            {
                if ((childCell == product.ProductName) && (product.Status.IsNewCell))
                {
                    flagMatchProduct = true;
                    if (!xmlAttributes[0].InnerText.Equals(actionCounter.ToString()))
                        flagMatchProduct = false;
                    //xmlAttributes[1] as already checked
                    if (!xmlAttributes[2].InnerText.Equals("ENC CELL"))
                        flagMatchProduct = false;
                    if (!xmlAttributes[3].InnerText.Equals(product.ProductType[4..]))
                        flagMatchProduct = false;
                    if (!xmlAttributes[5].InnerText.Equals(product.ProductName))
                        flagMatchProduct = false;
                    if (!xmlAttributes[8].InnerText.Equals(product.Agency))
                        flagMatchProduct = false;
                    if (!xmlAttributes[9].InnerText.Equals(product.ProviderCode))
                        flagMatchProduct = false;
                    if (!xmlAttributes[10].InnerText.Equals(product.Size))
                        flagMatchProduct = false;
                    if (!xmlAttributes[11].InnerText.Equals(product.Title))
                        flagMatchProduct = false;
                    if (!xmlAttributes[12].InnerText.Equals(product.EditionNumber))
                        flagMatchProduct = false;
                    if (!xmlAttributes[13].InnerText.Equals(product.UpdateNumber))
                        flagMatchProduct = false;
                    int[] ints = new int[] { 6, 7, 14 };
                    foreach (int i in ints)
                    {
                        if (!xmlAttributes[i].InnerText.Equals(""))
                            flagMatchProduct = false;
                    }
                }
                if (flagMatchProduct)
                {

                    Console.WriteLine("CREATE ENC CELL Action's Data is correct");
                    return;
                }

            }
            //if (!flagMatchProduct)
            //    Console.WriteLine("CREATE ENC CELL Action's Data is incorrect");

        }

        private static List<string> formActionSeq()
        {

            List<string> ActionSeq = new List<string>();
            ActionSeq.Add("CREATE ENC CELL");
            ActionSeq.Add("CREATE AVCS UNIT OF SALE");
            ActionSeq.Add("ASSIGN CELL TO AVCS UNIT OF SALE");
            ActionSeq.Add("REPLACED WITH NEW ENC CELL");
            ActionSeq.Add("CHANGE ENC CELL");
            ActionSeq.Add("CHANGE AVCS UNIT OF SALE");
            ActionSeq.Add("UPDATE ENC CELL EDITION UPDATE NUMBER");
            ActionSeq.Add("REMOVE ENC CELL FROM AVCS UNIT OF SALE");
            ActionSeq.Add("CANCEL ENC CELL");
            ActionSeq.Add("CANCEL AVCS UNIT OF SALE");
            return ActionSeq;
        }
    }
}
