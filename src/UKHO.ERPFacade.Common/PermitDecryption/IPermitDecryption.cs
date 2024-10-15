using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.PermitDecryption
{
    public interface IPermitDecryption
    {
        DecryptedPermit Decrypt(string permit);
    }
}
