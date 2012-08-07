// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class StringLengthAttributeConventionTests
    {
        [Fact]
        public void Apply_should_set_max_length_for_strings()
        {
            var propertyConfiguration = new StringPropertyConfiguration();

            new StringLengthAttributeConvention.StringLengthAttributeConventionImpl()
                .Apply(new MockPropertyInfo(), propertyConfiguration, new StringLengthAttribute(12));

            Assert.Equal(12, propertyConfiguration.MaxLength);
        }

        [Fact]
        public void Apply_should_not_set_max_length_for_strings_if_value_exists()
        {
            var propertyConfiguration = new StringPropertyConfiguration
                                            {
                                                MaxLength = 11
                                            };

            new StringLengthAttributeConvention.StringLengthAttributeConventionImpl()
                .Apply(new MockPropertyInfo(), propertyConfiguration, new StringLengthAttribute(12));

            Assert.Equal(11, propertyConfiguration.MaxLength);
        }

        [Fact]
        public void Apply_should_throw_when_invalid_length_given()
        {
            var propertyConfiguration = new StringPropertyConfiguration();

            Assert.Equal(
                Strings.StringLengthAttributeConvention_InvalidMaximumLength("P", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => new StringLengthAttributeConvention.StringLengthAttributeConventionImpl()
                              .Apply(new MockPropertyInfo(), propertyConfiguration, new StringLengthAttribute(0))).Message);
        }
    }
}
