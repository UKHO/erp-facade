using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.PermitDecryption
{
    public interface IPermitDecryption
    {
        PermitKey GetPermitKeys(string permit);
    }
}
