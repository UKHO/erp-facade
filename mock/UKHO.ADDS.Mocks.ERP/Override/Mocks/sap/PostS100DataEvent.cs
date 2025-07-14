using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using UKHO.ADDS.Mocks.Markdown;
using UKHO.ADDS.Mocks.States;

namespace UKHO.ADDS.Mocks.ERP.Override.Mocks.sap
{
    public class PostS100DataEvent : ServiceEndpointMock
    {
        public override void RegisterSingleEndpoint(IEndpointMock endpoint) => endpoint.MapPost("/z_shop_mat_info.asmx", (HttpRequest request) =>
        {
            
            var rawRequestBody = new StreamReader(request.Body).ReadToEnd();
            var body = JsonDocument.Parse(rawRequestBody).RootElement;
            var correlationId = string.Empty;

            if (body.TryGetProperty("data", out JsonElement data) && data.TryGetProperty("correlationId", out JsonElement correlationIdElement))
            {
                correlationId = correlationIdElement.GetString() ?? string.Empty;
            }

            if (correlationId.Contains("SAP401Unauthorized", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Unauthorized();
            }

            if (correlationId.Contains("SAP500InternalServerError", StringComparison.OrdinalIgnoreCase))
            {
                return Results.InternalServerError("Internal Server Error");
            }

            if (correlationId.Contains("SAP404NotFound", StringComparison.OrdinalIgnoreCase))
            {
                return Results.InternalServerError("Internal Server Error");
            }

            callBackErpFacade(request);

            return Results.Ok("S100 record received successfully");
        })
        .Produces<string>()
        .WithEndpointMetadata(endpoint, d =>
        {
            d.Append(new MarkdownHeader("SAP Post /z_shop_mat_info.asmx  ", 3));
        });

        private void callBackErpFacade(HttpRequest request)
        {
            var requestBody = request.Body.ToString();
            //  var xmlDocument = XDocument.Parse(requestBody);
            var xmlDocument = new XDocument();
          //  var url = GetConfiguration["SapCallbackConfiguration:Url"];
           // var test = GetConfiguration["Logging:LogLevel:Default"];


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

            //Task.Run(async () =>
            //{
            //    await Task.Delay(1000);

            //    using var httpClient = new HttpClient()
            //    {
            //        BaseAddress = new Uri(GetConfiguration["ErpFacadeConfiguration:ApiBaseUrl"])
            //    };
            //    var callbackRequest = new HttpRequestMessage(HttpMethod.Post, GetConfiguration["SapCallbackConfiguration:Url"])
            //    {
            //        Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            //    };
            //    callbackRequest.Headers.Add("X-API-Key", GetConfiguration["SapCallbackConfiguration:SharedApiKey"]);
            //    var callbackResponse = await httpClient.SendAsync(callbackRequest);
            //});
        }
    }
}
