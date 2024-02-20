using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.Permit_Decryption
{
    public interface IPermitDecryption
    {
        PermitKey GetPermitKeys(string permit);
    }
}
