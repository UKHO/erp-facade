namespace UKHO.ERPFacade.API.Models
{
    public class Scenario
    {
        public ScenarioType ScenarioType { get; set; }
        public bool IsCellReplaced { get; set; }
        public Product Product { get; set; }
        public List<string> InUnitOfSales { get; set; }
        public List<UnitOfSale> UnitOfSales { get; set; }
    }
}
