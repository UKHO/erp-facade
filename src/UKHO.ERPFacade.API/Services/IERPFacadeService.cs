﻿using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.Services
{
    public interface IErpFacadeService
    {
        List<UnitsOfSalePrices> BuildUnitOfSalePricePayload(List<PriceInformationEvent> priceInformationList);

        UnitOfSalePriceEventPayload BuildPriceEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string existingEesEvent);

        BulkPriceEventPayload BuildBulkPriceEventPayload(UnitsOfSalePrices unitsOfSalePriceList);
    }
}