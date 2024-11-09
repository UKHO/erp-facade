using WireMock.Server;

namespace UKHO.ERPFacade.StubService.Stubs
{
    public interface IStub
    {
        void ConfigureStub(WireMockServer server);
    }
}
