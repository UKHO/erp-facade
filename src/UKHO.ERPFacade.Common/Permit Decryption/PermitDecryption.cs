using System.Globalization;
using Microsoft.Extensions.Configuration;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.Permit_Decryption
{
    public class PermitDecryption : IPermitDecryption
    {
        private readonly IConfiguration _config;

        public PermitDecryption(IConfiguration config)
        {
            _config = config;
        }
        public PermitKey GetPermitKeys(string permit)
        {
          
            if (string.IsNullOrEmpty(permit)) return null;

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
            var permitHardwareIds = _config.GetValue<string>("PermitDecryptionHardwareId").Split(',').ToList();
            var i = 0;
            var hardwareIds = new byte[6];
            foreach (var hardwareId in permitHardwareIds)
            {
                hardwareIds[i++] = byte.Parse(hardwareId.Trim(), NumberStyles.AllowHexSpecifier);
            }

            return hardwareIds;
        }
    }
}
