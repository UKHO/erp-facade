using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Models
{
    [ExcludeFromCodeCoverage]
    public class ActionItemAttribute
    {
        public bool IsRequired { get; set; }
        public string Section { get; set; }
        public string JsonPropertyName { get; set; }
        public string XmlNodeName { get; set; }
        public int SortingOrder { get; set; }
    }
}
