using System;
using System.IO.Abstractions;
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
        public override void RegisterSingleEndpoint(IEndpointMock endpoint) => endpoint.MapPost("/z_shop_mat_info.asmx", async (HttpRequest request) =>
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            string body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            var xmlDocument = XDocument.Parse(body);
            var correlationId = xmlDocument.Descendants("CORRID").FirstOrDefault()?.Value ?? string.Empty;

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

            callBackErpFacade(request, xmlDocument);

            return Results.Ok("S100 record received successfully");
        })
        .Produces<string>()
        .WithEndpointMetadata(endpoint, d =>
        {
            d.Append(new MarkdownHeader("SAP Post /z_shop_mat_info.asmx  ", 3));
        });

        private void callBackErpFacade(HttpRequest request, XDocument xmlDocument)
        {
            var jsonString = File.ReadAllText("./Override/Files/sap/config.json");
            using var doc = JsonDocument.Parse(jsonString);

            doc.RootElement.TryGetProperty("ErpFacadeConfiguration", out JsonElement erpFacadeApiBase);
            var erpFacadeApiBaseUrl = erpFacadeApiBase.GetProperty("ApiBaseUrl").GetString() ?? string.Empty;

            doc.RootElement.TryGetProperty("SharedApiKeyConfiguration", out JsonElement sharedApiKeyConfiguration);
            var sharedApiKey = sharedApiKeyConfiguration.GetProperty("SharedApiKey").GetString() ?? string.Empty;

            doc.RootElement.TryGetProperty("SapCallbackConfiguration", out JsonElement SapCallbackConfiguration);
            var sapCallbackConfigurationUrl = SapCallbackConfiguration.GetProperty("Url").GetString() ?? string.Empty;
                               
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
                    BaseAddress = new Uri(erpFacadeApiBaseUrl)
                };
                var callbackRequest = new HttpRequestMessage(HttpMethod.Post, sapCallbackConfigurationUrl)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json"),
                };
                callbackRequest.Headers.Add("X-API-Key", sharedApiKey);
                var callbackResponse = await httpClient.SendAsync(callbackRequest);
            });
        }
    }
}
