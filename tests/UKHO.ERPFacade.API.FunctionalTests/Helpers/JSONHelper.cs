using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
   
    
    public class JSONHelper
    {
        private UnitOfSalePrice UnitOfSaleFinalJSONHelper { get; set; }
        public bool verifyUniqueProducts(List<UoSInputJSONHelper> jsonUoSInputPayload, string generatedJSONFilePath)
        {
            var jsonString = getDeserializedString(generatedJSONFilePath);
            UnitOfSaleFinalJSONHelper = JsonConvert.DeserializeObject<UnitOfSalePrice>(jsonString);
            var dupes = UnitOfSaleFinalJSONHelper.EventData.Data.UnitsOfSalesPrices.GroupBy(x => new { x.unitName })
                        .Where(x => x.Skip(1).Any());
            if (dupes.Count() > 0) 
            { return false; }
            else
            { return true;
            }
            
               
        }

        public string getDeserializedString(String filePath)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            return requestBody;
        }
    }
}
