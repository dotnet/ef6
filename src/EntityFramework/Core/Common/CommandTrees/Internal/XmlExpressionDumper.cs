// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees.Internal
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;

    /// <summary>
    ///     An implementation of ExpressionDumper that produces an XML string.
    /// </summary>
    internal class XmlExpressionDumper : ExpressionDumper
    {
        internal static Encoding DefaultEncoding
        {
            get { return Encoding.UTF8; }
        }

        private readonly XmlWriter _writer;

        internal XmlExpressionDumper(Stream stream)
            : this(stream, DefaultEncoding)
        {
        }

        internal XmlExpressionDumper(Stream stream, Encoding encoding)
        {
            var settings = new XmlWriterSettings();
            settings.CheckCharacters = false;
            settings.Indent = true;
            settings.Encoding = encoding;
            _writer = XmlWriter.Create(stream, settings);
            _writer.WriteStartDocument(true);
        }

        internal void Close()
        {
            _writer.WriteEndDocument();
            _writer.Flush();
            _writer.Close();
        }

        internal override void Begin(string name, Dictionary<string, object> attrs)
        {
            _writer.WriteStartElement(name);
            if (attrs != null)
            {
                foreach (var attr in attrs)
                {
                    _writer.WriteAttributeString(attr.Key, (null == attr.Value ? "" : attr.Value.ToString()));
                }
            }
        }

        internal override void End(string name)
        {
            _writer.WriteEndElement();
        }
    }
}
