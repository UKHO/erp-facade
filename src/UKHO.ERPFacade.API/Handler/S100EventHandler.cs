using System.Xml;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.S100Event;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.API.Handler
{
    public class S100EventHandler : EventHandler<S100EventData>
    {
        private readonly IOptions<SapConfiguration> _sapConfig;
        public S100EventHandler(ILogger<S100EventHandler> logger,
                                    IAzureTableReaderWriter azureTableReaderWriter,
                                    IAzureBlobEventWriter azureBlobEventWriter,
                                    ISapClient sapClient,
                                    IXmlHelper xmlHelper,
                                    IFileSystemHelper fileSystemHelper,
                                    IOptions<SapConfiguration> sapConfig) : base(logger,azureTableReaderWriter, azureBlobEventWriter, sapClient, xmlHelper, fileSystemHelper)
        {
            _sapConfig = sapConfig;
        }
        public override IEventData PrepareModel(string encEventJson)
        {
            IEventData eventData = new S100EventData();
            eventData.EventData = JsonConvert.DeserializeObject<S100EventPayloadData>(encEventJson);
            eventData.SapEndpointForEvent = _sapConfig.Value.SapEndpointForEncEvent;
            eventData.SapUsernameForEvent = _sapConfig.Value.SapUsernameForEncEvent;
            eventData.SapPasswordForEvent = _sapConfig.Value.SapPasswordForEncEvent;
            eventData.SapServiceOperationForEvent = _sapConfig.Value.SapServiceOperationForEncEvent;
            return eventData;
        }

        public override void BuildEncCellActions(S100EventData eventData, XmlDocument soapXml, XmlNode? actionItemNode)
        {
            throw new NotImplementedException();
        }

        public override void BuildUnitActions(S100EventData eventData, XmlDocument soapXml, XmlNode? actionItemNode)
        {
            throw new NotImplementedException();
        }
    }
}
