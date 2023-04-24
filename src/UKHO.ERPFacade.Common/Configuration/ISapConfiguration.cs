namespace UKHO.ERPFacade.Common.Configuration
{
    public interface ISapConfiguration
    {
        public string BaseAddress { get; set; }

        public string SapServiceOperation { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
