// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    ///     This class can be used to validate attribute content specific *before* updating an xml document, and without revalidating the entire document.
    ///     There is a *major* assumption in this class, namely that an XmlSchemaType for an attribute can be determined deterministically from a series of element names,
    ///     followed by an optional attribute name.  If there is any ambiguity, this  class will not work.  This assumption is valid for Escher Schemas.
    /// </summary>
    internal abstract class AttributeContentValidator
    {
        private readonly XmlSchemaSet _schemaSet;

        private const string _tempInstanceDocForValidation =
            "<data xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:ns=\"{0}\" xsi:type=\"ns:{1}\">{2}</data>";

        protected AttributeContentValidator(XmlSchemaSet schemaSet)
        {
            _schemaSet = schemaSet;
        }

        /// <summary>
        ///     Returns true if the proposed string is valid for the given attribute's XSD schema type
        /// </summary>
        /// <param name="proposedString"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        internal abstract bool IsValidAttributeValue(string proposedString, EFAttribute attribute);

        /// <summary>
        ///     Validates a string against a type specified by the given qname
        /// </summary>
        /// <param name="proposedString"></param>
        /// <param name="qname"></param>
        /// <returns></returns>
        internal bool IsValidStringForSchemaType(string proposedString, XmlQualifiedName qname)
        {
            return IsValidStringForSchema(proposedString, qname.Name, qname.Namespace);
        }

        /// <summary>
        ///     Validates a proposed string against a given "path" to a node.
        /// </summary>
        /// <param name="proposedString"></param>
        /// <param name="attributePath"></param>
        /// <returns></returns>
        internal bool IsValidStringForSchemaType(string proposedString, AttributePath attributePath)
        {
            var type = GetTypeNameForNode(attributePath);
            if (type == null)
            {
                Debug.Fail("didn't find XmlSchemaType for given attribute path");
                return false;
            }
            else
            {
                return IsValidStringForSchemaType(proposedString, type.QualifiedName);
            }
        }

        protected XmlSchemaSet SchemaSet
        {
            get { return _schemaSet; }
        }

        /// <summary>
        ///     Determines if a proposed string is valid for the given type in the given namespace.
        /// </summary>
        /// <param name="proposedString"></param>
        /// <param name="xsdTypeName"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        private bool IsValidStringForSchema(string proposedString, string xsdTypeName, string nameSpace)
        {
            // Escape the proposed string since if the string contains xml special characters (for example: '<', '>'), the method will always return false regardless the XSD type.
            var escapedProposedString = SecurityElement.Escape(proposedString);
            var s = String.Format(
                CultureInfo.InvariantCulture, _tempInstanceDocForValidation, nameSpace, xsdTypeName, escapedProposedString);
            return IsValidXmlDocument(s);
        }

        // returns true if the given string is valid for the SchemaSet associated with this instance
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected bool IsValidXmlDocument(string documentText)
        {
            var failed = false;
            var svec = new SchemaValidationErrorCollector();
            try
            {
                var settings = new XmlReaderSettings { Schemas = SchemaSet, ValidationType = ValidationType.Schema };
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += svec.ValidationCallBack;

                using (var reader = XmlReader.Create(new StringReader(documentText), settings))
                {
                    while (reader.Read())
                    {
                    }
                }
            }
            catch (Exception)
            {
                failed = true;
            }

            return svec.ErrorCount <= 0 && failed == false;
        }

        /// <summary>
        ///     Returns the XmlSchemaType for the given node.
        /// </summary>
        /// <param name="attributePath"></param>
        /// <returns></returns>
        protected XmlSchemaType GetTypeNameForNode(AttributePath attributePath)
        {
            if (attributePath == null)
            {
                throw new ArgumentNullException("attributePath");
            }

            var attributePathCopy = attributePath.Clone();

            if (attributePathCopy.Count < 1)
            {
                return null;
            }

            // prime the pump.  The first node on the stack should be a global element.
            var curr = attributePathCopy.PopFront();
            var schemaElement = GetXmlSchemaObject(SchemaSet.GlobalElements, curr.qname) as XmlSchemaElement;

            if (schemaElement == null)
            {
                Debug.Fail("Unable to find Global Schema Element " + curr.qname);
                return null;
            }

            var schemaType = GetXmlSchemaTypeFromXmlSchemaElement(schemaElement);

            if (schemaType == null)
            {
                Debug.Fail("Unable to find schema type for schema element " + curr.qname);
                return null;
            }

            while (attributePathCopy.Count > 0)
            {
                curr = attributePathCopy.PopFront();

                var xsct = schemaType as XmlSchemaComplexType;
                if (xsct != null)
                {
                    if (curr.type == AttributePath.QNameNodeTypePair.NodeType.Element)
                    {
                        XmlSchemaElement xe = null;
                        while (xe == null
                               && xsct != null)
                        {
                            xe = GetXmlSchemaElementFromParticle(xsct.Particle, curr.qname);

                            if (xe == null)
                            {
                                // check the content type particle
                                xe = GetXmlSchemaElementFromParticle(xsct.ContentTypeParticle, curr.qname);
                            }

                            if (xe == null)
                            {
                                // check the base type
                                xsct = xsct.BaseXmlSchemaType as XmlSchemaComplexType;
                            }
                        }

                        if (xe == null)
                        {
                            Debug.Fail("didn't find XmlSchemaElement for qname " + curr.qname + " in type " + schemaType.QualifiedName);
                            return null;
                        }
                        else
                        {
                            schemaType = GetXmlSchemaTypeFromXmlSchemaElement(xe);
                        }
                    }
                    else
                    {
                        // we need to find an attribute
                        // see if it is a global attribute first
                        var xa = GetXmlSchemaObject(SchemaSet.GlobalAttributes, curr.qname) as XmlSchemaAttribute;

                        // if that doesn't work try to see whether it's an attribute on the XmlSchemaComplexType xsct
                        if (xa == null)
                        {
                            xa = GetXmlSchemaAttributeFromXmlSchemaGroupBase(xsct, curr.qname);
                        }

                        if (xa == null)
                        {
                            Debug.Fail(
                                "didn't find XmlSchemaAttribute for qname " + curr.qname + " in Global Attributes or in type "
                                + xsct.QualifiedName);
                            return null;
                        }
                        else
                        {
                            schemaType = GetXmlSchemaTypeFromXmlSchemaAttribute(xa);
                        }
                    }
                }
                else
                {
                    Debug.Fail("not yet implemented!");
                }
            }

            return schemaType;
        }

        /// <summary>
        ///     Returns the XmlSchemaElement specified by the given qname defined in the given particle.
        /// </summary>
        /// <param name="particle"></param>
        /// <param name="qname"></param>
        /// <returns></returns>
        private XmlSchemaElement GetXmlSchemaElementFromParticle(XmlSchemaParticle particle, XmlQualifiedName qname)
        {
            XmlSchemaElement xe = null;
            var xsgb = particle as XmlSchemaGroupBase;
            var xsgr = particle as XmlSchemaGroupRef;
            if (xsgb != null)
            {
                xe = GetXmlElementFromXmlSchemaGroupBase(xsgb, qname);
            }
            else if (xsgr != null)
            {
                xe = GetXmlElementFromXmlSchemaGroupBase(xsgr.Particle, qname);
            }
            else if (particle != null)
            {
                Debug.Fail("Unable to handle case where particle type is " + particle.GetType());
            }
            return xe;
        }

        /// <summary>
        ///     Returns the XmlSchemaType for the given XmlSchemaAttribute
        /// </summary>
        /// <param name="xa"></param>
        /// <returns></returns>
        private XmlSchemaType GetXmlSchemaTypeFromXmlSchemaAttribute(XmlSchemaAttribute xa)
        {
            if (xa == null)
            {
                return null;
            }

            XmlSchemaType xst = xa.SchemaType;
            if (xst == null)
            {
                if (SchemaSet.GlobalTypes.Contains(xa.SchemaTypeName))
                {
                    xst = SchemaSet.GlobalTypes[xa.SchemaTypeName] as XmlSchemaType;
                }
            }

            if (xst == null)
            {
                return xa.AttributeSchemaType;
            }

            return xst;
        }

        /// <summary>
        ///     Returns the XmlSchema with the given namespace name from the instance's SchemaSet
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        private XmlSchema GetSchemaFromSchemaSet(string nameSpace)
        {
            XmlSchema schema = null;
            foreach (XmlSchema s in SchemaSet.Schemas(nameSpace))
            {
                schema = s;
                break;
            }

            return schema;
        }

        /// <summary>
        ///     Returns the XmlSchemaAttribute from the given XmlSchemaComplexType
        /// </summary>
        /// <param name="xsct"></param>
        /// <param name="qname"></param>
        /// <returns></returns>
        private XmlSchemaAttribute GetXmlSchemaAttributeFromXmlSchemaGroupBase(XmlSchemaComplexType xsct, XmlQualifiedName qname)
        {
            if (xsct == null)
            {
                throw new ArgumentNullException("xsct");
            }
            if (qname == null)
            {
                throw new ArgumentNullException("qname");
            }

            var attr = GetXmlSchemaAttributeFromAttributeCollection(xsct.Attributes, qname);
            if (attr == null)
            {
                if (xsct.ContentModel != null)
                {
                    var complexContentExtension = xsct.ContentModel.Content as XmlSchemaComplexContentExtension;
                    if (complexContentExtension != null)
                    {
                        attr = GetXmlSchemaAttributeFromAttributeCollection(complexContentExtension.Attributes, qname);
                    }
                }
            }

            if (attr == null)
            {
                var baseXsct = xsct.BaseXmlSchemaType as XmlSchemaComplexType;
                if (baseXsct != null)
                {
                    attr = GetXmlSchemaAttributeFromXmlSchemaGroupBase(baseXsct, qname);
                }
            }

            return attr;
        }

        /// <summary>
        ///     Returns the XmlSchemaAttribute from the given attribute collection.
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="qname"></param>
        /// <returns></returns>
        private XmlSchemaAttribute GetXmlSchemaAttributeFromAttributeCollection(
            XmlSchemaObjectCollection attributes, XmlQualifiedName qname)
        {
            if (attributes == null)
            {
                throw new ArgumentNullException("attributes");
            }
            if (qname == null)
            {
                throw new ArgumentNullException("qname");
            }

            XmlSchemaAttribute xmlSchemaAttribute = null;

            foreach (var xo in attributes)
            {
                var xsagr = xo as XmlSchemaAttributeGroupRef;
                var xa = xo as XmlSchemaAttribute;
                if (xsagr != null)
                {
                    var xsag = GetXmlSchemaAttributeGroupFromAttributeGroupRef(xsagr);

                    if (xsag == null)
                    {
                        continue;
                    }

                    xmlSchemaAttribute = GetXmlSchemaAttributeFromAttributeCollection(xsag.Attributes, qname);
                }
                else if (xa != null)
                {
                    if (xa.QualifiedName.Equals(qname))
                    {
                        xmlSchemaAttribute = xa;
                    }
                }

                if (xmlSchemaAttribute != null)
                {
                    break;
                }
            }
            return xmlSchemaAttribute;
        }

        /// <summary>
        ///     Returns the XmlSchemaAttributeGroup specified by the given XmlSchemaAttributeGroupRef from the instance's XmlSchemaSet.
        /// </summary>
        /// <param name="xsagr"></param>
        /// <returns></returns>
        private XmlSchemaAttributeGroup GetXmlSchemaAttributeGroupFromAttributeGroupRef(XmlSchemaAttributeGroupRef xsagr)
        {
            var schema = GetSchemaFromSchemaSet(xsagr.RefName.Namespace);
            XmlSchemaAttributeGroup xsag = null;

            if (schema != null)
            {
                if (schema.AttributeGroups.Contains(xsagr.RefName))
                {
                    xsag = schema.AttributeGroups[xsagr.RefName] as XmlSchemaAttributeGroup;
                }
            }
            else
            {
                // couldn't find specific schema, so scan all schemas' atribute groups.
                foreach (XmlSchema s in SchemaSet.Schemas())
                {
                    if (s.AttributeGroups.Contains(xsagr.RefName))
                    {
                        xsag = s.AttributeGroups[xsagr.RefName] as XmlSchemaAttributeGroup;
                        break;
                    }
                }
            }

            Debug.Assert(xsag != null, "didn't find XmlSchemaAttributeGroup " + xsagr.RefName + " in schema " + xsagr.RefName.Namespace);

            return xsag;
        }

        /// <summary>
        ///     Returns the XmlSchemaElement named by the given qname from the specified XmlSchemaGroupBase
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private XmlSchemaElement GetXmlElementFromXmlSchemaGroupBase(XmlSchemaGroupBase xsgb, XmlQualifiedName curr)
        {
            XmlSchemaElement schemaElement = null;

            foreach (var xso in xsgb.Items)
            {
                var xsgr = xso as XmlSchemaGroupRef;
                if (xsgr != null)
                {
                    schemaElement = GetXmlElementFromXmlSchemaGroupBase(xsgr.Particle, curr);
                }
                else if (xso is XmlSchemaElement)
                {

                    var xse = xso as XmlSchemaElement;
                    if (xse.QualifiedName.Equals(curr))
                    {
                        schemaElement = xse;
                    }
                }
                else if (xso is XmlSchemaChoice)
                {
                    var xsc = xso as XmlSchemaChoice;
                    schemaElement = GetXmlSchemaElement(xsc.Items, curr);
                }
                else if (xso is XmlSchemaAny)
                {
                    continue;
                }
                else if (xso is XmlSchemaSequence)
                {
                    var xss = xso as XmlSchemaSequence;
                    schemaElement = GetXmlSchemaElement(xss.Items, curr);
                }
                else
                {
                    Debug.Fail("unexpected type of XmlSchemaGroupBase!");
                }

                if (schemaElement != null)
                {
                    break;
                }
            }
            return schemaElement;
        }

        /// <summary>
        ///     returns the XmlSchemaType from the given XmlSchemaElement.
        /// </summary>
        /// <param name="schemaElement"></param>
        /// <returns></returns>
        private XmlSchemaType GetXmlSchemaTypeFromXmlSchemaElement(XmlSchemaElement schemaElement)
        {
            var schemaType = schemaElement.SchemaType;
            if (schemaType == null)
            {
                schemaType = GetXmlSchemaObject(SchemaSet.GlobalTypes, schemaElement.SchemaTypeName) as XmlSchemaType;
            }

            if (schemaType == null)
            {
                // if this was null, the element we have may be a ref to a global element, so look in the global elements to find its type
                if (SchemaSet.GlobalElements.Contains(schemaElement.QualifiedName))
                {
                    schemaElement = SchemaSet.GlobalElements[schemaElement.QualifiedName] as XmlSchemaElement;
                    schemaType = schemaElement.SchemaType;
                }
                if (schemaType == null)
                {
                    schemaType = GetXmlSchemaObject(SchemaSet.GlobalTypes, schemaElement.SchemaTypeName) as XmlSchemaType;
                }
            }

            Debug.Assert(schemaType != null, "Unable to find schema type " + schemaElement.SchemaTypeName + " in schemaSet's GlobalTypes");

            return schemaType;
        }

        /// <summary>
        ///     Returns the XmlSchemaObject with the given qname from the given table, or null if no such object exists
        /// </summary>
        /// <param name="objectTable"></param>
        /// <param name="qname"></param>
        /// <returns></returns>
        private static XmlSchemaObject GetXmlSchemaObject(XmlSchemaObjectTable objectTable, XmlQualifiedName qname)
        {
            XmlSchemaObject xo = null;
            if (objectTable.Contains(qname))
            {
                xo = objectTable[qname];
            }
            return xo;
        }

        /// <summary>
        ///     Returns the XmlSchemaElement with name qname from the given collection
        /// </summary>
        /// <param name="schemaObjects"></param>
        /// <param name="qname"></param>
        /// <returns></returns>
        private static XmlSchemaElement GetXmlSchemaElement(XmlSchemaObjectCollection schemaObjects, XmlQualifiedName qname)
        {
            XmlSchemaElement xeelement = null;
            foreach (var xo in schemaObjects)
            {
                var xe = xo as XmlSchemaElement;
                var xs = xo as XmlSchemaSequence;
                if (xe != null)
                {
                    if (xe.QualifiedName.Equals(qname))
                    {
                        xeelement = xe;
                    }
                }
                else if (xs != null)
                {
                    xeelement = GetXmlSchemaElement(xs.Items, qname);
                }

                if (xeelement != null)
                {
                    break;
                }
            }
            return xeelement;
        }

        /// <summary>
        ///     represents a path to an Attribute.  We assume that the XmlSchemaType for the given attribute can be deterministally
        ///     found given this AttributePath.
        /// </summary>
        internal class AttributePath
        {
            private readonly Stack<QNameNodeTypePair> _stack = new Stack<QNameNodeTypePair>();

            internal void PushFront(string name, string nsUri, bool isElement)
            {
                PushFront(new XmlQualifiedName(name, nsUri), isElement);
            }

            internal void PushFront(XmlQualifiedName qname, bool isElement)
            {
                _stack.Push(
                    new QNameNodeTypePair(qname, (isElement ? QNameNodeTypePair.NodeType.Element : QNameNodeTypePair.NodeType.Attribute)));
            }

            internal QNameNodeTypePair PopFront()
            {
                return _stack.Pop();
            }

            internal int Count
            {
                get { return _stack.Count; }
            }

            internal AttributePath Clone()
            {
                var clone = new AttributePath();
                var array = _stack.ToArray();
                for (var i = array.Length - 1; i > -1; i--)
                {
                    clone._stack.Push(array[i]);
                }
                return clone;
            }

            /// <summary>
            ///     Represents an node in an AttributePath
            /// </summary>
            internal class QNameNodeTypePair
            {
                internal XmlQualifiedName qname;
                internal NodeType type;

                internal QNameNodeTypePair(XmlQualifiedName n, NodeType t)
                {
                    qname = n;
                    type = t;
                }

                internal enum NodeType
                {
                    Attribute,
                    Element,
                }
            }
        }

        /// <summary>
        ///     Simple class to use to count the number of schema validation errors
        /// </summary>
        protected class SchemaValidationErrorCollector
        {
            private int _errorCount;

            internal int ErrorCount
            {
                get { return _errorCount; }
            }

            internal void ValidationCallBack(object sender, ValidationEventArgs e)
            {
                ++_errorCount;
            }
        }
    }
}
