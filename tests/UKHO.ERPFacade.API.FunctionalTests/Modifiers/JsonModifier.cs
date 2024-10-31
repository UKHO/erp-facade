using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Modifiers
{
    public static class JsonModifier
    {
        public static string UpdateTime(string requestBody)
        {
            //Create timestamp using the current date and time
            var currentTimeStamp = DateTime.Now.ToString(XmlFields.RecDateFormat);
            JObject jsonObj = JObject.Parse(requestBody);
            jsonObj["time"] = currentTimeStamp;
            return jsonObj.ToString();
        }

        public static (string, string) UpdateCorrelationId(string requestBody)
        {
            //Generate a random correlation ID
            string correlationId = Guid.NewGuid().ToString("N").Substring(0, 21);
            correlationId = correlationId.Insert(5, "-").Insert(11, "-").Insert(16, "-");
            string timestamp = DateTime.Now.ToString("yyyyMMddTHHmmss");
            correlationId = $"ft-{timestamp}-{correlationId}";
            JObject jsonObj = JObject.Parse(requestBody);
            jsonObj[JsonFields.DataNode]["correlationId"] = correlationId;
            return (jsonObj.ToString(), correlationId);
        }

        public static string UpdatePermitField(string requestBody, string permit)
        {
            JObject jsonObj = JObject.Parse(requestBody);
            var products = jsonObj[JsonFields.DataNode][JsonFields.Products];
            foreach (var product in products)
            {
                product[JsonFields.Permit] = permit;
            }
            return jsonObj.ToString();
        }

        public static string UpdateMandatoryAttribute(string payload, string attributeName, int index, string action)
        {
            JObject jsonObject = JObject.Parse(payload);

            string[] types = attributeName.Split(".");

            var tokens = types[0] == JsonFields.Products
                ? jsonObject.SelectTokens($"$.{JsonFields.ProductsNode}[{index}].{types[1]}").ToList()
                : types[0] == JsonFields.UnitsOfSale
                    ? jsonObject.SelectTokens($"$.{JsonFields.UnitsOfSaleNode}[{index}].{types[1]}").ToList()
                    : jsonObject.SelectTokens($"$.{JsonFields.UKHOWeekNumber}.{types[0]}").ToList();

            if (action == "Remove")
            {
                foreach (var token in tokens)
                {
                    JProperty parentProperty = (JProperty)token.Parent;
                    parentProperty?.Remove();
                }
            }
            else
            {
                foreach (var token in tokens)
                {
                    token.Replace(JValue.CreateNull());
                }
            }
            payload = jsonObject.ToString();
            return payload;
        }
    }
}
