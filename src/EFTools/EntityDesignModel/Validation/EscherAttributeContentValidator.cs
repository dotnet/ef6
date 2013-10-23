// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    /// <summary>
    ///     This class can be used to validate attribute content specific to EDMX documents *before* updating the XLinq tree, and without revalidating the entire document.
    /// </summary>
    internal class EscherAttributeContentValidator : AttributeContentValidator
    {
        private static readonly Regex QualifiedNameRegex =
            new Regex(
                @"^[\p{L}\p{Nl}][\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Pc}\p{Cf}]{0,}(\.[\p{L}\p{Nl}][\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Pc}\p{Cf}]{0,}){0,}$");

        private static readonly Regex SimpleIdentifierRegex =
            new Regex(@"^[\p{L}\p{Nl}][\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Pc}\p{Cf}]{0,}$");

        private static IDictionary<Version, EscherAttributeContentValidator> _instances;

        /// <summary>
        ///     Returns the static instance of the EscherAttributeContentValidator
        /// </summary>
        internal static EscherAttributeContentValidator GetInstance(Version schemaVersion)
        {
            if (_instances == null)
            {
                _instances = new Dictionary<Version, EscherAttributeContentValidator>(3);
            }

            if (!_instances.ContainsKey(schemaVersion))
            {
                _instances[schemaVersion] = new EscherAttributeContentValidator();
            }
            return _instances[schemaVersion];
        }

        internal XmlSchemaSet EdmxSchemaSet
        {
            get { return SchemaSet; }
        }

        /// <summary>
        ///     Access this class via the Instance property
        /// </summary>
        private EscherAttributeContentValidator()
            : base(BuildEdmxSchemaSet())
        {
        }

        /// <summary>
        ///     Returns true if the proposed string is valid for the given attribute's XSD schema type
        /// </summary>
        internal override bool IsValidAttributeValue(string proposedString, EFAttribute attribute)
        {
            if (attribute.XObject == null)
            {
                // attribute isn't hooked up into an xlinq tree, so we can't validate it.
                return true;
            }

            var nameStack = MakeAttributePathFromEFObject(attribute);
            if (nameStack == null)
            {
                return true;
            }
            else
            {
                return IsValidStringForSchemaType(proposedString, nameStack);
            }
        }

        /// <summary>
        ///     This is a convenience method that will create an AttributePath instance from a string of the form "xxx/yyy/zzz/attr".  This assumes that
        ///     "attr" is the name of an attribute with no namespace value, and that "xxx", "yyy" and "zzz" are element names with namespace nameSpaceUri.
        ///     This is a valid assumption for CSDL, SSDL & MSL XSD definitions, but is not necessarily true for all XSDs.
        /// </summary>
        internal static AttributePath MakeAttributePathFromString(string nameSpaceUri, string path)
        {
            var parts = path.Split('/');
            var attributePath = new AttributePath();
            for (var i = parts.Length - 1; i >= 0; --i)
            {
                // assume that the last one is always an attribute with empty namespace URI.  This is a valid assumption for our csdl/ssdl/msl cases
                if (i == parts.Length - 1)
                {
                    attributePath.PushFront(parts[i], String.Empty, false);
                }
                else
                {
                    attributePath.PushFront(parts[i], nameSpaceUri, true);
                }
            }
            return attributePath;
        }

        /// <summary>
        ///     This method will make an AttributePath for a given an EFObject.
        /// </summary>
        private static AttributePath MakeAttributePathFromEFObject(EFObject efobject)
        {
            var attributePath = new AttributePath();
            var curr = efobject.XObject;
            while (curr != null)
            {
                if (curr.NodeType == XmlNodeType.Document)
                {
                    var doc = (XDocument)curr;
                    attributePath.PushFront(doc.Root.Name.LocalName, doc.Root.Name.Namespace.NamespaceName, true);
                }
                else if (curr.NodeType == XmlNodeType.Attribute)
                {
                    var attr = (XAttribute)curr;
                    attributePath.PushFront(attr.Name.LocalName, attr.Name.Namespace.NamespaceName, false);
                }
                else if (curr.NodeType == XmlNodeType.Element)
                {
                    var el = (XElement)curr;
                    attributePath.PushFront(el.Name.LocalName, el.Name.Namespace.NamespaceName, true);
                }
                else
                {
                    Debug.Fail("Unexpected XObject type is neither an attribute nor an element");
                    return null;
                }

                curr = curr.Parent;
            }
            return attributePath;
        }

        /// <summary>
        ///     Builds the XmlSchemaSet to use for Escher document validation.  This retrieves Xml Schemas that are embedded as resources
        ///     in EntityFramework.dll & Microsoft.Data.Entity.Design.dll.  We use these schemas instead of those installed with VS because
        ///     we know that these schemas will not have been altered by users.
        /// </summary>
        private static XmlSchemaSet BuildEdmxSchemaSet()
        {
            var xmlSchemaSet = new XmlSchemaSet();
            var validationErrorCollector = new SchemaValidationErrorCollector();
            xmlSchemaSet.ValidationEventHandler += validationErrorCollector.ValidationCallBack;
            xmlSchemaSet.XmlResolver = new EdmRuntimeSchemaResolver();
            xmlSchemaSet.Add(
                SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version1),
                EdmxUtils.GetEDMXXsdResource(EntityFrameworkVersion.Version1));
            xmlSchemaSet.Add(
                SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version2),
                EdmxUtils.GetEDMXXsdResource(EntityFrameworkVersion.Version2));
            xmlSchemaSet.Add(
                SchemaManager.GetEDMXNamespaceName(EntityFrameworkVersion.Version3),
                EdmxUtils.GetEDMXXsdResource(EntityFrameworkVersion.Version3));
            xmlSchemaSet.Compile();
            return xmlSchemaSet;
        }

        /// <summary>
        ///     Returns true if the proposed name is valid for CSDL namespace name
        /// </summary>
        public static bool IsValidCsdlNamespaceName(string modelNamespace)
        {
            Debug.Assert(modelNamespace != null, "modelNamespace != null");

            // CSDL schema does not allow model namespace to be longer than 512 characters
            return modelNamespace.Length <= 512 && IsValidXmlAttributeValue(modelNamespace)
                   && QualifiedNameRegex.IsMatch(modelNamespace);
        }

        /// <summary>
        ///     Returns true if the proposed name is valid for CSDL entity container name
        /// </summary>
        public static bool IsValidCsdlEntityContainerName(string proposedName)
        {
            Debug.Assert(proposedName != null, "proposedName != null");

            return IsValidCsdlSimpleIdentifier(proposedName);
        }

        /// <summary>
        ///     Returns true if the proposed name is valid for CSDL entity set name
        /// </summary>
        internal static bool IsValidCsdlEntitySetName(string proposedName)
        {
            Debug.Assert(proposedName != null, "proposedName != null");

            return IsValidCsdlSimpleIdentifier(proposedName);
        }

        /// <summary>
        ///     Returns true if the proposed name is valid for CSDL entity type name
        /// </summary>
        public static bool IsValidCsdlEntityTypeName(string proposedName)
        {
            Debug.Assert(proposedName != null, "proposedName != null");

            return IsValidCsdlSimpleIdentifier(proposedName);
        }

        /// <summary>
        ///     Returns true if the proposed name is valid for CSDL complex type name
        /// </summary>
        public static bool IsValidCsdlComplexTypeName(string proposedName)
        {
            Debug.Assert(proposedName != null, "proposedName != null");

            return IsValidCsdlSimpleIdentifier(proposedName);
        }

        /// <summary>
        ///     Returns true if the proposed name is valid for CSDL enum type name
        /// </summary>
        public static bool IsValidCsdlEnumTypeName(string proposedName)
        {
            Debug.Assert(proposedName != null, "proposedName != null");

            return IsValidCsdlSimpleIdentifier(proposedName);
        }

        /// <summary>
        ///     Returns true if the proposed name is valid for CSDL enum member name
        /// </summary>
        public static bool IsValidCsdlEnumMemberName(string proposedName)
        {
            Debug.Assert(proposedName != null, "proposedName != null");

            return IsValidCsdlSimpleIdentifier(proposedName);
        }

        /// <summary>
        ///     Returns true if the proposed name is valid for CSDL property name
        /// </summary>
        public static bool IsValidCsdlPropertyName(string proposedName)
        {
            Debug.Assert(proposedName != null, "proposedName != null");

            return IsValidCsdlSimpleIdentifier(proposedName);
        }

        /// <summary>
        ///     Returns true if the proposed name is valid for CSDL navigation property name
        /// </summary>
        public static bool IsValidCsdlNavigationPropertyName(string proposedName)
        {
            Debug.Assert(proposedName != null, "proposedName != null");

            return IsValidCsdlSimpleIdentifier(proposedName);
        }

        /// <summary>
        ///     Returns true if the proposed name is valid for CSDL association name
        /// </summary>
        public static bool IsValidCsdlAssociationName(string proposedName)
        {
            Debug.Assert(proposedName != null, "proposedName != null");

            return IsValidCsdlSimpleIdentifier(proposedName);
        }

        /// <summary>
        ///     Returns true if the proposed name is valid for CSDL function import name
        /// </summary>
        public static bool IsValidCsdlFunctionImportName(string proposedName)
        {
            Debug.Assert(proposedName != null, "proposedName != null");

            return IsValidCsdlSimpleIdentifier(proposedName);
        }

        public static bool IsValidXmlAttributeValue(string s)
        {
            Debug.Assert(s != null, "s != null");

            try
            {
                XmlConvert.VerifyXmlChars(s);
            }
            catch (XmlException)
            {
                return false;
            }
            return true;
        }

        private static bool IsValidCsdlSimpleIdentifier(string identifier)
        {
            // CSDL schema does not allow simple identifiers to be longer than 480 characters
            return identifier.Length <= 480 && IsValidXmlAttributeValue(identifier)
                   && SimpleIdentifierRegex.IsMatch(identifier);
        }
    }
}
