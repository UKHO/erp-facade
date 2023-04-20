﻿using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.Common.IO
{
    public interface IAzureTableReaderWriter
    {
        Task UpsertEntity(JObject eesEvent, string traceId);
        Task<EESEventEntity> GetEntity(string traceId);
    }
}
