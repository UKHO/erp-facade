﻿
namespace UKHO.ERPFacade.Common.PermitDecryption
{
    public interface IBlowfishAlgorithm
    {
        /// <summary>
        /// implements Blowfish encryption algorithm
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        Boolean Encrypt(Byte[] buf);

        /// <summary>
        /// Implements Blowfish decryption algorithm
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        Boolean Decrypt(Byte[] buf);
    }
}
