// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using Xunit;

    public sealed class ConfigurationTypeFilterTests
    {
        [Fact]
        public void IsEntityTypeConfiguration_return_true_for_public_types_that_implement_entitytype_configuration()
        {
            Assert.True(new ConfigurationTypeFilter().IsEntityTypeConfiguration(typeof(SimplePublicEntityTypeConfiguration)));
        }

        [Fact]
        public void IsEntityTypeConfiguration_return_true_for_private_types_that_implement_entitytype_configuration()
        {
            Assert.True(new ConfigurationTypeFilter().IsEntityTypeConfiguration(typeof(SimplePrivateEntityTypeConfiguration)));
        }

        [Fact]
        public void IsEntityTypeConfiguration_return_true_for_non_abstract_types_that_implement_entitytype_configuration_in_hierarchy()
        {
            Assert.True(new ConfigurationTypeFilter().IsEntityTypeConfiguration(typeof(EntityTypeConfigurationInHierarchy)));
        }

        [Fact]
        public void IsEntityTypeConfiguration_return_false_for_abstract_entitype_configurations()
        {
            Assert.False(new ConfigurationTypeFilter().IsEntityTypeConfiguration(typeof(CommonEntityTypeConfiguration<Object>)));
        }

        [Fact]
        public void IsEntityTypeConfiguration_return_false_for_generic_type_definitions()
        {
            Assert.False(new ConfigurationTypeFilter().IsEntityTypeConfiguration(typeof(EntityTypeConfiguration<>)));
        }

        [Fact]
        public void IsEntityTypeConfiguration_return_false_for_not_valid_entitytypeconfigurations()
        {
            Assert.False(new ConfigurationTypeFilter().IsEntityTypeConfiguration(typeof(Object)));
            Assert.False(new ConfigurationTypeFilter().IsEntityTypeConfiguration(typeof(NotValidConfiguration)));
        }

        [Fact]
        public void IsEntityTypeConfiguration_return_false_for_complextype_configurations()
        {
            Assert.False(new ConfigurationTypeFilter().IsEntityTypeConfiguration(typeof(SimplePublicComplexTypeConfiguration)));
        }

        [Fact]
        public void IsComplexTypeConfiguration_return_true_for_public_types_that_implement_complextype_configuration()
        {
            Assert.True(new ConfigurationTypeFilter().IsComplexTypeConfiguration(typeof(SimplePublicComplexTypeConfiguration)));
        }

        [Fact]
        public void IsComplexTypeConfiguration_return_true_for_private_types_that_implement_complextype_configuration()
        {
            Assert.True(new ConfigurationTypeFilter().IsComplexTypeConfiguration(typeof(SimplePrivateComplexTypeConfiguration)));
        }

        [Fact]
        public void IsComplexTypeConfiguration_return_true_for_non_abstract_types_that_implement_complextype_configuration_in_hierarchy()
        {
            Assert.True(new ConfigurationTypeFilter().IsComplexTypeConfiguration(typeof(ComplexTypeConfigurationInHierarchy)));
        }

        [Fact]
        public void IsComplexTypeConfiguration_return_false_for_abstract_complextype_configurations()
        {
            Assert.False(new ConfigurationTypeFilter().IsComplexTypeConfiguration(typeof(CommonComplexTypeConfiguration<Object>)));
        }

        [Fact]
        public void IsComplexTypeConfiguration_return_false_for_generic_type_definitions()
        {
            Assert.False(new ConfigurationTypeFilter().IsComplexTypeConfiguration(typeof(ComplexTypeConfiguration<>)));
        }

        [Fact]
        public void IsComplexTypeConfiguration_return_false_for_not_valid_entitytypeconfigurations()
        {
            Assert.False(new ConfigurationTypeFilter().IsComplexTypeConfiguration(typeof(Object)));
            Assert.False(new ConfigurationTypeFilter().IsComplexTypeConfiguration(typeof(NotValidConfiguration)));
        }

        [Fact]
        public void IsComplexTypeConfiguration_return_false_for_entitytype_configurations()
        {
            Assert.False(new ConfigurationTypeFilter().IsComplexTypeConfiguration(typeof(SimplePublicEntityTypeConfiguration)));
        }

        public class SimplePublicEntityTypeConfiguration : EntityTypeConfiguration<Object>
        {
        }

        public class SimplePublicComplexTypeConfiguration : ComplexTypeConfiguration<Object>
        {
        }

        private class SimplePrivateEntityTypeConfiguration : EntityTypeConfiguration<Object>
        {
        }

        private class SimplePrivateComplexTypeConfiguration : ComplexTypeConfiguration<Object>
        {
        }

        private class EntityTypeConfigurationInHierarchy : CommonEntityTypeConfiguration<Object>
        {
        }

        private class ComplexTypeConfigurationInHierarchy : CommonComplexTypeConfiguration<Object>
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

        private class NotValidConfiguration : List<Object>
        {
        }
    }
}
