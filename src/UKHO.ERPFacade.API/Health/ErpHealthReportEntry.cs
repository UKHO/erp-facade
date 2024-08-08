namespace UKHO.ERPFacade.API.Health
{
    public class ErpHealthReportEntry
    {
        public IReadOnlyDictionary<string, object> Data { get; set; } = null!;
        public string? Description { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Exception { get; set; }
        public ErpHealthStatus Status { get; set; }
        public IEnumerable<string>? Tags { get; set; }
    }
}
