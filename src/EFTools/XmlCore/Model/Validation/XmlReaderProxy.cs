// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.Schema;

    internal class XmlReaderProxy : XmlReader, IXmlLineInfo
    {
        private readonly XmlReader _proxy;
        private readonly Uri _baseUri;
        private readonly IXmlLineInfo _lineNumberService;

        public XmlReaderProxy(XmlReader xmlreader, Uri baseUri, IXmlLineInfo lineNumberService)
        {
            Debug.Assert(xmlreader != null, "xmlreader != null");
            Debug.Assert(baseUri != null, "baseUri != null");

            _proxy = xmlreader;
            _baseUri = baseUri;
            _lineNumberService = lineNumberService;
        }

        public override string BaseURI
        {
            get { return _baseUri.ToString(); }
        }

        public bool HasLineInfo()
        {
            if (_lineNumberService != null)
            {
                return _lineNumberService.HasLineInfo();
            }

            var lineInfo = _proxy as IXmlLineInfo;
            if (lineInfo != null)
            {
                return lineInfo.HasLineInfo();
            }

            return false;
        }

        public int LineNumber
        {
            get
            {
                var lineNumber = 0;
                if (_lineNumberService != null)
                {
                    lineNumber = _lineNumberService.LineNumber;
                }
                else
                {
                    var lineInfo = _proxy as IXmlLineInfo;
                    if (lineInfo != null)
                    {
                        lineNumber = lineInfo.LineNumber;
                    }
                }
                return lineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                var linePosition = 0;
                if (_lineNumberService != null)
                {
                    linePosition = _lineNumberService.LinePosition;
                }
                else
                {
                    var lineInfo = _proxy as IXmlLineInfo;
                    if (lineInfo != null)
                    {
                        linePosition = lineInfo.LinePosition;
                    }
                }
                return linePosition;
            }
        }

        public override XmlReaderSettings Settings
        {
            get { return _proxy.Settings; }
        }

        public override XmlNodeType NodeType
        {
            get { return _proxy.NodeType; }
        }

        public override string Name
        {
            get { return _proxy.Name; }
        }

        public override string LocalName
        {
            get { return _proxy.LocalName; }
        }

        public override string NamespaceURI
        {
            get { return _proxy.NamespaceURI; }
        }

        public override string Prefix
        {
            get { return _proxy.Prefix; }
        }

        public override bool HasValue
        {
            get { return _proxy.HasValue; }
        }

        public override string Value
        {
            get { return _proxy.Value; }
        }

        public override int Depth
        {
            get { return _proxy.Depth; }
        }

        public override bool IsEmptyElement
        {
            get { return _proxy.IsEmptyElement; }
        }

        public override bool IsDefault
        {
            get { return _proxy.IsDefault; }
        }

        public override char QuoteChar
        {
            get { return _proxy.QuoteChar; }
        }

        public override XmlSpace XmlSpace
        {
            get { return _proxy.XmlSpace; }
        }

        public override string XmlLang
        {
            get { return _proxy.XmlLang; }
        }

        public override IXmlSchemaInfo SchemaInfo
        {
            get { return _proxy.SchemaInfo; }
        }

        public override Type ValueType
        {
            get { return _proxy.ValueType; }
        }

        public override int AttributeCount
        {
            get { return _proxy.AttributeCount; }
        }

        public override bool EOF
        {
            get { return _proxy.EOF; }
        }

        public override ReadState ReadState
        {
            get { return _proxy.ReadState; }
        }

        public override XmlNameTable NameTable
        {
            get { return _proxy.NameTable; }
        }

        public override bool CanResolveEntity
        {
            get { return _proxy.CanResolveEntity; }
        }

        public override bool CanReadBinaryContent
        {
            get { return _proxy.CanReadBinaryContent; }
        }

        public override bool CanReadValueChunk
        {
            get { return _proxy.CanReadValueChunk; }
        }

        public override bool HasAttributes
        {
            get { return _proxy.HasAttributes; }
        }

        public override object ReadContentAsObject()
        {
            return _proxy.ReadContentAsObject();
        }

        public override bool ReadContentAsBoolean()
        {
            return _proxy.ReadContentAsBoolean();
        }

        public override DateTime ReadContentAsDateTime()
        {
            return _proxy.ReadContentAsDateTime();
        }

        public override double ReadContentAsDouble()
        {
            return _proxy.ReadContentAsDouble();
        }

        public override float ReadContentAsFloat()
        {
            return _proxy.ReadContentAsFloat();
        }

        public override decimal ReadContentAsDecimal()
        {
            return _proxy.ReadContentAsDecimal();
        }

        public override int ReadContentAsInt()
        {
            return _proxy.ReadContentAsInt();
        }

        public override long ReadContentAsLong()
        {
            return _proxy.ReadContentAsLong();
        }

        public override string ReadContentAsString()
        {
            return _proxy.ReadContentAsString();
        }

        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            return _proxy.ReadContentAs(returnType, namespaceResolver);
        }

        public override object ReadElementContentAsObject()
        {
            return _proxy.ReadElementContentAsObject();
        }

        public override object ReadElementContentAsObject(string localName, string namespaceURI)
        {
            return _proxy.ReadElementContentAsObject(localName, namespaceURI);
        }

        public override bool ReadElementContentAsBoolean()
        {
            return _proxy.ReadElementContentAsBoolean();
        }

        public override bool ReadElementContentAsBoolean(string localName, string namespaceURI)
        {
            return _proxy.ReadElementContentAsBoolean(localName, namespaceURI);
        }

        public override DateTime ReadElementContentAsDateTime()
        {
            return _proxy.ReadElementContentAsDateTime();
        }

        public override DateTime ReadElementContentAsDateTime(string localName, string namespaceURI)
        {
            return _proxy.ReadElementContentAsDateTime(localName, namespaceURI);
        }

        public override double ReadElementContentAsDouble()
        {
            return _proxy.ReadElementContentAsDouble();
        }

        public override double ReadElementContentAsDouble(string localName, string namespaceURI)
        {
            return _proxy.ReadElementContentAsDouble(localName, namespaceURI);
        }

        public override float ReadElementContentAsFloat()
        {
            return _proxy.ReadElementContentAsFloat();
        }

        public override float ReadElementContentAsFloat(string localName, string namespaceURI)
        {
            return _proxy.ReadElementContentAsFloat(localName, namespaceURI);
        }

        public override decimal ReadElementContentAsDecimal()
        {
            return _proxy.ReadElementContentAsDecimal();
        }

        public override decimal ReadElementContentAsDecimal(string localName, string namespaceURI)
        {
            return _proxy.ReadElementContentAsDecimal(localName, namespaceURI);
        }

        public override int ReadElementContentAsInt()
        {
            return _proxy.ReadElementContentAsInt();
        }

        public override int ReadElementContentAsInt(string localName, string namespaceURI)
        {
            return _proxy.ReadElementContentAsInt(localName, namespaceURI);
        }

        public override long ReadElementContentAsLong()
        {
            return _proxy.ReadElementContentAsLong();
        }

        public override long ReadElementContentAsLong(string localName, string namespaceURI)
        {
            return _proxy.ReadElementContentAsLong(localName, namespaceURI);
        }

        public override string ReadElementContentAsString()
        {
            return _proxy.ReadElementContentAsString();
        }

        public override string ReadElementContentAsString(string localName, string namespaceURI)
        {
            return _proxy.ReadElementContentAsString(localName, namespaceURI);
        }

        public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            return _proxy.ReadElementContentAs(returnType, namespaceResolver);
        }

        public override object ReadElementContentAs(
            Type returnType, IXmlNamespaceResolver namespaceResolver, string localName, string namespaceURI)
        {
            return _proxy.ReadElementContentAs(returnType, namespaceResolver, localName, namespaceURI);
        }

        public override int ReadValueChunk(char[] buffer, int index, int count)
        {
            return _proxy.ReadValueChunk(buffer, index, count);
        }

        public override XmlNodeType MoveToContent()
        {
            return _proxy.MoveToContent();
        }

        public override void ReadStartElement()
        {
            _proxy.ReadStartElement();
        }

        public override void ReadStartElement(string name)
        {
            _proxy.ReadStartElement(name);
        }

        public override void ReadStartElement(string localname, string ns)
        {
            _proxy.ReadStartElement(localname, ns);
        }

        public override string ReadElementString()
        {
            return _proxy.ReadElementString();
        }

        public override string ReadElementString(string name)
        {
            return _proxy.ReadElementString(name);
        }

        public override string ReadElementString(string localname, string ns)
        {
            return _proxy.ReadElementString(localname, ns);
        }

        public override void ReadEndElement()
        {
            _proxy.ReadEndElement();
        }

        public override bool IsStartElement()
        {
            return _proxy.IsStartElement();
        }

        public override bool IsStartElement(string name)
        {
            return _proxy.IsStartElement(name);
        }

        public override bool IsStartElement(string localname, string ns)
        {
            return _proxy.IsStartElement(localname, ns);
        }

        public override bool ReadToFollowing(string name)
        {
            return _proxy.ReadToFollowing(name);
        }

        public override bool ReadToFollowing(string localName, string namespaceURI)
        {
            return _proxy.ReadToFollowing(localName, namespaceURI);
        }

        public override bool ReadToDescendant(string name)
        {
            return _proxy.ReadToDescendant(name);
        }

        public override bool ReadToDescendant(string localName, string namespaceURI)
        {
            return _proxy.ReadToDescendant(localName, namespaceURI);
        }

        public override bool ReadToNextSibling(string name)
        {
            return _proxy.ReadToNextSibling(name);
        }

        public override bool ReadToNextSibling(string localName, string namespaceURI)
        {
            return _proxy.ReadToNextSibling(localName, namespaceURI);
        }

        public override string ReadInnerXml()
        {
            return _proxy.ReadInnerXml();
        }

        public override string ReadOuterXml()
        {
            return _proxy.ReadOuterXml();
        }

        public override XmlReader ReadSubtree()
        {
            return new XmlReaderProxy(_proxy.ReadSubtree(), _baseUri, _lineNumberService);
        }

        public override string GetAttribute(string name)
        {
            return _proxy.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return _proxy.GetAttribute(name, namespaceURI);
        }

        public override string GetAttribute(int i)
        {
            return _proxy.GetAttribute(i);
        }

        public override bool MoveToAttribute(string name)
        {
            return _proxy.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return _proxy.MoveToAttribute(name, ns);
        }

        public override void MoveToAttribute(int i)
        {
            _proxy.MoveToAttribute(i);
        }

        public override bool MoveToFirstAttribute()
        {
            return _proxy.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return _proxy.MoveToNextAttribute();
        }

        public override bool MoveToElement()
        {
            return _proxy.MoveToElement();
        }

        public override bool ReadAttributeValue()
        {
            return _proxy.ReadAttributeValue();
        }

        public override bool Read()
        {
            return _proxy.Read();
        }

        public override void Close()
        {
            _proxy.Close();
        }

        public override void Skip()
        {
            _proxy.Skip();
        }

        public override string LookupNamespace(string prefix)
        {
            return _proxy.LookupNamespace(prefix);
        }

        public override void ResolveEntity()
        {
            _proxy.ResolveEntity();
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            return _proxy.ReadContentAsBase64(buffer, index, count);
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            return _proxy.ReadElementContentAsBase64(buffer, index, count);
        }

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            return _proxy.ReadContentAsBinHex(buffer, index, count);
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            return _proxy.ReadElementContentAsBinHex(buffer, index, count);
        }

        public override string ReadString()
        {
            return _proxy.ReadString();
        }
    }
}
