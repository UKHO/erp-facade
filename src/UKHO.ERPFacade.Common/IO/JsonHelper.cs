using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public class JsonHelper : IJsonHelper
    {
        public int GetPayloadJsonSize(string payloadJsonString)
        {
            var jsonSize = UTF8Encoding.UTF8.GetBytes(payloadJsonString).Length;

            return jsonSize;
        }
    }
}
