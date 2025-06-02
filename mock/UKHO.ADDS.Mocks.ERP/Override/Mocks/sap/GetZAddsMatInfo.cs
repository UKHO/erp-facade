using UKHO.ADDS.Mocks.Markdown;
using UKHO.ADDS.Mocks.States;

namespace UKHO.ADDS.Mocks.ERP.Override.Mocks.sap    
{
    public class GetZAddsMatInfo : ServiceEndpointMock
    {
        public override void RegisterSingleEndpoint(IEndpointMock endpoint) =>
            endpoint.MapPost("/z_adds_mat_info.asmx", (HttpRequest request) =>
            {
                var state = GetState(request);

                switch (state)
                {
                    case WellKnownState.Default:
                        // ADDS Mock will have the 'default' state unless we have told it otherwise
                        return Results.Ok();

                    default:
                        // Just send default responses
                        return WellKnownStateHandler.HandleWellKnownState(state);
                }
            })
                .Produces<string>()
                .WithEndpointMetadata(endpoint, d =>
                {
                    d.Append(new MarkdownHeader("SAP Get z_adds_mat_info  ", 3));
                    d.Append(new MarkdownParagraph("Get SAP z_adds_mat_info details return 200"));
                });
    }

}
