// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class AssociationInverseDiscoveryConventionTests
    {
        [Fact]
        public void Apply_should_correct_dangling_navigation_properties()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T")
                    .Association("A1", "S", RelationshipMultiplicity.ZeroOrOne, "T", "T", RelationshipMultiplicity.Many, null)
                    .Association("A2", "T", RelationshipMultiplicity.Many, "S", "S", RelationshipMultiplicity.ZeroOrOne, null);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            var navigationProperties
                = model.EntityTypes.SelectMany(e => e.NavigationProperties);

            Assert.Equal(2, navigationProperties.Count());

            var navigationProperty1 = navigationProperties.ElementAt(0);
            var navigationProperty2 = navigationProperties.ElementAt(1);
            var associationType = model.AssociationTypes.Single();

            Assert.Same(associationType, navigationProperty1.Association);
            Assert.Same(associationType, navigationProperty2.Association);
            Assert.Same(associationType.SourceEnd, navigationProperty1.FromEndMember);
            Assert.Same(associationType.TargetEnd, navigationProperty1.ResultEnd);
            Assert.Same(associationType.TargetEnd, navigationProperty2.FromEndMember);
            Assert.Same(associationType.SourceEnd, navigationProperty2.ResultEnd);
        }

        [Fact]
        public void Apply_should_transfer_constraint_and_clr_property_info_annotation()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T")
                    .Association("S", RelationshipMultiplicity.ZeroOrOne, "T", RelationshipMultiplicity.Many)
                    .Association("T", RelationshipMultiplicity.Many, "S", RelationshipMultiplicity.ZeroOrOne);

            var association2 = model.AssociationTypes.Last();

            var mockPropertyInfo = new MockPropertyInfo();
            association2.SourceEnd.SetClrPropertyInfo(mockPropertyInfo);

            var referentialConstraint 
                = new ReferentialConstraint(
                    association2.SourceEnd, 
                    association2.TargetEnd, 
                    new[] { new EdmProperty("P") }, 
                    new[] { new EdmProperty("D") });
            
            association2.Constraint = referentialConstraint;

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.AssociationTypes.Count());
            Assert.Equal(1, model.Containers.Single().AssociationSets.Count());

            var associationType = model.AssociationTypes.Single();

            Assert.NotSame(association2, associationType);
            Assert.Same(referentialConstraint, associationType.Constraint);
            Assert.Same(associationType.SourceEnd, referentialConstraint.FromRole);
            Assert.Same(associationType.TargetEnd, referentialConstraint.ToRole);
            Assert.Same(mockPropertyInfo.Object, associationType.TargetEnd.GetClrPropertyInfo());
        }

        [Fact]
        public void Apply_should_discover_optional_to_collection_association_pair()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T")
                    .Association("S", RelationshipMultiplicity.ZeroOrOne, "T", RelationshipMultiplicity.Many)
                    .Association("T", RelationshipMultiplicity.Many, "S", RelationshipMultiplicity.ZeroOrOne);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.AssociationTypes.Count());
            Assert.Equal(1, model.Containers.Single().AssociationSets.Count());

            var associationType = model.AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.Many, associationType.TargetEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Apply_should_discover_collection_to_collection_association_pair()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T")
                    .Association("S", RelationshipMultiplicity.ZeroOrOne, "T", RelationshipMultiplicity.Many)
                    .Association("T", RelationshipMultiplicity.ZeroOrOne, "S", RelationshipMultiplicity.Many);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.AssociationTypes.Count());
            Assert.Equal(1, model.Containers.Single().AssociationSets.Count());

            var associationType = model.AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.Many, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.Many, associationType.TargetEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Apply_should_discover_optional_to_optional_association_pair()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T")
                    .Association("S", RelationshipMultiplicity.Many, "T", RelationshipMultiplicity.ZeroOrOne)
                    .Association("T", RelationshipMultiplicity.Many, "S", RelationshipMultiplicity.ZeroOrOne);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.AssociationTypes.Count());
            Assert.Equal(1, model.Containers.Single().AssociationSets.Count());

            var associationType = model.AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.TargetEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Apply_should_discover_self_referencing_association_pair()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S")
                    .Association("S", RelationshipMultiplicity.ZeroOrOne, "S", RelationshipMultiplicity.Many)
                    .Association("S", RelationshipMultiplicity.Many, "S", RelationshipMultiplicity.ZeroOrOne);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.AssociationTypes.Count());
            Assert.Equal(1, model.Containers.Single().AssociationSets.Count());

            var associationType = model.AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.Many, associationType.TargetEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Apply_should_discover_for_multiple_entities()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T", "U")
                    .Association("S", RelationshipMultiplicity.Many, "T", RelationshipMultiplicity.ZeroOrOne)
                    .Association("T", RelationshipMultiplicity.Many, "S", RelationshipMultiplicity.ZeroOrOne)
                    .Association("T", RelationshipMultiplicity.Many, "U", RelationshipMultiplicity.ZeroOrOne)
                    .Association("U", RelationshipMultiplicity.Many, "T", RelationshipMultiplicity.ZeroOrOne)
                    .Association("U", RelationshipMultiplicity.Many, "S", RelationshipMultiplicity.ZeroOrOne)
                    .Association("S", RelationshipMultiplicity.Many, "U", RelationshipMultiplicity.ZeroOrOne);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(3, model.AssociationTypes.Count());
            Assert.Equal(3, model.Containers.Single().AssociationSets.Count());
        }

        [Fact]
        public void Apply_should_not_discover_when_too_many_associations()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T")
                    .Association("S", RelationshipMultiplicity.ZeroOrOne, "T", RelationshipMultiplicity.Many)
                    .Association("S", RelationshipMultiplicity.ZeroOrOne, "T", RelationshipMultiplicity.Many)
                    .Association("T", RelationshipMultiplicity.ZeroOrOne, "S", RelationshipMultiplicity.Many);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(3, model.AssociationTypes.Count());
            Assert.Equal(3, model.Containers.Single().AssociationSets.Count());
        }
    }
}
