using System.Text.Json;
using UKHO.ADDS.Mocks.Markdown;
using UKHO.ADDS.Mocks.States;

namespace UKHO.ADDS.Mocks.ERP.Override.Mocks.sap
{
    public class PostS57EncEvent : ServiceEndpointMock
    {
        public override void RegisterSingleEndpoint(IEndpointMock endpoint) =>
            endpoint.MapPost("/z_adds_mat_info", (HttpRequest request) =>
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

                return Results.Ok("S57 record received successfully");
            })
                .Produces<string>()
                .WithEndpointMetadata(endpoint, d =>
                {
                    d.Append(new MarkdownHeader("SAP Post z_adds_mat_info  ", 3));                   
                });
    }

}
