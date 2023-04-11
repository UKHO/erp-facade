using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using System.Diagnostics;

namespace UKHO.ERPFacade.API.Services
{
    public class TimeoutService : IHostedService
    {
        private Timer? _timer = null;
        private ILogger<TimeoutService> _logger;
        private double TIMEOUT_SPAN = 35;

        public TimeoutService(ILogger<TimeoutService> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckTimeoutTraces, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));
            await Task.CompletedTask;
        }

        private void CheckTimeoutTraces(object? state)
        {
            var tablename = "MockDataTable";
            var accountName = "mockstorageacct";
            var storageAccountKey = "YHUtIbzjaLIl3OaiWxwy2WOvPLPxoEOG9k8bJfdq5uhfIyhCSwqsaJohOILC4/OU1Ti9fTpYqGDi+AStRiOutg==";
            var storageUri = "https://mockstorageacct.table.core.windows.net/MockDataTable";

            var tableClient = new TableClient(
                new Uri(storageUri),
                tablename,
                new TableSharedKeyCredential(accountName, storageAccountKey));


            var _tableData = tableClient.Query<TableItem>(entity => 
            (entity.IsLogged.HasValue ? !entity.IsLogged.Value : true) && 
            ((!entity.ResponseTime.HasValue && ((entity.RequestTime - DateTime.Now) > TimeSpan.FromMinutes(30))) 
            ||
            (entity.ResponseTime.HasValue && ((entity.ResponseTime - entity.RequestTime) > TimeSpan.FromMinutes(30))))
            );

            foreach (var tableitem in _tableData)
            {
                LogAndUpdateEntity(tableClient, tableitem.PartitionKey, tableitem.RowKey, tableitem.ETag);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            await Task.CompletedTask;
        }

        public void LogAndUpdateEntity(TableClient tableClient,string partitionKey, string rowKey, ETag eTag) 
        {
            _logger.LogWarning("");
            TableEntity tableEntity = new TableEntity(partitionKey, rowKey)
                            {
                                 {"IsLogged", true }
                            };

            tableClient.UpdateEntity(tableEntity, eTag);
        }


        public class TableItem : ITableEntity
        {

            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public DateTimeOffset? Timestamp { get; set; }

            public string Data { get; set; }
            public DateTimeOffset? RequestTime { get; set; }
            public DateTimeOffset? ResponseTime { get; set; }
            public bool? IsLogged { get; set; }
            public ETag ETag { get; set; }
        }

    }
}
