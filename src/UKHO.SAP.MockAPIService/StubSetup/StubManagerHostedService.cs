using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using UKHO.SAP.MockAPIService.Stubs;
using WireMock.Server;
using WireMock.Settings;

namespace UKHO.SAP.MockAPIService.StubSetup
{
    public class StubManagerHostedService : IHostedService
    {
        private readonly StubFactory _stubFactory;
        private readonly WireMockServer _wireMockServer;

        public StubManagerHostedService(StubFactory stubFactory, IOptions<WireMockServerSettings> wireMockServerSettings)
        {
            _stubFactory = stubFactory;
            _wireMockServer = WireMockServer.Start(wireMockServerSettings.Value);
        }

        private void RegisterStubs()
        {
            RegisterStub(_stubFactory.CreateSapServiceStub());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            RegisterStubs();
            Console.WriteLine($"WireMock server is running at {_wireMockServer.Urls[0]}");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _wireMockServer.Stop();
            return Task.CompletedTask;
        }

        private void RegisterStub<T>(T stub) where T : IStub
        {
            stub.ConfigureStub(_wireMockServer);
        }
    }
}
