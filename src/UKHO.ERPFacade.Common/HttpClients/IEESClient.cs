namespace UKHO.ERPFacade.Common.HttpClients
{
    public interface IEESClient
    {
        Task<HttpResponseMessage> Get(string url);
    }
}
