using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    public interface IAzureQueueReaderWriter
    {
        Task AddMessageAsync(JObject rosEventJson);
    }
}
