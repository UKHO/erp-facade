
using Microsoft.Identity.Client;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class UnitOfSaleEndpoint 
    {

        private readonly RestClient client;        
             
        public UnitOfSaleEndpoint(string url)
        {
            var options = new RestClientOptions(url);
            client = new RestClient(options);         

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
            return response;
        }
    }
}
