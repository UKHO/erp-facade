using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.Permit_Decryption
{
    public class PermitDecryption : IPermitDecryption
    {
        public PermitKey GetPermitKeys(string permit)
        {
            var hardwareIds = GetHardwareIds();
            byte[] firstCellKey = null;
            byte[] secondCellKey = null;

            S63Cryption.GetEncKeysFromPermit(permit, hardwareIds, ref firstCellKey, ref secondCellKey);

            var keys = new PermitKey
            {
                ActiveKey = Convert.ToHexString(firstCellKey),
                NextKey = Convert.ToHexString(secondCellKey)
            };
            return keys;
        }
        

        private byte[] GetHardwareIds()
        {
            var mdsHardwareIdList = "6F, B6, 65, 9C, 7E".Split(',').ToList();
            var i = 0;
            var hardwareIds = new byte[6];
            foreach (var hardwareId in mdsHardwareIdList)
            {
                hardwareIds[i++] = byte.Parse(hardwareId.Trim(), NumberStyles.AllowHexSpecifier);
            }

            return hardwareIds;
        }
    }
}
