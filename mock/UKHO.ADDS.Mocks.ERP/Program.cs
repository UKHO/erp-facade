using UKHO.ADDS.Mocks.Configuration;
using UKHO.ADDS.Mocks.Domain.Configuration;
using UKHO.ADDS.Mocks.States;

namespace UKHO.ADDS.Mocks.ERP
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            MockServices.AddServices();            
            ServiceRegistry.AddDefinition(new ServiceDefinition("sap", "SAP Service", []));

            await MockServer.RunAsync(args);
        }
    }
}
