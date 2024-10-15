using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class SapXmlHelper
    {
        private static readonly string permitWithSameKeyActiveKey = Config.TestConfig.PermitWithSameKey.ActiveKey;
        private static readonly string permitWithSameKeyNextKey = Config.TestConfig.PermitWithSameKey.NextKey;
        private static readonly string permitWithDifferentKeyActiveKey = Config.TestConfig.PermitWithDifferentKey.ActiveKey;
        private static readonly string permitWithDifferentKeyNextKey = Config.TestConfig.PermitWithDifferentKey.NextKey;

        public static string GenerateRandomCorrelationId()
        {
            Guid guid = Guid.NewGuid();
            string randomCorrId = guid.ToString("N").Substring(0, 21);
            randomCorrId = randomCorrId.Insert(5, "-");
            randomCorrId = randomCorrId.Insert(11, "-");
            randomCorrId = randomCorrId.Insert(16, "-");
            var currentTimeStamp = DateTime.Now.ToString(Constants.RecDateFormat);
            randomCorrId = "ft-" + currentTimeStamp + "-" + randomCorrId;
            return randomCorrId;
        }

        public static string UpdateTimeAndCorrIdField(string requestBody, string generatedCorrelationId)
        {
            var currentTimeStamp = DateTime.Now.ToString(Constants.RecDateFormat);
            JObject jsonObj = JObject.Parse(requestBody);
            jsonObj["time"] = currentTimeStamp;
            jsonObj[Constants.DataNode]["correlationId"] = generatedCorrelationId;
            return jsonObj.ToString();
        }

        public static string UpdatePermitField(string requestBody, string permitState)
        {
            JObject jsonObj = JObject.Parse(requestBody);
            var products = jsonObj[Constants.DataNode][Constants.Products];

            string permit = permitState.Contains(Constants.PermitWithSameKey) ? Config.TestConfig.PermitWithSameKey.Permit
                : permitState.Contains(Constants.PermitWithDifferentKey) ? Config.TestConfig.PermitWithDifferentKey.Permit
                : permitState;

            foreach (var product in products)
            {
                product[Constants.Permit] = permit;
            }

            return jsonObj.ToString();
        }

        public static bool VerifyGeneratedXml(string generatedXmlFilePath, string xmlFilePath, string permitState)
        {
            XElement generatedXml;
            XElement expectedXml;

            using (StreamReader reader = new StreamReader(generatedXmlFilePath, Encoding.UTF8))
            {
                generatedXml = XElement.Load(reader);
            }

            using (StreamReader reader = new StreamReader(xmlFilePath, Encoding.UTF8))
            {
                expectedXml = XElement.Load(reader);
            }

            var generatedAttributes = generatedXml.Descendants("item").ToList();
            var expectedAttributes = expectedXml.Descendants("item").ToList();


            string activeKey = permitState == Constants.PermitWithSameKey ? permitWithSameKeyActiveKey : permitWithDifferentKeyActiveKey;
            string nextKey = permitState == Constants.PermitWithSameKey ? permitWithSameKeyNextKey : permitWithDifferentKeyNextKey;

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

                    if ((action == Constants.CreateEncCell || action == Constants.UpdateCell ) && (generatedAttribute.Name == Constants.ActiveKey || generatedAttribute.Name == Constants.NextKey))
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
    }
}
