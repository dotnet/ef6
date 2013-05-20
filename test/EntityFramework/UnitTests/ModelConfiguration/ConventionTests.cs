// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;
    using Moq.Protected;
    using Xunit;
    using StringPropertyConfiguration = System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration
        ;

    internal class ConventionTests
    {
        [Fact]
        public void Ctor_calls_Apply()
        {
            var mockConfigurationConvention = new Mock<Convention>();

            Assert.NotNull(mockConfigurationConvention.Object);

            mockConfigurationConvention.Protected().Verify("Apply", Times.Once());
        }

        [Fact]
        public void Entities_returns_a_new_object()
        {
            var convention = new Convention();
            var entities = convention.Types();

            Assert.NotNull(entities);
            Assert.NotSame(entities, convention.Types());
        }

        [Fact]
        public void Generic_Entities_returns_a_new_object()
        {
            var convention = new Convention();
            var entities = convention.Types<object>();

            Assert.NotNull(entities);
            Assert.NotSame(entities, convention.Types<object>());
        }

        [Fact]
        public void Properties_returns_a_new_object()
        {
            var convention = new Convention();
            var properties = convention.Properties();

            Assert.NotNull(properties);
            Assert.NotSame(properties, convention.Properties());
        }

        [Fact]
        public void Generic_Properties_returns_a_new_object()
        {
            var convention = new Convention();
            var properties = convention.Properties<int>();

            Assert.NotNull(properties);
            Assert.NotSame(properties, convention.Properties<int>());
        }

        [Fact]
        public void Generic_Properties_throws_on_invalid_type()
        {
            var convention = new Convention();
            Assert.Equal(
                Strings.ModelBuilder_PropertyFilterTypeMustBePrimitive(typeof(object)),
                Assert.Throws<InvalidOperationException>(() => convention.Properties<object>()).Message);
        }

        [Fact]
        public void Generic_Properties_filter_on_type()
        {
            var decimalProperty = new MockPropertyInfo(typeof(decimal), "Property1");
            var nullableDecimalProperty = new MockPropertyInfo(typeof(decimal?), "Property2");
            var nonDecimalProperty = new MockPropertyInfo(typeof(string), "Property3");

            var config = new Convention().Properties<decimal>();
            Assert.NotNull(config);
            Assert.Equal(1, config.Predicates.Count());

            var predicate = config.Predicates.Single();
            Assert.True(predicate(decimalProperty));
            Assert.True(predicate(nullableDecimalProperty));
            Assert.False(predicate(nonDecimalProperty));
        }

        [Fact]
        public void Apply_methods_delegate_to_ConventionsConfiguration()
        {
            var modelConfiguration = new ModelConfiguration();
            var mockPropertyInfo = new MockPropertyInfo(typeof(string), "S");
            Func<PropertyConfiguration> propertyConfiguration = () =>
                                                                new StringPropertyConfiguration();
            Func<StructuralTypeConfiguration> entityConfiguration = () => new EntityTypeConfiguration(typeof(object));

            Verify_method_delegates(
                c => c.ApplyModelConfiguration(modelConfiguration),
                c => c.ApplyModelConfiguration(modelConfiguration));
            Verify_method_delegates(
                c => c.ApplyModelConfiguration(typeof(object), modelConfiguration),
                c => c.ApplyModelConfiguration(typeof(object), modelConfiguration));
            Verify_method_delegates(
                c => c.ApplyPropertyConfiguration(mockPropertyInfo, propertyConfiguration, modelConfiguration),
                c => c.ApplyPropertyConfiguration(mockPropertyInfo, propertyConfiguration, modelConfiguration));
            Verify_method_delegates(
                c => c.ApplyPropertyConfiguration(mockPropertyInfo, modelConfiguration),
                c => c.ApplyPropertyConfiguration(mockPropertyInfo, modelConfiguration));
            Verify_method_delegates(
                c => c.ApplyPropertyTypeConfiguration(mockPropertyInfo, entityConfiguration, modelConfiguration),
                c => c.ApplyPropertyTypeConfiguration(mockPropertyInfo, entityConfiguration, modelConfiguration));
            Verify_method_delegates(
                c => c.ApplyTypeConfiguration(typeof(object), entityConfiguration, modelConfiguration),
                c => c.ApplyTypeConfiguration(typeof(object), entityConfiguration, modelConfiguration));
        }

        private void Verify_method_delegates(
            Action<Convention> methodInvoke,
            Expression<Action<ConventionsConfiguration>> mockMethodInvoke)
        {
            Assert.NotNull(methodInvoke);
            Assert.NotNull(mockMethodInvoke);

            var conventionsConfigurationMock = new Mock<ConventionsConfiguration>();
            var configurationConvention = new Convention(conventionsConfigurationMock.Object);

            methodInvoke(configurationConvention);
            conventionsConfigurationMock.Verify(mockMethodInvoke, Times.Once());
        }
    }
}
