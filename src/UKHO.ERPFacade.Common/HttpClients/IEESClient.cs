namespace UKHO.ERPFacade.Common.HttpClients
{
    public interface IEESClient
    {
        Task<HttpResponseMessage> Get(string url);
        Task<HttpResponseMessage> PostAsync(string url, string authToken, string cloudEventPayload);
    }
}
