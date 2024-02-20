
namespace UKHO.ERPFacade.Common.Permit_Decryption
{
    public interface IBFFactory
    {
        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cachingOptions"></param>
        /// <returns></returns>
        IBlowfishAlgorithm Get(byte[] key, CachingOptions cachingOptions = CachingOptions.Cache);
    }
}
