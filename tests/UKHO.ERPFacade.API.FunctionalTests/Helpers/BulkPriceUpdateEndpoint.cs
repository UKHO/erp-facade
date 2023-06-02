using RestSharp;


namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class BulkPriceUpdateEndpoint 
    {

        private readonly RestClient client;
        

        public BulkPriceUpdateEndpoint(string url)
        {
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
