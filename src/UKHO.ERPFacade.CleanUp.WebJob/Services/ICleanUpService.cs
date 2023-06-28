using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.CleanUp.WebJob.Services
{
    public interface ICleanUpService
    {
        void CleanUpAzureTableAndBlobs();
    }
}
