namespace UKHO.ERPFacade.API.Models
{
    public class ActionItemAttribute
    {
        public bool IsRequired { get; set; }
        public string Section { get; set; }
        public string JsonPropertyName { get; set; }
        public string XmlNodeName { get; set; }
    }
}
