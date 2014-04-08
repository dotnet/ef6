// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System;
    using System.Linq;
    using System.Xml;
    using Xunit;

    public class SchemaManagerTests
    {
        private const string CsdlNsV1 = "http://schemas.microsoft.com/ado/2006/04/edm";
        private const string CsdlNsV2 = "http://schemas.microsoft.com/ado/2008/09/edm";
        private const string CsdlNsV3 = "http://schemas.microsoft.com/ado/2009/11/edm";

        private const string SsdlNsV1 = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl";
        private const string SsdlNsV2 = "http://schemas.microsoft.com/ado/2009/02/edm/ssdl";
        private const string SsdlNsV3 = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";

        private const string MslNsV1 = "urn:schemas-microsoft-com:windows:storage:mapping:CS";
        private const string MslNsV2 = "http://schemas.microsoft.com/ado/2008/09/mapping/cs";
        private const string MslNsV3 = "http://schemas.microsoft.com/ado/2009/11/mapping/cs";

        private const string EdmxNsV1 = "http://schemas.microsoft.com/ado/2007/06/edmx";
        private const string EdmxNsV2 = "http://schemas.microsoft.com/ado/2008/10/edmx";
        private const string EdmxNsV3 = "http://schemas.microsoft.com/ado/2009/11/edmx";

        private const string EntityStoreSchemaGeneratorNs = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator";

        private const string CodeGenerationNs = "http://schemas.microsoft.com/ado/2006/04/codegeneration";

        private const string ProviderManifestNs = "http://schemas.microsoft.com/ado/2006/04/edm/providermanifest";

        private const string AnnotationNs = "http://schemas.microsoft.com/ado/2009/02/edm/annotation";

        [Fact]
        public void SchemaManager_GetCSDLNamespaceName_returns_correct_Csdl_namespaces_for_requested_version()
        {
            Assert.Equal(CsdlNsV1, SchemaManager.GetCSDLNamespaceName(new Version(1, 0, 0, 0)));
            Assert.Equal(CsdlNsV2, SchemaManager.GetCSDLNamespaceName(new Version(2, 0, 0, 0)));
            Assert.Equal(CsdlNsV3, SchemaManager.GetCSDLNamespaceName(new Version(3, 0, 0, 0)));
        }

        [Fact]
        public void SchemaManager_GetCSDLNamespaceName_returns_all_known_Csdl_namespaces()
        {
            var csdlNamespaces = SchemaManager.GetCSDLNamespaceNames();

            Assert.True(new[] { CsdlNsV1, CsdlNsV2, CsdlNsV3 }.SequenceEqual(csdlNamespaces));
        }

        [Fact]
        public void SchemaManager_GetSSDLNamespaceName_returns_correct_Ssdl_namespaces_for_requested_version()
        {
            Assert.Equal(SsdlNsV1, SchemaManager.GetSSDLNamespaceName(new Version(1, 0, 0, 0)));
            Assert.Equal(SsdlNsV2, SchemaManager.GetSSDLNamespaceName(new Version(2, 0, 0, 0)));
            Assert.Equal(SsdlNsV3, SchemaManager.GetSSDLNamespaceName(new Version(3, 0, 0, 0)));
        }

        [Fact]
        public void SchemaManager_GetSSDLNamespaceName_returns_all_known_Ssdl_namespaces()
        {
            var csdlNamespaces = SchemaManager.GetSSDLNamespaceNames();

            Assert.True(new[] { SsdlNsV1, SsdlNsV2, SsdlNsV3 }.SequenceEqual(csdlNamespaces));
        }

        [Fact]
        public void SchemaManager_GetMSLNamespaceName_returns_correct_Msl_namespaces_for_requested_version()
        {
            Assert.Equal(MslNsV1, SchemaManager.GetMSLNamespaceName(new Version(1, 0, 0, 0)));
            Assert.Equal(MslNsV2, SchemaManager.GetMSLNamespaceName(new Version(2, 0, 0, 0)));
            Assert.Equal(MslNsV3, SchemaManager.GetMSLNamespaceName(new Version(3, 0, 0, 0)));
        }

        [Fact]
        public void SchemaManager_GetMSLNamespaceName_returns_all_known_Msl_namespaces()
        {
            var csdlNamespaces = SchemaManager.GetMSLNamespaceNames();

            Assert.True(new[] { MslNsV1, MslNsV2, MslNsV3 }.SequenceEqual(csdlNamespaces));
        }

        [Fact]
        public void SchemaManager_GetEDMXNamespaceName_returns_correct_Edmx_namespaces_for_requested_version()
        {
            Assert.Equal(EdmxNsV1, SchemaManager.GetEDMXNamespaceName(new Version(1, 0, 0, 0)));
            Assert.Equal(EdmxNsV2, SchemaManager.GetEDMXNamespaceName(new Version(2, 0, 0, 0)));
            Assert.Equal(EdmxNsV3, SchemaManager.GetEDMXNamespaceName(new Version(3, 0, 0, 0)));
        }

        [Fact]
        public void SchemaManager_GetEDMXNamespaceName_returns_all_known_Edmx_namespaces()
        {
            var csdlNamespaces = SchemaManager.GetEDMXNamespaceNames();

            Assert.True(new[] { EdmxNsV1, EdmxNsV2, EdmxNsV3 }.SequenceEqual(csdlNamespaces));
        }

        [Fact]
        public void
            SchemaManager_GetEntityStoreSchemaGeneratorNamespaceName_returns_correct_EntityStoreSchemaGenerator_namespace()
        {
            Assert.Equal(EntityStoreSchemaGeneratorNs, SchemaManager.GetEntityStoreSchemaGeneratorNamespaceName());
        }

        [Fact]
        public void SchemaManager_GetCodeGenerationNamespaceName_returns_correct_CodeGeneration_namespace()
        {
            Assert.Equal(CodeGenerationNs, SchemaManager.GetCodeGenerationNamespaceName());
       }

        [Fact]
        public void SchemaManager_GetProviderManifestNamespaceName_returns_correct_ProviderManifest_namespace()
        {
            Assert.Equal(ProviderManifestNs, SchemaManager.GetProviderManifestNamespaceName());
        }

        [Fact]
        public void SchemaManager_GetAnnotationNamespaceName_returns_correct_Annotation_namespace()
        {
            Assert.Equal(AnnotationNs, SchemaManager.GetAnnotationNamespaceName());
        }

        [Fact]
        public void SchemaManager_GetAllNamespacesForVersion_returns_correct_namespaces_for_requested_version()
        {
            var namespacesV1 = new[] { EdmxNsV1, CsdlNsV1, SsdlNsV1, ProviderManifestNs, CodeGenerationNs, MslNsV1, AnnotationNs };
            var namespacesV2 = new[] { EdmxNsV2, CsdlNsV2, SsdlNsV2, ProviderManifestNs, CodeGenerationNs, MslNsV2, AnnotationNs };
            var namespacesV3 = new[] { EdmxNsV3, CsdlNsV3, SsdlNsV3, ProviderManifestNs, CodeGenerationNs, MslNsV3, AnnotationNs };

            Assert.True(namespacesV1.SequenceEqual(SchemaManager.GetAllNamespacesForVersion(new Version(1, 0, 0, 0))));
            Assert.True(namespacesV2.SequenceEqual(SchemaManager.GetAllNamespacesForVersion(new Version(2, 0, 0, 0))));
            Assert.True(namespacesV3.SequenceEqual(SchemaManager.GetAllNamespacesForVersion(new Version(3, 0, 0, 0))));
        }

        [Fact]
        public void SchemaManager_GetSchemaVersion_returns_correct_version_for_namespace()
        {
            var v1 = new Version(1, 0, 0, 0);
            var v2 = new Version(2, 0, 0, 0);
            var v3 = new Version(3, 0, 0, 0);


            Assert.Equal(v1, SchemaManager.GetSchemaVersion(CsdlNsV1));
            Assert.Equal(v2, SchemaManager.GetSchemaVersion(CsdlNsV2));
            Assert.Equal(v3, SchemaManager.GetSchemaVersion(CsdlNsV3));

            Assert.Equal(v1, SchemaManager.GetSchemaVersion(SsdlNsV1));
            Assert.Equal(v2, SchemaManager.GetSchemaVersion(SsdlNsV2));
            Assert.Equal(v3, SchemaManager.GetSchemaVersion(SsdlNsV3));

            Assert.Equal(v1, SchemaManager.GetSchemaVersion(MslNsV1));
            Assert.Equal(v2, SchemaManager.GetSchemaVersion(MslNsV2));
            Assert.Equal(v3, SchemaManager.GetSchemaVersion(MslNsV3));

            Assert.Equal(v1, SchemaManager.GetSchemaVersion(EdmxNsV1));
            Assert.Equal(v2, SchemaManager.GetSchemaVersion(EdmxNsV2));
            Assert.Equal(v3, SchemaManager.GetSchemaVersion(EdmxNsV3));

            Assert.Equal(v1, SchemaManager.GetSchemaVersion(null));
            Assert.Equal(v1, SchemaManager.GetSchemaVersion("abc"));
        }

        [Fact]
        public void SchemaManager_GetSchemaVersion_returns_null_for_unknown_namespace()
        {
            Assert.Equal(new Version(1, 0, 0, 0), SchemaManager.GetSchemaVersion("http://tempuri.org"));
        }

        [Fact]
        public void SchemaManager_GetEdmxNamespaceManager_namespace_manager_with_correct_bindings()
        {
            for (var majorVersion = 1; majorVersion <= 3; majorVersion++)
            {
                var version = new Version(majorVersion, 0, 0, 0);

                var nsMgr = SchemaManager.GetEdmxNamespaceManager(new NameTable(), version);

                Assert.Equal(SchemaManager.GetEDMXNamespaceName(version), nsMgr.LookupNamespace("edmx"));
                Assert.Equal(SchemaManager.GetCSDLNamespaceName(version), nsMgr.LookupNamespace("csdl"));
                Assert.Equal(SchemaManager.GetEntityStoreSchemaGeneratorNamespaceName(), nsMgr.LookupNamespace("essg"));
                Assert.Equal(SchemaManager.GetSSDLNamespaceName(version), nsMgr.LookupNamespace("ssdl"));
                Assert.Equal(SchemaManager.GetMSLNamespaceName(version), nsMgr.LookupNamespace("msl"));
            }
        }
    }
}
