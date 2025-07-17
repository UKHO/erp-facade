using System.Text.Json;
using UKHO.ADDS.Mocks.Headers;
using UKHO.ADDS.Mocks.Markdown;
using UKHO.ADDS.Mocks.States;

namespace UKHO.ADDS.Mocks.ERP.Override.Mocks.sap
{
    public class PostRecordOfSaleEvent : ServiceEndpointMock
    {
        public override void RegisterSingleEndpoint(IEndpointMock endpoint) => endpoint.MapPost("/z_adds_ros.asmx", (HttpRequest request) =>
        {
            var rawRequestBody = new StreamReader(request.Body).ReadToEnd();
            var body = JsonDocument.Parse(rawRequestBody).RootElement;
            var correlationId = string.Empty; 

            if ( body.TryGetProperty("data", out var data) &&  data.TryGetProperty("correlationId", out var correlationIdElement))
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

            return Results.Ok("Record of sale record received successfully");
        })
        .Produces<string>()
        .WithEndpointMetadata(endpoint, d =>
        {
            d.Append(new MarkdownHeader("SAP Post /z_adds_ros.asmx  ", 3));
        });
    }
}
