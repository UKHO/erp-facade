using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.Permit_Decryption
{
    public interface IPermitDecryption
    {
        PermitKey GetPermitKeys(string permit);
    }
}
