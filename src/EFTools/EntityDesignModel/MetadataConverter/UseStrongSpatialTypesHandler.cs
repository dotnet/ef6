// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Xml;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal sealed class UseStrongSpatialTypesHandler : MetadataConverterHandler
    {
        private readonly Version _targetSchemaVersion;

        internal UseStrongSpatialTypesHandler(Version targetSchemaVersion)
        {
            _targetSchemaVersion = targetSchemaVersion;
        }

        /// <summary>
        ///     If we are retargeting to a schema version which supports it, ensure
        ///     UseStrongSpatialTypes="false" is set on the CSDL Schema Element.
        ///     If we are retargeting to a schema version which does not support it, ensure
        ///     UseStrongSpatialTypes is absent from the CSDL Schema Element.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected override XmlDocument DoHandleConversion(XmlDocument doc)
        {
            var nsmgr = SchemaManager.GetEdmxNamespaceManager(doc.NameTable, _targetSchemaVersion);
            var annotationNamespace = SchemaManager.GetAnnotationNamespaceName();
            var csdlSchemaElement = (XmlElement)doc.SelectSingleNode("/edmx:Edmx/edmx:Runtime/edmx:ConceptualModels/csdl:Schema", nsmgr);
            if (csdlSchemaElement != null)
            {
                var useStrongSpatialTypesAttr =
                    csdlSchemaElement.Attributes[UseStrongSpatialTypesDefaultableValue.AttributeUseStrongSpatialTypes, annotationNamespace];
                if (EdmFeatureManager.GetUseStrongSpatialTypesFeatureState(_targetSchemaVersion).IsEnabled())
                {
                    // we are retargeting to a Schema Version that supports UseStrongSpatialTypes - add UseStrongSpatialTypes="false" if it is not present
                    if (useStrongSpatialTypesAttr == null)
                    {
                        useStrongSpatialTypesAttr = doc.CreateAttribute(
                            "annotation", UseStrongSpatialTypesDefaultableValue.AttributeUseStrongSpatialTypes, annotationNamespace);
                        useStrongSpatialTypesAttr.Value = "false";
                        csdlSchemaElement.Attributes.Append(useStrongSpatialTypesAttr);

                        // setting the xmlns:annotation attribute explicitly will ensure that the XmlReader does not come up
                        // with an auto-generated namespace prefix which may cause an NRE in the XmlEditor leading to a VS crash
                        var annotationXmlnsAttr = doc.CreateAttribute("xmlns", "annotation", "http://www.w3.org/2000/xmlns/");
                        annotationXmlnsAttr.Value = annotationNamespace;
                        csdlSchemaElement.SetAttributeNode(annotationXmlnsAttr);
                    }
                }
                else
                {
                    // we are retargeting to a Schema Version that does not support UseStrongSpatialTypes - remove UseStrongSpatialTypes if it is present
                    if (useStrongSpatialTypesAttr != null)
                    {
                        csdlSchemaElement.Attributes.Remove(useStrongSpatialTypesAttr);
                    }
                }
            }

            return doc;
        }
    }
}
