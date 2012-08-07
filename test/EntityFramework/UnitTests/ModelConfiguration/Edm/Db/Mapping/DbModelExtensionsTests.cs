// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Linq;
    using Xunit;

    public sealed class DbModelExtensionsTests
    {
        [Fact]
        public void GetComplexPropertyMappings_should_return_all_complex_property_mappings_for_type()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new DbDatabaseMetadata());
            var entitySet = new EdmEntitySet();
            var entitySetMapping = databaseMapping.AddEntitySetMapping(entitySet);
            var entityTypeMapping = new DbEntityTypeMapping();
            entitySetMapping.EntityTypeMappings.Add(entityTypeMapping);
            var entityTypeMappingFragment = new DbEntityTypeMappingFragment();
            entityTypeMapping.TypeMappingFragments.Add(entityTypeMappingFragment);
            var propertyMapping1 = new DbEdmPropertyMapping();
            var complexType = new EdmComplexType();
            complexType.SetClrType(typeof(object));
            propertyMapping1.PropertyPath.Add(
                new EdmProperty
                    {
                        PropertyType = new EdmTypeReference
                                           {
                                               EdmType = complexType
                                           }
                    });
            entityTypeMappingFragment.PropertyMappings.Add(propertyMapping1);
            var propertyMapping2 = new DbEdmPropertyMapping();
            propertyMapping2.PropertyPath.Add(
                new EdmProperty
                    {
                        PropertyType = new EdmTypeReference()
                    });
            propertyMapping2.PropertyPath.Add(
                new EdmProperty
                    {
                        PropertyType = new EdmTypeReference
                                           {
                                               EdmType = complexType
                                           }
                    });
            entityTypeMappingFragment.PropertyMappings.Add(propertyMapping2);

            Assert.Equal(2, databaseMapping.GetComplexPropertyMappings(typeof(object)).Count());
        }

        [Fact]
        public void GetEntitySetMappings_should_return_mappings()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new DbDatabaseMetadata());

            databaseMapping.AddAssociationSetMapping(new EdmAssociationSet());

            Assert.Equal(1, databaseMapping.GetAssociationSetMappings().Count());
        }

        [Fact]
        public void AddEntitySetMapping_should_add_mapping()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new DbDatabaseMetadata());
            var entitySet = new EdmEntitySet();

            var entitySetMapping = databaseMapping.AddEntitySetMapping(entitySet);

            Assert.NotNull(entitySetMapping);
            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count());
            Assert.Same(entitySet, entitySetMapping.EntitySet);
        }

        [Fact]
        public void Initialize_should_add_default_entity_container_mapping()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new DbDatabaseMetadata());

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Count);
        }

        [Fact]
        public void AddAssociationSetMapping_should_add_mapping()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new DbDatabaseMetadata());
            var associationSet = new EdmAssociationSet();

            var associationSetMapping = databaseMapping.AddAssociationSetMapping(associationSet);

            Assert.NotNull(associationSetMapping);
            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().AssociationSetMappings.Count());
            Assert.Same(associationSet, associationSetMapping.AssociationSet);
        }

        [Fact]
        public void GetEntityTypeMapping_should_return_mapping_for_type()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new DbDatabaseMetadata());
            var entityType = new EdmEntityType();
            var entityTypeMapping = new DbEntityTypeMapping
                                        {
                                            EntityType = entityType
                                        };
            databaseMapping.AddEntitySetMapping(new EdmEntitySet()).EntityTypeMappings.Add(entityTypeMapping);

            Assert.Same(entityTypeMapping, databaseMapping.GetEntityTypeMapping(entityType));
        }

        [Fact]
        public void GetEntityTypeMapping_should_return_mapping_for_type_by_clrType()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new DbDatabaseMetadata());
            var entityType = new EdmEntityType
                                 {
                                     Name = "Foo"
                                 };
            entityType.SetClrType(typeof(object));
            var entityTypeMapping = new DbEntityTypeMapping
                                        {
                                            EntityType = entityType
                                        };
            entityTypeMapping.SetClrType(typeof(object));
            databaseMapping.AddEntitySetMapping(new EdmEntitySet()).EntityTypeMappings.Add(entityTypeMapping);

            Assert.Same(entityTypeMapping, databaseMapping.GetEntityTypeMapping(typeof(object)));
        }

        [Fact]
        public void Can_get_and_set_mapping_for_entity_set()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel().Initialize(), new DbDatabaseMetadata());
            var entitySet = new EdmEntitySet();

            Assert.Same(databaseMapping.AddEntitySetMapping(entitySet), databaseMapping.GetEntitySetMapping(entitySet));
        }
    }
}
