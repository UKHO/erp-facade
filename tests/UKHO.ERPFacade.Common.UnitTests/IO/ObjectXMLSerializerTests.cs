using NUnit.Framework;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UKHO.ERPFacade.Common.IO;
using UKHO.SAP.MockAPIService.Models;

namespace UKHO.ERPFacade.Common.UnitTests.IO
{
    public class ObjectXMLSerializerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void WhenValidObjectIsPassed_ThenReturnsSerializedObjectString()
        {
            Z_ADDS_MAT_INFO z_ADDS_MAT_INFO = new()
            {
                IM_MATINFO = new() { CORRID = "123456" }
            };

            var serializer = new XmlSerializer(typeof(Z_ADDS_MAT_INFO));
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true };

            using (var xw = XmlWriter.Create(sb, settings))
            {
                serializer.Serialize(xw, z_ADDS_MAT_INFO);
            }

            var result = ObjectXMLSerializer<Z_ADDS_MAT_INFO>.SerializeObject(z_ADDS_MAT_INFO!);

            Assert.That(sb.ToString(), Is.EqualTo(result));
        }

        [Test]
        public void WhenNullObjectIsPassed_ThenReturnsNull()
        {
            Z_ADDS_MAT_INFO? nullObject = null;

            var serializer = new XmlSerializer(typeof(Z_ADDS_MAT_INFO));
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true };

            using (var xw = XmlWriter.Create(sb, settings))
            {
                serializer.Serialize(xw, nullObject);
            }

            var result = ObjectXMLSerializer<Z_ADDS_MAT_INFO>.SerializeObject(nullObject!);

            Assert.That(result, Is.Null);
        }
    }
}