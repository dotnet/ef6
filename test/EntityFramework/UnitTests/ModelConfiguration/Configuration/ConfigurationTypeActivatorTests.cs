// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class ConfigurationTypeActivatorTests
    {
        [Fact]
        public void Activate_create_instance_for_types_that_implement_entitytype_configuration()
        {
            Assert.Same(
                typeof(Random),
                new ConfigurationTypeActivator().Activate<EntityTypeConfiguration>(typeof(SimpleEntityTypeConfiguration)).ClrType);
        }

        [Fact]
        public void Activate_create_instance_for_types_that_implement_entitytype_configuration_in_hierarchy()
        {
            Assert.Same(
                typeof(Random),
                new ConfigurationTypeActivator().Activate<EntityTypeConfiguration>(typeof(EntityTypeConfigurationInHierarchy)).ClrType);
        }

        [Fact]
        public void Activate_create_instance_for_entitytypes_with_private_constructor()
        {
            Assert.Same(
                typeof(Random),
                new ConfigurationTypeActivator().Activate<EntityTypeConfiguration>(
                    typeof(SimpleEntityTypeConfigurationWithPrivateConstructor)).ClrType);
        }

        [Fact]
        public void Activate_throw_if_entitytype_dont_have_parameterless_constructor()
        {
            Assert.Equal(
                Strings.CreateConfigurationType_NoParameterlessConstructor(typeof(EntityTypeConfigurationWithoutDefaultConstructor).Name),
                Assert.Throws<InvalidOperationException>(
                    () => new ConfigurationTypeActivator().Activate<EntityTypeConfiguration>(
                        typeof(EntityTypeConfigurationWithoutDefaultConstructor))).Message);
        }

        [Fact]
        public void Activate_create_instance_for_types_that_implement_complextype_configuration()
        {
            Assert.Same(
                typeof(Random),
                new ConfigurationTypeActivator().Activate<ComplexTypeConfiguration>(typeof(SimpleComplexTypeConfiguration)).ClrType);
        }

        [Fact]
        public void Activate_create_instance_for_types_that_implement_complextype_configuration_in_hierarchy()
        {
            Assert.Same(
                typeof(Random),
                new ConfigurationTypeActivator().Activate<ComplexTypeConfiguration>(typeof(ComplexTypeConfigurationInHierarchy)).ClrType);
        }

        [Fact]
        public void Activate_create_instance_for_complextypes_with_private_constructor()
        {
            Assert.Same(
                typeof(Random),
                new ConfigurationTypeActivator().Activate<ComplexTypeConfiguration>(
                    typeof(SimpleComplexTypeConfigurationWithPrivateConstructor)).ClrType);
        }

        [Fact]
        public void Activate_throw_if_complextypeconfiguration_dont_have_parameterless_constructor()
        {
            Assert.Equal(
                Strings.CreateConfigurationType_NoParameterlessConstructor(typeof(ComplexTypeConfigurationWithoutDefaultConstructor).Name),
                Assert.Throws<InvalidOperationException>(
                    () => new ConfigurationTypeActivator().Activate<ComplexTypeConfiguration>(
                        typeof(ComplexTypeConfigurationWithoutDefaultConstructor))).Message);
        }

        private class SimpleEntityTypeConfiguration : EntityTypeConfiguration<Random>
        {
        }

        private class SimpleComplexTypeConfiguration : ComplexTypeConfiguration<Random>
        {
        }

        private class EntityTypeConfigurationInHierarchy : CommonEntityTypeConfiguration<Random>
        {
        }

        private class ComplexTypeConfigurationInHierarchy : CommonComplexTypeConfiguration<Random>
        {
        }

        private abstract class CommonEntityTypeConfiguration<T>
            : EntityTypeConfiguration<T>
            where T : class
        {
        }

        private abstract class CommonComplexTypeConfiguration<T>
            : ComplexTypeConfiguration<T>
            where T : class
        {
        }

        private class EntityTypeConfigurationWithoutDefaultConstructor
            : EntityTypeConfiguration<Random>
        {
            public EntityTypeConfigurationWithoutDefaultConstructor(object _)
            {
            }
        }

        private class ComplexTypeConfigurationWithoutDefaultConstructor
            : ComplexTypeConfiguration<Random>
        {
            public ComplexTypeConfigurationWithoutDefaultConstructor(object _)
            {
            }
        }

        private class SimpleEntityTypeConfigurationWithPrivateConstructor
            : EntityTypeConfiguration<Random>
        {
            private SimpleEntityTypeConfigurationWithPrivateConstructor()
            {
            }
        }

        private class SimpleComplexTypeConfigurationWithPrivateConstructor
            : ComplexTypeConfiguration<Random>
        {
            private SimpleComplexTypeConfigurationWithPrivateConstructor()
            {
            }
        }
    }
}
