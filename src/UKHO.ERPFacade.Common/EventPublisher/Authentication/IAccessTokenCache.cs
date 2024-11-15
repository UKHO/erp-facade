namespace UKHO.ERPFacade.Common.EventPublisher.Authentication
{
    public interface IAccessTokenCache
    {
        Task<string> GetTokenAsync(string scope);
    }
}
