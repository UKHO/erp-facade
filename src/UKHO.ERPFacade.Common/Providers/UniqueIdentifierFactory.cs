using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Providers
{
    [ExcludeFromCodeCoverage]
    public class UniqueIdentifierFactory : IUniqueIdentifierFactory
    {
        public string Create() => Guid.NewGuid().ToString();
    }
}
