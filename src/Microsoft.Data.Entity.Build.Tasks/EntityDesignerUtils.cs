// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Design
{
    internal static class EntityDesignerUtils
    {
        private static readonly EFNamespaceSet v1Namespaces = new EFNamespaceSet
        {
            Edmx = "http://schemas.microsoft.com/ado/2007/06/edmx",
            Csdl = "http://schemas.microsoft.com/ado/2006/04/edm",
            Msl = "urn:schemas-microsoft-com:windows:storage:mapping:CS",
            Ssdl = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl",
        };
        private static readonly EFNamespaceSet v2Namespaces = new EFNamespaceSet
        {
            Edmx = "http://schemas.microsoft.com/ado/2008/10/edmx",
            Csdl = "http://schemas.microsoft.com/ado/2008/09/edm",
            Msl = "http://schemas.microsoft.com/ado/2008/09/mapping/cs",
            Ssdl = "http://schemas.microsoft.com/ado/2009/02/edm/ssdl",
        };
        private static readonly EFNamespaceSet v3Namespaces = new EFNamespaceSet
        {
            Edmx = "http://schemas.microsoft.com/ado/2009/11/edmx",
            Csdl = "http://schemas.microsoft.com/ado/2009/11/edm",
            Msl = "http://schemas.microsoft.com/ado/2009/11/mapping/cs",
            Ssdl = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl",
        };

        /// <summary>
        /// Extract the Conceptual, Mapping and Storage nodes from an EDMX input streams, and extract the value of the
        /// metadataArtifactProcessing property.
        /// </summary>
        public static void ExtractConceptualMappingAndStorageNodes(
            StreamReader edmxInputStream,
            out XmlElement conceptualSchemaNode,
            out XmlElement mappingNode,
            out XmlElement storageSchemaNode,
            out string metadataArtifactProcessingValue)
        {
            // load up an XML document representing the edmx file
            XmlDocument xmlDocument = new XmlDocument();
            using (XmlReader reader = XmlReader.Create(edmxInputStream))
            {
                xmlDocument.Load(reader);
            }

            EFNamespaceSet set = v3Namespaces;
            if (xmlDocument.DocumentElement.NamespaceURI == v2Namespaces.Edmx)
            {
                set = v2Namespaces;
            }
            else if (xmlDocument.DocumentElement.NamespaceURI == v1Namespaces.Edmx)
            {
                set = v1Namespaces;
            }

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDocument.NameTable);
            nsMgr.AddNamespace("edmx", set.Edmx);
            nsMgr.AddNamespace("edm", set.Csdl);
            nsMgr.AddNamespace("ssdl", set.Ssdl);
            nsMgr.AddNamespace("map", set.Msl);

            // find the ConceptualModel Schema node
            conceptualSchemaNode = (XmlElement)xmlDocument.SelectSingleNode(
                "/edmx:Edmx/edmx:Runtime/edmx:ConceptualModels/edm:Schema",
                nsMgr);

            // find the StorageModel Schema node
            storageSchemaNode = (XmlElement)xmlDocument.SelectSingleNode(
                "/edmx:Edmx/edmx:Runtime/edmx:StorageModels/ssdl:Schema",
                nsMgr);

            // find the Mapping node
            mappingNode = (XmlElement)xmlDocument.SelectSingleNode(
                "/edmx:Edmx/edmx:Runtime/edmx:Mappings/map:Mapping",
                nsMgr);

            // find the Connection node
            metadataArtifactProcessingValue = string.Empty;
            XmlNodeList connectionProperties = xmlDocument.SelectNodes(
                "/edmx:Edmx/edmx:Designer/edmx:Connection/edmx:DesignerInfoPropertySet/edmx:DesignerProperty",
                nsMgr);
            if (connectionProperties != null)
            {
                foreach (XmlNode propertyNode in connectionProperties)
                {
                    foreach (XmlAttribute a in propertyNode.Attributes)
                    {
                        // treat attribute names case-sensitive (since it is xml), but attribute value case-insensitive
                        // to be accommodating.
                        if (a.Name.Equals("Name", StringComparison.Ordinal)
                            && a.Value.Equals("MetadataArtifactProcessing", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (XmlAttribute a2 in propertyNode.Attributes)
                            {
                                if (a2.Name.Equals("Value", StringComparison.Ordinal))
                                {
                                    metadataArtifactProcessingValue = a2.Value;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Utility method to ensure an XmlElement (containing the C, M or S element from the Edmx file) is sent out to
        /// a stream in the same format.
        /// </summary>
        public static void OutputXmlElementToStream(XmlElement xmlElement, Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };

            // set up output document
            XmlDocument outputXmlDoc = new XmlDocument();
            XmlNode importedElement = outputXmlDoc.ImportNode(xmlElement, deep: true);
            outputXmlDoc.AppendChild(importedElement);

            // write out XmlDocument
            XmlWriter writer = null;
            try
            {
                writer = XmlWriter.Create(stream, settings);
                outputXmlDoc.WriteTo(writer);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        private struct EFNamespaceSet
        {
            public string Edmx;
            public string Csdl;
            public string Msl;
            public string Ssdl;
        }
    }
}
