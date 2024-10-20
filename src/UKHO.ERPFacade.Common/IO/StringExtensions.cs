﻿using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public static class StringExtensions
    {
        public static string ToIndentedString(this XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }

        public static string SetXmlClosingTags(this string xmlString)
        {
            XmlDocument xmldoc = new();
            xmldoc.LoadXml(xmlString);
            XmlNodeList emptyElementList = xmldoc.SelectNodes(@"//*[not(node()) and count(@*) = 0]");

            for (int i = 0; i < emptyElementList.Count; i++)
            {
                emptyElementList[i].InnerText = "";
            }

            return xmldoc.InnerXml;
        }

        public static string RemoveNullFields(this string xml)
        {
            XmlDocument xmldoc = new();
            xmldoc.LoadXml(xml);

            XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
            mgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            XmlNodeList nullFields = xmldoc.SelectNodes("//*[@xsi:nil='true']", mgr);

            if (nullFields != null && nullFields.Count > 0)
            {
                for (int i = 0; i < nullFields.Count; i++)
                {
                    XmlDocumentFragment xmlDocFrag = xmldoc.CreateDocumentFragment();
                    string newNode = "<" + nullFields[i].Name + "></" + nullFields[i].Name + ">";
                    xmlDocFrag.InnerXml = newNode;

                    var previousNode = nullFields[i].PreviousSibling;
                    string Xpath = $"//*[local-name()='{previousNode.Name}']";

                    XmlElement element = (XmlElement)xmldoc.SelectSingleNode(Xpath);

                    nullFields[i].ParentNode.RemoveChild(nullFields[i]);

                    XmlNode parent = element.ParentNode;
                    parent.InsertAfter(xmlDocFrag, element);
                }
            }
            return xmldoc.InnerXml;
        }
    }
}
