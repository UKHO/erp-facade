namespace UKHO.ERPFacade.Common.Authentication
{
    public interface ITokenProvider
    {
        Task<string> GetTokenAsync(string scope);
    }
}
