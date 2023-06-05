using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider
{
    public interface ICloudEventFactory
    {
        CloudEvent<TData> Create<TData>(EventBase<TData> domainEvent);
    }
}
