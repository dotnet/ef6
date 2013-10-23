// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using Moq;
    using Xunit;

    public class EntitySetDefiningQueryConverterTests
    {
        private static readonly DbProviderManifest ProviderManifest =
            SqlProviderServices.Instance.GetProviderManifest("2008");

        private const string StoreSchemaAttributeNamespace = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator";

        [Fact]
        public void CreateTransientMetadataWorkspace_creates_workspace_with_provided_store_entity_sets()
        {
            var property =
                EdmProperty.CreatePrimitive(
                    "Id",
                    ProviderManifest.GetStoreTypes().Single(t => t.PrimitiveTypeKind == PrimitiveTypeKind.Int32));
            property.Nullable = false;

            var entityType =
                EntityType.Create("EntityType", "MyModel", DataSpace.SSpace, new[] { "Id" }, new[] { property }, null);

            var entitySet = EntitySet.Create("EntityTypeSet", "dbo", "EntityTypes", null, entityType, null);

            var workspace =
                EntitySetDefiningQueryConverter.CreateTransientMetadataWorkspace(
                    new List<EntitySet> { entitySet },
                    EntityFrameworkVersion.Version3,
                    "System.Data.SqlClient", "2008", ProviderManifest);

            Assert.NotNull(workspace);
            var storeItemCollection = (StoreItemCollection)workspace.GetItemCollection(DataSpace.SSpace);
            Assert.NotNull(storeItemCollection);
            Assert.Equal(1, storeItemCollection.GetEntityContainer("StoreModelContainer").EntitySets.Count);
            Assert.Equal(
                "EntityTypeSet",
                storeItemCollection.GetEntityContainer("StoreModelContainer").EntitySets.Single().Name);
            Assert.Equal(1, storeItemCollection.GetItems<EntityType>().Count);
            Assert.Equal("EntityType", storeItemCollection.GetItems<EntityType>().Single().Name);
            Assert.NotNull(workspace.GetItemCollection(DataSpace.CSpace));
            Assert.NotNull(workspace.GetItemCollection(DataSpace.CSSpace));
        }

        [Fact]
        public void CreateDefiningQuery_creates_query_for_entity_set()
        {
            var property =
                EdmProperty.CreatePrimitive(
                    "Id",
                    ProviderManifest.GetStoreTypes().Single(t => t.PrimitiveTypeKind == PrimitiveTypeKind.Int32));
            property.Nullable = false;

            var entityType =
                EntityType.Create("EntityType", "MyModel", DataSpace.SSpace, new[] { "Id" }, new[] { property }, null);

            var entitySet = EntitySet.Create("EntityTypeSet", "dbo", "EntityTypes", null, entityType, null);

            var workspace =
                EntitySetDefiningQueryConverter.CreateTransientMetadataWorkspace(
                    new List<EntitySet> { entitySet },
                    EntityFrameworkVersion.Version3,
                    "System.Data.SqlClient", "2008", ProviderManifest);

            Assert.NotNull(EntitySetDefiningQueryConverter.CreateDefiningQuery(entitySet, workspace, SqlProviderServices.Instance));
        }

        [Fact]
        public void CloneWithDefiningQuery_creates_new_equivalent_entity_set_but_with_defining_query()
        {
            var property =
                EdmProperty.CreatePrimitive(
                    "Id",
                    ProviderManifest.GetStoreTypes().Single(t => t.PrimitiveTypeKind == PrimitiveTypeKind.Int32));

            var customMetadataProperty =
                MetadataProperty.Create(
                    "http://tempUri:myProperty",
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    "value");

            var entityType =
                EntityType.Create("EntityType", "MyModel", DataSpace.SSpace, new[] { "Id" }, new[] { property }, null);

            var entitySet = EntitySet.Create("EntityTypeSet", "dbo", "EntityTypes", null, entityType, new[] { customMetadataProperty });

            var clonedEntitySet = EntitySetDefiningQueryConverter.CloneWithDefiningQuery(entitySet, "definingQuery");

            Assert.NotNull(clonedEntitySet);
            Assert.NotSame(clonedEntitySet, entitySet);
            Assert.Same(entitySet.Name, clonedEntitySet.Name);
            Assert.Same(entitySet.ElementType, clonedEntitySet.ElementType);
            Assert.Null(clonedEntitySet.Schema);
            Assert.Null(clonedEntitySet.Table);
            Assert.Equal(
                entitySet.Schema,
                clonedEntitySet.MetadataProperties.Single(p => p.Name == StoreSchemaAttributeNamespace + ":Schema")
                    .Value);
            Assert.Equal(
                entitySet.Table,
                clonedEntitySet.MetadataProperties.Single(p => p.Name == StoreSchemaAttributeNamespace + ":Name")
                    .Value);

            Assert.Equal(
                entitySet.MetadataProperties.Single(p => p.Name == "http://tempUri:myProperty").Value,
                clonedEntitySet.MetadataProperties.Single(p => p.Name == "http://tempUri:myProperty").Value);
        }

        [Fact]
        public void CloneWithDefiningQuery_does_not_creat_schema_and_table_extended_attributes_if_they_are_null()
        {
            var property =
                EdmProperty.CreatePrimitive(
                    "Id",
                    ProviderManifest.GetStoreTypes().Single(t => t.PrimitiveTypeKind == PrimitiveTypeKind.Int32));

            var customMetadataProperty =
                MetadataProperty.Create(
                    "http://tempUri:myProperty",
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    "value");

            var entityType =
                EntityType.Create("EntityType", "MyModel", DataSpace.SSpace, new[] { "Id" }, new[] { property }, null);

            var entitySet = EntitySet.Create("EntityTypeSet", null, null, null, entityType, new[] { customMetadataProperty });

            var clonedEntitySet = EntitySetDefiningQueryConverter.CloneWithDefiningQuery(entitySet, "definingQuery");

            Assert.Null(clonedEntitySet.Schema);
            Assert.Null(clonedEntitySet.Table);
            Assert.False(entitySet.MetadataProperties.Any(p => p.Name.EndsWith(StoreSchemaAttributeNamespace + ":Schema")));
            Assert.False(entitySet.MetadataProperties.Any(p => p.Name.EndsWith(StoreSchemaAttributeNamespace + ":Name")));
        }

        [Fact]
        public void Convert_can_convert_entitysets_without_defining_queries_to_entitysets_with_defining_queries()
        {
            var property1 =
                EdmProperty.CreatePrimitive(
                    "Id",
                    ProviderManifest.GetStoreTypes().Single(t => t.PrimitiveTypeKind == PrimitiveTypeKind.Int32));
            property1.Nullable = false;

            var entityType1 =
                EntityType.Create("EntityType1", "MyModel", DataSpace.SSpace, new[] { "Id" }, new[] { property1 }, null);

            var property2 =
                EdmProperty.CreatePrimitive(
                    "Id",
                    ProviderManifest.GetStoreTypes().Single(t => t.PrimitiveTypeKind == PrimitiveTypeKind.Int32));
            property2.Nullable = false;

            var entityType2 =
                EntityType.Create("EntityType2", "MyModel", DataSpace.SSpace, new[] { "Id" }, new[] { property2 }, null);

            var entitySets =
                new List<EntitySet>
                    {
                        EntitySet.Create("EntityType1Set", "dbo", "EntityTypes1", null, entityType1, null),
                        EntitySet.Create("EntityType2Set", "dbo", "EntityTypes2", null, entityType2, null)
                    };

            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(
                r => r.GetService(
                    It.Is<Type>(t => t == typeof(DbProviderServices)),
                    It.IsAny<string>())).Returns(SqlProviderServices.Instance);

            var convertedEntitySets =
                EntitySetDefiningQueryConverter.Convert(
                    entitySets,
                    EntityFrameworkVersion.Version3,
                    "System.Data.SqlClient",
                    "2008",
                    mockResolver.Object).ToList();

            Assert.NotNull(convertedEntitySets);
            Assert.Equal(entitySets.Select(e => e.Name), convertedEntitySets.Select(e => e.Name));
            Assert.True(convertedEntitySets.All(e => e.DefiningQuery != null));
        }
    }
}
