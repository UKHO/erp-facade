using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    public interface IAzureBlobEventWriter
    {
        Task UploadEvent(JObject eesEvent, string traceId);
    }
}
