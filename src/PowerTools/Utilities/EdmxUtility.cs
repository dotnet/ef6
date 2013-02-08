// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Design;
    using System.Data.Mapping;
    using System.Data.Metadata.Edm;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.DbContextPackage.Extensions;
    using Microsoft.DbContextPackage.Resources;

    internal class EdmxUtility
    {
        private static readonly IEnumerable<XNamespace> EDMX_NAMESPACES = new XNamespace[]
            {
                "http://schemas.microsoft.com/ado/2009/11/edmx",
                "http://schemas.microsoft.com/ado/2008/10/edmx",
                "http://schemas.microsoft.com/ado/2007/06/edmx"
            };

        private readonly string _edmxPath;

        public EdmxUtility(string edmxPath)
        {
            DebugCheck.NotEmpty(edmxPath);

            _edmxPath = edmxPath;
        }

        public StorageMappingItemCollection GetMappingCollection()
        {
            IList<EdmSchemaError> errors;
            var edmxFileName = Path.GetFileName(_edmxPath);

            EdmItemCollection edmCollection;
            using (var reader = CreateSectionReader(EdmxSection.Csdl))
            {
                edmCollection = MetadataItemCollectionFactory.CreateEdmItemCollection(
                    new[] { reader },
                    out errors);
                errors.HandleErrors(Strings.EdmSchemaError(edmxFileName, EdmxSection.Csdl.SectionName));
            }

            StoreItemCollection storeCollection;
            using (var reader = CreateSectionReader(EdmxSection.Ssdl))
            {
                storeCollection = MetadataItemCollectionFactory.CreateStoreItemCollection(
                    new[] { reader },
                    out errors);
                errors.HandleErrors(Strings.EdmSchemaError(edmxFileName, EdmxSection.Ssdl.SectionName));
            }

            StorageMappingItemCollection mappingCollection;
            using (var reader = CreateSectionReader(EdmxSection.Msl))
            {
                mappingCollection = MetadataItemCollectionFactory.CreateStorageMappingItemCollection(
                        edmCollection,
                        storeCollection,
                        new[] { reader },
                        out errors);
                errors.HandleErrors(Strings.EdmSchemaError(edmxFileName, EdmxSection.Msl.SectionName));
            }

            return mappingCollection;
        }

        private XmlReader CreateSectionReader(EdmxSection edmxSection)
        {
            DebugCheck.NotNull(edmxSection);

            var edmxDocument = XElement.Load(_edmxPath, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);

            var runtime = edmxDocument.Element(EDMX_NAMESPACES, "Runtime");
            if (runtime == null)
            {
                return null;
            }

            var section = runtime.Element(EDMX_NAMESPACES, edmxSection.SectionName);
            if (section == null)
            {
                return null;
            }

            var rootElement = section.Element(edmxSection.Namespaces, edmxSection.RootElementName);
            if (rootElement == null)
            {
                return null;
            }

            return rootElement.CreateReader();
        }

        private sealed class EdmxSection
        {
            static EdmxSection()
            {
                Csdl = new EdmxSection
                    {
                        Namespaces = new XNamespace[]
                            {
                                "http://schemas.microsoft.com/ado/2009/11/edm",
                                "http://schemas.microsoft.com/ado/2008/09/edm",
                                "http://schemas.microsoft.com/ado/2006/04/edm"
                            },
                        SectionName = "ConceptualModels",
                        RootElementName = "Schema"
                    };
                Msl = new EdmxSection
                    {
                        Namespaces = new XNamespace[]
                            {
                                "http://schemas.microsoft.com/ado/2009/11/mapping/cs",
                                "http://schemas.microsoft.com/ado/2008/09/mapping/cs",
                                "urn:schemas-microsoft-com:windows:storage:mapping:CS"
                            },
                        SectionName = "Mappings",
                        RootElementName = "Mapping"
                    };
                Ssdl = new EdmxSection
                    {
                        Namespaces = new XNamespace[]
                            {
                                "http://schemas.microsoft.com/ado/2009/11/edm/ssdl",
                                "http://schemas.microsoft.com/ado/2009/02/edm/ssdl",
                                "http://schemas.microsoft.com/ado/2006/04/edm/ssdl"
                            },
                        SectionName = "StorageModels",
                        RootElementName = "Schema"
                    };
            }

            private EdmxSection()
            {
            }

            public static EdmxSection Csdl { get; private set; }
            public static EdmxSection Msl { get; private set; }
            public static EdmxSection Ssdl { get; private set; }

            public IEnumerable<XNamespace> Namespaces { get; private set; }
            public string SectionName { get; private set; }
            public string RootElementName { get; private set; }
        }
    }
}
