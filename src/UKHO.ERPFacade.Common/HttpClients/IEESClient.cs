using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UKHO.ERPFacade.Common.HttpClients
{
    public interface IEESClient
    {
        Task<HttpResponseMessage> EESHealthCheck();
    }
}
