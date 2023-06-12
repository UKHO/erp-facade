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

        private AzureBlobStorageHelper azureBlobStorageHelper { get; set; }


        public UnitOfSaleEndpoint(string url)
        {
            var options = new RestClientOptions(url);
            client = new RestClient(options);
            azureTableHelper = new AzureTableHelper();
            azureBlobStorageHelper = new();
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

        public async Task<RestResponse> PostUoSResponseAsyncWithJSON(string filePath, string generatedJSONFolder, string token)
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
            string correlationId = jsonPayload[0].Corrid;


            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Assert.That(azureTableHelper.CheckResponseDateTime(correlationId), Is.True, "ResponseDateTime Not updated in Azure table");
                //1)download final JSON from container
                string generatedJSONFilePath = azureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedJSONFolder, correlationId, "json");
                //2)compare corr Id from final JSON to SAP(input) request
                //3)Check for Unique product array from final JSON against product present in input SAP request
                //4) Check for unique effective date array for every product
                //6) Check for unique duration for every effective date
                //7) Check RRP field values for all unitofsale prices
            }

            return response;
        }
    }
}
