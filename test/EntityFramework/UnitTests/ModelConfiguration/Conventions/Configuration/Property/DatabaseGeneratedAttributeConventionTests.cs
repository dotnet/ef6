// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using Xunit;

    public sealed class DatabaseGeneratedAttributeConventionTests : TestBase
    {
        [Fact]
        public void Apply_should_set_store_generated_pattern()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration();

            new DatabaseGeneratedAttributeConvention()
                .Apply(
                    new MockPropertyInfo(), propertyConfiguration, new ModelConfiguration(),
                    new DatabaseGeneratedAttribute(DatabaseGeneratedOption.None));

            Assert.Equal(DatabaseGeneratedOption.None, propertyConfiguration.DatabaseGeneratedOption);
        }

        [Fact]
        public void Apply_should_ignore_attribute_if_already_set()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration
                {
                    DatabaseGeneratedOption = DatabaseGeneratedOption.Computed
                };

            new DatabaseGeneratedAttributeConvention()
                .Apply(
                    new MockPropertyInfo(), propertyConfiguration, new ModelConfiguration(),
                    new DatabaseGeneratedAttribute(DatabaseGeneratedOption.None));

            Assert.Equal(DatabaseGeneratedOption.Computed, propertyConfiguration.DatabaseGeneratedOption);
        }
    }
}
