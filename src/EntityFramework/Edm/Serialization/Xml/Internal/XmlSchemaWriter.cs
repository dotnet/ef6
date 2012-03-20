namespace System.Data.Entity.Edm.Serialization.Xml.Internal
{
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;
    using System.Text;
    using System.Xml;

    internal abstract class XmlSchemaWriter
    {
        protected XmlWriter _xmlWriter;
        protected double _version;

        internal void WriteEndElement()
        {
            _xmlWriter.WriteEndElement();
        }

        protected string GetQualifiedTypeName(string prefix, string typeName)
        {
            var sb = new StringBuilder();
            return sb.Append(prefix).Append(".").Append(typeName).ToString();
        }

        internal static string GetTypeNameFromPrimitiveTypeKind(EdmPrimitiveTypeKind primitiveTypeKind)
        {
            return primitiveTypeKind.ToString();
        }

        internal static string GetLowerCaseStringFromBoolValue(bool value)
        {
            return value ? CsdlConstants.Value_True : CsdlConstants.Value_False;
        }
    }
}