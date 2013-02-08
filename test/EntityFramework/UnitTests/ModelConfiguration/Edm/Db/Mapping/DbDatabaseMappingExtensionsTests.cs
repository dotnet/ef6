// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping.UnitTests
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Linq;
    using Xunit;

    public sealed class DbDatabaseMappingExtensionsTests
    {
        [Fact]
        public void GetComplexPropertyMappings_should_return_all_complex_property_mappings_for_type()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));
            var entitySet = new EntitySet
                                {
                                    Name = "ES"
                                };
            var entitySetMapping = databaseMapping.AddEntitySetMapping(entitySet);
            var entityTypeMapping = new StorageEntityTypeMapping(null);
            entitySetMapping.AddTypeMapping(entityTypeMapping);
            var entityTypeMappingFragment = new StorageMappingFragment(entitySet, entityTypeMapping, false);
            entityTypeMapping.AddFragment(entityTypeMappingFragment);
            var complexType = new ComplexType("C");
            var propertyMapping1
                = new ColumnMappingBuilder(
                    new EdmProperty("C"),
                    new[]
                        {
                            EdmProperty.Complex("P1", complexType),
                            EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                        });
            var type = typeof(object);

            complexType.Annotations.SetClrType(type);

            entityTypeMappingFragment.AddColumnMapping(propertyMapping1);

            var propertyMapping2
                = new ColumnMappingBuilder(
                    new EdmProperty("C"),
                    new List<EdmProperty>
                        {
                            EdmProperty.Complex("P3", complexType),
                            EdmProperty.Primitive(
                                "P2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                        });
            entityTypeMappingFragment.AddColumnMapping(propertyMapping2);

            Assert.Equal(2, databaseMapping.GetComplexPropertyMappings(typeof(object)).Count());
        }

        [Fact]
        public void GetEntitySetMappings_should_return_mappings()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));

            databaseMapping.AddAssociationSetMapping(new AssociationSet("AS", new AssociationType()), new EntitySet());

            Assert.Equal(1, databaseMapping.GetAssociationSetMappings().Count());
        }

        [Fact]
        public void AddEntitySetMapping_should_add_mapping()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));
            var entitySet = new EntitySet
                                {
                                    Name = "ES"
                                };

            var entitySetMapping = databaseMapping.AddEntitySetMapping(entitySet);

            Assert.NotNull(entitySetMapping);
            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
            Assert.Same(entitySet, entitySetMapping.EntitySet);
        }

        [Fact]
        public void Initialize_should_add_default_entity_container_mapping()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Count);
        }

        [Fact]
        public void AddAssociationSetMapping_should_add_mapping()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));
            var associationSet = new AssociationSet("AS", new AssociationType());

            var associationSetMapping = databaseMapping.AddAssociationSetMapping(associationSet, new EntitySet());

            Assert.NotNull(associationSetMapping);
            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().AssociationSetMappings.Count());
            Assert.Same(associationSet, associationSetMapping.AssociationSet);
        }

        [Fact]
        public void GetEntityTypeMapping_should_return_mapping_for_type()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));
            var entityType = new EntityType();
            var entityTypeMapping = new StorageEntityTypeMapping(null);
            entityTypeMapping.AddType(entityType);
            databaseMapping.AddEntitySetMapping(
                new EntitySet
                    {
                        Name = "ES"
                    }).AddTypeMapping(entityTypeMapping);

            Assert.Same(entityTypeMapping, databaseMapping.GetEntityTypeMapping(entityType));
        }

        [Fact]
        public void GetEntityTypeMapping_should_return_mapping_for_type_by_clrType()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));
            var entityType = new EntityType
                                 {
                                     Name = "Foo"
                                 };
            var type = typeof(object);

            entityType.Annotations.SetClrType(type);
            var entityTypeMapping = new StorageEntityTypeMapping(null);
            entityTypeMapping.AddType(entityType);
            entityTypeMapping.SetClrType(typeof(object));
            databaseMapping.AddEntitySetMapping(
                new EntitySet
                    {
                        Name = "ES"
                    }).AddTypeMapping(entityTypeMapping);

            Assert.Same(entityTypeMapping, databaseMapping.GetEntityTypeMapping(typeof(object)));
        }

        [Fact]
        public void Can_get_and_set_mapping_for_entity_set()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));
            var entitySet = new EntitySet
                                {
                                    Name = "ES"
                                };

            Assert.Same(databaseMapping.AddEntitySetMapping(entitySet), databaseMapping.GetEntitySetMapping(entitySet));
        }
    }
}
