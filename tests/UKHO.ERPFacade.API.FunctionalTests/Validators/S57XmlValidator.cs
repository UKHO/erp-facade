using System.Text;
using System.Xml.Linq;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Validators
{
    public static class S57XmlValidator
    {
        public static bool VerifyXmlAttributes(string generatedXmlFilePath, string actualXmlFilePath, string correlationId, string activeKey, string nextKey)
        {
            XElement generatedXml;
            XElement expectedXml;

            using (StreamReader reader = new StreamReader(generatedXmlFilePath, Encoding.UTF8))
            {
                generatedXml = XElement.Load(reader);
            }

            using (StreamReader reader = new StreamReader(actualXmlFilePath, Encoding.UTF8))
            {
                expectedXml = XElement.Load(reader);
            }

            if(!VerifyRootElements(generatedXml, expectedXml, correlationId))
            {
                return false;
            }

            var generatedAttributes = generatedXml.Descendants("item").ToList();
            var expectedAttributes = expectedXml.Descendants("item").ToList();

            // Ensure both XMLs have the same number of items
            if (generatedAttributes.Count != expectedAttributes.Count)
            {
                Console.WriteLine("XML files have different number of items.");
                return false;
            }

            // Iterate over the items and compare their elements
            for (int i = 0; i < generatedAttributes.Count; i++)
            {
                var generatedAction = generatedAttributes[i];
                var expectedAction = expectedAttributes[i];
                string action = generatedAction.Element("ACTION")?.Value;

                foreach (var generatedAttribute in generatedAction.Elements())
                {
                    var expectedAttribute = expectedAction.Element(generatedAttribute.Name);

                    if ((action == ConfigFileFields.CreateEncCell || action == ConfigFileFields.UpdateCell) && (generatedAttribute.Name == XmlFields.ActiveKey || generatedAttribute.Name == XmlFields.NextKey))
                    {
                        string expectedValue = generatedAttribute.Name == "ACTIVEKEY" ? activeKey : nextKey;

                        if (generatedAttribute.Value != expectedValue && generatedAttribute.Value.Length > 0)
                        {
                            Console.WriteLine(
                                $"Mismatch in {generatedAttribute.Name} in item {i + 1}. XML1: {generatedAttribute.Value}, Expected: {expectedValue}");
                            return false;
                        }
                    }
                    else if (generatedAttribute.Value != expectedAttribute?.Value)
                    {
                        Console.WriteLine($"Mismatch in element {generatedAttribute.Name.LocalName} in item {i + 1}. XML1: {generatedAttribute.Value}, XML2: {expectedAttribute?.Value}");
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool VerifyRootElements(XElement generatedXml, XElement expectedXml, string expectedCorrelationId)
        {
            var generatedRootElements = GetRootElementValues(generatedXml);
            var expectedRootElements = GetRootElementValues(expectedXml);

            if (generatedRootElements.Count == 0 || generatedRootElements[0] != expectedCorrelationId)
            {
                return false;
            }

            if (generatedRootElements[1] != DateTime.Now.ToString("yyyyMMdd"))
            {
                return false;
            }

            for (int i = 2; i < generatedRootElements.Count; i++)
            {
                if (i >= expectedRootElements.Count || generatedRootElements[i] != expectedRootElements[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static List<string> GetRootElementValues(XElement xml)
        {
            var imMatInfo = xml.Descendants().FirstOrDefault(e => e.Name.LocalName == "IM_MATINFO");

            return imMatInfo == null ? new List<string>() : new List<string>
            {
                imMatInfo.Element("CORRID")?.Value,
                imMatInfo.Element("RECDATE")?.Value,
                imMatInfo.Element("NOOFACTIONS")?.Value,
                imMatInfo.Element("ORG")?.Value
            };
        }
    }
}
