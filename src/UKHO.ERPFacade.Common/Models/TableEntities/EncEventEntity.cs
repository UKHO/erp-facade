using Azure;
using Azure.Data.Tables;
using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Models.TableEntities
{
    [ExcludeFromCodeCoverage]
    public class EncEventEntity : ITableEntity
    {
        public string RowKey { get; set; } = default!;

        public string PartitionKey { get; set; } = default!;

        public DateTimeOffset? Timestamp { get; set; } = default!;

        public string CorrelationId { get; set; } = default!;     

        public DateTime? RequestDateTime { get; set; } = default!;
        
        public ETag ETag { get; set; } = default!;
    }
}
