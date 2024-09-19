using System.Configuration;
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
                _logger.LogError(EventIds.HardwareIdNotFoundException.ToEventId(), "Hardware Ids are not configured for permit decryption.");
                throw new ConfigurationErrorsException("Hardware Ids are not configured for permit decryption.");
            }
        }

        public PermitKey GetPermitKeys(string permit)
        {
            if (string.IsNullOrEmpty(permit))
            {
                _logger.LogError(EventIds.EmptyPermitStringException.ToEventId(), "Encrypted permit is empty in event payload.");
                throw new ERPFacadeException(EventIds.EmptyPermitStringException.ToEventId());
            }

            try
            {
                byte[] hardwareIds = GetHardwareIds();
                byte[] firstCellKey = null;
                byte[] secondCellKey = null;

                S63Crypt.GetEncKeysFromPermit(permit, hardwareIds, ref firstCellKey, ref secondCellKey);

                var keys = new PermitKey
                {
                    ActiveKey = Convert.ToHexString(firstCellKey),
                    NextKey = Convert.ToHexString(secondCellKey)
                };
                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.PermitDecryptionException.ToEventId(), ex, "Permit decryption failed and could not generate ActiveKey & NextKey.");
                throw new ERPFacadeException(EventIds.PermitDecryptionException.ToEventId());
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
