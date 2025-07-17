using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace UKHO.ADDS.Mocks.ERP.Override.Mocks.sap
{
    public static class SapCallBack
    { 
        public static void callBackErpFacade(HttpRequest request, IConfiguration _configuration)
        {
            var requestBody = request.Body.ToString();
            var xmlDocument = XDocument.Parse(requestBody);

            XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace rfcNs = "urn:sap-com:document:sap:rfc:functions";

            var correlationIdElement = xmlDocument
                .Element(soapNs + "Envelope")?
                .Element(soapNs + "Body")?
                .Element(rfcNs + "Z_SHOP_MAT_INFO")?
                .Element("IM_MATINFO")?
                .Element("CORRID");

            var correlationId = correlationIdElement?.Value;

            var payload = string.Format("{{\"correlationId\":\"{0}\"}}", correlationId);

            Task.Run(async () =>
            {
                await Task.Delay(1000);

                using var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(_configuration["ErpFacadeConfiguration:ApiBaseUrl"])
                };
                var callbackRequest = new HttpRequestMessage(HttpMethod.Post, _configuration["SapCallbackConfiguration:Url"])
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json"),
                };
                callbackRequest.Headers.Add("X-API-Key", _configuration["SapCallbackConfiguration:SharedApiKey"]);
                var callbackResponse = await httpClient.SendAsync(callbackRequest);
            });
        }
    }
}
