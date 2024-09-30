using WireMock.Server;

namespace UKHO.SAP.MockAPIService.Stubs
{
    public interface IStub
    {
        void ConfigureStub(WireMockServer server);
    }
}
