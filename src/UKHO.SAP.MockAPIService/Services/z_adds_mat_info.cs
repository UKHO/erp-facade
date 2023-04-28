using System.Xml.Serialization;
using UKHO.ERPFacade.Common.IO;
using UKHO.SAP.MockAPIService.Models;

namespace UKHO.SAP.MockAPIService.Services
{
    public class z_adds_mat_info : Iz_adds_mat_info
    {
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        public const string REQUESTFORMAT = "xml";

        public z_adds_mat_info(IAzureBlobEventWriter azureBlobEventWriter)
        {
            _azureBlobEventWriter = azureBlobEventWriter;
        }

        [return: XmlElement("Z_ADDS_MAT_INFOResponse", Namespace = "urn:sap-com:document:sap:rfc:functions")]
        public Z_ADDS_MAT_INFOResponse Z_ADDS_MAT_INFO([XmlElement("Z_ADDS_MAT_INFO", Namespace = "urn:sap-com:document:sap:rfc:functions")] Z_ADDS_MAT_INFO z_ADDS_MAT_INFO)
        {
            string requestXML = ObjectXMLSerializer<Z_ADDS_MAT_INFO>.SerializeObject(z_ADDS_MAT_INFO);

            Task.Run(async () => await _azureBlobEventWriter.UploadEvent(requestXML, REQUESTFORMAT, z_ADDS_MAT_INFO.IM_MATINFO.CORRID));

            return new Z_ADDS_MAT_INFOResponse()
            {
                EX_MESSAGE = "Record successfully received for " + z_ADDS_MAT_INFO.IM_MATINFO.CORRID,
                EX_STATUS = "0"
            };
        }
    }
}