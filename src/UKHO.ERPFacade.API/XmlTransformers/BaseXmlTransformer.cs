using System.Xml;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Operations;

namespace UKHO.ERPFacade.API.XmlTransformers
{
    public interface IBaseXmlTransformer
    {
        XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath);
        bool ValidateActionRules(SapAction action, object obj);
        bool IsPropertyNullOrEmpty(string propertyName, string propertyValue);
        void FinalizeSapXmlMessage(XmlDocument soapXml, string correlationId, XmlNode actionItemNode);
    }

    public abstract class BaseXmlTransformer : IBaseXmlTransformer
    {
        public BaseXmlTransformer()
        {
        }

        public abstract XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath);

        public bool ValidateActionRules(SapAction action, object obj)
        {
            bool isConditionSatisfied = false;

            //Return true if no rules for SAP action.
            if (action.Rules == null!) return true;

            foreach (var rules in action.Rules)
            {
                foreach (var conditions in rules.Conditions)
                {
                    object jsonFieldValue = CommonOperations.GetPropertyValue(conditions.AttributeName, obj, obj.GetType());

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
            // Extract all action item nodes
            var actionItems = actionItemNode.Cast<XmlNode>().ToList();
            int sequenceNumber = 1;

            // Sort based on the ActionNumber
            var sortedActionItems = actionItems
                .OrderBy(node => Convert.ToInt32(node.SelectSingleNode(XmlFields.ActionNumber)?.InnerText ?? "0"))
                .ToList();

            // Update the sequence number in the sorted list
            foreach (XmlNode actionItem in sortedActionItems)
            {
                var actionNumberNode = actionItem.SelectSingleNode(XmlFields.ActionNumber);
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

            //Set basic nodes
            var corrIdNode = soapXml.SelectSingleNode(XmlTemplateInfo.XpathCorrId);
            corrIdNode.InnerText = correlationId;

            var noOfActionsNode = soapXml.SelectSingleNode(XmlTemplateInfo.XpathCorrId);
            noOfActionsNode.InnerText = actionItemNode.ChildNodes.Count.ToString();

            var recDateNode = soapXml.SelectSingleNode(XmlTemplateInfo.XpathRecDate);
            recDateNode.InnerText = DateTime.UtcNow.ToString(XmlFields.RecDateFormat);

            var recTimeNode = soapXml.SelectSingleNode(XmlTemplateInfo.XpathRecTime);
            recTimeNode.InnerText = DateTime.UtcNow.ToString(XmlFields.RecTimeFormat);

            //Set action items
            var IM_MATINFONode = soapXml.SelectSingleNode(XmlTemplateInfo.XpathImMatInfo);
            IM_MATINFONode.AppendChild(actionItemNode);
        }

        public bool IsPropertyNullOrEmpty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
            {
                throw new ERPFacadeException(EventIds.EmptyEventJsonPropertyException.ToEventId(), $"Required details are missing in enccontentpublished event payload. | Property Name : {propertyName}");
            }
            else return false;
        }
    }
}
