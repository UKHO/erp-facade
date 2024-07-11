using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using UKHO.ADDS.SAP.Mock.Common.Models;
using UKHO.ADDS.SAP.Mock.ErpCallback;

namespace UKHO.ADDS.SAP.Mock.API.Services
{
    [ExcludeFromCodeCoverage]
    public class z_adds_mat_info : Iz_adds_mat_info
    {
        private ErpFacadePriceEndpointClient _priceEndpointClient;
        
        public z_adds_mat_info(ErpFacadePriceEndpointClient priceEndpointClient)
        {
            _priceEndpointClient = priceEndpointClient;
        }
        
        [return: XmlElement("Z_ADDS_MAT_INFOResponse", Namespace = "urn:sap-com:document:sap:rfc:functions")]
        public Z_ADDS_MAT_INFOResponse Z_ADDS_MAT_INFO([XmlElement("IM_MATINFO", Namespace = "")] IM_MATINFO iM_MATINFO)
        {
            
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _priceEndpointClient.SimulateCallBackFromSap(iM_MATINFO);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            
            return new Z_ADDS_MAT_INFOResponse()
            {
                EX_MESSAGE = "Record successfully received for CorrelationId :" + iM_MATINFO.CORRID,
                EX_STATUS = "0"
            };
        }
    }
}
