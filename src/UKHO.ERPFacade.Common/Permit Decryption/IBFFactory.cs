using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
