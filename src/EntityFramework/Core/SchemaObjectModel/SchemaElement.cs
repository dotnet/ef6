// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    // <summary>
    // Summary description for SchemaElement.
    // </summary>
    [DebuggerDisplay("Name={Name}")]
    internal abstract class SchemaElement
    {
        // see http://www.w3.org/TR/2006/REC-xml-names-20060816/
        internal const string XmlNamespaceNamespace = "http://www.w3.org/2000/xmlns/";

        #region Instance Fields

        private Schema _schema;
        private int _lineNumber;
        private int _linePosition;
        private string _name;

        private List<MetadataProperty> _otherContent;

        private readonly IDbDependencyResolver _resolver;

        #endregion

        #region Static Fields

        protected const int MaxValueVersionComponent = short.MaxValue;

        #endregion

        #region Public Properties

        internal int LineNumber
        {
            get { return _lineNumber; }
        }

        internal int LinePosition
        {
            get { return _linePosition; }
        }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        internal DocumentationElement Documentation { get; set; }

        internal SchemaElement ParentElement { get; private set; }

        internal Schema Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }

        public virtual string FQName
        {
            get { return Name; }
        }

        public virtual string Identity
        {
            get { return Name; }
        }

        public List<MetadataProperty> OtherContent
        {
            get
            {
                if (_otherContent == null)
                {
                    _otherContent = new List<MetadataProperty>();
                }

                return _otherContent;
            }
        }

        #endregion

        #region Internal Methods

        // <summary>
        // Validates this element and its children
        // </summary>
        internal virtual void Validate()
        {
        }

        internal void AddError(ErrorCode errorCode, EdmSchemaErrorSeverity severity, int lineNumber, int linePosition, object message)
        {
            AddError(errorCode, severity, SchemaLocation, lineNumber, linePosition, message);
        }

        internal void AddError(ErrorCode errorCode, EdmSchemaErrorSeverity severity, XmlReader reader, object message)
        {
            int lineNumber;
            int linePosition;
            GetPositionInfo(reader, out lineNumber, out linePosition);
            AddError(errorCode, severity, SchemaLocation, lineNumber, linePosition, message);
        }

        internal void AddError(ErrorCode errorCode, EdmSchemaErrorSeverity severity, object message)
        {
            AddError(errorCode, severity, SchemaLocation, LineNumber, LinePosition, message);
        }

        internal void AddError(ErrorCode errorCode, EdmSchemaErrorSeverity severity, SchemaElement element, object message)
        {
            AddError(errorCode, severity, element.Schema.Location, element.LineNumber, element.LinePosition, message);
        }

        internal void Parse(XmlReader reader)
        {
            GetPositionInfo(reader);

            var hasEndElement = !reader.IsEmptyElement;

            Debug.Assert(reader.NodeType == XmlNodeType.Element);
            for (var more = reader.MoveToFirstAttribute(); more; more = reader.MoveToNextAttribute())
            {
                ParseAttribute(reader);
            }
            HandleAttributesComplete();

            var done = !hasEndElement;
            var skipToNextElement = false;
            while (!done)
            {
                if (skipToNextElement)
                {
                    skipToNextElement = false;
                    reader.Skip();
                    if (reader.EOF)
                    {
                        break;
                    }
                }
                else
                {
                    if (!reader.Read())
                    {
                        break;
                    }
                }
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        skipToNextElement = ParseElement(reader);
                        break;

                    case XmlNodeType.EndElement:
                        {
                            done = true;
                            break;
                        }

                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.SignificantWhitespace:
                        ParseText(reader);
                        break;

                    // we ignore these childless elements
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.Comment:
                    case XmlNodeType.Notation:
                    case XmlNodeType.ProcessingInstruction:
                        {
                            break;
                        }

                    // we ignore these elements that can have children
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.EntityReference:
                        {
                            skipToNextElement = true;
                            break;
                        }

                    default:
                        {
                            AddError(
                                ErrorCode.UnexpectedXmlNodeType, EdmSchemaErrorSeverity.Error, reader,
                                Strings.UnexpectedXmlNodeType(reader.NodeType));
                            skipToNextElement = true;
                            break;
                        }
                }
            }
            HandleChildElementsComplete();
            if (reader.EOF
                && reader.Depth > 0)
            {
                AddError(
                    ErrorCode.MalformedXml, EdmSchemaErrorSeverity.Error, 0, 0,
                    Strings.MalformedXml(LineNumber, LinePosition));
            }
        }

        // <summary>
        // Set the current line number and position for an XmlReader
        // </summary>
        // <param name="reader"> the reader whose position is desired </param>
        internal void GetPositionInfo(XmlReader reader)
        {
            GetPositionInfo(reader, out _lineNumber, out _linePosition);
        }

        // <summary>
        // Get the current line number and position for an XmlReader
        // </summary>
        // <param name="reader"> the reader whose position is desired </param>
        // <param name="lineNumber"> the line number </param>
        // <param name="linePosition"> the line position </param>
        internal static void GetPositionInfo(XmlReader reader, out int lineNumber, out int linePosition)
        {
            var xmlLineInfo = reader as IXmlLineInfo;
            if (xmlLineInfo != null
                && xmlLineInfo.HasLineInfo())
            {
                lineNumber = xmlLineInfo.LineNumber;
                linePosition = xmlLineInfo.LinePosition;
            }
            else
            {
                lineNumber = 0;
                linePosition = 0;
            }
        }

        internal virtual void ResolveTopLevelNames()
        {
        }

        internal virtual void ResolveSecondLevelNames()
        {
        }

        #endregion

        #region Protected Methods

        internal SchemaElement(SchemaElement parentElement, IDbDependencyResolver resolver = null)
        {
            _resolver = resolver ?? DbConfiguration.DependencyResolver;

            if (parentElement != null)
            {
                ParentElement = parentElement;
                for (var element = parentElement; element != null; element = element.ParentElement)
                {
                    var schema = element as Schema;
                    if (schema != null)
                    {
                        Schema = schema;
                        break;
                    }
                }

                if (Schema == null)
                {
                    throw new InvalidOperationException(Strings.AllElementsMustBeInSchema);
                }
            }
        }

        internal SchemaElement(SchemaElement parentElement, string name, IDbDependencyResolver resolver = null)
            : this(parentElement, resolver)
        {
            _name = name;
        }

        protected virtual void HandleAttributesComplete()
        {
        }

        protected virtual void HandleChildElementsComplete()
        {
        }

        protected string HandleUndottedNameAttribute(XmlReader reader, string field)
        {
            var name = field;
            Debug.Assert(string.IsNullOrEmpty(field), string.Format(CultureInfo.CurrentCulture, "{0} is already defined", reader.Name));

            var success = Utils.GetUndottedName(Schema, reader, out name);
            if (!success)
            {
                return name;
            }

            return name;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "field")]
        protected ReturnValue<string> HandleDottedNameAttribute(XmlReader reader, string field)
        {
            var returnValue = new ReturnValue<string>();
            Debug.Assert(string.IsNullOrEmpty(field), string.Format(CultureInfo.CurrentCulture, "{0} is already defined", reader.Name));

            string value;
            if (!Utils.GetDottedName(Schema, reader, out value))
            {
                return returnValue;
            }

            returnValue.Value = value;
            return returnValue;
        }

        // <summary>
        // Use to handle an attribute with an int data type
        // </summary>
        // <param name="reader"> the reader positioned at the int attribute </param>
        // <param name="field"> The int field to be given the value found </param>
        // <returns> true if an int value was successfuly extracted from the attribute, false otherwise. </returns>
        internal bool HandleIntAttribute(XmlReader reader, ref int field)
        {
            int value;
            if (!Utils.GetInt(Schema, reader, out value))
            {
                return false;
            }

            field = value;
            return true;
        }

        // <summary>
        // Use to handle an attribute with an int data type
        // </summary>
        // <param name="reader"> the reader positioned at the int attribute </param>
        // <param name="field"> The int field to be given the value found </param>
        // <returns> true if an int value was successfuly extracted from the attribute, false otherwise. </returns>
        internal bool HandleByteAttribute(XmlReader reader, ref byte field)
        {
            byte value;
            if (!Utils.GetByte(Schema, reader, out value))
            {
                return false;
            }

            field = value;
            return true;
        }

        internal bool HandleBoolAttribute(XmlReader reader, ref bool field)
        {
            bool value;
            if (!Utils.GetBool(Schema, reader, out value))
            {
                return false;
            }

            field = value;
            return true;
        }

        // <summary>
        // Use this to jump through an element that doesn't need any processing
        // </summary>
        // <param name="reader"> xml reader currently positioned at an element </param>
        protected virtual void SkipThroughElement(XmlReader reader)
        {
            DebugCheck.NotNull(reader);
            Parse(reader);
        }

        protected virtual void SkipElement(XmlReader reader)
        {
            using (var subtree = reader.ReadSubtree())
            {
                while (subtree.Read())
                {
                    ;
                }
            }
        }

        #endregion

        #region Protected Properties

        protected string SchemaLocation
        {
            get
            {
                if (Schema != null)
                {
                    return Schema.Location;
                }
                return null;
            }
        }

        protected virtual bool HandleText(XmlReader reader)
        {
            return false;
        }

        internal virtual SchemaElement Clone(SchemaElement parentElement)
        {
            throw Error.NotImplemented();
        }

        #endregion

        #region Private Methods

        private void HandleDocumentationElement(XmlReader reader)
        {
            Documentation = new DocumentationElement(this);
            Documentation.Parse(reader);
        }

        protected virtual void HandleNameAttribute(XmlReader reader)
        {
            Name = HandleUndottedNameAttribute(reader, Name);
        }

        private void AddError(
            ErrorCode errorCode, EdmSchemaErrorSeverity severity, string sourceLocation, int lineNumber, int linePosition, object message)
        {
            EdmSchemaError error = null;
            var messageString = message as string;
            if (messageString != null)
            {
                error = new EdmSchemaError(messageString, (int)errorCode, severity, sourceLocation, lineNumber, linePosition);
            }
            else
            {
                var ex = message as Exception;
                if (ex != null)
                {
                    error = new EdmSchemaError(ex.Message, (int)errorCode, severity, sourceLocation, lineNumber, linePosition, ex);
                }
                else
                {
                    error = new EdmSchemaError(message.ToString(), (int)errorCode, severity, sourceLocation, lineNumber, linePosition);
                }
            }
            Schema.AddError(error);
        }

        // <summary>
        // Call handler for the current attribute
        // </summary>
        // <param name="reader"> XmlReader positioned at the attribute </param>
        private void ParseAttribute(XmlReader reader)
        {
#if false
    // the attribute value is schema invalid, just skip it; this avoids some duplicate errors at the expense of better error messages...
            if ( reader.SchemaInfo != null && reader.SchemaInfo.Validity == System.Xml.Schema.XmlSchemaValidity.Invalid )
                continue;
#endif
            var attributeNamespace = reader.NamespaceURI;
            if (attributeNamespace == XmlConstants.AnnotationNamespace
                && reader.LocalName == XmlConstants.UseStrongSpatialTypes
                && !ProhibitAttribute(attributeNamespace, reader.LocalName)
                && HandleAttribute(reader))
            {
                return;
            }
            else if (!Schema.IsParseableXmlNamespace(attributeNamespace, true))
            {
                AddOtherContent(reader);
            }
            else if (!ProhibitAttribute(attributeNamespace, reader.LocalName)
                     &&
                     HandleAttribute(reader))
            {
                return;
            }
            else if (reader.SchemaInfo == null
                     || reader.SchemaInfo.Validity != XmlSchemaValidity.Invalid)
            {
                // there's no handler for (namespace,name) and there wasn't a validation error. 
                // Report an error of our own if the node is in no namespace or if it is in one of our xml schemas tartget namespace.
                if (string.IsNullOrEmpty(attributeNamespace)
                    || Schema.IsParseableXmlNamespace(attributeNamespace, true))
                {
                    AddError(
                        ErrorCode.UnexpectedXmlAttribute, EdmSchemaErrorSeverity.Error, reader, Strings.UnexpectedXmlAttribute(reader.Name));
                }
            }
        }

        protected virtual bool ProhibitAttribute(string namespaceUri, string localName)
        {
            return false;
        }

        // <summary>
        // This overload assumes the default namespace
        // </summary>
        internal static bool CanHandleAttribute(XmlReader reader, string localName)
        {
            Debug.Assert(reader.NamespaceURI != null);
            return reader.NamespaceURI.Length == 0 && reader.LocalName == localName;
        }

        protected virtual bool HandleAttribute(XmlReader reader)
        {
            if (CanHandleAttribute(reader, XmlConstants.Name))
            {
                HandleNameAttribute(reader);
                return true;
            }

            return false;
        }

        private bool AddOtherContent(XmlReader reader)
        {
            int lineNumber;
            int linePosition;
            GetPositionInfo(reader, out lineNumber, out linePosition);

            MetadataProperty property;
            if (reader.NodeType
                == XmlNodeType.Element)
            {
                if (_schema.SchemaVersion == XmlConstants.EdmVersionForV1
                    ||
                    _schema.SchemaVersion == XmlConstants.EdmVersionForV1_1)
                {
                    // skip this element
                    // we don't support element annotations in v1 and v1.1
                    return true;
                }

                // in V1 and V1.1 the codegen can only appear as the attribute annotation and we want to maintain
                // the same behavior for V2, thus we throw if we encounter CodeGen namespace 
                // in structural annotation in V2 and furthur version
                if (_schema.SchemaVersion >= XmlConstants.EdmVersionForV2
                    && reader.NamespaceURI == XmlConstants.CodeGenerationSchemaNamespace)
                {
                    Debug.Assert(
                        XmlConstants.SchemaVersionLatest == XmlConstants.EdmVersionForV3,
                        "Please add checking for the latest namespace");

                    AddError(
                        ErrorCode.NoCodeGenNamespaceInStructuralAnnotation, EdmSchemaErrorSeverity.Error, lineNumber, linePosition,
                        Strings.NoCodeGenNamespaceInStructuralAnnotation(XmlConstants.CodeGenerationSchemaNamespace));
                    return true;
                }

                Debug.Assert(
                    !Schema.IsParseableXmlNamespace(reader.NamespaceURI, false),
                    "Structural annotation cannot use any edm reserved namespaces");

                // using this subtree aproach because when I call 
                // reader.ReadOuterXml() it positions me at the Node beyond
                // the end of the node I am starting on
                // which doesn't work with the parsing logic
                using (var subtree = reader.ReadSubtree())
                {
                    subtree.Read();
                    using (var stringReader = new StringReader(subtree.ReadOuterXml()))
                    {
                        var element = XElement.Load(stringReader);

                        property = CreateMetadataPropertyFromXmlElement(
                            element.Name.NamespaceName, element.Name.LocalName, element);
                    }
                }
            }
            else
            {
                if (reader.NamespaceURI == XmlNamespaceNamespace)
                {
                    // we don't bring in namespace definitions
                    return true;
                }

                Debug.Assert(reader.NodeType == XmlNodeType.Attribute, "called an attribute function when not on an attribute");
                property = CreateMetadataPropertyFromXmlAttribute(reader.NamespaceURI, reader.LocalName, reader.Value);
            }

            if (!OtherContent.Exists(mp => mp.Identity == property.Identity))
            {
                OtherContent.Add(property);
            }
            else
            {
                AddError(
                    ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, lineNumber, linePosition,
                    Strings.DuplicateAnnotation(property.Identity, FQName));
            }
            return false;
        }

        internal static MetadataProperty CreateMetadataPropertyFromXmlElement(
            string xmlNamespaceUri, string elementName, XElement value)
        {
            return MetadataProperty.CreateAnnotation(xmlNamespaceUri + ":" + elementName, value);
        }

        internal MetadataProperty CreateMetadataPropertyFromXmlAttribute(
            string xmlNamespaceUri, string attributeName, string value)
        {
            var serializer = _resolver.GetService<Func<IMetadataAnnotationSerializer>>(attributeName);
            var parsedValue = serializer == null ? value : serializer().Deserialize(attributeName, value);

            return MetadataProperty.CreateAnnotation(xmlNamespaceUri + ":" + attributeName, parsedValue);
        }

        // <summary>
        // Call handler for the current element
        // </summary>
        // <param name="reader"> XmlReader positioned at the element </param>
        // <returns> true if element content should be skipped </returns>
        private bool ParseElement(XmlReader reader)
        {
            var elementNamespace = reader.NamespaceURI;
            // for schema element that right under the schema, we just ignore them, since schema does not
            // have metadataproperties
            if (!Schema.IsParseableXmlNamespace(elementNamespace, true)
                && ParentElement != null)
            {
                return AddOtherContent(reader);
            }
            if (HandleElement(reader))
            {
                return false;
            }
            else
            {
                // we need to report an error if the namespace for this element is a target namespace for the xml schemas we are parsing against.
                // otherwise we assume that this is either a valid 'any' element or that the xsd validator has generated an error
                if (string.IsNullOrEmpty(elementNamespace)
                    || Schema.IsParseableXmlNamespace(reader.NamespaceURI, false))
                {
                    AddError(
                        ErrorCode.UnexpectedXmlElement, EdmSchemaErrorSeverity.Error, reader, Strings.UnexpectedXmlElement(reader.Name));
                }
                return true;
            }
        }

        protected bool CanHandleElement(XmlReader reader, string localName)
        {
            return reader.NamespaceURI == Schema.SchemaXmlNamespace && reader.LocalName == localName;
        }

        protected virtual bool HandleElement(XmlReader reader)
        {
            if (CanHandleElement(reader, XmlConstants.Documentation))
            {
                HandleDocumentationElement(reader);
                return true;
            }

            return false;
        }

        // <summary>
        // Handle text data.
        // </summary>
        // <param name="reader"> XmlReader positioned at Text, CData, or SignificantWhitespace </param>
        private void ParseText(XmlReader reader)
        {
            if (HandleText(reader))
            {
                return;
            }
            else if (reader.Value != null
                     && reader.Value.Trim().Length == 0)
            {
                // just ignore this text.  Don't add an error, since the value is just whitespace.
            }
            else
            {
                AddError(ErrorCode.TextNotAllowed, EdmSchemaErrorSeverity.Error, reader, Strings.TextNotAllowed(reader.Value));
            }
        }

        #endregion

        [Conditional("DEBUG")]
        internal static void AssertReaderConsidersSchemaInvalid(XmlReader reader)
        {
            Debug.Assert(
                reader.SchemaInfo == null ||
                reader.SchemaInfo.Validity != XmlSchemaValidity.Valid, "The xsd should see this as not acceptable");
        }
    }
}
