using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public static class ApiHeaderKeys
    {
        public const string XCorrelationIdHeaderKeyName = "_X-Correlation-ID";
        public const string ApiKeyHeaderKeyName = "X-API-Key";
    }
}
