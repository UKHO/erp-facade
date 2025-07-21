using System.Text.Json;
using System.Xml.Linq;
using UKHO.ADDS.Mocks.Headers;
using UKHO.ADDS.Mocks.Markdown;
using UKHO.ADDS.Mocks.States;

namespace UKHO.ADDS.Mocks.ERP.Override.Mocks.sap
{
    public class PostRecordOfSaleEvent : ServiceEndpointMock
    {
        public override void RegisterSingleEndpoint(IEndpointMock endpoint) => endpoint.MapPost("/z_adds_ros.asmx", async (HttpRequest request) =>
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

            return Results.Ok("Record of sale record received successfully");
        })
        .Produces<string>()
        .WithEndpointMetadata(endpoint, d =>
        {
            d.Append(new MarkdownHeader("SAP Post /z_adds_ros.asmx  ", 3));
        });
    }
}
