// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Edm.Serialization
{
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Edm.Serialization;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Xml;

    internal sealed class EdmxSerializer
    {
        private const string EdmXmlNamespaceV2 = "http://schemas.microsoft.com/ado/2008/10/edmx";

        private const string EdmXmlNamespaceV3 = "http://schemas.microsoft.com/ado/2009/11/edmx";

        private DbDatabaseMapping _databaseMapping;
        private double _version;
        private DbProviderInfo _providerInfo;
        private XmlWriter _xmlWriter;
        private string _namespace;

        public void Serialize(DbDatabaseMapping databaseMapping, DbProviderInfo providerInfo, XmlWriter xmlWriter)
        {
            Contract.Requires(xmlWriter != null);
            Contract.Requires(databaseMapping != null);
            Contract.Requires(providerInfo != null);
            Contract.Assert(databaseMapping.Model != null);
            Contract.Assert(databaseMapping.Database != null);

            _xmlWriter = xmlWriter;
            _databaseMapping = databaseMapping;
            _version = databaseMapping.Model.Version;
            _providerInfo = providerInfo;
            _namespace = _version == DataModelVersions.Version3 ? EdmXmlNamespaceV3 : EdmXmlNamespaceV2;

            _xmlWriter.WriteStartDocument();

            using (Element("Edmx", "Version", string.Format(CultureInfo.InvariantCulture, "{0:F1}", _version)))
            {
                WriteEdmxRuntime();
                WriteEdmxDesigner();
            }

            _xmlWriter.WriteEndDocument();
            _xmlWriter.Flush();
        }

        private void WriteEdmxRuntime()
        {
            using (Element("Runtime"))
            {
                using (Element("ConceptualModels"))
                {
                    _databaseMapping.Model.ValidateAndSerializeCsdl(_xmlWriter);
                }

                using (Element("Mappings"))
                {
                    new MslSerializer().Serialize(_databaseMapping, _xmlWriter);
                }

                using (Element("StorageModels"))
                {
                    new SsdlSerializer().Serialize(
                        _databaseMapping.Database,
                        _providerInfo.ProviderInvariantName,
                        _providerInfo.ProviderManifestToken,
                        _xmlWriter);
                }
            }
        }

        private void WriteEdmxDesigner()
        {
            using (Element("Designer"))
            {
                WriteEdmxConnection();
                WriteEdmxOptions();
                WriteEdmxDiagrams();
            }
        }

        private void WriteEdmxConnection()
        {
            using (Element("Connection"))
            {
                using (Element("DesignerInfoPropertySet"))
                {
                    WriteDesignerPropertyElement("MetadataArtifactProcessing", "EmbedInOutputAssembly");
                }
            }
        }

        private void WriteEdmxOptions()
        {
            using (Element("Options"))
            {
                using (Element("DesignerInfoPropertySet"))
                {
                    WriteDesignerPropertyElement("ValidateOnBuild", "False");
                    WriteDesignerPropertyElement("CodeGenerationStrategy", "None");
                    WriteDesignerPropertyElement("ProcessDependentTemplatesOnSave", "False");
                }
            }
        }

        private void WriteDesignerPropertyElement(string name, string value)
        {
            using (Element("DesignerProperty", "Name", name, "Value", value))
            {
            }
        }

        private void WriteEdmxDiagrams()
        {
            using (Element("Diagrams"))
            {
            }
        }

        private IDisposable Element(string elementName, params string[] attributes)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(elementName));
            Contract.Requires(attributes != null);

            _xmlWriter.WriteStartElement(elementName, _namespace);

            for (var i = 0; i < attributes.Length - 1; i += 2)
            {
                _xmlWriter.WriteAttributeString(attributes[i], attributes[i + 1]);
            }

            return new EndElement(_xmlWriter);
        }

        private class EndElement : IDisposable
        {
            private readonly XmlWriter _xmlWriter;

            public EndElement(XmlWriter xmlWriter)
            {
                _xmlWriter = xmlWriter;
            }

            public void Dispose()
            {
                _xmlWriter.WriteEndElement();
            }
        }
    }
}
