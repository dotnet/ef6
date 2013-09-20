// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
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
            var entityTypeMapping = new EntityTypeMapping(null);
            entitySetMapping.AddTypeMapping(entityTypeMapping);
            var entityTypeMappingFragment = new MappingFragment(entitySet, entityTypeMapping, false);
            entityTypeMapping.AddFragment(entityTypeMappingFragment);
            var complexType = new ComplexType("C");
            var propertyMapping1
                = new ColumnMappingBuilder(
                    new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })),
                    new[]
                        {
                            EdmProperty.CreateComplex("P1", complexType),
                            EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                        });
            var type = typeof(object);

            complexType.Annotations.SetClrType(type);

            entityTypeMappingFragment.AddColumnMapping(propertyMapping1);

            var propertyMapping2
                = new ColumnMappingBuilder(
                    new EdmProperty("C", TypeUsage.Create(new PrimitiveType() { DataSpace = DataSpace.SSpace })),
                    new List<EdmProperty>
                        {
                            EdmProperty.CreateComplex("P3", complexType),
                            EdmProperty.CreatePrimitive(
                                "P2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                        });
            entityTypeMappingFragment.AddColumnMapping(propertyMapping2);

            Assert.Equal(2, databaseMapping.GetComplexPropertyMappings(typeof(object)).Count());
        }

        [Fact]
        public void GetComplexParameterBindings_should_return_all_complex_parameter_bindings_for_type()
        {
            var databaseMapping
                = new DbDatabaseMapping()
                    .Initialize(
                        new EdmModel(DataSpace.CSpace),
                        new EdmModel(DataSpace.SSpace));

            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = databaseMapping.Model.AddEntitySet("ES", entityType);
            var entitySetMapping = databaseMapping.AddEntitySetMapping(entitySet);

            var complexType1 = new ComplexType();
            complexType1.Annotations.SetClrType(typeof(string));

            var complexType2 = new ComplexType();
            complexType2.Annotations.SetClrType(typeof(object));

            var storageModificationFunctionMapping
                = new ModificationFunctionMapping(
                    entitySet,
                    entityType,
                    new EdmFunction("F", "N", DataSpace.SSpace),
                    new[]
                        {
                            new ModificationFunctionParameterBinding(
                                new FunctionParameter(),
                                new ModificationFunctionMemberPath(
                                new EdmMember[]
                                    {
                                        EdmProperty.CreateComplex("C1", complexType1),
                                        EdmProperty.CreateComplex("C2", complexType2),
                                        new EdmProperty("M")
                                    },
                                null),
                                true
                                )
                        },
                    null,
                    null);

            entitySetMapping.AddModificationFunctionMapping(
                new EntityTypeModificationFunctionMapping(
                    entityType,
                    storageModificationFunctionMapping,
                    storageModificationFunctionMapping,
                    storageModificationFunctionMapping));

            Assert.Equal(3, databaseMapping.GetComplexParameterBindings(typeof(string)).Count());
            Assert.Equal(3, databaseMapping.GetComplexParameterBindings(typeof(object)).Count());
        }

        [Fact]
        public void GetEntitySetMappings_should_return_mappings()
        {
            var databaseMapping = new DbDatabaseMapping()
                .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));

            databaseMapping.AddAssociationSetMapping(
                new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)), new EntitySet());

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
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

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
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entityTypeMapping = new EntityTypeMapping(null);
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
            var entityType = new EntityType("Foo", "N", DataSpace.CSpace);
            var type = typeof(object);

            entityType.Annotations.SetClrType(type);
            var entityTypeMapping = new EntityTypeMapping(null);
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
