using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.PublishPriceChange.WebJob.Services
{
    public interface ISlicingPublishingService
    {
        void SliceAndPublishPriceChangeEvents();
    }
}
