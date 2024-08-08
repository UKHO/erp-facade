using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UKHO.ERPFacade.API.Health
{
    public class ErpHealthReport
    {
        public ErpHealthStatus Status { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public Dictionary<string, ErpHealthReportEntry> Entries { get; }

        public ErpHealthReport(Dictionary<string, ErpHealthReportEntry> entries, TimeSpan totalDuration)
        {
            Entries = entries ?? new Dictionary<string, ErpHealthReportEntry>();
            TotalDuration = totalDuration;
        }

        public static ErpHealthReport CreateFrom(HealthReport report, Func<Exception, string>? exceptionMessage = null)
        {
            var uiReport = new ErpHealthReport(new Dictionary<string, ErpHealthReportEntry>(), report.TotalDuration)
            {
                Status = (ErpHealthStatus)report.Status,
            };

            foreach (var item in report.Entries)
            {
                var entry = new ErpHealthReportEntry
                {
                    Data = item.Value.Data,
                    Description = item.Value.Description,
                    Duration = item.Value.Duration,
                    Status = (ErpHealthStatus)item.Value.Status
                };

                if (item.Value.Exception != null)
                {
                    var message = exceptionMessage == null ? item.Value.Exception?.Message : exceptionMessage(item.Value.Exception);

                    entry.Exception = message;
                    entry.Description = item.Value.Description ?? message;
                }

                entry.Tags = item.Value.Tags;

                uiReport.Entries.Add(item.Key, entry);
            }

            return uiReport;
        }

        public static ErpHealthReport CreateFrom(Exception exception, string entryName = "Endpoint")
        {
            var uiReport = new ErpHealthReport(new Dictionary<string, ErpHealthReportEntry>(), TimeSpan.FromSeconds(0))
            {
                Status = ErpHealthStatus.Unhealthy,
            };

            uiReport.Entries.Add(entryName, new ErpHealthReportEntry
            {
                Exception = exception.Message,
                Description = exception.Message,
                Duration = TimeSpan.FromSeconds(0),
                Status = ErpHealthStatus.Unhealthy
            });

            return uiReport;
        }
    }
}
