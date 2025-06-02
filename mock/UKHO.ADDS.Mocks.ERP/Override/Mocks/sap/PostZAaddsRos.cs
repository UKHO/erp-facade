using UKHO.ADDS.Mocks.Markdown;
using UKHO.ADDS.Mocks.Mime;
using UKHO.ADDS.Mocks.States;

namespace UKHO.ADDS.Mocks.Configuration.Mocks.sap
{
    public class PostZAaddsRos : ServiceEndpointMock
    {
        public override void RegisterSingleEndpoint(IEndpointMock endpoint) =>
            endpoint.MapPost("/z_adds_ros.asmx", (HttpRequest request) =>
            {
                var state = GetState(request);

                switch (state)
                {
                    case WellKnownState.Default:

                        var pathResult = endpoint.GetFile("response.xml");

                        if (pathResult.IsSuccess(out var file))
                        {
                            return Results.File(file.Path, MimeType.Text.Xml);
                        }

                        return Results.NotFound("Could not response xml");

                    default:
                        // Just send default responses
                        return WellKnownStateHandler.HandleWellKnownState(state);
                }
            })
            .Produces<string>()
            .WithEndpointMetadata(endpoint, d =>
            {
                d.Append(new MarkdownHeader("SAP Post to z_adds_ros.asmx ", 3));
                d.Append(new MarkdownParagraph("return 200 with the xml body"));
            });
    }
}
