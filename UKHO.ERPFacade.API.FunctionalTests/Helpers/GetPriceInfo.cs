using RestSharp;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class GetPriceInfo
    {
        private readonly TestConfiguration _config = new();
        private readonly string _requestBody = "{}";
        public RestResponse GetPriceInfoResponse()
        {
            var client = new RestClient(_config.erpfacadeConfig.BaseUrl);
            var request = new RestRequest(_config.erpfacadeConfig.BaseUrl + $"/erpfacade/getpriceinfo", Method.Post);

            request.AddParameter("application/json", _requestBody, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;
            RestResponse response = client.Execute(request);
            return response;
        }
    }
}
