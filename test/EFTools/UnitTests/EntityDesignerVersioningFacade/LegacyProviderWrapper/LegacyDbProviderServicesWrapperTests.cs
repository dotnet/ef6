// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using SystemDataCommon = System.Data.Common;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper
{
    using System;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.IO;
    using System.Xml;
    using Moq;
    using Moq.Protected;
    using Xunit;
    using DbCommandTree = System.Data.Common.CommandTrees.DbCommandTree;

    public class LegacyDbProviderServicesWrapperTests
    {
        private static readonly SystemDataCommon.DbProviderManifest
            LegacyProviderManifest =
                ((SystemDataCommon.DbProviderServices)
                 ((IServiceProvider)SystemDataCommon.DbProviderFactories.GetFactory("System.Data.SqlClient"))
                     .GetService(typeof(SystemDataCommon.DbProviderServices)))
                    .GetProviderManifest("2008");

        [Fact]
        public void GetDbProviderManifestToken_returns_providerManifestToken_from_wrapped_legacy_DbProviderServices()
        {
            var providerServicesMock = new Mock<SystemDataCommon.DbProviderServices>();
            providerServicesMock
                .Protected()
                .Setup<string>("GetDbProviderManifestToken", ItExpr.IsAny<SystemDataCommon.DbConnection>())
                .Returns("FakeProviderManifestToken");

            Assert.Equal(
                "FakeProviderManifestToken",
                new LegacyDbProviderServicesWrapper(providerServicesMock.Object)
                    .GetProviderManifestToken(new Mock<SystemDataCommon.DbConnection>().Object));
        }

        [Fact]
        public void CreateDbCommandDefinition_returns_wrapped_legacy_command_definition()
        {
            var commandDefinition =
                new LegacyDbProviderServicesWrapper(new Mock<SystemDataCommon.DbProviderServices>().Object)
                    .CreateCommandDefinition(
                        new LegacyDbProviderManifestWrapper(LegacyProviderManifest),
                        new DbQueryCommandTree(CreateMetadataWorkspace(), DataSpace.SSpace, DbExpressionBuilder.Constant(42), false));

            Assert.NotNull(commandDefinition);
            Assert.IsType<LegacyDbCommandDefinitionWrapper>(commandDefinition);
        }

        [Fact]
        public void CreateDbCommandDefinition_converts_legacy_ProviderIncompatibleException_to_non_legacy_ProviderIncompatibleException()
        {
            var expectedException = new InvalidOperationException("Test");
            try
            {
                var mockProviderServices = new Mock<SystemDataCommon.DbProviderServices>();
                mockProviderServices
                    .Protected()
                    .Setup("CreateDbCommandDefinition", ItExpr.IsAny<SystemDataCommon.DbProviderManifest>(), ItExpr.IsAny<DbCommandTree>())
                    .Throws(expectedException);

                new LegacyDbProviderServicesWrapper(mockProviderServices.Object)
                    .CreateCommandDefinition(
                        new LegacyDbProviderManifestWrapper(LegacyProviderManifest),
                        new DbQueryCommandTree(CreateMetadataWorkspace(), DataSpace.SSpace, DbExpressionBuilder.Constant(42), false));

                throw new InvalidOperationException("Expected exception but none thrown.");
            }
            catch (ProviderIncompatibleException exception)
            {
                Assert.Same(expectedException, exception.InnerException);
            }
        }

        private static MetadataWorkspace CreateMetadataWorkspace()
        {
            const string ssdl =
                @"<Schema Namespace=""NorthwindEF5Model.Store"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2008"" Alias=""Self"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm/ssdl"" xmlns:store=""http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator"">"
                +
                @"  <EntityType Name=""Customers"">" +
                @"    <Key>" +
                @"      <PropertyRef Name=""CustomerID"" />" +
                @"    </Key>" +
                @"    <Property Name=""CustomerID"" Type=""nchar"" MaxLength=""5"" Nullable=""false"" />" +
                @"    <Property Name=""CompanyName"" Type=""nvarchar"" MaxLength=""40"" Nullable=""false"" />" +
                @"  </EntityType>" +
                @"  <EntityContainer Name=""Container"" />" +
                @"</Schema>";

            const string csdl =
                @"<Schema xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"" Namespace=""dummy"">" +
                @"    <EntityContainer Name=""DummyContainer""/>" +
                @"</Schema>";

            const string msl =
                @"<Mapping Space=""C-S"" xmlns=""http://schemas.microsoft.com/ado/2009/11/mapping/cs"">" +
                @"  <EntityContainerMapping StorageEntityContainer=""Container"" CdmEntityContainer=""DummyContainer"" />" +
                @"</Mapping>";

            var storeItemCollection = Utils.CreateStoreItemCollection(ssdl);
            var edmItemCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(csdl)) });
            var mappingItemCollection =
                new StorageMappingItemCollection(
                    edmItemCollection,
                    storeItemCollection,
                    new[] { XmlReader.Create(new StringReader(msl)) });

            return new MetadataWorkspace(() => edmItemCollection, () => storeItemCollection, () => mappingItemCollection);
        }
    }
}
