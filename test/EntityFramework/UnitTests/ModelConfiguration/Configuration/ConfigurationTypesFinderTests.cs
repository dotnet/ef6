// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using Moq;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using Xunit;


    public sealed class ConfigurationTypesFinderTests
    {
        [Fact]
        public void AddConfigurationTypesToModel_adds_entitytypeconfigurations_into_model()
        {
            var entityTypeConfiguration = new EntityTypeConfiguration(typeof(Random));
            
            var filter = new Mock<ConfigurationTypeFilter>();
            filter.Setup(f => f.IsEntityTypeConfiguration(It.IsAny<Type>())).Returns(true);

            var activator = new Mock<ConfigurationTypeActivator>();
            activator.Setup(a => a.Activate<EntityTypeConfiguration>(It.IsAny<Type>()))
                     .Returns(entityTypeConfiguration);

            var finder = new ConfigurationTypesFinder(activator.Object, filter.Object);
            
            var modelConfiguration = new ModelConfiguration();
            finder.AddConfigurationTypesToModel(new[] { typeof(Object) }, modelConfiguration);

            Assert.Same(entityTypeConfiguration, modelConfiguration.Entity(typeof(Random)));
        }

        [Fact]
        public void AddConfigurationTypesToModel_adds_complextypeconfigurations_into_model()
        {
            var complexTypeConfiguration = new ComplexTypeConfiguration(typeof(Random));

            var filter = new Mock<ConfigurationTypeFilter>();
            filter.Setup(f => f.IsEntityTypeConfiguration(It.IsAny<Type>())).Returns(false);
            filter.Setup(f => f.IsComplexTypeConfiguration(It.IsAny<Type>())).Returns(true);

            var activator = new Mock<ConfigurationTypeActivator>();
            activator.Setup(a => a.Activate<ComplexTypeConfiguration>(It.IsAny<Type>()))
                     .Returns(complexTypeConfiguration);

            var finder = new ConfigurationTypesFinder(activator.Object, filter.Object);

            var modelConfiguration = new ModelConfiguration();
            finder.AddConfigurationTypesToModel(new[] { typeof(Object) }, modelConfiguration);

            Assert.Same(complexTypeConfiguration, modelConfiguration.ComplexType(typeof(Random)));
        }

        [Fact]
        public void AddConfigurationTypesToModel_ignores_types_that_are_not_configuration_types()
        {
            var filter = new Mock<ConfigurationTypeFilter>();
            filter.Setup(f => f.IsEntityTypeConfiguration(It.IsAny<Type>())).Returns(false);
            filter.Setup(f => f.IsComplexTypeConfiguration(It.IsAny<Type>())).Returns(false);

            var activator = new Mock<ConfigurationTypeActivator>();

            var finder = new ConfigurationTypesFinder(activator.Object, filter.Object);

            finder.AddConfigurationTypesToModel(new[] { typeof(Object) }, new ModelConfiguration());

            activator.Verify(m => m.Activate<EntityTypeConfiguration>(It.IsAny<Type>()), Times.Never());
            activator.Verify(m => m.Activate<ComplexTypeConfiguration>(It.IsAny<Type>()), Times.Never());
        }

    }
}
