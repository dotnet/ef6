// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using LegacyMetadata = System.Data.Metadata.Edm;

namespace Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper.LegacyMetadataExtensions
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Xunit;

    public class TypeUsageExtensionsTests
    {
        [Fact]
        public void ToStoreLegacyType_returns_legacy_type_for_EntityType()
        {
            const string ssdl =
                "<Schema Namespace='AdventureWorksModel.Store' Provider='System.Data.SqlClient' ProviderManifestToken='2008' xmlns='http://schemas.microsoft.com/ado/2009/11/edm/ssdl'>"
                +
                "  <EntityContainer Name='AdventureWorksModelStoreContainer'>" +
                "    <EntitySet Name='Entities' EntityType='AdventureWorksModel.Store.Entities' Schema='dbo' />" +
                "  </EntityContainer>" +
                "  <EntityType Name='Entities'>" +
                "    <Key>" +
                "      <PropertyRef Name='Id' />" +
                "    </Key>" +
                "    <Property Name='Id' Type='int' StoreGeneratedPattern='Identity' Nullable='false' />" +
                "    <Property Name='Name' Type='nvarchar(max)' Nullable='false' />" +
                "  </EntityType>" +
                "</Schema>";

            var storeItemCollection = Utils.CreateStoreItemCollection(ssdl);
            var legacyStoreItemCollection =
                new LegacyMetadata.StoreItemCollection(new[] { XmlReader.Create(new StringReader(ssdl)) });

            var entityTypeUsage =
                TypeUsage.CreateDefaultTypeUsage(storeItemCollection.GetItem<EntityType>("AdventureWorksModel.Store.Entities"));
            var legacyEntityTypeUsage =
                entityTypeUsage.ToLegacyStoreTypeUsage(legacyStoreItemCollection.GetItems<LegacyMetadata.EdmType>().ToArray());

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(legacyEntityTypeUsage, entityTypeUsage);
        }

        [Fact]
        public void ToLegacyEdmTypeUsage_returns_legacy_type_for_RowType()
        {
            const string csdl =
                "<Schema Namespace='AdventureWorksModel' Alias='Self' p1:UseStrongSpatialTypes='false' xmlns:annotation='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns:p1='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns='http://schemas.microsoft.com/ado/2009/11/edm'>"
                +
                "  <Function Name='LastNamesAfter'>" +
                "    <Parameter Name='someString' Type='Edm.String' />" +
                "    <ReturnType>" +
                "      <RowType>" +
                "        <Property Name='FirstName' Type='Edm.String' Nullable='false' />" +
                "        <Property Name='LastName' Type='Edm.String' Nullable='false' />" +
                "      </RowType>" +
                "    </ReturnType>" +
                "    <DefiningExpression>dummy</DefiningExpression>" +
                "  </Function>" +
                "</Schema>";

            var edmItemCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(csdl)) });
            var rowTypeUsage =
                edmItemCollection.GetItems<EdmFunction>().Single(f => f.FullName == "AdventureWorksModel.LastNamesAfter")
                    .ReturnParameters[0]
                    .TypeUsage;

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(rowTypeUsage.ToLegacyEdmTypeUsage(), rowTypeUsage);
        }

        [Fact]
        public void ToLegacyStoreTypeUsage_returns_legacy_type_for_RowType()
        {
            const string ssdl =
                "<Schema Namespace='AdventureWorksModel.Store' Provider='System.Data.SqlClient' ProviderManifestToken='2008' xmlns='http://schemas.microsoft.com/ado/2009/11/edm/ssdl'>"
                +
                "  <Function Name='GetNames'>" +
                "    <ReturnType>" +
                "      <CollectionType>" +
                "        <RowType>" +
                "          <Property Name='FirstName' Type='nvarchar' Nullable='false' />" +
                "          <Property Name='LastName' Type='nvarchar' Nullable='false' />" +
                "        </RowType>" +
                "      </CollectionType>" +
                "    </ReturnType>" +
                "  </Function>" +
                "</Schema>";

            var storeItemCollection = Utils.CreateStoreItemCollection(ssdl);
            var legacyStoreItemCollection =
                new LegacyMetadata.StoreItemCollection(new[] { XmlReader.Create(new StringReader(ssdl)) });

            var rowTypeUsage =
                ((CollectionType)storeItemCollection
                                     .GetItems<EdmFunction>().Single(f => f.Name == "GetNames")
                                     .ReturnParameters[0].TypeUsage.EdmType).TypeUsage;

            var legacyRowTypeUsage =
                rowTypeUsage.ToLegacyStoreTypeUsage(
                    legacyStoreItemCollection.GetItems<LegacyMetadata.EdmType>().ToArray());

            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(legacyRowTypeUsage, rowTypeUsage);
        }

        /// <summary>
        ///     Type conversion relies on the fact that PrimitiveTypeKind enum members in EF6 have the
        ///     same values as they had in EF5 and therefore to convert a legacy PrimitiveTypeKind to
        ///     EF6 PrimitiveTypeKind we can the original value to int and recast it to EF6 PrimitiveTypeKind.
        ///     If for some reason this changes type conversion will stop working. This test exists to
        ///     detect this kind of change.
        /// </summary>
        [Fact]
        public void Verify_PrimitiveTypeKind_enum_values_not_changed()
        {
            foreach (var legacyPrimitiveTypeKind in typeof(LegacyMetadata.PrimitiveTypeKind).GetEnumValues())
            {
                var primitiveTypeKind = (PrimitiveTypeKind)Enum.Parse(typeof(PrimitiveTypeKind), legacyPrimitiveTypeKind.ToString());

                Assert.Equal((int)legacyPrimitiveTypeKind, (int)primitiveTypeKind);
            }
        }

        [Fact]
        private void ConcurrencyMode_facet_not_lost_when_converting_to_legacy_TypeUsage()
        {
            const string csdl =
                "<Schema Namespace='AdventureWorksModel' Alias='Self' p1:UseStrongSpatialTypes='false' xmlns:annotation='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns:p1='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns='http://schemas.microsoft.com/ado/2009/11/edm'>"
                +
                "  <EntityType Name='Customer'>" +
                "    <Key>" +
                "      <PropertyRef Name='Id' />" +
                "    </Key>" +
                "    <Property Type='String' Name='Id' Nullable='false' />" +
                "    <Property Type='String' Name='timestamp' Nullable='false' ConcurrencyMode='Fixed' />" +
                "  </EntityType>" +
                "</Schema>";

            var edmItemCollection = new EdmItemCollection(new[] { XmlReader.Create(new StringReader(csdl)) });
            var typeUsage =
                edmItemCollection
                    .GetItem<EntityType>("AdventureWorksModel.Customer")
                    .Properties.Single(p => p.Name == "timestamp")
                    .TypeUsage;
            var legacyTypeUsage = typeUsage.ToLegacyEdmTypeUsage();
            TypeUsageVerificationHelper.VerifyTypeUsagesEquivalent(legacyTypeUsage, typeUsage);
        }
    }
}
