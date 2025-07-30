using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.Common.Operations.IO.Azure
{
    public interface IAzureQueueReaderWriter
    {
        Task AddMessageAsync(JObject rosEventJson);
    }
}
