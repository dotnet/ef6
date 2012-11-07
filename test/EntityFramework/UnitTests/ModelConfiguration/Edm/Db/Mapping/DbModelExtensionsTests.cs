// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Linq;
    using Xunit;

    public sealed class DbModelExtensionsTests
    {
        [Fact]
        public void GetComplexPropertyMappings_should_return_all_complex_property_mappings_for_type()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new EdmModel());
            var entitySet = new EntitySet();
            var entitySetMapping = databaseMapping.AddEntitySetMapping(entitySet);
            var entityTypeMapping = new DbEntityTypeMapping();
            entitySetMapping.EntityTypeMappings.Add(entityTypeMapping);
            var entityTypeMappingFragment = new DbEntityTypeMappingFragment();
            entityTypeMapping.TypeMappingFragments.Add(entityTypeMappingFragment);
            var propertyMapping1 = new DbEdmPropertyMapping();
            var complexType = new ComplexType("C");
            var type = typeof(object);

            complexType.Annotations.SetClrType(type);
            propertyMapping1.PropertyPath.Add(EdmProperty.Complex("P1", complexType));
            entityTypeMappingFragment.PropertyMappings.Add(propertyMapping1);
            var propertyMapping2 = new DbEdmPropertyMapping();
            propertyMapping2.PropertyPath.Add(EdmProperty.Primitive("P2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));
            propertyMapping2.PropertyPath.Add(EdmProperty.Complex("P3", complexType));
            entityTypeMappingFragment.PropertyMappings.Add(propertyMapping2);

            Assert.Equal(2, databaseMapping.GetComplexPropertyMappings(typeof(object)).Count());
        }

        [Fact]
        public void GetEntitySetMappings_should_return_mappings()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new EdmModel());

            databaseMapping.AddAssociationSetMapping(new AssociationSet("AS", new AssociationType()));

            Assert.Equal(1, databaseMapping.GetAssociationSetMappings().Count());
        }

        [Fact]
        public void AddEntitySetMapping_should_add_mapping()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new EdmModel());
            var entitySet = new EntitySet();

            var entitySetMapping = databaseMapping.AddEntitySetMapping(entitySet);

            Assert.NotNull(entitySetMapping);
            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
            Assert.Same(entitySet, entitySetMapping.EntitySet);
        }

        [Fact]
        public void Initialize_should_add_default_entity_container_mapping()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new EdmModel());

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Count);
        }

        [Fact]
        public void AddAssociationSetMapping_should_add_mapping()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new EdmModel());
            var associationSet = new AssociationSet("AS", new AssociationType());

            var associationSetMapping = databaseMapping.AddAssociationSetMapping(associationSet);

            Assert.NotNull(associationSetMapping);
            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().AssociationSetMappings.Count());
            Assert.Same(associationSet, associationSetMapping.AssociationSet);
        }

        [Fact]
        public void GetEntityTypeMapping_should_return_mapping_for_type()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new EdmModel());
            var entityType = new EntityType();
            var entityTypeMapping = new DbEntityTypeMapping
                                        {
                                            EntityType = entityType
                                        };
            databaseMapping.AddEntitySetMapping(new EntitySet()).EntityTypeMappings.Add(entityTypeMapping);

            Assert.Same(entityTypeMapping, databaseMapping.GetEntityTypeMapping(entityType));
        }

        [Fact]
        public void GetEntityTypeMapping_should_return_mapping_for_type_by_clrType()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new EdmModel());
            var entityType = new EntityType
                                 {
                                     Name = "Foo"
                                 };
            var type = typeof(object);

            entityType.Annotations.SetClrType(type);
            var entityTypeMapping = new DbEntityTypeMapping
                                        {
                                            EntityType = entityType
                                        };
            entityTypeMapping.SetClrType(typeof(object));
            databaseMapping.AddEntitySetMapping(new EntitySet()).EntityTypeMappings.Add(entityTypeMapping);

            Assert.Same(entityTypeMapping, databaseMapping.GetEntityTypeMapping(typeof(object)));
        }

        [Fact]
        public void Can_get_and_set_mapping_for_entity_set()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new EdmModel());
            var entitySet = new EntitySet();

            Assert.Same(databaseMapping.AddEntitySetMapping(entitySet), databaseMapping.GetEntitySetMapping(entitySet));
        }
    }
}
