using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Xml.Serialization;
using System.Xml;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    [TestFixture]
    public class RoSXmlHelper
    {
        private static JsonInputRoSWebhookHelper UpdatedJsonPayload { get; set; }
        private static readonly List<string> s_attrNotMatched = new();

        public static async Task<bool> CheckXmlAttributes(JsonInputRoSWebhookHelper jsonPayload, string xmlFilePath,
            string updatedRequestBody)
        {
            UpdatedJsonPayload = JsonConvert.DeserializeObject<JsonInputRoSWebhookHelper>(updatedRequestBody);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(File.ReadAllText(xmlFilePath));

            while (xmlDoc.DocumentElement.Name == "soap:Envelope" || xmlDoc.DocumentElement.Name == "soap:Body")
            {
                string tempXmlString = xmlDoc.DocumentElement.InnerXml;
                xmlDoc.LoadXml(tempXmlString);
            }

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlDoc.InnerXml));
            var reader = new XmlTextReader(ms) { Namespaces = false };
            var serializer = new XmlSerializer(typeof(Z_ADDS_ROS));
            var result = (Z_ADDS_ROS)serializer.Deserialize(reader);
            JsonInputRoSWebhookHelper.Recordsofsale roSJsonFields = UpdatedJsonPayload.data.recordsOfSale;
            Z_ADDS_ROSIM_ORDER roSXmlField = result.IM_ORDER;

            Assert.That(VerifyPresenseOfMandatoryXMLAtrributes(roSXmlField).Result,Is.True);
            Assert.That(UpdatedJsonPayload.data.correlationId.Equals(roSXmlField.GUID),
                "GUID in xml is same a corrid as in EES JSON");

            if (roSXmlField.SERVICETYPE.Equals(roSJsonFields.productType))
            {
                if (roSXmlField.LICTRANSACTION.Equals("MAINTAINHOLDINGS"))
                {
                  Assert.That(VerifyMaintainHolding(roSXmlField, roSJsonFields), Is.True);
                }
                if (roSXmlField.LICTRANSACTION.Equals("NEWLICENCE"))
                {
                    Assert.That(VerifyNewLicence(roSXmlField, roSJsonFields), Is.True);
                }
            }
            await Task.CompletedTask;
            Console.WriteLine("XML has correct data");
            return true;
        }

        private static bool? VerifyMaintainHolding(Z_ADDS_ROSIM_ORDER roSXmlField,
            JsonInputRoSWebhookHelper.Recordsofsale roSJsonFields)
        {
           
            if (!roSXmlField.LICNO.Equals(roSJsonFields.sapId))
                s_attrNotMatched.Add(nameof(roSXmlField.LICNO));
            if (!roSXmlField.PO.Equals(roSJsonFields.poref))
                s_attrNotMatched.Add(nameof(roSXmlField.PO));
            if (!roSXmlField.ADSORDNO.Equals(roSJsonFields.ordernumber))
                s_attrNotMatched.Add(nameof(roSXmlField.ADSORDNO));
            

            string[] fieldNames = { "SOLDTOACC", "LICENSEEACC", "STARTDATE", "ENDDATE", "VNAME", "IMO", "CALLSIGN", "SHOREBASED", "FLEET", "USERS", "ENDUSERID", "ECDISMANUF", "LTYPE", "LICDUR" };
            Z_ADDS_ROSIM_ORDERItem[] items = roSXmlField.PROD;
            JsonInputRoSWebhookHelper.Unitsofsale[] unitofsales = roSJsonFields.unitsOfSale;
            VerifyBlankFields(roSXmlField, fieldNames);
            VerifyProductFields(items, unitofsales);

            if (s_attrNotMatched.Count == 0)
            {
                Console.WriteLine("MAINTAINHOLDINGS event XML is correct");
                return true;
            }
            else
            {
                Console.WriteLine("MAINTAINHOLDINGS event XML is incorrect");
                Console.WriteLine("Not matching attributes are:");
                foreach (string attribute in s_attrNotMatched)
                { Console.WriteLine(attribute); }
                return false;
            }
        }
        private static bool? VerifyNewLicence(Z_ADDS_ROSIM_ORDER roSXmlField,
           JsonInputRoSWebhookHelper.Recordsofsale roSJsonFields)
        {

            if (!roSXmlField.SOLDTOACC.Equals(roSJsonFields.distributorCustomerNumber))
                s_attrNotMatched.Add(nameof(roSXmlField.SOLDTOACC));
            if (!roSXmlField.LICENSEEACC.Equals(roSJsonFields.shippingCoNumber))
                s_attrNotMatched.Add(nameof(roSXmlField.LICENSEEACC));
            if (!roSXmlField.STARTDATE.Equals(roSJsonFields.orderDate))
                s_attrNotMatched.Add(nameof(roSXmlField.STARTDATE));
            if (!roSXmlField.ENDDATE.Equals(roSJsonFields.holdingsExpiryDate))
                s_attrNotMatched.Add(nameof(roSXmlField.ENDDATE));
            if (!roSXmlField.VNAME.Equals(roSJsonFields.vesselName))
                s_attrNotMatched.Add(nameof(roSXmlField.VNAME));
            if (!roSXmlField.IMO.Equals(roSJsonFields.imoNumber))
                s_attrNotMatched.Add(nameof(roSXmlField.IMO));
            if (!roSXmlField.CALLSIGN.Equals(roSJsonFields.callSign))
                s_attrNotMatched.Add(nameof(roSXmlField.CALLSIGN));
            if (!roSXmlField.SHOREBASED.Equals(roSJsonFields.shoreBased))
                s_attrNotMatched.Add(nameof(roSXmlField.SHOREBASED));
            if (!roSXmlField.FLEET.Equals(roSJsonFields.fleetName))
                s_attrNotMatched.Add(nameof(roSXmlField.FLEET));
            if (!roSXmlField.USERS.Equals(roSJsonFields.numberLicenceUsers))
                s_attrNotMatched.Add(nameof(roSXmlField.USERS));
            if (!roSXmlField.ENDUSERID.Equals(roSJsonFields.licenseId))
                s_attrNotMatched.Add(nameof(roSXmlField.ENDUSERID));
            if (!roSXmlField.ECDISMANUF.Equals(roSJsonFields.upn))
                s_attrNotMatched.Add(nameof(roSXmlField.ECDISMANUF));
            if (!roSXmlField.LTYPE.Equals(roSJsonFields.licenceType))
                s_attrNotMatched.Add(nameof(roSXmlField.LTYPE));
            if (!roSXmlField.LICDUR.Equals(roSJsonFields.licenceDuration))
                s_attrNotMatched.Add(nameof(roSXmlField.LICDUR));
            


            string[] fieldNames = { "LICNO","FLEET" };
            string[] fieldNamesProduct = { "REPEAT" };
            Z_ADDS_ROSIM_ORDERItem[] items = roSXmlField.PROD;
            JsonInputRoSWebhookHelper.Unitsofsale[] unitofsales = roSJsonFields.unitsOfSale;
            VerifyBlankFields(roSXmlField, fieldNames);
            VerifyProductFields(items, unitofsales);
            VerifyBlankProductFields(items, fieldNamesProduct);
            if (s_attrNotMatched.Count == 0)
            {
                Console.WriteLine("New Licence event XML is correct");
                return true;
            }
            else
            {
                Console.WriteLine("New Licence event XML is incorrect");
                Console.WriteLine("Not matching attributes are:");
                foreach (string attribute in s_attrNotMatched)
                { Console.WriteLine(attribute); }
                return false;
            }
        }

        private static void VerifyBlankFields(Z_ADDS_ROSIM_ORDER roSXmlField, string[] fieldNames)
        {
            foreach (string field in fieldNames)
            {
                if (!typeof(Z_ADDS_ROSIM_ORDER).GetProperty(field).GetValue(roSXmlField, null).Equals(""))
                    s_attrNotMatched.Add(typeof(Z_ADDS_ROSIM_ORDER).GetProperty(field).Name);
            }
        }

        private static void VerifyProductFields(Z_ADDS_ROSIM_ORDERItem[] roSResult, JsonInputRoSWebhookHelper.Unitsofsale[] unitofsales)
        {
            int i = 0;
            foreach (Z_ADDS_ROSIM_ORDERItem prodxml in roSResult)
                //for (int i=0; i<=unitofsales.Length;i++)
                {
                
                    if (!prodxml.ID.Equals(unitofsales[i].unitName))
                        s_attrNotMatched.Add(nameof(prodxml.ID));
                    if (!prodxml.ENDDA.Equals(unitofsales[i].endDate))
                        s_attrNotMatched.Add(nameof(prodxml.ENDDA));
                    if (!prodxml.DURATION.Equals(unitofsales[i].duration))
                        s_attrNotMatched.Add(nameof(prodxml.DURATION));
                    if (!prodxml.RENEW.Equals(unitofsales[i].renew))
                        s_attrNotMatched.Add(nameof(prodxml.RENEW));
                   if (!prodxml.REPEAT.Equals(unitofsales[i].repeat))
                        s_attrNotMatched.Add(nameof(prodxml.REPEAT));
                i++; 
                }
                
            
        }

        public static async Task<bool> VerifyPresenseOfMandatoryXMLAtrributes(Z_ADDS_ROSIM_ORDER order)
        {
            List<string> ActionAttributesSeq = new List<string>();
            ActionAttributesSeq = Config.TestConfig.RosLicenceUpdateXMLList.ToList<string>();
            List<string> CurrentActionAttributes = new List<string>();
            CurrentActionAttributes.Clear();
            Type arrayType = order.GetType();
            System.Reflection.PropertyInfo[] properties = arrayType.GetProperties();
            foreach (System.Reflection.PropertyInfo property in properties)
            {
                CurrentActionAttributes.Add(property.Name);
            }
            for (int i = 0; i < ActionAttributesSeq.Count; i++)
            {
                if (CurrentActionAttributes[i] != ActionAttributesSeq[i])
                {
                    Console.WriteLine("First missed Attribute is:" + ActionAttributesSeq[i] +
                                    " for Record of sales fields:");
                    return false;
                }
            }


            List<string> ActionAttributesSeqProd = new List<string>();
            ActionAttributesSeqProd = Config.TestConfig.RoSLicenceUpdatedProdXMLList.ToList<string>();
            List<string> CurrentActionAttributesProd = new List<string>();
            CurrentActionAttributesProd.Clear();
            Z_ADDS_ROSIM_ORDERItem[] items =order.PROD;
            foreach (Z_ADDS_ROSIM_ORDERItem prodorderItem in items) {

                Type arrayTypeProd = prodorderItem.GetType();
                System.Reflection.PropertyInfo[] propertiesProd = arrayTypeProd.GetProperties();
                foreach (System.Reflection.PropertyInfo propertyprod in propertiesProd)
                {
                    CurrentActionAttributesProd.Add(propertyprod.Name);
                }
                for (int i = 0; i < ActionAttributesSeqProd.Count; i++)
                {
                    if (CurrentActionAttributesProd[i] != ActionAttributesSeqProd[i])
                    {
                        Console.WriteLine("First missed Attribute is:" + ActionAttributesSeqProd[i] +
                                        " for RoS UnitOfSales feild:");
                        return false;
                    }
                }
            }
            Console.WriteLine("Mandatory attributes are present in  XML");
            await Task.CompletedTask;
            return true;
        }
        private static void VerifyBlankProductFields(Z_ADDS_ROSIM_ORDERItem[] items, string[] fieldNamesProduct)
        {
            foreach (Z_ADDS_ROSIM_ORDERItem blankItem in items)
            {
                foreach (string field in fieldNamesProduct)
                {
                    if (!typeof(Z_ADDS_ROSIM_ORDERItem).GetProperty(field).GetValue(blankItem, null).Equals(""))
                        s_attrNotMatched.Add(typeof(Z_ADDS_ROSIM_ORDERItem).GetProperty(field).Name);
                }
            }
        }
    }
}
