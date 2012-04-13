namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using Xunit;

    public sealed class ForeignKeyAssociationMultiplicityConventionTests
    {
        [Fact]
        public void Apply_should_set_principal_end_kind_to_required_when_all_properties_not_nullable()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType = new EdmEntityType();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Optional;
            associationType.TargetEnd.EntityType = new EdmEntityType();

            var associationConstraint = new EdmAssociationConstraint();
            associationConstraint.DependentProperties.Add(new EdmProperty().AsPrimitive());
            associationConstraint.DependentProperties.Add(new EdmProperty().AsPrimitive());
            associationConstraint.DependentProperties.Each(p => p.PropertyType.IsNullable = false);
            associationConstraint.DependentEnd = associationType.TargetEnd;

            associationType.Constraint = associationConstraint;

            ((IEdmConvention<EdmAssociationType>)new ForeignKeyAssociationMultiplicityConvention())
                .Apply(associationType, new EdmModel().Initialize());

            Assert.True(associationType.SourceEnd.IsRequired());
        }
    }
}