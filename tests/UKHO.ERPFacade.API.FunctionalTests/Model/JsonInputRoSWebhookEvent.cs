﻿using Newtonsoft.Json;

namespace UKHO.ERPFacade.API.FunctionalTests.Model
{
    public class JsonInputRoSWebhookEvent
    {

        
            public string specversion { get; set; }
            public string type { get; set; }
            public string source { get; set; }
            public string id { get; set; }
            public DateTime time { get; set; }
            public string subject { get; set; }
            public string datacontenttype { get; set; }
            public Data data { get; set; }
        

        public class Data
        {
            public string correlationId { get; set; }
            public string[] relatedEvents { get; set; }
            public Recordsofsale recordsOfSale { get; set; }
        }

        public class Recordsofsale
        {
            public string licenseId { get; set; }
            public string productType { get; set; }
            public string transactionType { get; set; }
            public string distributorCustomerNumber { get; set; }
            public string shippingCoNumber { get; set; }
            public string ordernumber { get; set; }
            public string orderDate { get; set; }
            [JsonProperty("po-ref")]
            public string poref { get; set; }
            public string holdingsExpiryDate { get; set; }
            public string sapId { get; set; }
            public string vesselName { get; set; }
            public string imoNumber { get; set; }
            public string callSign { get; set; }
            public string licenceType { get; set; }
            public string shoreBased { get; set; }
            public string fleetName { get; set; }
            public string numberLicenceUsers { get; set; }
            public string ecdisManufacturerName { get; set; }
            public string licenceDuration { get; set; }
            public Unitsofsale[] unitsOfSale { get; set; }
        }

        public class Unitsofsale
        {
            public string unitName { get; set; }
            public string endDate { get; set; }
            public string duration { get; set; }
            public string renew { get; set; }
            public string repeat { get; set; }
        }

    }
}
