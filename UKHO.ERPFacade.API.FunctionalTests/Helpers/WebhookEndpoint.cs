using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    
    public class WebhookEndpoint
    {
        private readonly string _requestBody = "{}";
        public RestResponse GetWebhookResponse()
        {
            var client=new RestClient();
            var request=new RestRequest();

            request.AddParameter("application/json", _requestBody, ParameterType.RequestBody);
            RestResponse response=client.Execute(request);
            return response;
        }
    }
}
