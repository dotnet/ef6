// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class AssociationTypeExtensionsTests
    {
        [Fact]
        public void Initialize_should_create_association_ends()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());

            Assert.NotNull(associationType.SourceEnd);
            Assert.NotNull(associationType.TargetEnd);
        }

        [Fact]
        public void Can_mark_association_as_independent()
        {
            var associationType = new AssociationType();

            Assert.False(associationType.IsIndependent());

            associationType.MarkIndependent();

            Assert.True(associationType.IsIndependent());
        }

        [Fact]
        public void GetOtherEnd_should_return_correct_end()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());

            Assert.Same(associationType.SourceEnd, associationType.GetOtherEnd(associationType.TargetEnd));
            Assert.Same(associationType.TargetEnd, associationType.GetOtherEnd(associationType.SourceEnd));
        }

        [Fact]
        public void Can_get_and_set_configuration_facet()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            associationType.SetConfiguration(42);

            Assert.Equal(42, associationType.GetConfiguration());
        }

        [Fact]
        public void IsRequiredToMany_should_be_true_when_source_required_and_target_many()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;

            Assert.True(associationType.IsRequiredToMany());
        }

        [Fact]
        public void IsManyToRequired_should_be_true_when_source_many_and_target_required()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;

            Assert.True(associationType.IsManyToRequired());
        }

        [Fact]
        public void IsManyToMany_should_be_true_when_source_many_and_target_many()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;

            Assert.True(associationType.IsManyToMany());
        }

        [Fact]
        public void IsSelfReferencing_returns_true_when_source_type_matches_target_type()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", associationType.SourceEnd.GetEntityType());

            Assert.True(associationType.IsSelfReferencing());
        }

        [Fact]
        public void IsSelfReferencing_returns_true_when_ends_have_same_base_type()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());

            Assert.False(associationType.IsSelfReferencing());

            associationType.SourceEnd.GetEntityType().BaseType
                = associationType.TargetEnd.GetEntityType().BaseType
                  = new EntityType();

            Assert.True(associationType.IsSelfReferencing());
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_optional_to_many()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;

            AssociationEndMember principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.SourceEnd, principalEnd);
            Assert.Same(associationType.TargetEnd, dependentEnd);
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_required_to_many()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;

            AssociationEndMember principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.SourceEnd, principalEnd);
            Assert.Same(associationType.TargetEnd, dependentEnd);
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_many_to_optional()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;

            AssociationEndMember principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.TargetEnd, principalEnd);
            Assert.Same(associationType.SourceEnd, dependentEnd);
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_many_to_required()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;

            AssociationEndMember principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.TargetEnd, principalEnd);
            Assert.Same(associationType.SourceEnd, dependentEnd);
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_required_to_optional()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;

            AssociationEndMember principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.SourceEnd, principalEnd);
            Assert.Same(associationType.TargetEnd, dependentEnd);
        }

        [Fact]
        public void TryGuessPrincipalAndDependentEnds_should_return_correct_ends_for_optional_to_required()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;

            AssociationEndMember principalEnd, dependentEnd;
            associationType.TryGuessPrincipalAndDependentEnds(out principalEnd, out dependentEnd);

            Assert.Same(associationType.TargetEnd, principalEnd);
            Assert.Same(associationType.SourceEnd, dependentEnd);
        }
    }
}
