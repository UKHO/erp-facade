using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Model;

using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using Microsoft.Extensions.Options;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class BulkPriceUpdateEndpoint
    {

        private readonly RestClient client;
        private readonly JSONHelper _jSONHelper;
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));
        

        public BulkPriceUpdateEndpoint(string url)
        {
            var options = new RestClientOptions(url);
            client = new RestClient(options);         

        }
        public void PostBulkPriceUpdateResponseJSON()
        {
            Console.WriteLine("In Bulk Price Update");
            return;
        }

        public async Task<RestResponse> PostBPUpdateResponseAsync(string filePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/erpfacade/bulkpriceinformation", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostBPUpdateResponseAsyncWithJson(string filePath, string generatedProductJsonFolder, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/erpfacade/bulkpriceinformation", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            return response;
        }

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            return response;
        }
    }
}
