using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.TableEntities;
using UKHO.ERPFacade.Common.Models;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO.Azure;
using System.Xml;
using UKHO.ERPFacade.Common.IO;
using Microsoft.Azure.Amqp;

namespace UKHO.ERPFacade.API.Handler
{
    public abstract class EventHandler<T>: IEventHandler
    {
        private readonly ILogger<EventHandler> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly ISapClient _sapClient;
        private readonly IXmlHelper _xmlHelper;
        private readonly IFileSystemHelper _fileSystemHelper;        

        private const string ActionNumber = "ACTIONNUMBER";
        private const string XpathCorrId = $"//*[local-name()='CORRID']";
        private const string XpathNoOfActions = $"//*[local-name()='NOOFACTIONS']";
        private const string XpathRecDate = $"//*[local-name()='RECDATE']";
        private const string XpathRecTime = $"//*[local-name()='RECTIME']";
        private const string RecDateFormat = "yyyyMMdd";
        private const string RecTimeFormat = "hhmmss";


        public EventHandler(IAzureTableReaderWriter azureTableReaderWriter,
                                 IAzureBlobEventWriter azureBlobEventWriter,
                                 ISapClient sapClient,
                                 IXmlHelper xmlHelper,
                                 IFileSystemHelper fileSystemHelper
                                    )
        {
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _sapClient = sapClient;
            _xmlHelper = xmlHelper;
            _fileSystemHelper = fileSystemHelper;
        }

        public abstract string EventType { get; }

        public async Task HandleEvent(string encEventJson, IEventData eventData)
        {
            EncEventEntity encEventEntity = new()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                CorrelationId = eventData.CorrelationId,
                RequestDateTime = null
            };

            _logger.LogInformation(EventIds.AddingEntryForEncContentPublishedEventInAzureTable.ToEventId(), "Adding/Updating entry for enccontentpublished event in azure table.");
            await _azureTableReaderWriter.UpsertEntity(eventData.CorrelationId, Constants.S57EventTableName, encEventEntity);

            _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlobStarted.ToEventId(), "Uploading enccontentpublished event payload in blob storage.");
            await _azureBlobEventWriter.UploadEvent(encEventJson.ToString(), eventData.CorrelationId, Constants.S57EncEventFileName);
            _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlobCompleted.ToEventId(), "The enccontentpublished event payload is uploaded in blob storage successfully.");

            var sapPayload = await BuildSapMessageXml(eventData);

            _logger.LogInformation(EventIds.UploadSapXmlPayloadInAzureBlobStarted.ToEventId(), "Uploading the SAP XML payload in blob storage.");
            await _azureBlobEventWriter.UploadEvent(sapPayload.ToIndentedString(), eventData.CorrelationId, Constants.SapXmlPayloadFileName);
            _logger.LogInformation(EventIds.UploadSapXmlPayloadInAzureBlobCompleted.ToEventId(), "SAP XML payload is uploaded in blob storage successfully.");

            var response = await _sapClient.PostEventData(sapPayload, eventData.SapEndpointForEvent, eventData.SapServiceOperationForEvent, eventData.SapUsernameForEvent, eventData.SapPasswordForEvent);

            if (!response.IsSuccessStatusCode)
            {
                throw new ERPFacadeException(EventIds.RequestToSapFailed.ToEventId(), $"An error occurred while sending a request to SAP. | {response.StatusCode}");
            }
            _logger.LogInformation(EventIds.EncUpdateSentToSap.ToEventId(), "ENC update has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

            await _azureTableReaderWriter.UpdateEntity(eventData.CorrelationId, Constants.S57EventTableName, new[] { new KeyValuePair<string, DateTime>("RequestDateTime", DateTime.UtcNow) });

        }

        public async Task<XmlDocument> BuildSapMessageXml(IEventData eventData) 
        {
            string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, eventData.SapXmlPath);

            // Check if SAP XML payload template exists
            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                throw new ERPFacadeException(EventIds.SapXmlTemplateNotFound.ToEventId(), "The SAP XML payload template does not exist.");
            }

            var soapXml = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

            var actionItemNode = soapXml.SelectSingleNode(eventData.XpathActionItems);

            _logger.LogInformation(EventIds.GenerationOfSapXmlPayloadStarted.ToEventId(), "Generation of SAP XML payload started.");

            //// Build SAP actions for ENC Cell
            await BuildEncCellActions(eventData.EventData, soapXml, actionItemNode);                   

            // Build SAP actions for Units
            await BuildUnitActions(eventData.EventData, soapXml, actionItemNode);

            // Finalize SAP XML message
            FinalizeSapXmlMessage(soapXml, eventData.CorrelationId, actionItemNode, eventData.XpathImMatInfo);

            _logger.LogInformation(EventIds.GenerationOfSapXmlPayloadCompleted.ToEventId(), "Generation of SAP XML payload completed.");

            return soapXml;
        }

        public abstract Task BuildEncCellActions(T eventData, XmlDocument soapXml, XmlNode? actionItemNode);
        public abstract Task BuildUnitActions(T eventData, XmlDocument soapXml, XmlNode actionItemNode);
        private void FinalizeSapXmlMessage(XmlDocument soapXml, string correlationId, XmlNode actionItemNode, string XpathImMatInfo)
        {
            var xmlNode = SortXmlPayload(actionItemNode);

            SetXmlNodeValue(soapXml, XpathCorrId, correlationId);
            SetXmlNodeValue(soapXml, XpathNoOfActions, xmlNode.ChildNodes.Count.ToString());
            SetXmlNodeValue(soapXml, XpathRecDate, DateTime.UtcNow.ToString(RecDateFormat));
            SetXmlNodeValue(soapXml, XpathRecTime, DateTime.UtcNow.ToString(RecTimeFormat));

            var IM_MATINFONode = soapXml.SelectSingleNode(XpathImMatInfo);
            IM_MATINFONode.AppendChild(xmlNode);
        }
        private XmlNode SortXmlPayload(XmlNode actionItemNode)
        {
            // Extract all action item nodes
            var actionItems = actionItemNode.Cast<XmlNode>().ToList();
            int sequenceNumber = 1;

            // Sort based on the ActionNumber
            var sortedActionItems = actionItems
                .OrderBy(node => Convert.ToInt32(node.SelectSingleNode(ActionNumber)?.InnerText ?? "0"))
                .ToList();

            // Update the sequence number in the sorted list
            foreach (XmlNode actionItem in sortedActionItems)
            {
                var actionNumberNode = actionItem.SelectSingleNode(ActionNumber);
                if (actionNumberNode != null)
                {
                    actionNumberNode.InnerText = sequenceNumber.ToString();
                    sequenceNumber++;
                }
            }

            // Clear existing children and append sorted action items
            actionItemNode.RemoveAll();
            foreach (XmlNode actionItem in sortedActionItems)
            {
                actionItemNode.AppendChild(actionItem);
            }
            return actionItemNode;
        }
        private void SetXmlNodeValue(XmlDocument xmlDoc, string xPath, string value)
        {
            var node = xmlDoc.SelectSingleNode(xPath);
            if (node != null)
            {
                node.InnerText = value;
            }
        }

        /// <summary>
        /// Returns true if given product/unit satisfies rules for given action.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected bool ValidateActionRules(SapAction action, object obj)
        {
            bool isConditionSatisfied = false;

            //Return true if no rules for SAP action.
            if (action.Rules == null!) return true;

            foreach (var rules in action.Rules)
            {
                foreach (var conditions in rules.Conditions)
                {
                    object jsonFieldValue = CommonHelper.ParseXmlNode(conditions.AttributeName, obj, obj.GetType());

                    if (jsonFieldValue != null! && jsonFieldValue.ToString() == conditions.AttributeValue)
                    {
                        isConditionSatisfied = true;
                    }
                    else
                    {
                        isConditionSatisfied = false;
                        break;
                    }
                }
                if (isConditionSatisfied) break;
            }
            return isConditionSatisfied;
        }
        protected void AppendChildNode(XmlElement parentNode, XmlDocument doc, string nodeName, string value)
        {
            var childNode = doc.CreateElement(nodeName);
            childNode.InnerText = value ?? string.Empty;
            parentNode.AppendChild(childNode);
        }

        protected void ProcessAttributes(string action, IEnumerable<ActionItemAttribute> attributes, XmlDocument soapXml, object source, List<(int, XmlElement)> actionAttributes, DecryptedPermit decryptedPermit = null, string replacedBy = null)
        {
            foreach (var attribute in attributes)
            {
                try
                {
                    var attributeNode = soapXml.CreateElement(attribute.XmlNodeName);

                    if (attribute.IsRequired)
                    {
                        switch (attribute.XmlNodeName)
                        {
                            case Constants.ReplacedBy:
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, replacedBy)) attributeNode.InnerText = GetXmlNodeValue(replacedBy.ToString(), attribute.XmlNodeName);
                                break;
                            case Constants.ActiveKey:
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, decryptedPermit.ActiveKey)) attributeNode.InnerText = GetXmlNodeValue(decryptedPermit.ActiveKey, attribute.XmlNodeName);
                                break;
                            case Constants.NextKey:
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, decryptedPermit.NextKey)) attributeNode.InnerText = GetXmlNodeValue(decryptedPermit.NextKey, attribute.XmlNodeName);
                                break;
                            default:
                                var jsonFieldValue = CommonHelper.ParseXmlNode(attribute.JsonPropertyName, source, source.GetType()).ToString();
                                if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, jsonFieldValue))
                                {
                                    attributeNode.InnerText = GetXmlNodeValue(jsonFieldValue.ToString(), attribute.XmlNodeName);
                                }
                                break;
                        }
                    }
                    else
                    {
                        attributeNode.InnerText = string.Empty;
                    }
                    actionAttributes.Add((attribute.SortingOrder, attributeNode));
                }
                catch (Exception ex)
                {
                    throw new ERPFacadeException(EventIds.BuildingSapActionInformationException.ToEventId(), $"Error while generating SAP action information. | Action : {action} | XML Attribute : {attribute.XmlNodeName} | ErrorMessage : {ex.Message}");
                }
            }
        }

        protected string GetXmlNodeValue(string fieldValue, string xmlNodeName = null)
        {
            // Return first 2 characters if the node is Agency, else limit other nodes to 250 characters
            return xmlNodeName == Constants.Agency ? CommonHelper.ToSubstring(fieldValue, 0, Constants.MaxAgencyXmlNodeLength) : CommonHelper.ToSubstring(fieldValue, 0, Constants.MaxXmlNodeLength);
        }
        private bool IsPropertyNullOrEmpty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
            {
                throw new ERPFacadeException(EventIds.EmptyEventJsonPropertyException.ToEventId(), $"Required details are missing in enccontentpublished event payload. | Property Name : {propertyName}");
            }
            else return false;
        }
    }
}
