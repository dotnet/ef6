// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Linq;
    using Xunit;

    public sealed class NavigationPropertyMapperTests
    {
        [Fact]
        public void Map_should_set_default_association_multiplicity_to_collection_to_optional()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel().Initialize();
            var entityType = new EdmEntityType();
            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(new MockPropertyInfo(new MockType("Target"), "Nav"), entityType,
                    () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, model.Namespaces.Single().AssociationTypes.Count);

            var associationType = model.Namespaces.Single().AssociationTypes.Single();

            Assert.Equal(EdmAssociationEndKind.Many, associationType.SourceEnd.EndKind);
            Assert.Equal(EdmAssociationEndKind.Optional, associationType.TargetEnd.EndKind);
        }

        [Fact]
        public void Map_should_create_association_sets_for_associations()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel().Initialize();
            var entityType = new EdmEntityType { Name = "Source" };
            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(new MockPropertyInfo(new MockType("Target"), "Nav"), entityType,
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
            var model = new EdmModel().Initialize();
            var entityType = new EdmEntityType();
            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(new MockPropertyInfo(typeof(List<NavigationPropertyMapperTests>), "Nav"), entityType,
                    () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, model.Namespaces.Single().AssociationTypes.Count);

            var associationType = model.Namespaces.Single().AssociationTypes.Single();

            Assert.Equal(EdmAssociationEndKind.Optional, associationType.SourceEnd.EndKind);
            Assert.Equal(EdmAssociationEndKind.Many, associationType.TargetEnd.EndKind);
        }

        [Fact]
        public void Map_should_not_detect_arrays_as_collection_associations()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel().Initialize();
            var entityType = new EdmEntityType();
            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(new MockPropertyInfo(typeof(NavigationPropertyMapperTests[]), "Nav"), entityType,
                    () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(0, model.Namespaces.Single().AssociationTypes.Count);
        }

        [Fact]
        public void Map_should_create_navigation_property_for_association()
        {
            var modelConfiguration = new ModelConfiguration();
            var model = new EdmModel().Initialize();
            var entityType = new EdmEntityType();
            var mappingContext = new MappingContext(modelConfiguration, new ConventionsConfiguration(), model);

            new NavigationPropertyMapper(new TypeMapper(mappingContext))
                .Map(new MockPropertyInfo(new MockType("Target"), "Nav"), entityType,
                    () => new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, entityType.DeclaredNavigationProperties.Count);

            var navigationProperty = entityType.NavigationProperties.Single();

            Assert.Equal("Nav", navigationProperty.Name);
            Assert.NotNull(navigationProperty.Association);
            Assert.NotSame(entityType, navigationProperty.ResultEnd.EntityType);
        }
    }
}