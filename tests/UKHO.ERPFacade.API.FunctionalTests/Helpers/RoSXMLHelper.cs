﻿using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Xml.Serialization;
using System.Xml;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    [TestFixture]
    public class RoSXmlHelper
    {
        private static JsonInputRoSWebhookEvent jsonPayload;
        private static readonly List<string> s_attrNotMatched = new();

        public static async Task<bool> CheckXmlAttributes(string generatedXmlFilePath, string requestBody, List<JsonInputRoSWebhookEvent> listOfEventJson)
        {
            jsonPayload = JsonConvert.DeserializeObject<JsonInputRoSWebhookEvent>(requestBody);

            //Read XML payload generated by webjob
            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(await File.ReadAllTextAsync(generatedXmlFilePath));
            while (xmlDoc.DocumentElement is { Name: XmlTemplateInfo.SoapEnvelope or XmlTemplateInfo.SoapBody })
            {
                string xmlString = xmlDoc.DocumentElement.InnerXml;
                xmlDoc.LoadXml(xmlString);
            }

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlDoc.InnerXml));
            var reader = new XmlTextReader(memoryStream) { Namespaces = false };
            var serializer = new XmlSerializer(typeof(Z_ADDS_ROS));
            var result = (Z_ADDS_ROS)serializer.Deserialize(reader);


            if (result == null)
            {
                return false;
            }

            Z_ADDS_ROSIM_ORDER rosXmlPayload = result.IM_ORDER;

            Assert.Multiple(() =>
            {
                Assert.That(jsonPayload.data.correlationId, Is.EqualTo(rosXmlPayload.GUID), "GUID in xml is same a corrid as in EES JSON");
                Assert.That(VerifyPresenseOfMandatoryXMLAtrributes(rosXmlPayload).Result, Is.True);
            });

            JsonInputRoSWebhookEvent.Recordsofsale roSJsonFields = jsonPayload.data.recordsOfSale;

            if (rosXmlPayload.SERVICETYPE.Equals(roSJsonFields.productType))
            {
                switch (rosXmlPayload.LICTRANSACTION)
                {
                    case RoSTransactionTypes.MaintainHoldingsType:
                        Assert.That(VerifyMaintainHolding(rosXmlPayload, roSJsonFields, listOfEventJson), Is.True);
                        break;
                    case RoSTransactionTypes.NewLicenceType:
                        Assert.That(VerifyNewLicence(rosXmlPayload, roSJsonFields, listOfEventJson), Is.True);
                        break;
                    case RoSTransactionTypes.MigrateNewLicenceType:
                        Assert.That(VerifyMigrateNewLicence(rosXmlPayload, roSJsonFields, listOfEventJson), Is.True);
                        break;
                    case RoSTransactionTypes.MigrateExistingLicenceType:
                        Assert.That(VerifyMigrateExistingLicence(rosXmlPayload, roSJsonFields, listOfEventJson), Is.True);
                        break;
                    case RoSTransactionTypes.ConvertLicenceType:
                        Assert.That(VerifyConvertTrialToFullLicence(rosXmlPayload, roSJsonFields, listOfEventJson), Is.True);
                        break;
                }
            }
            await Task.CompletedTask;

            Console.WriteLine("XML has correct data");

            return true;
        }

        private static bool? VerifyMaintainHolding(Z_ADDS_ROSIM_ORDER rosXmlPayload, JsonInputRoSWebhookEvent.Recordsofsale roSJsonPayload, IEnumerable<JsonInputRoSWebhookEvent> listOfEventJsons)
        {
            if (!rosXmlPayload.LICNO.Equals(roSJsonPayload.sapId))
                s_attrNotMatched.Add(nameof(rosXmlPayload.LICNO));
            if (!rosXmlPayload.PO.Equals(roSJsonPayload.poref))
                s_attrNotMatched.Add(nameof(rosXmlPayload.PO));
            if (!rosXmlPayload.ADSORDNO.Equals(roSJsonPayload.ordernumber))
                s_attrNotMatched.Add(nameof(rosXmlPayload.ADSORDNO));

            string[] fieldNames = { XmlFields.SoldToAcc, XmlFields.LicenceAcc, XmlFields.StartDate, XmlFields.EndDate, XmlFields.VName, XmlFields.Imo, XmlFields.CallSign, XmlFields.ShoreBased, XmlFields.Fleet, XmlFields.Users, XmlFields.EndUserId, XmlFields.EcdisManUf, XmlFields.LType, XmlFields.LicDur };
            Z_ADDS_ROSIM_ORDERItem[] xmlUnitOfSaleItems = rosXmlPayload.PROD;
            List<JsonInputRoSWebhookEvent.Unitsofsale> jsonUnitOfSalesItems = listOfEventJsons.SelectMany(eventJson => eventJson.data.recordsOfSale.unitsOfSale).ToList();

            VerifyBlankFields(rosXmlPayload, fieldNames);
            VerifyProductFields(xmlUnitOfSaleItems, jsonUnitOfSalesItems);

            if (s_attrNotMatched.Count == 0)
            {
                Console.WriteLine("MAINTAINHOLDINGS event XML is correct");
                return true;
            }
            {
                Console.WriteLine("MAINTAINHOLDINGS event XML is incorrect");
                Console.WriteLine("Not matching attributes are:");
                foreach (string attribute in s_attrNotMatched)
                { Console.WriteLine(attribute); }
                return false;
            }
        }
        private static bool? VerifyNewLicence(Z_ADDS_ROSIM_ORDER rosXmlPayload, JsonInputRoSWebhookEvent.Recordsofsale roSJsonPayload, List<JsonInputRoSWebhookEvent> listOfEventJsons)
        {
            if (!rosXmlPayload.SOLDTOACC.Equals(roSJsonPayload.distributorCustomerNumber))
                s_attrNotMatched.Add(nameof(rosXmlPayload.SOLDTOACC));
            if (!rosXmlPayload.LICENSEEACC.Equals(roSJsonPayload.shippingCoNumber))
                s_attrNotMatched.Add(nameof(rosXmlPayload.LICENSEEACC));
            if (!rosXmlPayload.STARTDATE.Equals(roSJsonPayload.orderDate))
                s_attrNotMatched.Add(nameof(rosXmlPayload.STARTDATE));
            if (!rosXmlPayload.ENDDATE.Equals(roSJsonPayload.holdingsExpiryDate))
                s_attrNotMatched.Add(nameof(rosXmlPayload.ENDDATE));
            if (!rosXmlPayload.VNAME.Equals(roSJsonPayload.vesselName))
                s_attrNotMatched.Add(nameof(rosXmlPayload.VNAME));
            if (!rosXmlPayload.IMO.Equals(roSJsonPayload.imoNumber))
                s_attrNotMatched.Add(nameof(rosXmlPayload.IMO));
            if (!rosXmlPayload.CALLSIGN.Equals(roSJsonPayload.callSign))
                s_attrNotMatched.Add(nameof(rosXmlPayload.CALLSIGN));
            if (!rosXmlPayload.SHOREBASED.Equals(roSJsonPayload.shoreBased))
                s_attrNotMatched.Add(nameof(rosXmlPayload.SHOREBASED));
            if (!rosXmlPayload.FLEET.Equals(roSJsonPayload.fleetName))
                s_attrNotMatched.Add(nameof(rosXmlPayload.FLEET));
            if (!rosXmlPayload.USERS.Equals(roSJsonPayload.numberLicenceUsers))
                s_attrNotMatched.Add(nameof(rosXmlPayload.USERS));
            if (!rosXmlPayload.ENDUSERID.Equals(roSJsonPayload.licenseId))
                s_attrNotMatched.Add(nameof(rosXmlPayload.ENDUSERID));
            if (!rosXmlPayload.ECDISMANUF.Equals(roSJsonPayload.ecdisManufacturerName))
                s_attrNotMatched.Add(nameof(rosXmlPayload.ECDISMANUF));
            if (!rosXmlPayload.LTYPE.Equals(roSJsonPayload.licenceType))
                s_attrNotMatched.Add(nameof(rosXmlPayload.LTYPE));
            if (!rosXmlPayload.LICDUR.Equals(roSJsonPayload.licenceDuration))
                s_attrNotMatched.Add(nameof(rosXmlPayload.LICDUR));

            List<string> blankFieldNames = new() { XmlFields.LicNo, XmlFields.Fleet };
            List<string> blankProductFieldNames = new() { XmlFields.Repeat };
            Z_ADDS_ROSIM_ORDERItem[] xmlUnitOfSaleItems = rosXmlPayload.PROD;
            List<JsonInputRoSWebhookEvent.Unitsofsale> jsonUnitOfSalesItems = listOfEventJsons.SelectMany(eventJson => eventJson.data.recordsOfSale.unitsOfSale).ToList();

            VerifyBlankFields(rosXmlPayload, blankFieldNames);
            VerifyProductFields(xmlUnitOfSaleItems, jsonUnitOfSalesItems);
            VerifyBlankProductFields(xmlUnitOfSaleItems, blankProductFieldNames);

            if (s_attrNotMatched.Count == 0)
            {
                Console.WriteLine("NEWLICENCE event XML is correct");
                return true;
            }
            {
                Console.WriteLine("NEWLICENCE event XML is incorrect");
                Console.WriteLine("Not matching attributes are:");
                foreach (string attribute in s_attrNotMatched)
                { Console.WriteLine(attribute); }
                return false;
            }
        }

        private static bool? VerifyMigrateNewLicence(Z_ADDS_ROSIM_ORDER rosXmlPayload, JsonInputRoSWebhookEvent.Recordsofsale roSJsonPayload, List<JsonInputRoSWebhookEvent> listOfEventJsons)
        {
            if (!rosXmlPayload.SOLDTOACC.Equals(roSJsonPayload.distributorCustomerNumber))
                s_attrNotMatched.Add(nameof(rosXmlPayload.SOLDTOACC));
            if (!rosXmlPayload.LICENSEEACC.Equals(roSJsonPayload.shippingCoNumber))
                s_attrNotMatched.Add(nameof(rosXmlPayload.LICENSEEACC));
            if (!rosXmlPayload.STARTDATE.Equals(roSJsonPayload.orderDate))
                s_attrNotMatched.Add(nameof(rosXmlPayload.STARTDATE));
            if (!rosXmlPayload.ENDDATE.Equals(roSJsonPayload.holdingsExpiryDate))
                s_attrNotMatched.Add(nameof(rosXmlPayload.ENDDATE));
            if (!rosXmlPayload.VNAME.Equals(roSJsonPayload.vesselName))
                s_attrNotMatched.Add(nameof(rosXmlPayload.VNAME));
            if (!rosXmlPayload.IMO.Equals(roSJsonPayload.imoNumber))
                s_attrNotMatched.Add(nameof(rosXmlPayload.IMO));
            if (!rosXmlPayload.CALLSIGN.Equals(roSJsonPayload.callSign))
                s_attrNotMatched.Add(nameof(rosXmlPayload.CALLSIGN));
            if (!rosXmlPayload.SHOREBASED.Equals(roSJsonPayload.shoreBased))
                s_attrNotMatched.Add(nameof(rosXmlPayload.SHOREBASED));
            if (!rosXmlPayload.USERS.Equals(roSJsonPayload.numberLicenceUsers))
                s_attrNotMatched.Add(nameof(rosXmlPayload.USERS));
            if (!rosXmlPayload.ENDUSERID.Equals(roSJsonPayload.licenseId))
                s_attrNotMatched.Add(nameof(rosXmlPayload.ENDUSERID));
            if (!rosXmlPayload.ECDISMANUF.Equals(roSJsonPayload.ecdisManufacturerName))
                s_attrNotMatched.Add(nameof(rosXmlPayload.ECDISMANUF));
            if (!rosXmlPayload.LTYPE.Equals(roSJsonPayload.licenceType))
                s_attrNotMatched.Add(nameof(rosXmlPayload.LTYPE));
            if (!rosXmlPayload.LICDUR.Equals(roSJsonPayload.licenceDuration))
                s_attrNotMatched.Add(nameof(rosXmlPayload.LICDUR));

            List<string> blankFieldNames = new() { XmlFields.LicNo, XmlFields.Fleet };
            List<string> blankProductFieldNames = new() { XmlFields.Repeat };
            Z_ADDS_ROSIM_ORDERItem[] xmlUnitOfSaleItems = rosXmlPayload.PROD;
            List<JsonInputRoSWebhookEvent.Unitsofsale> jsonUnitOfSalesItems = listOfEventJsons.SelectMany(eventJson => eventJson.data.recordsOfSale.unitsOfSale).ToList();

            VerifyBlankFields(rosXmlPayload, blankFieldNames);
            VerifyProductFields(xmlUnitOfSaleItems, jsonUnitOfSalesItems);
            VerifyBlankProductFields(xmlUnitOfSaleItems, blankProductFieldNames);

            if (s_attrNotMatched.Count == 0)
            {
                Console.WriteLine("NEWLICENCE event XML is correct");
                return true;
            }
            {
                Console.WriteLine("NEWLICENCE event XML is incorrect");
                Console.WriteLine("Not matching attributes are:");
                foreach (string attribute in s_attrNotMatched)
                { Console.WriteLine(attribute); }
                return false;
            }
        }


        private static bool? VerifyMigrateExistingLicence(Z_ADDS_ROSIM_ORDER rosXmlPayload, JsonInputRoSWebhookEvent.Recordsofsale roSJsonPayload, IEnumerable<JsonInputRoSWebhookEvent> listOfEventJsons)
        {
            if (!rosXmlPayload.LICNO.Equals(roSJsonPayload.sapId))
                s_attrNotMatched.Add(nameof(rosXmlPayload.LICNO));
            if (!rosXmlPayload.PO.Equals(roSJsonPayload.poref))
                s_attrNotMatched.Add(nameof(rosXmlPayload.PO));
            if (!rosXmlPayload.ADSORDNO.Equals(roSJsonPayload.ordernumber))
                s_attrNotMatched.Add(nameof(rosXmlPayload.ADSORDNO));

            string[] fieldNames = { XmlFields.SoldToAcc, XmlFields.LicenceAcc, XmlFields.StartDate, XmlFields.EndDate, XmlFields.VName, XmlFields.Imo, XmlFields.CallSign, XmlFields.ShoreBased, XmlFields.Fleet, XmlFields.Users, XmlFields.EndUserId, XmlFields.EcdisManUf, XmlFields.LType, XmlFields.LicDur };
            Z_ADDS_ROSIM_ORDERItem[] xmlUnitOfSaleItems = rosXmlPayload.PROD;
            List<JsonInputRoSWebhookEvent.Unitsofsale> jsonUnitOfSalesItems = listOfEventJsons.SelectMany(eventJson => eventJson.data.recordsOfSale.unitsOfSale).ToList();

            VerifyBlankFields(rosXmlPayload, fieldNames);
            VerifyProductFields(xmlUnitOfSaleItems, jsonUnitOfSalesItems);

            if (s_attrNotMatched.Count == 0)
            {
                Console.WriteLine("MAINTAINHOLDINGS event XML is correct");
                return true;
            }
            {
                Console.WriteLine("MAINTAINHOLDINGS event XML is incorrect");
                Console.WriteLine("Not matching attributes are:");
                foreach (string attribute in s_attrNotMatched)
                { Console.WriteLine(attribute); }
                return false;
            }
        }

        private static bool? VerifyConvertTrialToFullLicence(Z_ADDS_ROSIM_ORDER rosXmlPayload, JsonInputRoSWebhookEvent.Recordsofsale roSJsonPayload, IEnumerable<JsonInputRoSWebhookEvent> listOfEventJsons)
        {
            if (!rosXmlPayload.LICNO.Equals(roSJsonPayload.sapId))
                s_attrNotMatched.Add(nameof(rosXmlPayload.LICNO));
            if (!rosXmlPayload.LTYPE.Equals(roSJsonPayload.licenceType))
                s_attrNotMatched.Add(nameof(rosXmlPayload.LTYPE));
            if (!rosXmlPayload.LICDUR.Equals(roSJsonPayload.licenceDuration))
                s_attrNotMatched.Add(nameof(rosXmlPayload.LICDUR));
            if (!rosXmlPayload.PO.Equals(roSJsonPayload.poref))
                s_attrNotMatched.Add(nameof(rosXmlPayload.PO));
            if (!rosXmlPayload.ADSORDNO.Equals(roSJsonPayload.ordernumber))
                s_attrNotMatched.Add(nameof(rosXmlPayload.ADSORDNO));

            string[] fieldNames = { XmlFields.SoldToAcc, XmlFields.LicenceAcc, XmlFields.StartDate, XmlFields.EndDate, XmlFields.VName, XmlFields.Imo, XmlFields.CallSign, XmlFields.ShoreBased, XmlFields.Fleet, XmlFields.Users, XmlFields.EndUserId, XmlFields.EcdisManUf };
            Z_ADDS_ROSIM_ORDERItem[] xmlUnitOfSaleItems = rosXmlPayload.PROD;
            List<JsonInputRoSWebhookEvent.Unitsofsale> jsonUnitOfSalesItems = listOfEventJsons.SelectMany(eventJson => eventJson.data.recordsOfSale.unitsOfSale).ToList();

            VerifyBlankFields(rosXmlPayload, fieldNames);
            VerifyProductFields(xmlUnitOfSaleItems, jsonUnitOfSalesItems);

            if (s_attrNotMatched.Count == 0)
            {
                Console.WriteLine("CONVERTLICENCE event XML is correct");
                return true;
            }
            {
                Console.WriteLine("CONVERTLICENCE event XML is incorrect");
                Console.WriteLine("Not matching attributes are:");
                foreach (string attribute in s_attrNotMatched)
                { Console.WriteLine(attribute); }
                return false;
            }
        }

        private static void VerifyBlankFields(Z_ADDS_ROSIM_ORDER rosXmlPayload, IEnumerable<string> blankFieldNames)
        {
            foreach (string field in blankFieldNames)
            {
                if (!typeof(Z_ADDS_ROSIM_ORDER).GetProperty(field).GetValue(rosXmlPayload, null).Equals(""))
                    s_attrNotMatched.Add(typeof(Z_ADDS_ROSIM_ORDER).GetProperty(field).Name);
            }
        }

        private static void VerifyProductFields(IEnumerable<Z_ADDS_ROSIM_ORDERItem> rosProductList, List<JsonInputRoSWebhookEvent.Unitsofsale> unitofsales)
        {
            int i = 0;
            foreach (Z_ADDS_ROSIM_ORDERItem product in rosProductList)
            {
                if (!product.ID.Equals(unitofsales[i].unitName))
                    s_attrNotMatched.Add(nameof(product.ID));
                if (!product.ENDDA.Equals(unitofsales[i].endDate))
                    s_attrNotMatched.Add(nameof(product.ENDDA));
                if (!product.DURATION.Equals(unitofsales[i].duration))
                    s_attrNotMatched.Add(nameof(product.DURATION));
                if (!product.RENEW.Equals(unitofsales[i].renew))
                    s_attrNotMatched.Add(nameof(product.RENEW));
                if (!product.REPEAT.Equals(unitofsales[i].repeat))
                    s_attrNotMatched.Add(nameof(product.REPEAT));
                i++;
            }
        }

        public static async Task<bool> VerifyPresenseOfMandatoryXMLAtrributes(Z_ADDS_ROSIM_ORDER order)
        {
            List<string> actionAttributesSeq = Config.TestConfig.RosLicenceUpdateXmlList.ToList<string>();
            List<string> currentActionAttributes = new();
            currentActionAttributes.Clear();
            Type arrayType = order.GetType();
            System.Reflection.PropertyInfo[] properties = arrayType.GetProperties();
            currentActionAttributes.AddRange(properties.Select(property => property.Name));
            for (int i = 0; i < actionAttributesSeq.Count; i++)
            {
                if (currentActionAttributes[i] != actionAttributesSeq[i])
                {
                    Console.WriteLine("First missed Attribute is:" + actionAttributesSeq[i] +
                                    " for Record of sales fields:");
                    return false;
                }
            }

            List<string> actionAttributesSeqProd = Config.TestConfig.RoSLicenceUpdatedProdXmlList.ToList<string>();
            List<string> currentActionAttributesProd = new();
            currentActionAttributesProd.Clear();
            Z_ADDS_ROSIM_ORDERItem[] items = order.PROD;
            foreach (Z_ADDS_ROSIM_ORDERItem prodorderItem in items)
            {
                Type arrayTypeProd = prodorderItem.GetType();
                System.Reflection.PropertyInfo[] propertiesProd = arrayTypeProd.GetProperties();
                currentActionAttributesProd.AddRange(propertiesProd.Select(property => property.Name));
                for (int i = 0; i < actionAttributesSeqProd.Count; i++)
                {
                    if (currentActionAttributesProd[i] != actionAttributesSeqProd[i])
                    {
                        Console.WriteLine("First missed Attribute is:" + actionAttributesSeqProd[i] +
                                        " for RoS UnitOfSales field:");
                        return false;
                    }
                }
            }
            Console.WriteLine("Mandatory attributes are present in  XML");
            await Task.CompletedTask;
            return true;
        }

        private static void VerifyBlankProductFields(IEnumerable<Z_ADDS_ROSIM_ORDERItem> xmlUnitOfSaleItems, List<string> blankProductFieldNames)
        {
            foreach (Z_ADDS_ROSIM_ORDERItem item in xmlUnitOfSaleItems)
            {
                foreach (string field in blankProductFieldNames.Where(field => !typeof(Z_ADDS_ROSIM_ORDERItem).GetProperty(field).GetValue(item, null).Equals("")))
                {
                    s_attrNotMatched.Add(typeof(Z_ADDS_ROSIM_ORDERItem).GetProperty(field).Name);
                }
            }
        }
    }
}
