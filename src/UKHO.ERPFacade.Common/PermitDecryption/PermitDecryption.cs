using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.PermitDecryption
{
    [ExcludeFromCodeCoverage]
    public class PermitDecryption : IPermitDecryption
    {
        private readonly ILogger<PermitDecryption> _logger;
        private readonly IOptions<PermitConfiguration> _permitConfiguration;

        public PermitDecryption(ILogger<PermitDecryption> logger, IOptions<PermitConfiguration> permitConfiguration)
        {
            _permitConfiguration = permitConfiguration ?? throw new ArgumentNullException(nameof(permitConfiguration));
            _logger = logger;
        }

        public DecryptedPermit Decrypt(string encryptedPermit)
        {
            if (string.IsNullOrEmpty(encryptedPermit)) return null;
            try
            {
                byte[] hardwareIds = GetHardwareIds();
                byte[] firstCellKey = null;
                byte[] secondCellKey = null;

                S63Crypt.GetEncKeysFromPermit(encryptedPermit, hardwareIds, ref firstCellKey, ref secondCellKey);

                var decryptedPermit = new DecryptedPermit
                {
                    ActiveKey = Convert.ToHexString(firstCellKey),
                    NextKey = Convert.ToHexString(secondCellKey)
                };
                return decryptedPermit;
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.PermitDecryptionException.ToEventId(), ex, "Permit decryption failed and could not generate ActiveKey & NextKey. | Exception : {Exception}", ex.Message);
                return null;
            }
        }

        private byte[] GetHardwareIds()
        {
            var permitHardwareIds = _permitConfiguration.Value.PermitDecryptionHardwareId.Split(',').ToList();
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
