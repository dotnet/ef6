// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity;
    using System.Data.Entity.Edm;
    using System.Linq;
    using Xunit;

    public sealed class EdmModelExtensionsTests
    {
        [Fact]
        public void Can_get_and_set_provider_info_annotation()
        {
            var model = new EdmModel();
            var providerInfo = ProviderRegistry.Sql2008_ProviderInfo;

            model.SetProviderInfo(providerInfo);

            Assert.Same(providerInfo, model.GetProviderInfo());
        }

        [Fact]
        public void HasCascadeDeletePath_should_return_true_for_simple_cascade()
        {
            var model = new EdmModel().Initialize();
            var entityTypeA = model.AddEntityType("A");
            var entityTypeB = model.AddEntityType("B");
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType = entityTypeA;
            associationType.TargetEnd.EntityType = entityTypeB;
            associationType.SourceEnd.DeleteAction = EdmOperationAction.Cascade;
            model.AddAssociationType(associationType);

            Assert.True(model.HasCascadeDeletePath(entityTypeA, entityTypeB));
            Assert.False(model.HasCascadeDeletePath(entityTypeB, entityTypeA));
        }

        [Fact]
        public void HasCascadeDeletePath_should_return_true_for_transitive_cascade()
        {
            var model = new EdmModel().Initialize();
            var entityTypeA = model.AddEntityType("A");
            var entityTypeB = model.AddEntityType("B");
            var entityTypeC = model.AddEntityType("B");
            var associationTypeA = new EdmAssociationType().Initialize();
            associationTypeA.SourceEnd.EntityType = entityTypeA;
            associationTypeA.TargetEnd.EntityType = entityTypeB;
            associationTypeA.SourceEnd.DeleteAction = EdmOperationAction.Cascade;
            model.AddAssociationType(associationTypeA);
            var associationTypeB = new EdmAssociationType().Initialize();
            associationTypeB.SourceEnd.EntityType = entityTypeB;
            associationTypeB.TargetEnd.EntityType = entityTypeC;
            associationTypeB.SourceEnd.DeleteAction = EdmOperationAction.Cascade;
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
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("A");
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType
                = associationType.TargetEnd.EntityType
                  = entityType;
            associationType.SourceEnd.DeleteAction = EdmOperationAction.Cascade;
            model.AddAssociationType(associationType);

            Assert.True(model.HasCascadeDeletePath(entityType, entityType));
        }

        [Fact]
        public void GetClrTypes_should_return_ospace_types()
        {
            var model = new EdmModel().Initialize();
            model.AddEntityType("A").SetClrType(typeof(object));
            model.AddEntityType("B").SetClrType(typeof(string));

            Assert.Equal(2, model.GetClrTypes().Count());
        }

        [Fact]
        public void GetValidationErrors_should_return_validation_errors()
        {
            var model = new EdmModel().Initialize();
            model.AddEntitySet("S", new EdmEntityType());

            var validationErrors = model.GetCsdlErrors();

            Assert.Equal(1, validationErrors.Count());
        }

        [Fact]
        public void Validate_should_throw()
        {
            var model = new EdmModel().Initialize();
            model.AddEntitySet("S", new EdmEntityType());

            Assert.Throws<ModelValidationException>(() => model.ValidateCsdl());
        }

        [Fact]
        public void GetEntitySets_should_return_all_sets()
        {
            var model = new EdmModel().Initialize();
            model.AddEntitySet("S", new EdmEntityType());
            model.AddEntitySet("T", new EdmEntityType());

            Assert.Equal(2, model.GetEntitySets().Count());
        }

        [Fact]
        public void GenerateDatabaseMapping_should_return_mapping()
        {
            var model = new EdmModel().Initialize();

            Assert.NotNull(model.GenerateDatabaseMapping(ProviderRegistry.Sql2008_ProviderManifest));
        }

        [Fact]
        public void GetEntitySet_should_return_entity_set()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("Foo");
            model.AddEntitySet("FooSet", entityType);

            var entitySet = model.GetEntitySet(entityType);

            Assert.NotNull(entitySet);
            Assert.Same(entityType, entitySet.ElementType);
        }

        [Fact]
        public void GetAssociationSet_should_return_association_set()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("Foo");
            model.AddEntitySet("FooSet", entityType);
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType = entityType;
            associationType.TargetEnd.EntityType = entityType;
            model.AddAssociationSet("FooSet", associationType);

            var associationSet = model.GetAssociationSet(associationType);

            Assert.NotNull(associationSet);
            Assert.Same(associationType, associationSet.ElementType);
        }

        [Fact]
        public void GetStructuralType_should_return_entity_or_complex_type()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("E");
            var complexType = model.AddComplexType("C");

            Assert.Same(entityType, model.GetStructuralType("E"));
            Assert.Same(complexType, model.GetStructuralType("C"));
        }

        [Fact]
        public void ReplaceEntitySet_should_remove_set_with_type()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("Foo");
            model.AddEntitySet("FooSet", entityType);

            model.ReplaceEntitySet(entityType, null);

            Assert.Equal(1, model.GetEntityTypes().Count());
            Assert.Equal(0, model.Containers.First().EntitySets.Count());
        }

        [Fact]
        public void Initialize_should_create_default_container_and_namespace()
        {
            var model = new EdmModel().Initialize();

            Assert.NotNull(model.Name);
            Assert.Equal(1, model.Containers.Count);
            Assert.NotNull(model.Containers.Single().Name);
            Assert.Equal(1, model.Namespaces.Count);
            Assert.NotNull(model.Namespaces.Single().Name);
        }

        [Fact]
        public void GetAssociationsBetween_should_return_matching_associations()
        {
            var model = new EdmModel().Initialize();

            var entityTypeA = model.AddEntityType("Foo");
            var entityTypeB = model.AddEntityType("Bar");

            Assert.Equal(0, model.GetAssociationTypesBetween(entityTypeA, entityTypeB).Count());

            model.AddAssociationType(
                "Foo_Bar",
                entityTypeA, EdmAssociationEndKind.Optional,
                entityTypeB, EdmAssociationEndKind.Many);

            model.AddAssociationType(
                "Bar_Foo",
                entityTypeB, EdmAssociationEndKind.Optional,
                entityTypeA, EdmAssociationEndKind.Many);

            Assert.Equal(2, model.GetAssociationTypesBetween(entityTypeA, entityTypeB).Count());
            Assert.Equal(2, model.GetAssociationTypesBetween(entityTypeB, entityTypeA).Count());
            Assert.Equal(0, model.GetAssociationTypesBetween(entityTypeA, entityTypeA).Count());
        }

        [Fact]
        public void AddEntityType_should_create_and_add_to_default_namespace()
        {
            var model = new EdmModel().Initialize();

            var entityType = model.AddEntityType("Foo");

            Assert.NotNull(entityType);
            Assert.Equal("Foo", entityType.Name);
            Assert.True(model.Namespaces.Single().EntityTypes.Contains(entityType));
        }

        [Fact]
        public void AddComplexType_should_create_and_add_to_default_namespace()
        {
            var model = new EdmModel().Initialize();

            var complexType = model.AddComplexType("Foo");

            Assert.NotNull(complexType);
            Assert.Equal("Foo", complexType.Name);
            Assert.True(model.Namespaces.Single().ComplexTypes.Contains(complexType));
        }

        [Fact]
        public void GetEntityTypes_should_return_correct_types()
        {
            var model = new EdmModel().Initialize();

            Assert.Same(model.Namespaces.First().EntityTypes, model.GetEntityTypes());
        }

        [Fact]
        public void GetComplexTypes_should_return_correct_types()
        {
            var model = new EdmModel().Initialize();

            Assert.Same(model.Namespaces.First().ComplexTypes, model.GetComplexTypes());
        }

        [Fact]
        public void GetAssociationType_should_return_correct_type()
        {
            var model = new EdmModel().Initialize();

            var associationType = new EdmAssociationType().Initialize();
            associationType.Name = "Foo";
            model.Namespaces.Single().AssociationTypes.Add(associationType);

            Assert.Same(associationType, model.GetAssociationType("Foo"));
        }

        [Fact]
        public void GetAssociationTypes_should_return_correct_types()
        {
            var model = new EdmModel().Initialize();

            Assert.Same(model.Namespaces.First().AssociationTypes, model.GetAssociationTypes());
        }

        [Fact]
        public void GetEntityType_should_return_correct_type()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("Foo");

            var foundEntityType = model.GetEntityType("Foo");

            Assert.NotNull(foundEntityType);
            Assert.Same(entityType, foundEntityType);
        }

        [Fact]
        public void GetComplexType_should_return_correct_type()
        {
            var model = new EdmModel().Initialize();
            var complexType = model.AddComplexType("Foo");

            var foundComplexType = model.GetComplexType("Foo");

            Assert.NotNull(foundComplexType);
            Assert.Same(complexType, foundComplexType);
        }

        [Fact]
        public void AddEntitySet_should_create_and_add_to_default_container()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("Foo");

            var entitySet = model.AddEntitySet("FooSet", entityType);

            Assert.NotNull(entitySet);
            Assert.Equal("FooSet", entitySet.Name);
            Assert.Same(entityType, entitySet.ElementType);
            Assert.True(model.Containers.Single().EntitySets.Contains(entitySet));
        }

        [Fact]
        public void RemoveEntityType_should_remove_type_and_set()
        {
            var model = new EdmModel().Initialize();
            var entityType = model.AddEntityType("Foo");
            model.AddEntitySet("FooSet", entityType);

            model.RemoveEntityType(entityType);

            Assert.Equal(0, model.GetEntityTypes().Count());
            Assert.Equal(0, model.Containers.First().EntitySets.Count());
        }

        [Fact]
        public void RemoveAssociationType_should_remove_type_and_set()
        {
            var model = new EdmModel().Initialize();
            var associationType = model.AddAssociationType(
                "A",
                new EdmEntityType(), EdmAssociationEndKind.Optional,
                new EdmEntityType(), EdmAssociationEndKind.Many);
            model.AddAssociationSet("FooSet", associationType);

            model.RemoveAssociationType(associationType);

            Assert.Equal(0, model.GetAssociationTypes().Count());
            Assert.Equal(0, model.Containers.First().AssociationSets.Count());
        }

        [Fact]
        public void AddAssociationType_should_create_and_add_to_default_namespace()
        {
            var model = new EdmModel().Initialize();
            var sourceEntityType = model.AddEntityType("Source");
            var targetEntityType = model.AddEntityType("Target");

            var associationType = model.AddAssociationType(
                "Foo",
                sourceEntityType, EdmAssociationEndKind.Required,
                targetEntityType, EdmAssociationEndKind.Many);

            Assert.NotNull(associationType);
            Assert.Equal("Foo", associationType.Name);
            Assert.Same(sourceEntityType, associationType.SourceEnd.EntityType);
            Assert.Equal(EdmAssociationEndKind.Required, associationType.SourceEnd.EndKind);
            Assert.Same(targetEntityType, associationType.TargetEnd.EntityType);
            Assert.Equal(EdmAssociationEndKind.Many, associationType.TargetEnd.EndKind);
            Assert.True(model.Namespaces.Single().AssociationTypes.Contains(associationType));
        }

        [Fact]
        public void AddAssociationSet_should_create_and_add_to_default_container()
        {
            var model = new EdmModel().Initialize();
            var sourceEntityType = model.AddEntityType("Source");
            var targetEntityType = model.AddEntityType("Target");
            var associationType = model.AddAssociationType(
                "Foo",
                sourceEntityType, EdmAssociationEndKind.Required,
                targetEntityType, EdmAssociationEndKind.Many);

            var associationSet = model.AddAssociationSet("FooSet", associationType);

            Assert.NotNull(associationSet);
            Assert.Equal("FooSet", associationSet.Name);
            Assert.Same(associationType, associationSet.ElementType);

            Assert.True(model.Containers.Single().AssociationSets.Contains(associationSet));
        }

        [Fact]
        public void GetDerivedTypes_must_return_list_of_direct_descendants()
        {
            var model = new EdmModel().Initialize();
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
