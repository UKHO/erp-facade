using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Xml;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents.S57;
using UKHO.ERPFacade.Common.PermitDecryption;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.API.Tests.XmlTransformers
{
    public class S57XmlTransformerTests
    {
        //private ILogger<S57XmlTransformer> _logger;
        //private IXmlHelper _xmlHelper;
        //private IWeekDetailsProvider _weekDetailsProvider;
        //private IPermitDecryption _permitDecryption;
        //private IOptions<SapActionConfiguration> _sapActionConfig;

        //private S57XmlTransformer _s57XmlTransformer;

        //[SetUp]
        //public void Setup()
        //{
        //    _logger = A.Fake<ILogger<S57XmlTransformer>>();
        //    _xmlHelper = A.Fake<IXmlHelper>();
        //    _weekDetailsProvider = A.Fake<IWeekDetailsProvider>();
        //    _permitDecryption = A.Fake<IPermitDecryption>();
        //    _sapActionConfig = A.Fake<IOptions<SapActionConfiguration>>();

        //    _s57XmlTransformer = new S57XmlTransformer(_logger, _xmlHelper, _weekDetailsProvider, _permitDecryption, _sapActionConfig);
        //}

        //[Test]
        //public void BuildXmlPayload_WithS57EventData_ReturnsXmlDocument()
        //{
        //    // Arrange
        //    var eventData = A.Fake<S57EventData>();
        //    var xmlTemplatePath = "path/to/xml/template";

        //    var s57EventXmlPayload = new XmlDocument();
        //    A.CallTo(() => _xmlHelper.CreateXmlDocument(A<string>._)).Returns(s57EventXmlPayload);

        //    // Act
        //    var result = _s57XmlTransformer.BuildXmlPayload(eventData, xmlTemplatePath);

        //    // Assert
        //    Assert.AreEqual(s57EventXmlPayload, result);
        //}
    }
}
