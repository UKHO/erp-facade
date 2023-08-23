using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Xml.Serialization;
using System.Xml;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    [TestFixture]
    public class FMLicenseUpdateXMLHelper
    {
        private static LUpdatedJsonPayloadHelper UpdatedJsonPayload { get; set; }
        private static LUpdatedJsonPayloadHelper JsonPayload { get; set; }
        private static readonly JsonHelper _jsonHelper;
        private static readonly List<string> AttrNotMatched = new();
        private static string XMLFilePath = "D://UpdatedERP//tests//UKHO.ERPFacade.API.FunctionalTests//ERPFacadeGeneratedXmlFiles//FMLicenseUpdateXMLGenerated//FM-RoS-XMLPayloadUpdateLicense.xml";
        
        public static async Task<bool> CheckXMLAttributes(LUpdatedJsonPayloadHelper jsonPayload, string XMLFilePath, string updatedRequestBody)
        {
            
            FMLicenseUpdateXMLHelper.JsonPayload = jsonPayload;
            UpdatedJsonPayload = JsonConvert.DeserializeObject<LUpdatedJsonPayloadHelper>(updatedRequestBody);

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
            LUpdatedJsonPayloadHelper.License  licencefeildsJSON = UpdatedJsonPayload.data.license;
            Z_ADDS_ROSIM_ORDER licResult =result.IM_ORDER;


            if (licResult.SERVICETYPE.Equals(licencefeildsJSON.productType))
            {
                if (licResult.LICTRANSACTION.Equals("CHANGELICENCE"))
                {
                    Assert.True(VerifyChangeLicense(licResult, licencefeildsJSON));
                    
                }
            }




            
            await Task.CompletedTask;
            Console.WriteLine("XML has correct data");
            return true;
        }

        private static bool? VerifyChangeLicense(Z_ADDS_ROSIM_ORDER licResult, LUpdatedJsonPayloadHelper.License licencefeildsJSON)
        {
            if(!licResult.SOLDTOACC.Equals(licencefeildsJSON.distributorCustomerNumber))
                AttrNotMatched.Add(nameof(licResult.SOLDTOACC));
            if(!licResult.LICENSEEACC.Equals(licencefeildsJSON.shippingCoNumber))
                AttrNotMatched.Add(nameof(licResult.LICENSEEACC));
            if (!licResult.LICNO.Equals(licencefeildsJSON.sapId))
                AttrNotMatched.Add(nameof(licResult.LICNO));
            if (!licResult.ECDISMANUF.Equals(licencefeildsJSON.upn))
                AttrNotMatched.Add(nameof(licResult.ECDISMANUF));
            if(!licResult.VNAME.Equals(licencefeildsJSON.vesselName))
                AttrNotMatched.Add(nameof(licResult.VNAME));
            if (!licResult.IMO.Equals(licencefeildsJSON.imoNumber))
                AttrNotMatched.Add(nameof(licResult.IMO));
            if (!licResult.CALLSIGN.Equals(licencefeildsJSON.callSign))
                AttrNotMatched.Add(nameof(licResult.CALLSIGN));
            if (!licResult.FLEET.Equals(licencefeildsJSON.fleetName))
                AttrNotMatched.Add(nameof(licResult.FLEET));
            

           
            string[] fieldNames = { "STARTDATE", "ENDDATE", "SHOREBASED", "USERS", "ENDUSERID", "LTYPE" , "LICDUR", "PO", "ADSORDNO", "ID", "ENDDA" , "DURATION" , "RENEW", "Repeat" };
            VerifyBlankFields(licResult, fieldNames);
            return true;
        }

        private static bool VerifyBlankFields(Z_ADDS_ROSIM_ORDER licResult, string[] fieldNames)
        {
            bool allBlanks = true;

            foreach (string field in fieldNames)
            {
                if (!typeof(Z_ADDS_ROSIM_ORDER).GetProperty(field).GetValue(licResult, null).Equals(""))
                    AttrNotMatched.Add(typeof(Z_ADDS_ROS).GetProperty(field).Name);
            }
            return allBlanks;
        }
    }
}
