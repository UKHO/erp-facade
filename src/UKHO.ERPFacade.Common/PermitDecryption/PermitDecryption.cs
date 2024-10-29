using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Exceptions;
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

            if (string.IsNullOrEmpty(_permitConfiguration.Value.PermitDecryptionHardwareId))
            {
                throw new ERPFacadeException(EventIds.HardwareIdsConfigurationMissingException.ToEventId(), "Hardware ids configuration missing.");
            }
        }

        public DecryptedPermit Decrypt(string encryptedPermit)
        {
            if (string.IsNullOrEmpty(encryptedPermit))
            {
                throw new ERPFacadeException(EventIds.EmptyPermitStringException.ToEventId(), "Permit string is empty in S57 enccontentpublished event.");
            }

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
                throw new ERPFacadeException(EventIds.PermitDecryptionException.ToEventId(), $"Permit decryption failed and could not generate ActiveKey and NextKey. | Exception : {ex.Message}");
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
