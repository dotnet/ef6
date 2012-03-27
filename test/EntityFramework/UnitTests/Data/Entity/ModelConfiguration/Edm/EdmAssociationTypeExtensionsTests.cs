namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity.Edm;
    using Xunit;

    public sealed class EdmAssociationTypeExtensionsTests
    {
        [Fact]
        public void Initialize_should_create_association_ends()
        {
            var associationType = new EdmAssociationType().Initialize();

            Assert.NotNull(associationType.SourceEnd);
            Assert.NotNull(associationType.TargetEnd);
        }

        [Fact]
        public void Can_mark_association_as_independent()
        {
            var associationType = new EdmAssociationType();

            Assert.False(associationType.IsIndependent());

            associationType.MarkIndependent();

            Assert.True(associationType.IsIndependent());
        }

        [Fact]
        public void GetOtherEnd_should_return_correct_end()
        {
            var associationType = new EdmAssociationType().Initialize();

            Assert.Same(associationType.SourceEnd, associationType.GetOtherEnd(associationType.TargetEnd));
            Assert.Same(associationType.TargetEnd, associationType.GetOtherEnd(associationType.SourceEnd));
        }

        [Fact]
        public void Can_get_and_set_configuration_facet()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SetConfiguration(42);

            Assert.Equal(42, associationType.GetConfiguration());
        }

        [Fact]
        public void HasDeleteAction_should_be_true_if_either_end_is_not_none_action()
        {
            var associationType = new EdmAssociationType().Initialize();

            Assert.False(associationType.HasDeleteAction());

            associationType.SourceEnd.DeleteAction = EdmOperationAction.Cascade;

            Assert.True(associationType.HasDeleteAction());

            associationType = new EdmAssociationType().Initialize();
            associationType.TargetEnd.DeleteAction = EdmOperationAction.Cascade;

            Assert.True(associationType.HasDeleteAction());
        }

        [Fact]
        public void IsRequiredToMany_should_be_true_when_source_required_and_target_many()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Required;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Many;

            Assert.True(associationType.IsRequiredToMany());
        }

        [Fact]
        public void IsManyToRequired_should_be_true_when_source_many_and_target_required()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Many;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Required;

            Assert.True(associationType.IsManyToRequired());
        }

        [Fact]
        public void IsManyToMany_should_be_true_when_source_many_and_target_many()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Many;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Many;

            Assert.True(associationType.IsManyToMany());
        }

        [Fact]
        public void IsSelfReferencing_returns_true_when_source_type_matches_target_type()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType
                = associationType.TargetEnd.EntityType
                    = new EdmEntityType();

            Assert.True(associationType.IsSelfReferencing());
        }

        [Fact]
        public void IsSelfReferencing_returns_true_when_ends_have_same_base_type()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType = new EdmEntityType();
            associationType.TargetEnd.EntityType = new EdmEntityType();

            Assert.False(associationType.IsSelfReferencing());

            associationType.SourceEnd.EntityType.BaseType
                = associationType.TargetEnd.EntityType.BaseType
                    = new EdmEntityType();

            Assert.True(associationType.IsSelfReferencing());
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_optional_to_many()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Optional;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Many;

            EdmAssociationEnd principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.SourceEnd, principalEnd);
            Assert.Same(associationType.TargetEnd, dependentEnd);
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_required_to_many()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Required;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Many;

            EdmAssociationEnd principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.SourceEnd, principalEnd);
            Assert.Same(associationType.TargetEnd, dependentEnd);
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_many_to_optional()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Many;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Optional;

            EdmAssociationEnd principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.TargetEnd, principalEnd);
            Assert.Same(associationType.SourceEnd, dependentEnd);
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_many_to_required()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Many;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Required;

            EdmAssociationEnd principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.TargetEnd, principalEnd);
            Assert.Same(associationType.SourceEnd, dependentEnd);
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_required_to_optional()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Required;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Optional;

            EdmAssociationEnd principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.SourceEnd, principalEnd);
            Assert.Same(associationType.TargetEnd, dependentEnd);
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_optional_to_required()
        {
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Optional;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Required;

            EdmAssociationEnd principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.TargetEnd, principalEnd);
            Assert.Same(associationType.SourceEnd, dependentEnd);
        }
    }
}