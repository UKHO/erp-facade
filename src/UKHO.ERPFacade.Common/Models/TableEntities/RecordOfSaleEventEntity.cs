﻿using System.Diagnostics.CodeAnalysis;
using Azure.Data.Tables;
using Azure;

namespace UKHO.ERPFacade.Common.Models.TableEntities
{ 
    [ExcludeFromCodeCoverage]
    public class RecordOfSaleEventEntity : ITableEntity
    {
        public string RowKey { get; set; } = default!;

        public string PartitionKey { get; set; } = default!;

        public DateTimeOffset? Timestamp { get; set; } = default!;

        public string CorrelationId { get; set; } = default!;

        public string Status { get; set; } = default!;
         
        public ETag ETag { get; set; } = default!;
    }
}
