using System.Xml;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Models.SapActionConfigurationModels;
using UKHO.ERPFacade.Common.Operations;

namespace UKHO.ERPFacade.API.XmlTransformers
{
    public abstract class BaseXmlTransformer : IXmlTransformer
    {
        public abstract XmlDocument BuildXmlPayload<T>(T eventData, string xmlTemplatePath);

        public bool ValidateActionRules(Actions action, object obj)
        {
            bool isConditionSatisfied = false;

            //Return true if no rules for SAP action.
            if (action.Rules == null!) return true;

            foreach (var rules in action.Rules)
            {
                foreach (var conditions in rules.Conditions)
                {
                    object jsonAttributeValue = Extractor.ExtractJsonAttributeValue(conditions.AttributeName, obj, obj.GetType());

                    if (jsonAttributeValue != null! && string.Equals(jsonAttributeValue.ToString(), conditions.AttributeValue, StringComparison.InvariantCultureIgnoreCase))
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

        public void FinalizeSapXmlMessage(XmlDocument soapXml, string correlationId, XmlNode actionItemNode, string xmlPathInfo)
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

            var noOfActionsNode = soapXml.SelectSingleNode(XmlTemplateInfo.XpathNoOfActions);
            noOfActionsNode.InnerText = actionItemNode.ChildNodes.Count.ToString();

            DateTime currentDateTime = DateTime.UtcNow;

            var recDateNode = soapXml.SelectSingleNode(XmlTemplateInfo.XpathRecDate);
            recDateNode.InnerText = currentDateTime.ToString(DateTimeFormats.RecDateFormat);

            var recTimeNode = soapXml.SelectSingleNode(XmlTemplateInfo.XpathRecTime);
            recTimeNode.InnerText = currentDateTime.ToString(DateTimeFormats.RecTimeFormat);

            //Set action items
            var parentInfoNode = soapXml.SelectSingleNode(xmlPathInfo);
            parentInfoNode.AppendChild(actionItemNode);
        }
    }
}
