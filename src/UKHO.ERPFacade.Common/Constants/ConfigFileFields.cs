using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public static class ConfigFileFields
    {
        public const string ProductSection = "Product";
        public const string UnitOfSaleSection = "UnitOfSale";
        public const string UkhoWeekNumberSection = "UkhoWeekNumber";

        public const string CreateEncCell = "CREATE ENC CELL";
        public const string UpdateCell = "UPDATE ENC CELL EDITION UPDATE NUMBER";
        public const string ReplaceEncCellAction = "REPLACED WITH ENC CELL";
        public const string ChangeEncCellAction = "CHANGE ENC CELL";
 }
}
