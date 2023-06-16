using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.Monitoring.WebJob.Services
{
    public interface IMonitoringService
    {
        void MonitorIncompleteTransactions();
    }
}
