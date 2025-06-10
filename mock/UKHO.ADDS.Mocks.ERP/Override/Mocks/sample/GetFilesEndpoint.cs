using UKHO.ADDS.Mocks.Markdown;
using UKHO.ADDS.Mocks.Mime;
using UKHO.ADDS.Mocks.States;

namespace UKHO.ADDS.Mocks.SampleService.Override.Mocks.sample
{
    public class GetFilesEndpoint : ServiceEndpointMock
    {
        public override void RegisterSingleEndpoint(IEndpointMock endpoint) =>
            endpoint.MapGet("/files", (HttpRequest request) =>
            {
                var state = GetState(request);

                switch (state)
                {
                    case WellKnownState.Default:
                        // ADDS Mock will have the 'default' state unless we have told it otherwise
                        return Results.Ok("This is a result");

                    case "get-file":

                        var pathResult = GetFile("readme.txt");

                        if (pathResult.IsSuccess(out var file))
                        {
                            return Results.File(file.Open(), file.MimeType);
                        }

                        return Results.NotFound("Could not find the path in the /files GET method");

                    default:
                        // Just send default responses
                        return WellKnownStateHandler.HandleWellKnownState(state);
                }
            })
                .Produces<string>()
                .WithEndpointMetadata(endpoint, d =>
                {
                    d.Append(new MarkdownHeader("Gets a file", 3));
                    d.Append(new MarkdownParagraph("Just a demo method, nothing too exciting"));
                });
    }
}
