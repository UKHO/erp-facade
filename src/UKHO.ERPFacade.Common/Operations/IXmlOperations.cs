﻿using System.Xml;

namespace UKHO.ERPFacade.Common.Operations
{
    public interface IXmlOperations
    {
        XmlDocument CreateXmlDocument(string xmlPath);
        string CreateXmlPayLoad<T>(T anyobject);
        void AppendChildNode(XmlElement parentNode, XmlDocument doc, string nodeName, string value);
    }
}