using System.Text.Json;
using UKHO.ADDS.Mocks.Markdown;
using UKHO.ADDS.Mocks.States;

namespace UKHO.ADDS.Mocks.ERP.Override.Mocks.sap
{
    public class PostEesEvent : ServiceEndpointMock
    {
        public override void RegisterSingleEndpoint(IEndpointMock endpoint) => endpoint.MapPost("/Ese", (HttpRequest request) =>
        {
            var rawRequestBody = new StreamReader(request.Body).ReadToEnd();
            var body = JsonDocument.Parse(rawRequestBody).RootElement;
            var correlationId = string.Empty;

            if (body.TryGetProperty("data", out var data) && data.TryGetProperty("correlationId", out var correlationIdElement))
            {
                correlationId = correlationIdElement.GetString() ?? string.Empty;
            }

            if (correlationId.Contains("SAP200OkEES401Unauthorized", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Unauthorized();
            }

            if (correlationId.Contains("SAP200OkEES500InternalServerError", StringComparison.OrdinalIgnoreCase))
            {
                return Results.InternalServerError("Internal Server Error");
            }

            if (correlationId.Contains("SAP200OkEES400BadRequest", StringComparison.OrdinalIgnoreCase))
            {
                return Results.InternalServerError("BadRequest");
            }            

            if (correlationId.Contains("SAP404NotFound", StringComparison.OrdinalIgnoreCase))
            {
                return Results.InternalServerError("NotFound");
            }

            if (correlationId.Contains("SAP200OkEES403Forbidden", StringComparison.OrdinalIgnoreCase))
            {
                return Results.InternalServerError("Forbidden");
            }

            return Results.Ok("EES record received successfully");
        })
        .Produces<string>()
        .WithEndpointMetadata(endpoint, d =>
        {
            d.Append(new MarkdownHeader("SAP Post /Ees  ", 3));
        });
    }
}
