using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.SAP.MockAPIService.Models;

namespace UKHO.SAP.MockAPIService.Services
{
    [ExcludeFromCodeCoverage]
    public class z_adds_mat_info : Iz_adds_mat_info
    {
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly MockService _mockService;

        public const string RequestFormat = "xml";
        public const string HealthCheckKey = "HEALTHCHECK";

        public z_adds_mat_info(IAzureBlobEventWriter azureBlobEventWriter, MockService mockService)
        {
            _azureBlobEventWriter = azureBlobEventWriter;
            _mockService = mockService;
        }

        [return: XmlElement("Z_ADDS_MAT_INFOResponse", Namespace = "urn:sap-com:document:sap:rfc:functions")]
        public Z_ADDS_MAT_INFOResponse Z_ADDS_MAT_INFO([XmlElement("IM_MATINFO", Namespace = "")] IM_MATINFO iM_MATINFO)
        {
            Z_ADDS_MAT_INFO z_ADDS_MAT_INFO = new()
            {
                IM_MATINFO = iM_MATINFO
            };
            if (iM_MATINFO.CORRID != HealthCheckKey)
            {
                string requestXML = ObjectXMLSerializer<Z_ADDS_MAT_INFO>.SerializeObject(z_ADDS_MAT_INFO).WriteXmlClosingTags();

                Task.Run(async () => await _azureBlobEventWriter.UploadEvent(requestXML, iM_MATINFO.CORRID, iM_MATINFO.CORRID + '.' + RequestFormat));

                _mockService.CleanUp();
            }
           
            return new Z_ADDS_MAT_INFOResponse()
            {
                EX_MESSAGE = "Record successfully received for CorrelationId :" + iM_MATINFO.CORRID,
                EX_STATUS = "0"
            };
        }
    }
}
