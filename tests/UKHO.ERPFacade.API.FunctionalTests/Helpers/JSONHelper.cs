﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class JsonHelper
    {
        private static readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private static readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        public static async Task<List<JsonInputRoSWebhookEvent>> GetEventJsonListUsingFileNameAsync(List<string> fileNames)
        {
            List<JsonInputRoSWebhookEvent> listOfEventJsons = new();

            foreach (var filePath in fileNames.Select(fileName => Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", fileName)))
            {
                string requestBody;

                using (StreamReader streamReader = new(filePath))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                JsonInputRoSWebhookEvent eventPayloadJson = JsonConvert.DeserializeObject<JsonInputRoSWebhookEvent>(requestBody);
                listOfEventJsons.Add(eventPayloadJson);
            }
            return listOfEventJsons;
        }
    }
}
