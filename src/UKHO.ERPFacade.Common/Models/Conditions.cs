using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class Conditions
    {
        public string AttributeDataType { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
    }
}
