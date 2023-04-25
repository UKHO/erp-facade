namespace UKHO.ERPFacade.API.Models
{
    public class SapAction
    {
        public int ActionNumber { get; set; }
        public string Action { get; set; }
        public string Product { get; set; }
        public ICollection<ActionItemAttribute> Attributes { get; set; }
    }
}
