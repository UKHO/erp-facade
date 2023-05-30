
using Microsoft.Identity.Client;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class BulkPriceUpdateEndpoint 
    {

        private readonly RestClient client;
        private readonly RestClient client2;
        private readonly ADAuthTokenProvider _authToken;
        

        public BulkPriceUpdateEndpoint(string url)
        {
            _authToken = new();
            var options = new RestClientOptions(url);
            client = new RestClient(options);         

        }
        public void  PostBulkPriceUpdateResponse()
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
    }
}
