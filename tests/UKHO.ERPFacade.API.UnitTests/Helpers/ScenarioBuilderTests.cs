using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.UnitTests.Helpers
{
    [TestFixture]
    public class ScenarioBuilderTests
    {
        private ILogger<ScenarioBuilder> _fakeLogger;
        private IOptions<ScenarioRuleConfiguration> _fakeScenarioRuleConfig;
        private ScenarioBuilder _fakeScenarioBuilder;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<ScenarioBuilder>>();
            _fakeScenarioRuleConfig = Options.Create(InitConfiguration().GetSection("ScenarioRuleConfiguration").Get<ScenarioRuleConfiguration>())!;
            _fakeScenarioBuilder = new ScenarioBuilder(_fakeLogger, _fakeScenarioRuleConfig);
        }

        private IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + @"/ConfigurationFiles")
               .AddJsonFile("ScenarioRules.json")
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        [Test]
        public void WhenEesEventDataPassedWithIsNewCellAsTrue_ThenReturnsNewCellScenario()
        {
            var fakeEventData = new EESEvent()
            {
                Data = new Data()
                {
                    Products = new List<Product>()
                    {
                        new Product()
                        {
                            ReplacedBy = new List<string>(),
                            InUnitsOfSale = new List<string> { "Cell1", "UnitOfSale1", "UnitofSale2" },
                            ProductName = "Cell1",
                            Status = new Status()
                            {
                            IsNewCell= true,
                            StatusName="New Edition"
                            },
                            ContentChanged=false
                        }
                    },
                    UnitsOfSales = new List<UnitOfSale>()
                    {
                        new UnitOfSale()
                        {
                            UnitName = "Cell1",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string> { "Cell1"},
                                RemoveProducts = new List<string>()
                            }
                        },
                        new UnitOfSale()
                        {
                            UnitName = "UnitOfSale1",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string> { "Cell1"},
                                RemoveProducts = new List<string>()
                            }
                        },
                        new UnitOfSale()
                        {
                            UnitName = "UnitOfSale2",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string> { "Cell1"},
                                RemoveProducts = new List<string>()
                            }
                        }
                    },
                }
            };

            var result = _fakeScenarioBuilder.BuildScenarios(fakeEventData);

            result.Count().Should().BeGreaterThan(0);
            result.Should().Satisfy(scenario => scenario.ScenarioType == ScenarioType.NewCell);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.IdentifyScenarioStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Identifying the scenarios based on received ENC content publish event.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.ScenarioIdentified.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Scenario identified for {ProductName} is {ScenarioName}.").MustHaveHappenedOnceExactly();
        }


        [Test]
        public void WhenEesEventDataDoesNotMatchAnyScenarioCondition_ThenEmptyScenarioList()
        {
            var fakeEventData = new EESEvent()
            {
                Data = new Data()
                {
                    Products = new List<Product>()
                    {
                        new Product()
                        {
                            ReplacedBy = new List<string>(),
                            InUnitsOfSale = new List<string> { "Cell1", "UnitOfSale1", "UnitofSale2" },
                            ProductName = "Cell1",
                            Status = new Status()
                            {
                            IsNewCell= false,
                            StatusName="New Edition"
                            },
                            ContentChanged=true
                        }
                    },
                    UnitsOfSales = new List<UnitOfSale>()
                    {
                        new UnitOfSale()
                        {
                            UnitName = "Cell1",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string>(),
                                RemoveProducts = new List<string>()
                            }
                        },
                        new UnitOfSale()
                        {
                            UnitName = "UnitOfSale1",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string>(),
                                RemoveProducts = new List<string>()
                            }
                        },
                        new UnitOfSale()
                        {
                            UnitName = "UnitOfSale2",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string>(),
                                RemoveProducts = new List<string>()
                            }
                        }
                    },
                }
            };

            var result = _fakeScenarioBuilder.BuildScenarios(fakeEventData);

            result.Count().Should().Be(0);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.IdentifyScenarioStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Identifying the scenarios based on received ENC content publish event.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.ScenarioIdentified.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Scenario identified for {ProductName} is {ScenarioName}.").MustNotHaveHappened();
        }

        [Test]
        public void WhenEesEventDataPassedWithStatusNameAsCancellationUpdateAndContentChangedAsTrue_ThenReturnsCancellAndReplaceCellScenario()
        {
            var fakeEventData = new EESEvent()
            {
                Data = new Data()
                {
                    Products = new List<Product>()
                    {
                        new Product()
                        {
                            ReplacedBy = new List<string> { "Cell2", "Cell3" },
                            InUnitsOfSale = new List<string> { "Cell1", "UnitOfSale1", "UnitofSale2" },
                            ProductName = "Cell1",
                            Status = new Status()
                            {
                            IsNewCell= false,
                            StatusName="Cancellation Update"
                            },
                            ContentChanged=true
                        }
                    },
                    UnitsOfSales = new List<UnitOfSale>()
                    {
                        new UnitOfSale()
                        {
                            UnitName = "Cell1",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string> { "Cell2", "Cell3" },
                                RemoveProducts = new List<string> { "Cell1" }
                            }
                        },
                        new UnitOfSale()
                        {
                            UnitName = "UnitOfSale1",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string> { "Cell2", "Cell3" },
                                RemoveProducts = new List<string> { "Cell1" }
                            }
                        },
                        new UnitOfSale()
                        {
                            UnitName = "UnitOfSale2",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string> { "Cell2", "Cell3" },
                                RemoveProducts = new List<string> { "Cell1" }
                            }
                        }
                    },
                }
            };

            var result = _fakeScenarioBuilder.BuildScenarios(fakeEventData);

            result.Count().Should().BeGreaterThan(0);
            result.Should().Satisfy(scenario => scenario.ScenarioType == ScenarioType.CancelReplaceCell);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.IdentifyScenarioStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Identifying the scenarios based on received ENC content publish event.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.ScenarioIdentified.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Scenario identified for {ProductName} is {ScenarioName}.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenEesEventDataPassedWithContentChangedAsFalse_ThenReturnsChangeMoveCellScenario()
        {
            var fakeEventData = new EESEvent()
            {
                Data = new Data()
                {
                    Products = new List<Product>()
                    {
                        new Product()
                        {
                            ReplacedBy = new List<string>(),
                            InUnitsOfSale = new List<string> { "Cell1", "UnitOfSale1", "UnitofSale2" },
                            ProductName = "Cell1",
                            Status = new Status()
                            {
                            IsNewCell= false,
                            StatusName="New Edition"
                            },
                            ContentChanged=false
                        }
                    },
                    UnitsOfSales = new List<UnitOfSale>()
                    {
                        new UnitOfSale()
                        {
                            UnitName = "Cell1",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string>(),
                                RemoveProducts = new List<string>{ "Cell1"}
                            }
                        },
                        new UnitOfSale()
                        {
                            UnitName = "UnitOfSale1",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string> { "Cell1"},
                                RemoveProducts = new List<string>()
                            }
                        },
                        new UnitOfSale()
                        {
                            UnitName = "UnitOfSale2",
                            CompositionChanges = new CompositionChanges()
                            {
                                AddProducts = new List<string>(),
                                RemoveProducts = new List<string>()
                            }
                        }
                    },
                }
            };

            var result = _fakeScenarioBuilder.BuildScenarios(fakeEventData);

            result.Count().Should().BeGreaterThan(0);
            result.Should().Satisfy(scenario => scenario.ScenarioType == ScenarioType.ChangeMoveCell);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.IdentifyScenarioStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Identifying the scenarios based on received ENC content publish event.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.ScenarioIdentified.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Scenario identified for {ProductName} is {ScenarioName}.").MustHaveHappenedOnceExactly();
        }

    }
}
