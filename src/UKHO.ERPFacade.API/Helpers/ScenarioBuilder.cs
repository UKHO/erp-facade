using Microsoft.Extensions.Options;
using System.Reflection;
using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public class ScenarioBuilder : IScenarioBuilder
    {
        private readonly ILogger<SapMessageBuilder> _logger;
        private readonly IOptions<ScenarioRuleConfiguration> _scenarioRuleConfig;

        public ScenarioBuilder(ILogger<SapMessageBuilder> logger,
                                IOptions<ScenarioRuleConfiguration> scenarioRuleConfig)
        {
            _logger = logger;
            _scenarioRuleConfig = scenarioRuleConfig;
        }

        public List<Scenario> BuildScenarios(EESEvent eventData)
        {
            List<Scenario> scenarios = new();

            bool restLoop = false;
            bool scenarioIdentified = false;

            //This loop is to identify the scenarios in given EES event request.
            foreach (Product product in eventData.Data.Products)
            {
                Scenario scenarioObj = new();

                foreach (var scenario in _scenarioRuleConfig.Value.ScenarioRules)
                {
                    foreach (var rule in scenario.Rules)
                    {
                        object jsonFieldValue = GetProp(rule.AttributeName, product, product.GetType());

                        if (jsonFieldValue != null && jsonFieldValue.ToString() == rule.AttriuteValue)
                        {
                            restLoop = true;
                            continue;
                        }
                        else
                        {
                            restLoop = false;
                            break;
                        }
                    }
                    if (restLoop)
                    {
                        scenarioObj.ScenarioType = scenario.Scenario;
                        scenarioObj.IsCellReplaced = product.ReplacedBy.Any();
                        scenarioObj.Product = product;
                        scenarioObj.InUnitOfSales = product.InUnitsOfSale;
                        scenarioObj.UnitOfSales = eventData.Data.UnitsOfSales.Where(x => product.InUnitsOfSale.Contains(x.UnitName) ||
                                                                                   (x.CompositionChanges.AddProducts.Contains(product.ProductName) ||
                                                                                    x.CompositionChanges.RemoveProducts.Contains(product.ProductName))).ToList();

                        scenarios.Add(scenarioObj);

                        scenarioIdentified = true;
                    }
                    if (scenarioIdentified) break;
                }
            }
            return scenarios;
        }

        public static object GetProp(string name, object obj, Type type)
        {
            var parts = name.Split('.').ToList();
            var currentPart = parts[0];
            PropertyInfo info = type.GetProperty(currentPart);
            if (info == null) { return null; }
            if (name.IndexOf(".") > -1)
            {
                parts.Remove(currentPart);
                return GetProp(string.Join(".", parts), info.GetValue(obj, null), info.PropertyType);
            }
            else
            {
                return info.GetValue(obj, null).ToString();
            }
        }
    }
}
