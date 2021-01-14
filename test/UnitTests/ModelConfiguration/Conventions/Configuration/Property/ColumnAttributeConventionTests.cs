// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using Xunit;
    using PrimitivePropertyConfiguration = System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration;

    public sealed class ColumnAttributeConventionTests
    {
        [Fact]
        public void Apply_should_set_column_name_type_and_order()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration();

            new ColumnAttributeConvention()
                .Apply(new ConventionPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration),
                new ColumnAttribute("Foo"){TypeName = "bar", Order = 1});

            Assert.Equal("Foo", propertyConfiguration.ColumnName);
            Assert.Equal("bar", propertyConfiguration.ColumnType);
            Assert.Equal(1, propertyConfiguration.ColumnOrder);
        }

        [Fact]
        public void Apply_should_not_set_column_name_type_or_when_already_set()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration
                                            {
                                                ColumnName = "Bar",
                                                ColumnType = "foo",
                                                ColumnOrder = 0
                                            };

            new ColumnAttributeConvention()
                .Apply(new ConventionPrimitivePropertyConfiguration(new MockPropertyInfo(), () => propertyConfiguration), new ColumnAttribute("Foo"){TypeName = "bar", Order = 1});

            Assert.Equal("Bar", propertyConfiguration.ColumnName);
            Assert.Equal("foo", propertyConfiguration.ColumnType);
            Assert.Equal(0, propertyConfiguration.ColumnOrder);
        }
    }
}
