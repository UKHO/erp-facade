using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.Common.Helpers
{
    public interface IAzureBlobStorageHelper
    {
        Task UploadEvent(JObject eesEvent, string traceId);
    }
}
