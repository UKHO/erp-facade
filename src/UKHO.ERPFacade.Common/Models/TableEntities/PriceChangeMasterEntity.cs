using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Data.Tables;

namespace UKHO.ERPFacade.Common.Models.TableEntities
{
    [ExcludeFromCodeCoverage]
    public class PriceChangeMasterEntity : ITableEntity
    {
        public string RowKey { get; set; } = default!;

        public string PartitionKey { get; set; } = default!;

        public DateTimeOffset? Timestamp { get; set; } = default!;

        public string CorrId { get; set; } = default!;

        public string Status { get; set; } = default!;

        public DateTime? PublishDateTime { get; set; } = default!;

        public DateTime? CreatedDateTime { get; set; } = default!;

        public int ProductCount { get; set; } = default!;

        public ETag ETag { get; set; } = default!;
    }
}
