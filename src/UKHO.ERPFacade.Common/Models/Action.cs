using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class Action
    {
        public string ActionNumber { get; set; }
        public string ActionName { get; set; }
        public string Product { get; set; }
        public string ProductType { get; set; }
        public string ChildCell { get; set; }
        public string ProductName { get; set; }
        public string Cancelled { get; set; }
        public string ReplacedBy { get; set; }
        public string Agency { get; set; }
        public string Provider { get; set; }
        public string EncSize { get; set; }
        public string Title { get; set; }
        public string EditionNumber { get; set; }
        public string UpdateNumber { get; set; }
        public string UnitType { get; set; }
    }
}
