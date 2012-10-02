// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Linq;
    using Xunit;

    public sealed class EntityTypeMappingGeneratorTests
    {
        [Fact]
        public void Generate_should_add_set_mapping_and_table_and_set_clr_type()
        {
            var databaseMapping = CreateEmptyModel();
            var entityType = new EdmEntityType
                                 {
                                     Name = "E"
                                 };
            var entitySet = databaseMapping.Model.AddEntitySet("ESet", entityType);
            entityType.SetClrType(typeof(object));

            new EntityTypeMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(entityType, databaseMapping);

            Assert.NotNull(databaseMapping.GetEntitySetMapping(entitySet));
            Assert.Same(entityType, databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Single().EntityType);
            Assert.Same(typeof(object), databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Single().GetClrType());
            Assert.NotNull(databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Single().TypeMappingFragments.Single().Table);
        }

        [Fact]
        public void Generate_should_map_scalar_properties_to_columns()
        {
            var databaseMapping = CreateEmptyModel();
            var entityType = new EdmEntityType
                                 {
                                     Name = "E"
                                 };
            entityType.AddPrimitiveProperty("P1").PropertyType.EdmType = EdmPrimitiveType.Int32;
            entityType.AddPrimitiveProperty("P2").PropertyType.EdmType = EdmPrimitiveType.String;
            var entitySet = databaseMapping.Model.AddEntitySet("ESet", entityType);
            entityType.SetClrType(typeof(object));

            new EntityTypeMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(entityType, databaseMapping);

            var entityTypeMappingFragment
                = databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Single().TypeMappingFragments.Single();

            Assert.Equal(2, entityTypeMappingFragment.PropertyMappings.Count());
            Assert.Equal(2, entityTypeMappingFragment.Table.Columns.Count());
        }

        [Fact]
        public void Generate_should_flatten_complex_properties_to_columns()
        {
            var databaseMapping = CreateEmptyModel();
            var entityType = new EdmEntityType
                                 {
                                     Name = "E"
                                 };
            var complexType = new EdmComplexType
                                  {
                                      Name = "C"
                                  };
            complexType.AddPrimitiveProperty("P1").PropertyType.EdmType = EdmPrimitiveType.Int32;
            entityType.AddComplexProperty("C1", complexType);
            entityType.AddPrimitiveProperty("P2").PropertyType.EdmType = EdmPrimitiveType.String;
            var entitySet = databaseMapping.Model.AddEntitySet("ESet", entityType);
            entityType.SetClrType(typeof(object));

            new EntityTypeMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(entityType, databaseMapping);

            var entityTypeMappingFragment
                = databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Single().TypeMappingFragments.Single();

            Assert.Equal(2, entityTypeMappingFragment.PropertyMappings.Count());
            Assert.Equal(2, entityTypeMappingFragment.Table.Columns.Count());
        }

        private static DbDatabaseMapping CreateEmptyModel()
        {
            return new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new DbDatabaseMetadata().Initialize());
        }
    }
}
