using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.CleanUp.WebJob.Services
{
    public class CleanUpService : ICleanUpService
    {
        private readonly IOptions<CleanupWebJobConfiguration> _cleanupWebjobConfig;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;

        public CleanUpService(IOptions<CleanupWebJobConfiguration> cleanupWebjobConfig,
                               IAzureTableReaderWriter azureTableReaderWriter,
                               IAzureBlobReaderWriter azureBlobReaderWriter)
        {
            _cleanupWebjobConfig = cleanupWebjobConfig ?? throw new ArgumentNullException(nameof(cleanupWebjobConfig));
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
            _azureBlobReaderWriter = azureBlobReaderWriter ?? throw new ArgumentNullException(nameof(azureBlobReaderWriter));
        }

        public void Clean()
        {
            CleanS57Data(PartitionKeys.S57PartitionKey);
        }

        private void CleanS57Data(string partitionKey)
        {
            var entities = _azureTableReaderWriter.GetAllEntities(partitionKey);

            foreach (var entity in entities)
            {
                if (entity["RequestDateTime"] == null)
                    continue;

                var correlationId = entity.RowKey.ToString();

                TimeSpan timediff = DateTime.Now - Convert.ToDateTime(entity["RequestDateTime"].ToString());

                if (timediff.Days > int.Parse(_cleanupWebjobConfig.Value.CleanUpDurationInDays))
                {
                    Task.FromResult(_azureTableReaderWriter.DeleteEntityAsync(partitionKey, correlationId));

                    _azureBlobReaderWriter.DeleteContainer(correlationId);
                }
            }
        }
    }
}
