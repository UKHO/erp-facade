using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Modifiers
{
    public static class JsonModifier
    {
        public static string UpdateTime(string requestBody)
        {
            //Create timestamp using the current date and time
            var currentTimeStamp = DateTime.Now.ToString(XmlFields.EventJsonDateTimeFormat);
            JObject jsonObj = JObject.Parse(requestBody);
            jsonObj["time"] = currentTimeStamp;
            return jsonObj.ToString();
        }

        public static (string, string) UpdateCorrelationId(string requestBody, string correlationId = null)
        {
            JObject jsonObj = JObject.Parse(requestBody);

            if (!string.IsNullOrEmpty(correlationId))
            {
                jsonObj[JsonFields.DataNode]["correlationId"] = correlationId;
                return (jsonObj.ToString(), correlationId);
            }
            //Generate a new correlation ID
            string newCorrelationId = Guid.NewGuid().ToString();
            newCorrelationId = $"ft-{newCorrelationId}";
            jsonObj[JsonFields.DataNode]["correlationId"] = newCorrelationId;
            return (jsonObj.ToString(), newCorrelationId);
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

        public static string UpdateMandatoryAttribute(string payload, string attributeName, int index)
        {
            JObject jsonObject = JObject.Parse(payload);
            string[] types = attributeName.Split(".");
            var tokens = types[0] == JsonFields.Products
                ? jsonObject.SelectTokens($"$.{JsonFields.ProductsNode}[{index}].{types[1]}").ToList()
                : types[0] == JsonFields.UnitsOfSale
                    ? jsonObject.SelectTokens($"$.{JsonFields.UnitsOfSaleNode}[{index}].{types[1]}").ToList()
                    : jsonObject.SelectTokens($"$.{JsonFields.UKHOWeekNumber}.{types[0]}").ToList();
            tokens.ToList().ForEach(token =>
            {
                JProperty parentProperty = (JProperty)token.Parent;
                parentProperty?.Remove();
            });
            payload = jsonObject.ToString();
            return payload;
        }
    }
}
