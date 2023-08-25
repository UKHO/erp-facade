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
    public class FmLicenceUpdateXMLHelper
    {
        private static JsonInputLicenceUpdateHelper UpdatedJsonPayload { get; set; }
        private static readonly List<string> AttrNotMatched = new();

        public static async Task<bool> CheckXMLAttributes(JsonInputLicenceUpdateHelper jsonPayload, string XMLFilePath,
            string updatedRequestBody)
        {
            UpdatedJsonPayload = JsonConvert.DeserializeObject<JsonInputLicenceUpdateHelper>(updatedRequestBody);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(File.ReadAllText(XMLFilePath));

            while (xmlDoc.DocumentElement.Name == "soap:Envelope" || xmlDoc.DocumentElement.Name == "soap:Body")
            {
                string tempXmlString = xmlDoc.DocumentElement.InnerXml;
                xmlDoc.LoadXml(tempXmlString);
            }

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlDoc.InnerXml));
            var reader = new XmlTextReader(ms) { Namespaces = false };
            var serializer = new XmlSerializer(typeof(Z_ADDS_ROS));
            var result = (Z_ADDS_ROS)serializer.Deserialize(reader);
            JsonInputLicenceUpdateHelper.License licenceJsonFields = UpdatedJsonPayload.data.license;
            Z_ADDS_ROSIM_ORDER licResult = result.IM_ORDER;

            Assert.True(VerifyPresenseOfMandatoryXMLAtrributes(licResult).Result);
            Assert.That(UpdatedJsonPayload.data.correlationId.Equals(licResult.GUID),
                "GUID in xml is same a corrid as in EES JSON");

            if (licResult.SERVICETYPE.Equals(licenceJsonFields.productType))
            {
                if (licResult.LICTRANSACTION.Equals("CHANGELICENCE"))
                {
                    Assert.True(VerifyChangeLicense(licResult, licenceJsonFields));
                }
            }
            await Task.CompletedTask;
            Console.WriteLine("XML has correct data");
            return true;
        }

        private static bool? VerifyChangeLicense(Z_ADDS_ROSIM_ORDER licResult,
            JsonInputLicenceUpdateHelper.License licenceFieldsJson)
        {
            if (!licResult.SOLDTOACC.Equals(licenceFieldsJson.distributorCustomerNumber))
                AttrNotMatched.Add(nameof(licResult.SOLDTOACC));
            if (!licResult.LICENSEEACC.Equals(licenceFieldsJson.shippingCoNumber))
                AttrNotMatched.Add(nameof(licResult.LICENSEEACC));
            if (!licResult.LICNO.Equals(licenceFieldsJson.sapId))
                AttrNotMatched.Add(nameof(licResult.LICNO));
            if (!licResult.ECDISMANUF.Equals(licenceFieldsJson.upn))
                AttrNotMatched.Add(nameof(licResult.ECDISMANUF));
            if (!licResult.VNAME.Equals(licenceFieldsJson.vesselName))
                AttrNotMatched.Add(nameof(licResult.VNAME));
            if (!licResult.IMO.Equals(licenceFieldsJson.imoNumber))
                AttrNotMatched.Add(nameof(licResult.IMO));
            if (!licResult.CALLSIGN.Equals(licenceFieldsJson.callSign))
                AttrNotMatched.Add(nameof(licResult.CALLSIGN));
            if (!licResult.FLEET.Equals(licenceFieldsJson.fleetName))
                AttrNotMatched.Add(nameof(licResult.FLEET));
            if (!licResult.ENDUSERID.Equals(licenceFieldsJson.licenseId))
                AttrNotMatched.Add(nameof(licResult.ENDUSERID));
            if (!licResult.USERS.Equals(licenceFieldsJson.numberLicenceUsers))
                AttrNotMatched.Add(nameof(licResult.USERS));

            string[] fieldNames = { "STARTDATE", "ENDDATE", "SHOREBASED", "LTYPE", "LICDUR", "PO", "ADSORDNO" };
            string[] fieldNamesProduct = { "ID", "ENDDA", "DURATION", "RENEW", "REPEAT" };
            Z_ADDS_ROSIM_ORDERItem[] items = licResult.PROD;
            VerifyBlankFields(licResult, fieldNames);
            VerifyBlankProductFields(items[0], fieldNamesProduct);

            if (AttrNotMatched.Count == 0)
            {
                Console.WriteLine("CHANGELICENCE event XML is correct");
                return true;
            }
            else
            {
                Console.WriteLine("CHANGELICENCE event XML is incorrect");
                Console.WriteLine("Not matching attributes are:");
                foreach (string attribute in AttrNotMatched)
                { Console.WriteLine(attribute); }
                return false;
            }
            return true;
        }

        private static void VerifyBlankFields(Z_ADDS_ROSIM_ORDER licResult, string[] fieldNames)
        {
            foreach (string field in fieldNames)
            {
                if (!typeof(Z_ADDS_ROSIM_ORDER).GetProperty(field).GetValue(licResult, null).Equals(""))
                    AttrNotMatched.Add(typeof(Z_ADDS_ROSIM_ORDER).GetProperty(field).Name);
            }
        }

        private static void VerifyBlankProductFields(Z_ADDS_ROSIM_ORDERItem items, string[] fieldNamesProduct)
        {
            foreach (string field in fieldNamesProduct)
            {
                if (!typeof(Z_ADDS_ROSIM_ORDERItem).GetProperty(field).GetValue(items, null).Equals(""))
                    AttrNotMatched.Add(typeof(Z_ADDS_ROSIM_ORDERItem).GetProperty(field).Name);
            }
        }

        public static async Task<bool> VerifyPresenseOfMandatoryXMLAtrributes(Z_ADDS_ROSIM_ORDER order)
        {
            List<string> ActionAttributesSeq = new List<string>();
            ActionAttributesSeq = Config.TestConfig.RosLicenceUpdateXMLList.ToList<string>();
            List<string> CurrentActionAttributes = new List<string>();
            CurrentActionAttributes.Clear();
            Type arrayType = order.GetType();
            var properties = arrayType.GetProperties();
            foreach (var property in properties)
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
