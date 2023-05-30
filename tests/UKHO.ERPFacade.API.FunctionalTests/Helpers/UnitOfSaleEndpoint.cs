
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class UnitOfSaleEndpoint 
    {

        private readonly RestClient client;
        private readonly RestClient client2;
        private readonly ADAuthTokenProvider _authToken;
        private AzureTableHelper azureTableHelper { get; set; }


        public UnitOfSaleEndpoint(string url)
        {
            _authToken = new();
            var options = new RestClientOptions(url);
            client = new RestClient(options);
            azureTableHelper = new AzureTableHelper();
        }
        public void  PostUnitOfSaleResponse()
        {
            Console.WriteLine("In Unit of Sale");
           
            return;
        }

        public async Task<RestResponse> PostUoSResponseAsync(string filePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }

            
            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);
           

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            JsonPayloadHelper jsonPayload = JsonConvert.DeserializeObject<JsonPayloadHelper>(requestBody);
            string traceID = jsonPayload.Data.TraceId;



            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Assert.That(azureTableHelper.CheckResponseDateTime(traceID), Is.True, "ResponseDateTime Not updated in Azure table");
            }
            //Assert.That(azureTableHelper.CheckResponseDateTime(traceID), Is.True, "ResponseDateTime Not updated in Azure table");
            return response;
        }
    }
}
