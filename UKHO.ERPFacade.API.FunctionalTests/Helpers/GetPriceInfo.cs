using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class GetPriceInfo
    {
        private readonly TestConfiguration _config = new();
        private readonly string _requestBody = "{}";
        public RestResponse GetPriceInfoResponse()
        {
            var client = new RestClient(_config.erpfacadeDevConfig.BaseUrl);
            var request = new RestRequest(_config.erpfacadeDevConfig.BaseUrl + $"/erpfacade/getpriceinfo", Method.Post);

            request.AddParameter("application/json", _requestBody, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;
            RestResponse response = client.Execute(request);
            return response;
        }
    }
}
