using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{


    public class JSONHelper
    {
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
