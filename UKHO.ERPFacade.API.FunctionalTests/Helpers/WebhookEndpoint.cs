﻿using RestSharp;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    
    public class WebhookEndpoint
    {
        private readonly TestConfiguration _config = new();
        private readonly string _requestBody = "{\r\n    \"specversion\": \"1.0\",\r\n    \"type\": \"uk.gov.ukho.encpublishing.enccontentpublished.v2\",\r\n    \"source\": \"https://encpublishing.ukho.gov.uk\",\r\n    \"id\": \"2f03a25f-28b3-46ea-b009-5943250a9a41\",\r\n    \"time\": \"2020-10-13T12:08:03.4880776Z\",\r\n    \"_COMMENT\": \"A comma separated list of products\",\r\n    \"subject\": \"GB302409, GB602376\",\r\n    \"datacontenttype\": \"application/json\",\r\n    \"data\": {\r\n        \"traceId\": \"367ce4a4-1d62-4f56-b359-59e178d77100\",\r\n        \"products\": [\r\n            {\r\n                \"productType\": \"ENC S57\",\r\n                \"dataSetName\": \"GB302409.001\",\r\n                \"productName\": \"GB302409\",\r\n                \"title\": \"Taiwan - West Coast - Kao-Hsiung Kang to P'eng-Hu Ch'un-Tao\",\r\n                \"scale\": 90000,\r\n                \"usageBand\": 1,\r\n                \"editionNumber\": 2,\r\n                \"updateNumber\": 6,\r\n                \"mayAffectHoldings\": true,\r\n                \"permit\": \"permitString\",\r\n                \"providerName\": \"IC-ENC\",\r\n                \"_COMMENT\": \" provider code not included as will be in ERP Facade\",\r\n                \"_ENUM\": [\r\n                    \"IC-ENC\",\r\n                    \"IC-GB\",\r\n                    \"IC-UK\",\r\n                    \"PRIMAR\",\r\n                    \"VAR Unique\",\r\n                    \"VAR\"\r\n                ],\r\n                \"size\": \"large\",\r\n                \"_COMMENT\": \" size code not included as will be in ERP Facade\",\r\n                \"_ENUM\": [\r\n                    \"large\",\r\n                    \"medium\",\r\n                    \"small\"\r\n                ],\r\n                \"agency\": \"GB\",\r\n                \"bundle\": [\r\n                    {\r\n                        \"bundleType\": \"DVD\",\r\n                        \"_ENUM\": [\r\n                            \"DVD\"\r\n                        ],\r\n                        \"location\": \"M2;B1\"\r\n                    }\r\n                ],\r\n                \"status\": {\r\n                    \"statusName\": \"Update\",\r\n                    \"_ENUM\": [\r\n                        \"New Edition\",\r\n                        \"Re-issue\",\r\n                        \"Update\",\r\n                        \"Cancellation Update\",\r\n                        \"Withdrawn\",\r\n                        \"Suspended\"\r\n                    ],\r\n                    \"statusDate\": \"2020-07-16T19:20:30.45+01:00\",\r\n                    \"isNewCell\": false,\r\n                    \"_COMMENT\": \"A cell new to the service\"\r\n                },\r\n                \"replaces\": [\r\n                    \"GB602376\",\r\n                    \"GB503230\"\r\n                ],\r\n                \"replacedBy\": [],\r\n                \"additionalCoverage\": [],\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {\r\n                        \"northLimit\": 24.146815,\r\n                        \"southLimit\": 22.581615,\r\n                        \"eastLimit\": 120.349635,\r\n                        \"westLimit\": 119.39142\r\n                    },\r\n                    \"polygons\": [\r\n                        {\r\n                            \"polygon\": [\r\n                                {\r\n                                    \"latitude\": 24,\r\n                                    \"longitude\": 121.5\r\n                                },\r\n                                {\r\n                                    \"latitude\": 121,\r\n                                    \"longitude\": 56\r\n                                },\r\n                                {\r\n                                    \"latitude\": 45,\r\n                                    \"longitude\": 78\r\n                                },\r\n                                {\r\n                                    \"latitude\": 119,\r\n                                    \"longitude\": 121.5\r\n                                }\r\n                            ]\r\n                        },\r\n                        {\r\n                            \"polygon\": [\r\n                                {\r\n                                    \"latitude\": 24,\r\n                                    \"longitude\": 121.5\r\n                                },\r\n                                {\r\n                                    \"latitude\": 121,\r\n                                    \"longitude\": 56\r\n                                },\r\n                                {\r\n                                    \"latitude\": 45,\r\n                                    \"longitude\": 78\r\n                                },\r\n                                {\r\n                                    \"latitude\": 119,\r\n                                    \"longitude\": 121.5\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                \"_COMMENT\": \"The units in which product is included, including any 1-1 unit\",\r\n                \"inUnitsOfSale\": [\r\n                    \"GB302409\",\r\n                    \"P0397\",\r\n                    \"P5272\",\r\n                    \"PAYSF\",\r\n                    \"RFP07\"\r\n                ],\r\n                \"s63\": {\r\n                    \"name\": \"GB302409.001\",\r\n                    \"hash\": \"5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea\",\r\n                    \"location\": \"8198201e-78ce-4af8-9145-ad68ba0472e2\",\r\n                    \"fileSize\": \"4500\",\r\n                    \"compression\": true,\r\n                    \"s57Crc\": \"5C06E104\"\r\n                },\r\n                \"signature\": {\r\n                    \"name\": \"GB302409.001\",\r\n                    \"hash\": \"fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9\",\r\n                    \"location\": \"0ecf2f38-a876-4d77-bd0e-0d901d3a0e73\",\r\n                    \"fileSize\": \"2500\"\r\n                },\r\n                \"ancillaryFiles\": [\r\n                    {\r\n                        \"name\": \"GB123_04.TXT\",\r\n                        \"hash\": \"d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b\",\r\n                        \"location\": \"2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5\",\r\n                        \"fileSize\": \"1240\"\r\n                    },\r\n                    {\r\n                        \"name\": \"GB125_01.TXT\",\r\n                        \"hash\": \"bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b\",\r\n                        \"location\": \"1ad0f9c3-8c93-495a-99a1-06a36410faa9\",\r\n                        \"fileSize\": \"1360\"\r\n                    },\r\n                    {\r\n                        \"name\": \"GB162_01.TXT\",\r\n                        \"hash\": \"81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6\",\r\n                        \"location\": \"eb443aad-394c-4eb0-b391-415a261605a1\",\r\n                        \"fileSize\": \"1360\"\r\n                    }\r\n                ]\r\n            },\r\n            {\r\n                \"productType\": \"ENC S57\",\r\n                \"dataSetName\": \"GB602376.001\",\r\n                \"productName\": \"GB602376\",\r\n                \"title\": \"Kao-hsiung\",\r\n                \"scale\": 12000,\r\n                \"usageBand\": 6,\r\n                \"editionNumber\": 8,\r\n                \"updateNumber\": 2,\r\n                \"mayAffectHoldings\": true,\r\n                \"permit\": \"4A4E096BC7SDSAFEQAE71194324\",\r\n                \"providerName\": \"IC-ENC\",\r\n                \"_COMMENT\": \" provider code not included as will be in ERP Facade\",\r\n                \"_ENUM\": [\r\n                    \"IC-ENC\",\r\n                    \"IC-GB\",\r\n                    \"IC-UK\",\r\n                    \"PRIMAR\",\r\n                    \"VAR Unique\",\r\n                    \"VAR\"\r\n                ],\r\n                \"size\": \"large\",\r\n                \"_COMMENT\": \" size code not included as will be in ERP Facade\",\r\n                \"_ENUM\": [\r\n                    \"large\",\r\n                    \"medium\",\r\n                    \"small\"\r\n                ],\r\n                \"agency\": \"GB\",\r\n                \"bundle\": [\r\n                    {\r\n                        \"bundleType\": \"DVD\",\r\n                        \"_ENUM\": [\r\n                            \"DVD\"\r\n                        ],\r\n                        \"location\": \"M2;B1\"\r\n                    }\r\n                ],\r\n                \"status\": {\r\n                    \"statusName\": \"Cancelled\",\r\n                    \"statusDate\": \"2020-07-16T19:20:30.45+01:00\",\r\n                    \"isNewCell\": false\r\n                },\r\n                \"replaces\": [],\r\n                \"replacedBy\": [\r\n                    \"GB302409\"\r\n                ],\r\n                \"additionalCoverage\": [],\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {\r\n                        \"northLimit\": 22.643255,\r\n                        \"southLimit\": 22.4625767,\r\n                        \"eastLimit\": 120.34972,\r\n                        \"westLimit\": 120.219475\r\n                    },\r\n                    \"polygons\": [\r\n                        {\r\n                            \"polygon\": [\r\n                                {\r\n                                    \"latitude\": 22.57515,\r\n                                    \"longitude\": 120.21948\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.64326,\r\n                                    \"longitude\": 120\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.57154,\r\n                                    \"longitude\": 120\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.57515,\r\n                                    \"longitude\": 121.21948\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                \"s63\": {\r\n                    \"name\": \"GB602376.001\",\r\n                    \"hash\": \"5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea\",\r\n                    \"location\": \"8198201e-78ce-4af8-9145-ad68ba0472e2\",\r\n                    \"fileSize\": \"4500\",\r\n                    \"compression\": true,\r\n                    \"s57Crc\": \"5C06E104\"\r\n                },\r\n                \"signature\": {\r\n                    \"name\": \"GB602376.001\",\r\n                    \"hash\": \"fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9\",\r\n                    \"location\": \"0ecf2f38-a876-4d77-bd0e-0d901d3a0e73\",\r\n                    \"fileSize\": \"2500\"\r\n                },\r\n                \"ancillaryFiles\": [\r\n                    {\r\n                        \"name\": \"GB602376_04.TXT\",\r\n                        \"hash\": \"d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b\",\r\n                        \"location\": \"2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5\",\r\n                        \"fileSize\": \"1240\"\r\n                    }\r\n                ]\r\n            }\r\n        ],\r\n        \"_COMMENT\": \"Prices for all units in event will be included, including Cancelled Cell\",\r\n        \"unitsOfSale\": [\r\n            {\r\n                \"unitName\": \"GB302409\",\r\n                \"title\": \"Kao-Hsiung P'eng-Hu Ch'un-Tao\",\r\n                \"unitType\": \"AVCS Units Coastal\",\r\n                \"status\": \"ForSale\",\r\n                \"_ENUM\": [\r\n                    \"ForSale\",\r\n                    \"NotForSale\"\r\n                ],\r\n                \"_COMMENT\": \"BoundingBox or polygon or both or neither\",\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {\r\n                        \"northLimit\": 24.146815,\r\n                        \"southLimit\": 22.581615,\r\n                        \"eastLimit\": 120.349635,\r\n                        \"westLimit\": 119.39142\r\n                    },\r\n                    \"polygons\": []\r\n                },\r\n                \"compositionChanges\": {\r\n                    \"addProducts\": [],\r\n                    \"removeProducts\": []\r\n                }\r\n            },\r\n            {\r\n                \"unitName\": \"GB602376\",\r\n                \"title\": \"Kao-Hsiung\",\r\n                \"unitType\": \"AVCS Units Berthing\",\r\n                \"status\": \"NotForSale\",\r\n                \"_COMMENT\": \"avcs_cat has either boundingBox or polygon - should both be included?\",\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {\r\n                        \"northLimit\": 22.643255,\r\n                        \"southLimit\": 22.4625767,\r\n                        \"eastLimit\": 120.34972,\r\n                        \"westLimit\": 120.219475\r\n                    },\r\n                    \"polygons\": [\r\n                        {\r\n                            \"polygon\": []\r\n                        }\r\n                    ]\r\n                },\r\n                \"compositionChanges\": {\r\n                    \"addProducts\": [],\r\n                    \"removeProducts\": [\r\n                        \"GB602376\"\r\n                    ]\r\n                }\r\n            },\r\n            {\r\n                \"unitName\": \"P0397\",\r\n                \"title\": \"Kaohsiung\",\r\n                \"unitType\": \"AVCS Folio Port\",\r\n                \"status\": \"ForSale\",\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {},\r\n                    \"polygons\": [\r\n                        {\r\n                            \"polygon\": [\r\n                                {\r\n                                    \"latitude\": 22.57515,\r\n                                    \"longitude\": 120.21948\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.64326,\r\n                                    \"longitude\": 120\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.57154,\r\n                                    \"longitude\": 120\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.57515,\r\n                                    \"longitude\": 121.21948\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                \"compositionChanges\": {\r\n                    \"addProducts\": [\r\n                        \"GB302409\"\r\n                    ],\r\n                    \"removeProducts\": [\r\n                        \"GB602376\"\r\n                    ]\r\n                }\r\n            },\r\n            {\r\n                \"unitName\": \"P5272\",\r\n                \"title\": \"Ta-Lin-Pu Offshore Oil Terminal\",\r\n                \"unitType\": \"AVCS Folio Port\",\r\n                \"status\": \"ForSale\",\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {},\r\n                    \"polygons\": [\r\n                        {\r\n                            \"polygon\": [\r\n                                {\r\n                                    \"latitude\": 22.57515,\r\n                                    \"longitude\": 120.21948\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.64326,\r\n                                    \"longitude\": 120\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.57154,\r\n                                    \"longitude\": 120\r\n                                },\r\n                                {\r\n                                    \"latitude\": 22.57515,\r\n                                    \"longitude\": 121.21948\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                \"compositionChanges\": {\r\n                    \"addProducts\": [\r\n                        \"GB302409\"\r\n                    ],\r\n                    \"removeProducts\": [\r\n                        \"GB602376\"\r\n                    ]\r\n                }\r\n            },\r\n            {\r\n                \"unitName\": \"PAYSF\",\r\n                \"title\": \"World Folio\",\r\n                \"unitType\": \"AVCS Folio Transit\",\r\n                \"status\": \"ForSale\",\r\n                \"geographicLimit\": {\r\n                    \"boundingBox\": {},\r\n                    \"polygons\": [\r\n                        {\r\n                            \"polygon\": [\r\n                                {\r\n                                    \"latitude\": 89,\r\n                                    \"longitude\": -179.995\r\n                                },\r\n                                {\r\n                                    \"latitude\": 89,\r\n                                    \"longitude\": 0\r\n                                },\r\n                                {\r\n                                    \"latitude\": 89,\r\n                                    \"longitude\": 179.995\r\n                                },\r\n                                {\r\n                                    \"latitude\": -89,\r\n                                    \"longitude\": 179.995\r\n                                },\r\n                                {\r\n                                    \"latitude\": -89,\r\n                                    \"longitude\": 0\r\n                                },\r\n                                {\r\n                                    \"latitude\": -89,\r\n                                    \"longitude\": -179.995\r\n                                },\r\n                                {\r\n                                    \"latitude\": 89,\r\n                                    \"longitude\": -179.995\r\n                                }\r\n                            ]\r\n                        }\r\n                    ]\r\n                },\r\n                \"compositionChanges\": {\r\n                    \"addProducts\": [\r\n                        \"GB302409\"\r\n                    ],\r\n                    \"removeProducts\": [\r\n                        \"GB602376\"\r\n                    ]\r\n                }\r\n            }\r\n        ]\r\n    }\r\n}";

        public RestResponse PostWebhookResponseFile()
        {
            var client = new RestClient(_config.erpfacadeConfig.BaseUrl);
            var request = new RestRequest(_config.erpfacadeConfig.BaseUrl + $"/webhook/newenccontentpublishedeventreceived", Method.Post);

            request.AddParameter("application/json", _requestBody, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;
            RestResponse response = client.Execute(request);
            return response;
        }

        public RestResponse GetWebhookOptionResponse()
        {
            String requestBody = "{ }";
            var client = new RestClient(_config.erpfacadeConfig.BaseUrl);
            var request = new RestRequest(_config.erpfacadeConfig.BaseUrl + $"/webhook/newenccontentpublishedeventoptions", Method.Options);

            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;
            RestResponse response = client.Execute(request);
            return response;
        }
    }
}
