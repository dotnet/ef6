// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services.UnitTests
{
    using System.Data.Entity.Core.Metadata;
    using System.Data.Entity.Core.Metadata.Edm;
    
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Linq;
    using Xunit;

    public sealed class EntityTypeMappingGeneratorTests
    {
        [Fact]
        public void Generate_should_add_set_mapping_and_table_and_set_clr_type()
        {
            var databaseMapping = CreateEmptyModel();
            var entityType = new EntityType
                                 {
                                     Name = "E"
                                 };
            var entitySet = databaseMapping.Model.AddEntitySet("ESet", entityType);
            var type = typeof(object);

            entityType.Annotations.SetClrType(type);

            new EntityTypeMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(entityType, databaseMapping);

            Assert.NotNull(databaseMapping.GetEntitySetMapping(entitySet));
            Assert.Same(entityType, databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Single().EntityType);
            Assert.Same(typeof(object), databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Single().GetClrType());
            Assert.NotNull(databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Single().MappingFragments.Single().Table);
        }

        [Fact]
        public void Generate_should_map_scalar_properties_to_columns()
        {
            var databaseMapping = CreateEmptyModel();
            var entityType = new EntityType
                                 {
                                     Name = "E"
                                 };
            var property = EdmProperty.Primitive("P1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property);
            var property1 = EdmProperty.Primitive("P2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var entitySet = databaseMapping.Model.AddEntitySet("ESet", entityType);
            var type = typeof(object);

            entityType.Annotations.SetClrType(type);

            new EntityTypeMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(entityType, databaseMapping);

            var entityTypeMappingFragment
                = databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Single().MappingFragments.Single();

            Assert.Equal(2, entityTypeMappingFragment.ColumnMappings.Count());
            Assert.Equal(2, entityTypeMappingFragment.Table.Properties.Count());
        }

        [Fact]
        public void Generate_should_flatten_complex_properties_to_columns()
        {
            var databaseMapping = CreateEmptyModel();
            var entityType = new EntityType
                                 {
                                     Name = "E"
                                 };
            var complexType = new ComplexType("C");

            var property = EdmProperty.Primitive("P1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            complexType.AddMember(property);
            entityType.AddComplexProperty("C1", complexType);
            var property1 = EdmProperty.Primitive("P2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var entitySet = databaseMapping.Model.AddEntitySet("ESet", entityType);
            var type = typeof(object);

            entityType.Annotations.SetClrType(type);

            new EntityTypeMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest).Generate(entityType, databaseMapping);

            var entityTypeMappingFragment
                = databaseMapping.GetEntitySetMapping(entitySet).EntityTypeMappings.Single().MappingFragments.Single();

            Assert.Equal(2, entityTypeMappingFragment.ColumnMappings.Count());
            Assert.Equal(2, entityTypeMappingFragment.Table.Properties.Count());
        }

        private static DbDatabaseMapping CreateEmptyModel()
        {
            return new DbDatabaseMapping()
                .Initialize(new EdmModel().InitializeConceptual(), new EdmModel().InitializeConceptual());
        }
    }
}
