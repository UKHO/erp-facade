using System.Xml;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public class SapMessageHelper
    {
        private readonly IFileSystemHelper _fileSystemHelper;
        private readonly IXmlHelper _xmlHelper;

        public SapMessageHelper(IFileSystemHelper fileSystemHelper, IXmlHelper xmlHelper)
        {
            _fileSystemHelper = fileSystemHelper;
            _xmlHelper = xmlHelper;
        }

        public XmlDocument CreateXmlDocument(string templatePath)
        {
            string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, templatePath);

            // Check if SAP XML payload template exists
            if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
            {
                throw new ERPFacadeException(EventIds.SapXmlTemplateNotFound.ToEventId(), "The SAP XML payload template does not exist.");
            }

            return _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);
        }

        public bool ValidateActionRules(SapAction action, object obj)
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

        public void FinalizeSapXmlMessage(XmlDocument soapXml, string correlationId, XmlNode actionItemNode)
        {
            var xmlNode = SortXmlPayload(actionItemNode);

            SetXmlNodeValue(soapXml, Constants.XpathCorrId, correlationId);
            SetXmlNodeValue(soapXml, Constants.XpathNoOfActions, xmlNode.ChildNodes.Count.ToString());
            SetXmlNodeValue(soapXml, Constants.XpathRecDate, DateTime.UtcNow.ToString(Constants.RecDateFormat));
            SetXmlNodeValue(soapXml, Constants.XpathRecTime, DateTime.UtcNow.ToString(Constants.RecTimeFormat));

            var IM_MATINFONode = soapXml.SelectSingleNode(Constants.XpathImMatInfo);
            IM_MATINFONode.AppendChild(xmlNode);
        }

        public bool IsPropertyNullOrEmpty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
            {
                throw new ERPFacadeException(EventIds.EmptyEventJsonPropertyException.ToEventId(), $"Required details are missing in enccontentpublished event payload. | Property Name : {propertyName}");
            }
            else return false;
        }

        public void ProcessAttributes(string action, IEnumerable<ActionItemAttribute> attributes, XmlDocument soapXml, object source, List<(int, XmlElement)> actionAttributes, string replacedBy = null)
        {
            foreach (var attribute in attributes)
            {
                try
                {
                    var attributeNode = soapXml.CreateElement(attribute.XmlNodeName);

                    if (attribute.IsRequired)
                    {
                        if (attribute.XmlNodeName == Constants.ReplacedBy)
                        {
                            if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, replacedBy)) attributeNode.InnerText = GetXmlNodeValue(replacedBy.ToString(), attribute.XmlNodeName);
                        }
                        else
                        {
                            var jsonFieldValue = CommonHelper.ParseXmlNode(attribute.JsonPropertyName, source, source.GetType()).ToString();
                            if (!IsPropertyNullOrEmpty(attribute.JsonPropertyName, jsonFieldValue))
                            {
                                attributeNode.InnerText = GetXmlNodeValue(jsonFieldValue.ToString(), attribute.XmlNodeName);
                            }
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

        public string GetXmlNodeValue(string fieldValue, string xmlNodeName = null)
        {
            // Return first 2 characters if the node is Agency, else limit other nodes to 250 characters
            return xmlNodeName == Constants.Agency ? CommonHelper.ToSubstring(fieldValue, 0, Constants.MaxAgencyXmlNodeLength) : CommonHelper.ToSubstring(fieldValue, 0, Constants.MaxXmlNodeLength);
        }

        public void AppendChildNode(XmlElement parentNode, XmlDocument doc, string nodeName, string value)
        {
            var childNode = doc.CreateElement(nodeName);
            childNode.InnerText = value ?? string.Empty;
            parentNode.AppendChild(childNode);
        }

        //private methods
        private void SetXmlNodeValue(XmlDocument xmlDoc, string xPath, string value)
        {
            var node = xmlDoc.SelectSingleNode(xPath);
            if (node != null)
            {
                node.InnerText = value;
            }
        }

        private XmlNode SortXmlPayload(XmlNode actionItemNode)
        {
            // Extract all action item nodes
            var actionItems = actionItemNode.Cast<XmlNode>().ToList();
            int sequenceNumber = 1;

            // Sort based on the ActionNumber
            var sortedActionItems = actionItems
                .OrderBy(node => Convert.ToInt32(node.SelectSingleNode(Constants.ActionNumber)?.InnerText ?? "0"))
                .ToList();

            // Update the sequence number in the sorted list
            foreach (XmlNode actionItem in sortedActionItems)
            {
                var actionNumberNode = actionItem.SelectSingleNode(Constants.ActionNumber);
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
    }
}
