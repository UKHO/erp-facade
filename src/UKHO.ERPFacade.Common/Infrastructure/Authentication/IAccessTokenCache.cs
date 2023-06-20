namespace UKHO.ERPFacade.Common.Infrastructure.Authentication
{
    public interface IAccessTokenCache
    {
        Task<string> GetTokenAsync(string scope);
    }
}