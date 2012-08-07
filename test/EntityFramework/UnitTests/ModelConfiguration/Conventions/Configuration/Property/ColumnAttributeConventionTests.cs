// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using Xunit;

    public sealed class ColumnAttributeConventionTests
    {
        [Fact]
        public void Apply_should_set_column_name()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration();

            new ColumnAttributeConvention.ColumnAttributeConventionImpl()
                .Apply(new MockPropertyInfo(), propertyConfiguration, new ColumnAttribute("Foo"));

            Assert.Equal("Foo", propertyConfiguration.ColumnName);
        }

        [Fact]
        public void Apply_should_not_set_column_name_when_already_set()
        {
            var propertyConfiguration = new PrimitivePropertyConfiguration
                                            {
                                                ColumnName = "Bar"
                                            };

            new ColumnAttributeConvention.ColumnAttributeConventionImpl()
                .Apply(new MockPropertyInfo(), propertyConfiguration, new ColumnAttribute("Foo"));

            Assert.Equal("Bar", propertyConfiguration.ColumnName);
        }
    }
}
