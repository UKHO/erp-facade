using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Model
{
    public class JsonInputPriceChangeHelper
    {


        public string corrid { get; set; }
        public string org { get; set; }
        public string productname { get; set; }
        public string duration { get; set; }
        public string effectivedate { get; set; }
        public string effectivetime { get; set; }
        public string price { get; set; }
        public string currency { get; set; }
        public string futuredate { get; set; }
        public string futuretime { get; set; }
        public string futureprice { get; set; }
        public string futurecurr { get; set; }
        public string reqdate { get; set; }
        public string reqtime { get; set; }
    }
}
