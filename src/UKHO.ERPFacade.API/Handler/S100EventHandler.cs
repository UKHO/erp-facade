using System.Xml;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.S100Event;

namespace UKHO.ERPFacade.API.Handler
{
    public class S100EventHandler : EventHandler<S100EventPayload>
    {
        public S100EventHandler(IAzureTableReaderWriter azureTableReaderWriter,
                                    IAzureBlobEventWriter azureBlobEventWriter,
                                    ISapClient sapClient,
                                    IXmlHelper xmlHelper,
                                    IFileSystemHelper fileSystemHelper) : base(azureTableReaderWriter, azureBlobEventWriter, sapClient, xmlHelper, fileSystemHelper)
        {

        }
        public override string EventType
        {
            get
            {
                return "uk.gov.ukho.encpublishing.enccontentpublished.v2";
            }
        }

        public override Task BuildEncCellActions(S100EventPayload eventData, XmlDocument soapXml, XmlNode? actionItemNode)
        {
            throw new NotImplementedException();
        }

        public override Task BuildUnitActions(S100EventPayload eventData, XmlDocument soapXml, XmlNode? actionItemNode)
        {
            throw new NotImplementedException();
        }
    }
}
