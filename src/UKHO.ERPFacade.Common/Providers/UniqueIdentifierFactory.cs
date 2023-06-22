namespace UKHO.ERPFacade.Common.Providers
{
    public class UniqueIdentifierFactory : IUniqueIdentifierFactory
    {
        public string Create() => Guid.NewGuid().ToString();
    }
}
