using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Data.Tables;

namespace UKHO.ERPFacade.Common.Models.TableEntities
{
    [ExcludeFromCodeCoverage]
    public class UnitPriceChangeEntity : ITableEntity
    {
        public string RowKey { get; set; } = default!;

        public string PartitionKey { get; set; } = default!;

        public DateTimeOffset? Timestamp { get; set; } = default!;

        public string MasterCorrId { get; set; } = default!;

        public string EventId { get; set; } = default!;

        public string Status { get; set; } = default!;

        public string UnitName { get; set; } = default!;

        public DateTime? PublishDateTime { get; set; } = default!;

        public ETag ETag { get; set; } = default!;
    }
}
