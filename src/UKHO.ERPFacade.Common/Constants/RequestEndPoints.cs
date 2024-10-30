using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public static class RequestEndPoints
    {
        public const string S57RequestEndPoint = "/webhook/newenccontentpublishedeventreceived";
        public const string LicenceUpdatedRequestEndPoint = "/webhook/licenceupdatedpublishedeventreceived";
        public const string RoSWebhookRequestEndPoint = "/webhook/recordofsalepublishedeventreceived";
    }
}
