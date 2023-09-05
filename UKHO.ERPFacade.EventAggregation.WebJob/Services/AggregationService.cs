using UKHO.ERPFacade.Common.IO.Azure;

namespace UKHO.ERPFacade.EventAggregation.WebJob.Services
{
    public class AggregationService : IAggregationService
    {
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;

        public AggregationService(IAzureTableReaderWriter azureTableReaderWriter)
        {
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
        }

        //Paramter to this method will be new model which will contain correlationid & relatedEvents
        public void ProcessSlicedEvents()
        {
            //Logic to validate if all related events are received in ERP Facade
            //{
                //check if all relatedEvents are exists in given container(correlationid)
            //}

            //Call _recordOfSaleSapMessageBuilder.BuildSapMessageXml();

            //Call SAP client
        }
    }
}
