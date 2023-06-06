
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class UnitOfSaleEndpoint 
    {

        private readonly RestClient client;

        private AzureTableHelper azureTableHelper { get; set; }


        public UnitOfSaleEndpoint(string url)
        {
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
            List<UoSInputJSONHelper> jsonPayload = JsonConvert.DeserializeObject<List<UoSInputJSONHelper>>(requestBody);
            string traceID = jsonPayload[0].Corrid;


            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Assert.That(azureTableHelper.CheckResponseDateTime(traceID), Is.True, "ResponseDateTime Not updated in Azure table");
            }
            
            return response;
        }
    }
}
