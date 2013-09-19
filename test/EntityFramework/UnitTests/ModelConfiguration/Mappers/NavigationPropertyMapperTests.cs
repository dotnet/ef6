// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Linq;
    using Xunit;

    public sealed class NavigationPropertyMapperTests
    {
        [Fact]
        public void Map_should_set_namespace_when_provided_via_model_configuration()
        {
            var modelConfiguration 
                = new ModelConfiguration
                                         {
                                             ModelNamespace = "Foo"
                                         };
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            model.AddEntitySet("Source", entityType);
            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(
                    new MockPropertyInfo(new MockType("Target"), "Nav"), entityType,
                    () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, model.AssociationTypes.Count());

            var associationType = model.AssociationTypes.Single();

            Assert.Equal("Foo", associationType.NamespaceName);
        }

        [Fact]
        public void Map_should_set_default_association_multiplicity_to_collection_to_optional()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            model.AddEntitySet("Source", entityType);
            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(
                    new MockPropertyInfo(new MockType("Target"), "Nav"), entityType,
                    () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, model.AssociationTypes.Count());

            var associationType = model.AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.Many, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.TargetEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Map_should_create_association_sets_for_associations()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType("Source", "N", DataSpace.CSpace);
            model.AddEntitySet("Source", entityType);

            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(
                    new MockPropertyInfo(new MockType("Target"), "Nav"), entityType,
                    () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, model.Containers.Single().AssociationSets.Count);

            var associationSet = model.Containers.Single().AssociationSets.Single();

            Assert.NotNull(associationSet);
            Assert.NotNull(associationSet.ElementType);
            Assert.Equal("Source_Nav", associationSet.Name);
        }

        [Fact]
        public void Map_should_detect_collection_associations_and_set_correct_end_kinds()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            model.AddEntitySet("Source", entityType);
            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(
                    new MockPropertyInfo(typeof(List<NavigationPropertyMapperTests>), "Nav"), entityType,
                    () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, model.AssociationTypes.Count());

            var associationType = model.AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.Many, associationType.TargetEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Map_should_not_detect_arrays_as_collection_associations()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(
                    new MockPropertyInfo(typeof(NavigationPropertyMapperTests[]), "Nav"), entityType,
                    () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(0, model.AssociationTypes.Count());
        }

        [Fact]
        public void Map_should_create_navigation_property_for_association()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            model.AddEntitySet("Source", entityType);
            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(
                    new MockPropertyInfo(new MockType("Target"), "Nav"), entityType,
                    () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, entityType.DeclaredNavigationProperties.Count);

            var navigationProperty = entityType.NavigationProperties.Single();

            Assert.Equal("Nav", navigationProperty.Name);
            Assert.NotNull(navigationProperty.Association);
            Assert.NotSame(entityType, navigationProperty.ResultEnd.GetEntityType());
        }

        [Fact]
        public void Map_should_set_clr_property_info_on_assocation_source_end()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            model.AddEntitySet("Source", entityType);
            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            var mockPropertyInfo = new MockPropertyInfo(new MockType("Target"), "Nav");

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(
                    mockPropertyInfo, entityType,
                    () => new EntityTypeConfiguration(typeof(object)));

            var associationType = model.AssociationTypes.Single();

            Assert.Same(mockPropertyInfo.Object, associationType.SourceEnd.GetClrPropertyInfo());
        }
    }
}
