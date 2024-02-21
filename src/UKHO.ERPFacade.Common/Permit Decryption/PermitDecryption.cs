using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.Permit_Decryption
{
    public class PermitDecryption : IPermitDecryption
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PermitDecryption> _logger;

        public PermitDecryption(IConfiguration config, ILogger<PermitDecryption> logger)
        {
            _config = config;
            _logger = logger;
        }

        public PermitKey GetPermitKeys(string permit)
        {
            if (string.IsNullOrEmpty(permit)) return null;
            try
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
            catch (Exception ex)
            {
                _logger.LogInformation(EventIds.PermitdecryptionException.ToEventId(), Convert.ToString("An error occurred while decrypting the permit string."), ex.Message);
                return null;
            }
        }

        private byte[] GetHardwareIds()
        {
            var permitHardwareIds = _config.GetValue<string>("PermitDecryptionHardwareId").Split(',').ToList();
            int i = 0;
            byte[] hardwareIds = new byte[6];
            foreach (string? hardwareId in permitHardwareIds)
            {
                hardwareIds[i++] = byte.Parse(hardwareId.Trim(), NumberStyles.AllowHexSpecifier);
            }

            return hardwareIds;
        }
    }
}
