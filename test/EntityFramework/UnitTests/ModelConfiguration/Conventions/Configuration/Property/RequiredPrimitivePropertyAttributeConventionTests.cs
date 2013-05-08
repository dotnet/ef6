// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using Xunit;
    using StringPropertyConfiguration = System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.StringPropertyConfiguration;

    public sealed class RequiredPrimitivePropertyAttributeConventionTests : TestBase
    {
        [Fact]
        public void Apply_should_unset_optional_flag()
        {
            var propertyConfiguration = new StringPropertyConfiguration();

            new RequiredPrimitivePropertyAttributeConvention()
                .Apply(new MockPropertyInfo(), propertyConfiguration, new ModelConfiguration(), new RequiredAttribute());

            Assert.Equal(false, propertyConfiguration.IsNullable);
        }

        [Fact]
        public void Apply_should_ignore_attribute_if_already_set()
        {
            var propertyConfiguration = new StringPropertyConfiguration
                                            {
                                                IsNullable = true
                                            };

            new RequiredPrimitivePropertyAttributeConvention()
                .Apply(new MockPropertyInfo(), propertyConfiguration, new ModelConfiguration(), new RequiredAttribute());

            Assert.Equal(true, propertyConfiguration.IsNullable);
        }
    }
}
