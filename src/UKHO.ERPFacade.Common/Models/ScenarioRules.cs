﻿using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class Rule
    {
        public List<Conditions> Conditions { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Conditions
    {
        public string AttributeDataType { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
    }

    public enum ScenarioType
    {
        NewCell = 1,
        CancelReplaceCell = 2,
        UpdateCell = 3,
        ChangeMoveCell = 4,
        ChangeUnitOfSale = 5
    }
}
