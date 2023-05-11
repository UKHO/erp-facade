﻿using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public class XmlHelper : IXmlHelper
    {
        public XmlDocument CreateXmlDocument(string xmlPath)
        {
            XmlDocument xmlDocument = new();
            xmlDocument.Load(xmlPath);
            return xmlDocument;
        }
    }
}
