// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.XmlDesignerBase.Model.StandAlone
{
    using System;
    using System.Xml;
    using System.Xml.Linq;

    internal class AnnotatedTreeBuilder
    {
        internal const int EmptyElementOffset = 3;
        internal const int ElementStartTagOffset = 2;
        internal const int ProcessingInstructionOffset = 3;
        internal const int CommentOffset = 5;
        internal const int CDataOffset = 10;
        internal const int TextOffset = 1;

        // parent will hold the current parent when looping through the
        // xmlreader
        private XElement _parent;

        // root will hold the root element that will be returned from this function
        private XElement _root;

        // prevNode will hold the node in the XLinq tree that was last
        // processesed.	 This is used for calculating end of open and close
        // tags.
        private XNode _prevNode;

        // lastReadElementEmpty will indicate if the last read element was
        // empty.  This will be used to help annotate the close tag info for
        // that element.
        private bool _lastReadElementEmpty;

        private TextRange _lastTextRange;

        internal XDocument Build(Uri uri)
        {
            using (var xmlReader = XmlReader.Create(uri.AbsoluteUri))
            {
                return Build(xmlReader);
            }
        }

        internal XDocument Build(XmlReader xmlReader)
        {
            XDocument doc = null;
            _parent = null;
            _root = null;
            _prevNode = null;
            _lastReadElementEmpty = false;
            _lastTextRange = null;

            // Enable getting line and column info
            var lineInfo = xmlReader as IXmlLineInfo;
            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        ProcessElement(xmlReader, lineInfo);
                        break;

                    case XmlNodeType.EndElement:
                        ProcessEndElement(lineInfo);
                        break;

                    case XmlNodeType.Text:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.Whitespace:
                        UpdateOpenTagEndInfo(
                            lineInfo.LineNumber,
                            lineInfo.LinePosition - TextOffset);
                        _prevNode = new XText(xmlReader.Value);
                        UpdateTextRange(_prevNode, lineInfo);
                        if (_parent != null)
                        {
                            _parent.Add(_prevNode);
                        }
                        break;

                    case XmlNodeType.CDATA:
                        UpdateOpenTagEndInfo(
                            lineInfo.LineNumber,
                            lineInfo.LinePosition - CDataOffset);
                        _prevNode = new XCData(xmlReader.Value);
                        UpdateTextRange(_prevNode, lineInfo);
                        if (_parent != null)
                        {
                            _parent.Add(_prevNode);
                        }
                        break;

                    case XmlNodeType.Comment:
                        UpdateOpenTagEndInfo(
                            lineInfo.LineNumber,
                            lineInfo.LinePosition - CommentOffset);
                        _prevNode = new XComment(xmlReader.Value);
                        UpdateTextRange(_prevNode, lineInfo);
                        if (_parent != null)
                        {
                            _parent.Add(_prevNode);
                        }
                        break;

                    case XmlNodeType.ProcessingInstruction:
                        UpdateOpenTagEndInfo(
                            lineInfo.LineNumber,
                            lineInfo.LinePosition - ProcessingInstructionOffset);
                        _prevNode = new XProcessingInstruction(
                            xmlReader.Name, xmlReader.Value);
                        UpdateTextRange(_prevNode, lineInfo);
                        if (_parent != null)
                        {
                            _parent.Add(_prevNode);
                        }
                        break;
                    case XmlNodeType.EntityReference:
                        xmlReader.ResolveEntity();
                        break;
                    case XmlNodeType.DocumentType:
                        break;
                    case XmlNodeType.XmlDeclaration:
                        break;
                    case XmlNodeType.EndEntity:
                        break;
                    default:
                        throw new InvalidOperationException(Resources.TreeBuilder_UnknownNodeType);
                }
            }
            doc = new XDocument(_root);
            doc.Root.EnsureAnnotation();
            return doc;
        }

        private void ProcessElement(XmlReader xmlReader, IXmlLineInfo lineInfo)
        {
            UpdateOpenTagEndInfo(
                lineInfo.LineNumber,
                lineInfo.LinePosition - ElementStartTagOffset);

            var localName = xmlReader.LocalName;
            var nsUri = xmlReader.NamespaceURI;
            var element = new XElement(XName.Get(localName, nsUri));

            _lastReadElementEmpty = xmlReader.IsEmptyElement;

            CreateInitialTextRangeAnnotation(
                element, lineInfo.LineNumber, lineInfo.LinePosition - 1);
            ProcessAttributes(xmlReader, element, lineInfo);

            _prevNode = element;
            if (_parent == null)
            {
                _root = element;
            }
            else
            {
                _parent.Add(element);
            }
            if (!_lastReadElementEmpty)
            {
                _parent = element;
            }
        }

        private void UpdateTextRange(XObject xobject, IXmlLineInfo lineInfo)
        {
            if (_lastTextRange != null)
            {
                _lastTextRange.CloseEndLine = lineInfo.LineNumber;
                _lastTextRange.CloseEndColumn = lineInfo.LinePosition - 1;
                _lastTextRange = null;
            }

            _lastTextRange = new TextRange();
            _lastTextRange.OpenStartLine = lineInfo.LineNumber;
            _lastTextRange.OpenStartColumn = lineInfo.LinePosition;
            xobject.SetTextRange(_lastTextRange);
        }

        private void ProcessAttributes(XmlReader xmlReader, XElement element, IXmlLineInfo lineInfo)
        {
            if (xmlReader.HasAttributes)
            {
                while (xmlReader.MoveToNextAttribute())
                {
                    if (_lastTextRange != null)
                    {
                        _lastTextRange.CloseEndLine = lineInfo.LineNumber;
                        _lastTextRange.CloseEndColumn = lineInfo.LinePosition - 1;
                    }
                    _lastTextRange = new TextRange();
                    _lastTextRange.OpenStartLine = lineInfo.LineNumber;
                    _lastTextRange.OpenStartColumn = lineInfo.LinePosition;

                    XName attributeName = null;
                    if (xmlReader.NamespaceURI == XNamespace.Xmlns.NamespaceName)
                    {
                        if (string.IsNullOrEmpty(xmlReader.Prefix))
                        {
                            attributeName = "xmlns";
                        }
                        else
                        {
                            attributeName = XNamespace.Xmlns + xmlReader.LocalName;
                        }
                    }
                    else
                    {
                        attributeName = XNamespace.Get(xmlReader.NamespaceURI) + xmlReader.LocalName;
                    }
                    var attribute = new XAttribute(attributeName, xmlReader.Value);
                    attribute.SetTextRange(_lastTextRange);
                    element.Add(attribute);
                }
            }
        }

        private void ProcessEndElement(IXmlLineInfo lineInfo)
        {
            UpdateOpenTagEndInfo(
                lineInfo.LineNumber,
                lineInfo.LinePosition - 3);

            UpdateCloseTagEndInfo(_parent, lineInfo.LineNumber, lineInfo.LinePosition);
            _prevNode = null;

            // Bump up one level in the tree
            if (_parent != null)
            {
                _parent = _parent.Parent;
            }
        }

        private static void CreateInitialTextRangeAnnotation(
            XElement element, int openStartLine, int openStartColumn)
        {
            var elemTextRange = new ElementTextRange();
            elemTextRange.OpenStartLine = openStartLine;
            elemTextRange.OpenStartColumn = openStartColumn;
            element.SetTextRange(elemTextRange);
        }

        private void UpdateOpenTagEndInfo(int openEndLine, int openEndCol)
        {
            if ((_prevNode == null)
                || (_prevNode.NodeType != XmlNodeType.Element))
            {
                return;
            }

            var element = _prevNode as XElement;
            if (_lastReadElementEmpty)
            {
                // Go ahead and update close info for empty element
                UpdateCloseTagEndInfo(element, openEndLine, openEndCol);
            }
            var elemTextRange = element.GetTextRange();
            elemTextRange.OpenEndLine = openEndLine;
            elemTextRange.OpenEndColumn = openEndCol;

            if (_lastTextRange != null)
            {
                _lastTextRange.CloseEndLine = openEndLine;
                _lastTextRange.CloseEndColumn =
                    (_lastReadElementEmpty ? openEndCol - 2 : openEndCol - 1);
                _lastTextRange = null;
            }
        }

        private static void UpdateCloseTagEndInfo(XNode node, int closeEndLine, int closeEndCol)
        {
            if ((node == null)
                || (node.NodeType != XmlNodeType.Element))
            {
                return;
            }

            var element = node as XElement;
            var elemTextRange = element.GetTextRange();
            elemTextRange.CloseEndLine = closeEndLine;
            elemTextRange.CloseEndColumn = closeEndCol;
        }
    }
}
