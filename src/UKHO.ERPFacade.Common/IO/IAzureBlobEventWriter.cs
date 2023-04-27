using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.Common.IO
{
    public interface IAzureBlobEventWriter
    {
        Task UploadEvent(JObject eesEvent, string traceId);

        Task UploadXMLEvent(string xml, string traceId);
    }
}