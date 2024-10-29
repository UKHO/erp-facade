using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public class RoSTransactionTypes
    {
        public const string MaintainHoldingsType = "MAINTAINHOLDINGS";
        public const string NewLicenceType = "NEWLICENCE";
        public const string MigrateNewLicenceType = "MIGRATENEWLICENCE";
        public const string MigrateExistingLicenceType = "MIGRATEEXISTINGLICENCE";
        public const string ConvertLicenceType = "CONVERTLICENCE";
        public const string LicTransaction = "CHANGELICENCE";
    }
}
