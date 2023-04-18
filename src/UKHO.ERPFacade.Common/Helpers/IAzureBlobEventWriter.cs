using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.Common.IO
{
    public interface IAzureBlobEventWriter
    {
        Task UploadEvent(JObject eesEvent, string traceId, string correlationId);
    }
}
