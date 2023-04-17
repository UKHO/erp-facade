using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class WebhookPayload
    {
        public partial class Welcome
        {
            public string Specversion { get; set; }
            public string Type { get; set; }
            public Uri Source { get; set; }
            public Guid Id { get; set; }
            public DateTimeOffset Time { get; set; }
            public string Comment { get; set; }
            public string Subject { get; set; }
            public string Datacontenttype { get; set; }
            public Data Data { get; set; }
        }

        public partial class Data
        {
            public Guid TraceId { get; set; }
            public Product[] Products { get; set; }
            public string Comment { get; set; }
            public UnitsOfSale[] UnitsOfSale { get; set; }
        }

        public partial class Product
        {
            public string ProductType { get; set; }
            public string DataSetName { get; set; }
            public string ProductName { get; set; }
            public string Title { get; set; }
            public long Scale { get; set; }
            public long UsageBand { get; set; }
            public long EditionNumber { get; set; }
            public long UpdateNumber { get; set; }
            public bool MayAffectHoldings { get; set; }
            public string Permit { get; set; }
            public string ProviderName { get; set; }
            public string Comment { get; set; }
            public string[] Enum { get; set; }
            public string Size { get; set; }
            public string Agency { get; set; }
            public Bundle[] Bundle { get; set; }
            public Status Status { get; set; }
            public string[] Replaces { get; set; }
            public string[] ReplacedBy { get; set; }
            public object[] AdditionalCoverage { get; set; }
            public GeographicLimit GeographicLimit { get; set; }
            public string[] InUnitsOfSale { get; set; }
            public S63 S63 { get; set; }
            public Signature Signature { get; set; }
            public Signature[] AncillaryFiles { get; set; }
        }

        public partial class Signature
        {
            public string Name { get; set; }
            public string Hash { get; set; }
            public Guid Location { get; set; }
            public long FileSize { get; set; }
        }

        public partial class Bundle
        {
            public string BundleType { get; set; }
            public string[] Enum { get; set; }
            public string Location { get; set; }
        }

        public partial class GeographicLimit
        {
            public BoundingBox BoundingBox { get; set; }
            public GeographicLimitPolygon[] Polygons { get; set; }
        }

        public partial class BoundingBox
        {
            public double? NorthLimit { get; set; }
            public double? SouthLimit { get; set; }
            public double? EastLimit { get; set; }
            public double? WestLimit { get; set; }
        }

        public partial class GeographicLimitPolygon
        {
            public PolygonPolygon[] Polygon { get; set; }
        }

        public partial class PolygonPolygon
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        public partial class S63
        {
            public string Name { get; set; }
            public string Hash { get; set; }
            public Guid Location { get; set; }
            public long FileSize { get; set; }
            public bool Compression { get; set; }
            public string S57Crc { get; set; }
        }

        public partial class Status
        {
            public string StatusName { get; set; }
            public string[] Enum { get; set; }
            public DateTimeOffset StatusDate { get; set; }
            public bool IsNewCell { get; set; }
            public string Comment { get; set; }
        }

        public partial class UnitsOfSale
        {
            public string UnitName { get; set; }
            public string Title { get; set; }
            public string UnitType { get; set; }
            public string Status { get; set; }
            public string[] Enum { get; set; }
            public string Comment { get; set; }
            public GeographicLimit GeographicLimit { get; set; }
            public CompositionChanges CompositionChanges { get; set; }
        }

        public partial class CompositionChanges
        {
            public string[] AddProducts { get; set; }
            public string[] RemoveProducts { get; set; }
        }
    }
}
