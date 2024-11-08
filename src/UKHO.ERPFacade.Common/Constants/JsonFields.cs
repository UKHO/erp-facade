using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public static class JsonFields
    {
        public const string EventIdKey = "id";
        public const string DataNode = "data";
        public const string CorrelationIdKey = "data.correlationId";
        public const string UKHOWeekNumber = "data.ukhoWeekNumber";
        public const string ProductsNode = "data.products";
        public const string UnitsOfSaleNode = "data.unitsOfSale";
        public const string Products = "products";
        public const string UnitsOfSale = "unitsOfSale";
        public const string Permit = "permit";
        public const string UnitSaleType = "unit";
        public const string UnitOfSaleStatusForSale = "ForSale";

        public const string PermitWithSameKey = "PermitWithSameKey";
        public const string PermitWithDifferentKey = "PermitWithDifferentKey";
        public const string AioKey = "AIO";
    }
}
