// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class TypeNameForeignKeyDiscoveryConventionTests
    {
        [Fact]
        public void Apply_should_discover_simple_matching_foreign_key()
        {
            var associationType = CreateAssociationType();

            var pkProperty = new EdmProperty().AsPrimitive();
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty);

            var fkProperty = new EdmProperty().AsPrimitive();
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty);

            // Foo.Id == Bar.FooId
            associationType.SourceEnd.EntityType.Name = "Foo";
            pkProperty.Name = "Id";
            fkProperty.Name = "FooId";

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.DependentEnd);
            Assert.Equal("FooId", associationType.Constraint.DependentProperties.Single().Name);
        }

        [Fact]
        public void Apply_should_discover_simple_matching_foreign_key_with_different_casing()
        {
            var associationType = CreateAssociationType();

            var pkProperty = new EdmProperty().AsPrimitive();
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty);

            var fkProperty = new EdmProperty().AsPrimitive();
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty);

            // Foo.ID == Bar.FooId
            associationType.SourceEnd.EntityType.Name = "Foo";
            pkProperty.Name = "ID";
            fkProperty.Name = "FooId";

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.DependentEnd);
            Assert.Equal("FooId", associationType.Constraint.DependentProperties.Single().Name);
        }

        [Fact]
        public void Apply_should_discover_composite_matching_foreign_keys()
        {
            var associationType = CreateAssociationType();

            var pkProperty1 = new EdmProperty().AsPrimitive();
            var pkProperty2 = new EdmProperty().AsPrimitive();
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty1);
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty2);

            var fkProperty1 = new EdmProperty().AsPrimitive();
            var fkProperty2 = new EdmProperty().AsPrimitive();
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty1);
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty2);

            // Foo.Id1 == Bar.FooId1 && Foo.Id2 == Bar.FooId2
            associationType.SourceEnd.EntityType.Name = "Foo";
            pkProperty1.Name = "Id1";
            pkProperty2.Name = "Id2";
            fkProperty1.Name = "FooId1";
            fkProperty2.Name = "FooId2";

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.DependentEnd);
            Assert.Equal(2, associationType.Constraint.DependentProperties.Count());
        }

        [Fact]
        public void Apply_should_not_discover_when_multiple_associations_exist()
        {
            var associationType = CreateAssociationType();

            var pkProperty = new EdmProperty().AsPrimitive();
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty);

            var fkProperty = new EdmProperty().AsPrimitive();
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty);

            // Foo.Id == Bar.FooId
            associationType.SourceEnd.EntityType.Name = "Foo";
            pkProperty.Name = "Id";
            fkProperty.Name = "FooId";

            var model = new EdmModel().Initialize();
            model.Namespaces.Single().AssociationTypes.Add(associationType);
            model.Namespaces.Single().AssociationTypes.Add(associationType);

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, model);

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_property_types_are_incompatible()
        {
            var associationType = CreateAssociationType();

            var pkProperty = new EdmProperty().AsPrimitive();
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty);

            var fkProperty = new EdmProperty().AsPrimitive();
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty);

            // Foo.Id == Bar.FooId
            associationType.SourceEnd.EntityType.Name = "Foo";
            pkProperty.Name = "Id";
            fkProperty.Name = "FooId";
            pkProperty.PropertyType = new EdmTypeReference
                                          {
                                              EdmType = EdmPrimitiveType.Binary
                                          };
            fkProperty.PropertyType = new EdmTypeReference
                                          {
                                              EdmType = EdmPrimitiveType.String
                                          };

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Null(associationType.Constraint);
        }

        private static EdmAssociationType CreateAssociationType()
        {
            var associationType = new EdmAssociationType().Initialize();

            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Optional;
            associationType.SourceEnd.EntityType = new EdmEntityType();

            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Many;
            associationType.TargetEnd.EntityType = new EdmEntityType();

            return associationType;
        }
    }
}
