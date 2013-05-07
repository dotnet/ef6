// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class MaxLengthAttributeConventionTests : TestBase
    {
        [Fact]
        public void Apply_should_set_is_max_length_when_no_length_given()
        {
            var propertyConfiguration = new StringPropertyConfiguration();

            new MaxLengthAttributeConvention()
                .Apply(
                    new LightweightPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration),
                    new MaxLengthAttribute());

            Assert.Null(propertyConfiguration.MaxLength);
            Assert.Equal(true, propertyConfiguration.IsMaxLength);
        }

        [Fact]
        public void Apply_should_not_set_is_max_length_if_value_exists()
        {
            var propertyConfiguration = new StringPropertyConfiguration
                {
                    IsMaxLength = false
                };

            new MaxLengthAttributeConvention()
                .Apply(
                    new LightweightPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration),
                    new MaxLengthAttribute());

            Assert.Null(propertyConfiguration.MaxLength);
            Assert.Equal(false, propertyConfiguration.IsMaxLength);
        }

        [Fact]
        public void Apply_should_set_max_length_to_given_value()
        {
            var propertyConfiguration = new StringPropertyConfiguration();

            new MaxLengthAttributeConvention()
                .Apply(
                    new LightweightPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration),
                    new MaxLengthAttribute(100));

            Assert.Equal(100, propertyConfiguration.MaxLength);
            Assert.Null(propertyConfiguration.IsMaxLength);
        }

        [Fact]
        public void Apply_should_not_set_max_length_if_value_exists()
        {
            var propertyConfiguration = new StringPropertyConfiguration
                {
                    MaxLength = 200
                };

            new MaxLengthAttributeConvention()
                .Apply(
                    new LightweightPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration),
                    new MaxLengthAttribute(100));

            Assert.Equal(200, propertyConfiguration.MaxLength);
            Assert.Null(propertyConfiguration.IsMaxLength);
        }

        [Fact]
        public void Apply_should_throw_on_invalid_value()
        {
            var propertyConfiguration = new StringPropertyConfiguration();

            Assert.Equal(
                Strings.MaxLengthAttributeConvention_InvalidMaxLength("P", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => new MaxLengthAttributeConvention()
                              .Apply(
                                  new LightweightPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration),
                                  new MaxLengthAttribute(-2))).Message);
        }
    }
}
