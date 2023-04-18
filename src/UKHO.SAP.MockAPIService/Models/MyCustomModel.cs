using System.Runtime.Serialization;

namespace UKHO.SAP.MockAPIService.Models
{
    [DataContract]
    public class MyCustomModel
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Email { get; set; }
    }
}
