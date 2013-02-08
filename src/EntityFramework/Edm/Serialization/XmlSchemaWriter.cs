// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Text;
    using System.Xml;

    internal abstract class XmlSchemaWriter
    {
        protected XmlWriter _xmlWriter;
        protected double _version;

        internal virtual void WriteEndElement()
        {
            _xmlWriter.WriteEndElement();
        }

        protected static string GetQualifiedTypeName(string prefix, string typeName)
        {
            var sb = new StringBuilder();
            return sb.Append(prefix).Append(".").Append(typeName).ToString();
        }

        internal static string GetLowerCaseStringFromBoolValue(bool value)
        {
            return value ? XmlConstants.True : XmlConstants.False;
        }
    }
}
