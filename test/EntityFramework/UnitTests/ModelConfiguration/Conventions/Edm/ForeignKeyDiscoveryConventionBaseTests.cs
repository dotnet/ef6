namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class ForeignKeyDiscoveryConventionBaseTests
    {
        [Fact]
        public void Apply_should_not_discover_when_independent_constraint()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.MarkIndependent();

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_existing_constraint()
        {
            var associationType = new EdmAssociationType().Initialize();
            var associationConstraint = new EdmAssociationConstraint();

            associationType.Constraint = associationConstraint;

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Same(associationConstraint, associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_no_clear_principal()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EndKind
                = associationType.TargetEnd.EndKind
                    = EdmAssociationEndKind.Many;

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_no_key_properties()
        {
            var associationType = CreateAssociationType();

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_for_subset_key_match()
        {
            var associationType = CreateAssociationType();

            var pkProperty1 = new EdmProperty().AsPrimitive();
            var pkProperty2 = new EdmProperty().AsPrimitive();
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty1);
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty2);

            var fkProperty1 = new EdmProperty().AsPrimitive();
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty1);

            // Foo.Id1 == Bar.FooId1 && Foo.Id2 == ?
            associationType.SourceEnd.EntityType.Name = "Foo";
            pkProperty1.Name = "Id1";
            pkProperty2.Name = "Id2";
            fkProperty1.Name = "FooId1";

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_property_types_incompatible()
        {
            var associationType = CreateAssociationType();

            var pkProperty = new EdmProperty().AsPrimitive();
            pkProperty.PropertyType.EdmType = EdmPrimitiveType.Int32;
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty);

            var fkProperty = new EdmProperty().AsPrimitive();
            fkProperty.PropertyType.EdmType = EdmPrimitiveType.String;
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty);

            // Foo.Id == Bar.FooId
            associationType.SourceEnd.EntityType.Name = "Foo";
            pkProperty.Name = "Id";
            fkProperty.Name = "FooId";

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_not_discover_when_result_would_be_incorrectly_identifying()
        {
            var associationType = CreateAssociationType();

            var pkProperty = new EdmProperty().AsPrimitive();
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty);

            var fkProperty = new EdmProperty().AsPrimitive();
            associationType.TargetEnd.EntityType.DeclaredKeyProperties.Add(fkProperty);
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty);

            // Foo.Id == Bar.FooId
            associationType.SourceEnd.EntityType.Name = "Foo";
            pkProperty.Name = "Id";
            fkProperty.Name = "FooId";

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.Null(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_discover_when_fk_is_non_identifying_dependent_pk_component()
        {
            var associationType = CreateAssociationType();

            var pkProperty = new EdmProperty().AsPrimitive();
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty);

            var fkProperty = new EdmProperty().AsPrimitive();
            associationType.TargetEnd.EntityType.DeclaredKeyProperties.Add(fkProperty);
            associationType.TargetEnd.EntityType.DeclaredKeyProperties.Add(new EdmProperty().AsPrimitive());
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty);

            // Foo.Id == Bar.FooId
            associationType.SourceEnd.EntityType.Name = "Foo";
            pkProperty.Name = "Id";
            fkProperty.Name = "FooId";

            ((IEdmConvention<EdmAssociationType>)new TypeNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.NotNull(associationType.Constraint);
        }

        [Fact]
        public void Apply_should_discover_optional_dependent_when_foreign_key_in_dependent_pk()
        {
            var associationType = CreateAssociationType();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Required;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Optional;

            var pkProperty = new EdmProperty().AsPrimitive();
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty);

            var fkProperty = new EdmProperty().AsPrimitive();
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty);
            associationType.TargetEnd.EntityType.DeclaredKeyProperties.Add(fkProperty);

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
        public void Apply_should_not_discover_optional_dependent_when_foreign_key_not_in_dependent_pk()
        {
            var associationType = CreateAssociationType();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Required;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Optional;

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