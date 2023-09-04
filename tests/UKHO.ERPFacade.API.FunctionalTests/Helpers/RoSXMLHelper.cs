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
    public class RoSXMLHelper
    {
        private static JsonInputRoSWebhookHelper UpdatedJsonPayload { get; set; }
        private static readonly List<string> s_attrNotMatched = new();

        public static async Task<bool> CheckXmlAttributes(JsonInputRoSWebhookHelper jsonPayload, string xmlFilePath,
            string updatedRequestBody)
        {
            UpdatedJsonPayload = JsonConvert.DeserializeObject<JsonInputRoSWebhookHelper>(updatedRequestBody);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(File.ReadAllText(xmlFilePath));

            while (xmlDoc.DocumentElement.Name == "soap:Envelope1" || xmlDoc.DocumentElement.Name == "soap:Body")
            {
                string tempXmlString = xmlDoc.DocumentElement.InnerXml;
                xmlDoc.LoadXml(tempXmlString);
            }

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlDoc.InnerXml));
            var reader = new XmlTextReader(ms) { Namespaces = false };
            var serializer = new XmlSerializer(typeof(Z_ADDS_ROS_INFO));
            var result = (Z_ADDS_ROS_INFO)serializer.Deserialize(reader);
            JsonInputRoSWebhookHelper.Recordsofsale roSJsonFields = jsonPayload.data.recordsOfSale;
            IM_ORDER roSXmlField = result.IM_ORDER;

            Assert.That(VerifyPresenseOfMandatoryXMLAtrributes(roSXmlField).Result,Is.True);
            Assert.That(UpdatedJsonPayload.data.correlationId.Equals(roSXmlField.GUID),
                "GUID in xml is same a corrid as in EES JSON");

            if (roSXmlField.SERVICETYPE.Equals(roSJsonFields.productType))
            {
                if (roSXmlField.LICTRANSACTION.Equals("MAINTAINHOLDINGS"))
                {
                    Assert.That(VerifyChangeLicense(roSXmlField, roSJsonFields), Is.True);
                }
            }
            await Task.CompletedTask;
            Console.WriteLine("XML has correct data");
            return true;
        }

        private static bool? VerifyChangeLicense(IM_ORDER roSResult,
            JsonInputRoSWebhookHelper.Recordsofsale roSJsonFields)
        {
            if (!roSResult.SOLDTOACC.Equals(roSJsonFields.distributorCustomerNumber))
                s_attrNotMatched.Add(nameof(roSResult.SOLDTOACC));
            if (!roSResult.LICENSEEACC.Equals(roSJsonFields.shippingCoNumber))
                s_attrNotMatched.Add(nameof(roSResult.LICENSEEACC));
            if (!roSResult.LICNO.Equals(roSJsonFields.sapId))
                s_attrNotMatched.Add(nameof(roSResult.LICNO));
            if (!roSResult.ECDISMANUF.Equals(roSJsonFields.upn))
                s_attrNotMatched.Add(nameof(roSResult.ECDISMANUF));
            if (!roSResult.VNAME.Equals(roSJsonFields.vesselName))
                s_attrNotMatched.Add(nameof(roSResult.VNAME));
            if (!roSResult.IMO.Equals(roSJsonFields.imoNumber))
                s_attrNotMatched.Add(nameof(roSResult.IMO));
            if (!roSResult.CALLSIGN.Equals(roSJsonFields.callSign))
                s_attrNotMatched.Add(nameof(roSResult.CALLSIGN));
            if (!roSResult.FLEET.Equals(roSJsonFields.fleetName))
                s_attrNotMatched.Add(nameof(roSResult.FLEET));
            if (!roSResult.USERS.Equals(roSJsonFields.numberLicenceUsers))
                s_attrNotMatched.Add(nameof(roSResult.USERS));

            string[] fieldNames = { "STARTDATE", "ENDDATE", "SHOREBASED", "LTYPE", "LICDUR", "PO", "ADSORDNO" };
            string[] fieldNamesProduct = { "ID", "ENDDA", "DURATION", "RENEW", "REPEAT" };
            IM_ORDERItem[] items = roSResult.PROD;
            VerifyBlankFields(roSResult, fieldNames);
            VerifyBlankProductFields(items[0], fieldNamesProduct);

            if (s_attrNotMatched.Count == 0)
            {
                Console.WriteLine("CHANGELICENCE event XML is correct");
                return true;
            }
            else
            {
                Console.WriteLine("CHANGELICENCE event XML is incorrect");
                Console.WriteLine("Not matching attributes are:");
                foreach (string attribute in s_attrNotMatched)
                { Console.WriteLine(attribute); }
                return false;
            }
        }

        private static void VerifyBlankFields(IM_ORDER licResult, string[] fieldNames)
        {
            foreach (string field in fieldNames)
            {
                if (!typeof(IM_ORDER).GetProperty(field).GetValue(licResult, null).Equals(""))
                    s_attrNotMatched.Add(typeof(IM_ORDER).GetProperty(field).Name);
            }
        }

        private static void VerifyBlankProductFields(IM_ORDERItem items, string[] fieldNamesProduct)
        {
            foreach (string field in fieldNamesProduct)
            {
                if (!typeof(Z_ADDS_ROSIM_ORDERItem).GetProperty(field).GetValue(items, null).Equals(""))
                    s_attrNotMatched.Add(typeof(Z_ADDS_ROSIM_ORDERItem).GetProperty(field).Name);
            }
        }

        public static async Task<bool> VerifyPresenseOfMandatoryXMLAtrributes(IM_ORDER order)
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
                                    " for action number:");
                    return false;
                }
            }

            Console.WriteLine("Mandatory attributes are present in  XML");
            await Task.CompletedTask;
            return true;
        }
    }
}
