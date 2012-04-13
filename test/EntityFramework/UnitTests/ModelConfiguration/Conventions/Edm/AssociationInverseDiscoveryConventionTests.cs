namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
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
                    .Association("A1", "S", EdmAssociationEndKind.Optional, "T", "T", EdmAssociationEndKind.Many, null)
                    .Association("A2", "T", EdmAssociationEndKind.Many, "S", "S", EdmAssociationEndKind.Optional, null);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            var navigationProperties
                = model.GetEntityTypes().SelectMany(e => e.NavigationProperties);

            Assert.Equal(2, navigationProperties.Count());

            var navigationProperty1 = navigationProperties.ElementAt(0);
            var navigationProperty2 = navigationProperties.ElementAt(1);
            var associationType = model.GetAssociationTypes().Single();

            Assert.Same(associationType, navigationProperty1.Association);
            Assert.Same(associationType, navigationProperty2.Association);
            Assert.Same(associationType.TargetEnd, navigationProperty1.ResultEnd);
            Assert.Same(associationType.SourceEnd, navigationProperty2.ResultEnd);
        }

        [Fact]
        public void Apply_should_discover_optional_to_collection_association_pair()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T")
                    .Association("S", EdmAssociationEndKind.Optional, "T", EdmAssociationEndKind.Many)
                    .Association("T", EdmAssociationEndKind.Many, "S", EdmAssociationEndKind.Optional);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(1, model.Containers.Single().AssociationSets.Count());

            var associationType = model.GetAssociationTypes().Single();

            Assert.Equal(EdmAssociationEndKind.Optional, associationType.SourceEnd.EndKind);
            Assert.Equal(EdmAssociationEndKind.Many, associationType.TargetEnd.EndKind);
        }

        [Fact]
        public void Apply_should_discover_collection_to_collection_association_pair()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T")
                    .Association("S", EdmAssociationEndKind.Optional, "T", EdmAssociationEndKind.Many)
                    .Association("T", EdmAssociationEndKind.Optional, "S", EdmAssociationEndKind.Many);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(1, model.Containers.Single().AssociationSets.Count());

            var associationType = model.GetAssociationTypes().Single();

            Assert.Equal(EdmAssociationEndKind.Many, associationType.SourceEnd.EndKind);
            Assert.Equal(EdmAssociationEndKind.Many, associationType.TargetEnd.EndKind);
        }

        [Fact]
        public void Apply_should_discover_optional_to_optional_association_pair()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T")
                    .Association("S", EdmAssociationEndKind.Many, "T", EdmAssociationEndKind.Optional)
                    .Association("T", EdmAssociationEndKind.Many, "S", EdmAssociationEndKind.Optional);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(1, model.Containers.Single().AssociationSets.Count());

            var associationType = model.GetAssociationTypes().Single();

            Assert.Equal(EdmAssociationEndKind.Optional, associationType.SourceEnd.EndKind);
            Assert.Equal(EdmAssociationEndKind.Optional, associationType.TargetEnd.EndKind);
        }

        [Fact]
        public void Apply_should_discover_self_referencing_association_pair()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S")
                    .Association("S", EdmAssociationEndKind.Optional, "S", EdmAssociationEndKind.Many)
                    .Association("S", EdmAssociationEndKind.Many, "S", EdmAssociationEndKind.Optional);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(1, model.GetAssociationTypes().Count());
            Assert.Equal(1, model.Containers.Single().AssociationSets.Count());

            var associationType = model.GetAssociationTypes().Single();

            Assert.Equal(EdmAssociationEndKind.Optional, associationType.SourceEnd.EndKind);
            Assert.Equal(EdmAssociationEndKind.Many, associationType.TargetEnd.EndKind);
        }

        [Fact]
        public void Apply_should_discover_for_multiple_entities()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T", "U")
                    .Association("S", EdmAssociationEndKind.Many, "T", EdmAssociationEndKind.Optional)
                    .Association("T", EdmAssociationEndKind.Many, "S", EdmAssociationEndKind.Optional)
                    .Association("T", EdmAssociationEndKind.Many, "U", EdmAssociationEndKind.Optional)
                    .Association("U", EdmAssociationEndKind.Many, "T", EdmAssociationEndKind.Optional)
                    .Association("U", EdmAssociationEndKind.Many, "S", EdmAssociationEndKind.Optional)
                    .Association("S", EdmAssociationEndKind.Many, "U", EdmAssociationEndKind.Optional);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(3, model.GetAssociationTypes().Count());
            Assert.Equal(3, model.Containers.Single().AssociationSets.Count());
        }

        [Fact]
        public void Apply_should_not_discover_when_too_many_associations()
        {
            EdmModel model
                = new TestModelBuilder()
                    .Entities("S", "T")
                    .Association("S", EdmAssociationEndKind.Optional, "T", EdmAssociationEndKind.Many)
                    .Association("S", EdmAssociationEndKind.Optional, "T", EdmAssociationEndKind.Many)
                    .Association("T", EdmAssociationEndKind.Optional, "S", EdmAssociationEndKind.Many);

            ((IEdmConvention)new AssociationInverseDiscoveryConvention()).Apply(model);

            Assert.Equal(3, model.GetAssociationTypes().Count());
            Assert.Equal(3, model.Containers.Single().AssociationSets.Count());
        }
    }
}