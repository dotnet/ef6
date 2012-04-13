namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class PrimaryKeyNameForeignKeyDiscoveryConventionTests
    {
        [Fact]
        public void Apply_should_discover_simple_matching_foreign_key()
        {
            var associationType = CreateAssociationType();

            var pkProperty = new EdmProperty().AsPrimitive();
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty);

            var fkProperty = new EdmProperty().AsPrimitive();
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty);

            // Foo.PId == Bar.PId
            pkProperty.Name = "PId";
            fkProperty.Name = "PId";

            ((IEdmConvention<EdmAssociationType>)new PrimaryKeyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.DependentEnd);
            Assert.Equal("PId", associationType.Constraint.DependentProperties.Single().Name);
        }

        [Fact]
        public void Apply_should_discover_simple_matching_foreign_key_with_different_casing()
        {
            var associationType = CreateAssociationType();

            var pkProperty = new EdmProperty().AsPrimitive();
            associationType.SourceEnd.EntityType.DeclaredKeyProperties.Add(pkProperty);

            var fkProperty = new EdmProperty().AsPrimitive();
            associationType.TargetEnd.EntityType.DeclaredProperties.Add(fkProperty);

            // Foo.PID == Bar.PId
            pkProperty.Name = "PID";
            fkProperty.Name = "PId";

            ((IEdmConvention<EdmAssociationType>)new PrimaryKeyNameForeignKeyDiscoveryConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.DependentEnd);
            Assert.Equal("PId", associationType.Constraint.DependentProperties.Single().Name);
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

            // Foo.PId1 == Bar.PId1 && Foo.PId2 == Bar.PId2
            pkProperty1.Name = "PId1";
            pkProperty2.Name = "PId2";
            fkProperty1.Name = "PId1";
            fkProperty2.Name = "PId2";

            ((IEdmConvention<EdmAssociationType>)new PrimaryKeyNameForeignKeyDiscoveryConvention())
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

            // Foo.PId == Bar.PId
            pkProperty.Name = "PId";
            fkProperty.Name = "PId";

            var model = new EdmModel().Initialize();
            model.Namespaces.Single().AssociationTypes.Add(associationType);
            model.Namespaces.Single().AssociationTypes.Add(associationType);

            ((IEdmConvention<EdmAssociationType>)new PrimaryKeyNameForeignKeyDiscoveryConvention())
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

            // Foo.PId == Bar.PId
            pkProperty.Name = "PId";
            fkProperty.Name = "PId";
            pkProperty.PropertyType = new EdmTypeReference() { EdmType = EdmPrimitiveType.Binary };
            fkProperty.PropertyType = new EdmTypeReference() { EdmType = EdmPrimitiveType.String };

            ((IEdmConvention<EdmAssociationType>)new PrimaryKeyNameForeignKeyDiscoveryConvention())
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