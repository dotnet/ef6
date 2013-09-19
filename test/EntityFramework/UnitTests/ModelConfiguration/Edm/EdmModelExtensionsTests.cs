// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public sealed class EdmModelExtensionsTests
    {
        [Fact]
        public void HasCascadeDeletePath_should_return_true_for_simple_cascade()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityTypeA = model.AddEntityType("A");
            var entityTypeB = model.AddEntityType("B");
            var associationType
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
                      {
                          SourceEnd = new AssociationEndMember("S", entityTypeA),
                          TargetEnd = new AssociationEndMember("T", entityTypeB)
                      };
            associationType.SourceEnd.DeleteBehavior = OperationAction.Cascade;
            model.AddAssociationType(associationType);

            Assert.True(model.HasCascadeDeletePath(entityTypeA, entityTypeB));
            Assert.False(model.HasCascadeDeletePath(entityTypeB, entityTypeA));
        }

        [Fact]
        public void HasCascadeDeletePath_should_return_true_for_transitive_cascade()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityTypeA = model.AddEntityType("A");
            var entityTypeB = model.AddEntityType("B");
            var entityTypeC = model.AddEntityType("B");
            var associationTypeA
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
                      {
                          SourceEnd = new AssociationEndMember("S", entityTypeA),
                          TargetEnd = new AssociationEndMember("T", entityTypeB)
                      };

            associationTypeA.SourceEnd.DeleteBehavior = OperationAction.Cascade;
            model.AddAssociationType(associationTypeA);
            var associationTypeB
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
                      {
                          SourceEnd = new AssociationEndMember("S", entityTypeB),
                          TargetEnd = new AssociationEndMember("T", entityTypeC)
                      };

            associationTypeB.SourceEnd.DeleteBehavior = OperationAction.Cascade;
            model.AddAssociationType(associationTypeB);

            Assert.True(model.HasCascadeDeletePath(entityTypeA, entityTypeB));
            Assert.True(model.HasCascadeDeletePath(entityTypeB, entityTypeC));
            Assert.True(model.HasCascadeDeletePath(entityTypeA, entityTypeC));
            Assert.False(model.HasCascadeDeletePath(entityTypeB, entityTypeA));
            Assert.False(model.HasCascadeDeletePath(entityTypeC, entityTypeB));
            Assert.False(model.HasCascadeDeletePath(entityTypeC, entityTypeA));
        }

        [Fact]
        public void HasCascadeDeletePath_should_return_true_for_self_ref_cascade()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("A");
            var associationType
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
                      {
                          SourceEnd = new AssociationEndMember("S", entityType)
                      };
            associationType.TargetEnd = new AssociationEndMember("T", associationType.SourceEnd.GetEntityType());

            associationType.SourceEnd.DeleteBehavior = OperationAction.Cascade;
            model.AddAssociationType(associationType);

            Assert.True(model.HasCascadeDeletePath(entityType, entityType));
        }

        [Fact]
        public void GetClrTypes_should_return_ospace_types()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var type1 = typeof(object);
            var tempQualifier1 = model.AddEntityType("A");

            tempQualifier1.Annotations.SetClrType(type1);
            var type = typeof(string);

            model.AddEntityType("B").Annotations.SetClrType(type);

            Assert.Equal(2, model.GetClrTypes().Count());
        }

        [Fact]
        public void Validate_should_throw()
        {
            var model = new EdmModel(DataSpace.CSpace);
            model.AddEntitySet("S", new EntityType("E", "N", DataSpace.CSpace));

            Assert.Throws<ModelValidationException>(() => model.Validate());
        }

        [Fact]
        public void GetEntitySets_should_return_all_sets()
        {
            var model = new EdmModel(DataSpace.CSpace);
            model.AddEntitySet("S", new EntityType("E", "N", DataSpace.CSpace));
            model.AddEntitySet("T", new EntityType("E", "N", DataSpace.CSpace));

            Assert.Equal(2, model.GetEntitySets().Count());
        }

        [Fact]
        public void GenerateDatabaseMapping_should_return_mapping()
        {
            var model = new EdmModel(DataSpace.CSpace);

            Assert.NotNull(model.GenerateDatabaseMapping(ProviderRegistry.Sql2008_ProviderInfo, ProviderRegistry.Sql2008_ProviderManifest));
        }

        [Fact]
        public void GetEntitySet_should_return_entity_set()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("Foo");
            model.AddEntitySet("FooSet", entityType);

            var entitySet = model.GetEntitySet(entityType);

            Assert.NotNull(entitySet);
            Assert.Same(entityType, entitySet.ElementType);
        }

        [Fact]
        public void GetAssociationSet_should_return_association_set()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("Foo");
            model.AddEntitySet("FooESet", entityType);
            var associationType
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
                      {
                          SourceEnd = new AssociationEndMember("S", entityType),
                          TargetEnd = new AssociationEndMember("T", entityType)
                      };

            model.AddAssociationSet("FooSet", associationType);

            var associationSet = model.GetAssociationSet(associationType);

            Assert.NotNull(associationSet);
            Assert.Same(associationType, associationSet.ElementType);
        }

        [Fact]
        public void GetStructuralType_should_return_entity_or_complex_type()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("E");
            var complexType = model.AddComplexType("C");

            Assert.Same(entityType, model.GetStructuralType("E"));
            Assert.Same(complexType, model.GetStructuralType("C"));
        }

        [Fact]
        public void ReplaceEntitySet_should_remove_set_with_type()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("Foo");
            model.AddEntitySet("FooSet", entityType);

            model.ReplaceEntitySet(entityType, null);

            Assert.Equal(1, model.EntityTypes.Count());
            Assert.Equal(0, model.Containers.First().EntitySets.Count());
        }

        [Fact]
        public void Initialize_should_create_default_container_and_namespace()
        {
            var model = new EdmModel(DataSpace.CSpace);

            Assert.Equal(1, model.Containers.Count());
            Assert.NotNull(model.Containers.Single().Name);
        }

        [Fact]
        public void GetAssociationsBetween_should_return_matching_associations()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var entityTypeA = model.AddEntityType("Foo");
            var entityTypeB = model.AddEntityType("Bar");

            Assert.Equal(0, model.GetAssociationTypesBetween(entityTypeA, entityTypeB).Count());

            model.AddAssociationType(
                "Foo_Bar",
                entityTypeA, RelationshipMultiplicity.ZeroOrOne,
                entityTypeB, RelationshipMultiplicity.Many);

            model.AddAssociationType(
                "Bar_Foo",
                entityTypeB, RelationshipMultiplicity.ZeroOrOne,
                entityTypeA, RelationshipMultiplicity.Many);

            Assert.Equal(2, model.GetAssociationTypesBetween(entityTypeA, entityTypeB).Count());
            Assert.Equal(2, model.GetAssociationTypesBetween(entityTypeB, entityTypeA).Count());
            Assert.Equal(0, model.GetAssociationTypesBetween(entityTypeA, entityTypeA).Count());
        }

        [Fact]
        public void AddEntityType_should_create_and_add_with_default_namespace()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var entityType = model.AddEntityType("Foo");

            Assert.NotNull(entityType);
            Assert.Equal("Foo", entityType.Name);
            Assert.Equal(EdmModelExtensions.DefaultModelNamespace, entityType.NamespaceName);
            Assert.True(model.EntityTypes.Contains(entityType));
        }

        [Fact]
        public void AddEntityType_should_create_and_add_when_custom_namespace()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var entityType = model.AddEntityType("Foo", "Bar");

            Assert.NotNull(entityType);
            Assert.Equal("Foo", entityType.Name);
            Assert.Equal("Bar", entityType.NamespaceName);
            Assert.True(model.EntityTypes.Contains(entityType));
        }

        [Fact]
        public void AddEnumType_should_create_and_add_with_default_namespace()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var enumType = model.AddEnumType("Foo");

            Assert.NotNull(enumType);
            Assert.Equal("Foo", enumType.Name);
            Assert.Equal(EdmModelExtensions.DefaultModelNamespace, enumType.NamespaceName);
            Assert.True(model.EnumTypes.Contains(enumType));
        }

        [Fact]
        public void AddEnumType_should_create_and_add_when_custom_namespace()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var enumType = model.AddEnumType("Foo", "Bar");

            Assert.NotNull(enumType);
            Assert.Equal("Foo", enumType.Name);
            Assert.Equal("Bar", enumType.NamespaceName);
            Assert.True(model.EnumTypes.Contains(enumType));
        }

        [Fact]
        public void AddComplexType_should_create_and_add_with_default_namespace()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var complexType = model.AddComplexType("Foo");

            Assert.NotNull(complexType);
            Assert.Equal("Foo", complexType.Name);
            Assert.Equal(EdmModelExtensions.DefaultModelNamespace, complexType.NamespaceName);
            Assert.True(model.ComplexTypes.Contains(complexType));
        }

        [Fact]
        public void AddComplexType_should_create_and_add_when_custom_namespace()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var complexType = model.AddComplexType("Foo", "Bar");

            Assert.NotNull(complexType);
            Assert.Equal("Foo", complexType.Name);
            Assert.Equal("Bar", complexType.NamespaceName);
            Assert.True(model.ComplexTypes.Contains(complexType));
        }

        [Fact]
        public void GetEntityTypes_should_return_correct_types()
        {
            var model = new EdmModel(DataSpace.CSpace);

            Assert.Same(model.EntityTypes, model.EntityTypes);
        }

        [Fact]
        public void GetComplexTypes_should_return_correct_types()
        {
            var model = new EdmModel(DataSpace.CSpace);

            Assert.Same(model.ComplexTypes, model.ComplexTypes);
        }

        [Fact]
        public void GetAssociationType_should_return_correct_type()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var associationType
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
                      {
                          SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace)),
                          TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace)),
                          Name = "Foo"
                      };
            model.AddItem(associationType);

            Assert.Same(associationType, model.GetAssociationType("Foo"));
        }

        [Fact]
        public void GetAssociationTypes_should_return_correct_types()
        {
            var model = new EdmModel(DataSpace.CSpace);

            Assert.Same(model.AssociationTypes, model.AssociationTypes);
        }

        [Fact]
        public void GetEntityType_should_return_correct_type()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("Foo");

            var foundEntityType = model.GetEntityType("Foo");

            Assert.NotNull(foundEntityType);
            Assert.Same(entityType, foundEntityType);
        }

        [Fact]
        public void GetComplexType_should_return_correct_type()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var complexType = model.AddComplexType("Foo");

            var foundComplexType = model.GetComplexType("Foo");

            Assert.NotNull(foundComplexType);
            Assert.Same(complexType, foundComplexType);
        }

        [Fact]
        public void GetStructuralOrEnumType_should_return_correct_type_for_EntityType()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("Foo");

            Assert.Same(entityType, model.GetStructuralOrEnumType("Foo"));
        }

        [Fact]
        public void GetStructuralOrEnumType_should_return_correct_type_for_ComplexType()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var complexType = model.AddComplexType("Foo");

            Assert.Same(complexType, model.GetStructuralOrEnumType("Foo"));
        }

        [Fact]
        public void GetStructuralOrEnumType_should_return_correct_type_for_EnumType()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var enumType = model.AddEnumType("Foo");

            Assert.Same(enumType, model.GetStructuralOrEnumType("Foo"));
        }

        [Fact]
        public void AddEntitySet_should_create_and_add_to_default_container()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("Foo");

            var entitySet = model.AddEntitySet("FooSet", entityType);

            Assert.NotNull(entitySet);
            Assert.Equal("FooSet", entitySet.Name);
            Assert.Same(entityType, entitySet.ElementType);
            Assert.True(model.Containers.Single().EntitySets.Contains(entitySet));
        }

        [Fact]
        public void AddAssociationSet_should_create_and_add_to_default_container_explicit_overload()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            model.AddAssociationSet(associationSet);

            Assert.True(model.Containers.Single().AssociationSets.Contains(associationSet));
        }

        [Fact]
        public void RemoveEntityType_should_remove_type_and_set()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = model.AddEntityType("Foo");
            model.AddEntitySet("FooSet", entityType);

            model.RemoveEntityType(entityType);

            Assert.Equal(0, model.EntityTypes.Count());
            Assert.Equal(0, model.Containers.First().EntitySets.Count());
        }

        [Fact]
        public void RemoveAssociationType_should_remove_type_and_set()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var sourceEntityType = new EntityType("E", "N", DataSpace.CSpace);
            var targetEntityType = new EntityType("E", "N", DataSpace.CSpace);

            model.AddEntitySet("S", sourceEntityType);
            model.AddEntitySet("T", targetEntityType);

            var associationType
                = model.AddAssociationType(
                    "A",
                    sourceEntityType, RelationshipMultiplicity.ZeroOrOne,
                    targetEntityType, RelationshipMultiplicity.Many);

            model.AddAssociationSet("FooSet", associationType);

            model.RemoveAssociationType(associationType);

            Assert.Equal(0, model.AssociationTypes.Count());
            Assert.Equal(0, model.Containers.First().AssociationSets.Count());
        }

        [Fact]
        public void AddAssociationType_should_create_and_add_with_default_namespace()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var sourceEntityType = model.AddEntityType("Source");
            var targetEntityType = model.AddEntityType("Target");

            var associationType = model.AddAssociationType(
                "Foo",
                sourceEntityType, RelationshipMultiplicity.One,
                targetEntityType, RelationshipMultiplicity.Many);

            Assert.NotNull(associationType);
            Assert.Equal("Foo", associationType.Name);
            Assert.Equal(EdmModelExtensions.DefaultModelNamespace, associationType.NamespaceName);
            Assert.Same(sourceEntityType, associationType.SourceEnd.GetEntityType());
            Assert.Equal(RelationshipMultiplicity.One, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Same(targetEntityType, associationType.TargetEnd.GetEntityType());
            Assert.Equal(RelationshipMultiplicity.Many, associationType.TargetEnd.RelationshipMultiplicity);
            Assert.True(model.AssociationTypes.Contains(associationType));
        }

        [Fact]
        public void AddAssociationType_should_create_and_add_with_custom_namespace()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var sourceEntityType = model.AddEntityType("Source");
            var targetEntityType = model.AddEntityType("Target");

            var associationType = model.AddAssociationType(
                "Foo",
                sourceEntityType, RelationshipMultiplicity.One,
                targetEntityType, RelationshipMultiplicity.Many,
                "Bar");

            Assert.NotNull(associationType);
            Assert.Equal("Foo", associationType.Name);
            Assert.Equal("Bar", associationType.NamespaceName);
            Assert.Same(sourceEntityType, associationType.SourceEnd.GetEntityType());
            Assert.Equal(RelationshipMultiplicity.One, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Same(targetEntityType, associationType.TargetEnd.GetEntityType());
            Assert.Equal(RelationshipMultiplicity.Many, associationType.TargetEnd.RelationshipMultiplicity);
            Assert.True(model.AssociationTypes.Contains(associationType));
        }

        [Fact]
        public void AddAssociationSet_should_create_and_add_to_default_container()
        {
            var model = new EdmModel(DataSpace.CSpace);

            var sourceEntityType = model.AddEntityType("Source");
            var targetEntityType = model.AddEntityType("Target");

            model.AddEntitySet("S", sourceEntityType);
            model.AddEntitySet("T", targetEntityType);

            var associationType = model.AddAssociationType(
                "Foo",
                sourceEntityType, RelationshipMultiplicity.One,
                targetEntityType, RelationshipMultiplicity.Many);

            var associationSet = model.AddAssociationSet("FooSet", associationType);

            Assert.NotNull(associationSet);
            Assert.Equal("FooSet", associationSet.Name);
            Assert.Same(associationType, associationSet.ElementType);

            Assert.True(model.Containers.Single().AssociationSets.Contains(associationSet));
        }

        [Fact]
        public void GetDerivedTypes_must_return_list_of_direct_descendants()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entity1 = model.AddEntityType("E1");
            var entity2 = model.AddEntityType("E2");
            var entity3 = model.AddEntityType("E3");
            var entity4 = model.AddEntityType("E4");
            entity2.BaseType = entity1;
            entity3.BaseType = entity1;
            entity4.BaseType = entity2;

            var derivedTypes = model.GetDerivedTypes(entity1).ToList();

            Assert.Equal(2, derivedTypes.Count);
            Assert.Same(entity2, derivedTypes[0]);
            Assert.Same(entity3, derivedTypes[1]);
        }
    }
}
