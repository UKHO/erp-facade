
using FluentAssertions;
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
        private JSONHelper jsonHelper { get; set; }


        public UnitOfSaleEndpoint(string url)
        {
            var options = new RestClientOptions(url);
            client = new RestClient(options);
            azureTableHelper = new AzureTableHelper();
            jsonHelper = new();
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
            List<UoSInputJSONHelper> jsonUoSInputPayload = JsonConvert.DeserializeObject<List<UoSInputJSONHelper>>(requestBody);
            string traceID = jsonUoSInputPayload[0].Corrid;


            //if (response.StatusCode == System.Net.HttpStatusCode.OK)
            //{
                Assert.That(azureTableHelper.CheckResponseDateTime(traceID), Is.True, "ResponseDateTime Not updated in Azure table");
                //var generatedJSONFilePath =
                var generatedJSONFilePath = "C:\\Users\\Sadha1501493\\GitHubRepo\\erp-facade\\tests\\UKHO.ERPFacade.API.FunctionalTests\\ERPFacadeGeneratedJSONFiles\\367ce4a4-1d62-4f56-b359-230601new001.JSON";
                Assert.That(jsonHelper.verifyUniqueProducts(jsonUoSInputPayload, generatedJSONFilePath), Is.True, "Final UoS JSON has duplicate UnitsOfSalesPrices");
            // }

            return response;
        }
    }
}
