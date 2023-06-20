using Azure;
using Azure.Data.Tables;
using System.Diagnostics.CodeAnalysis;

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

        public ETag ETag { get; set; } = default!;
    }
}
