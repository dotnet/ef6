// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using Xunit;

    public class PropertyMappingConfigurationTests
    {
        [Fact]
        public static void ColumnName_is_set_on_internal_configuration()
        {
            var primitivePropertyConfiguration = new PrimitivePropertyConfiguration();
            primitivePropertyConfiguration.ColumnName = "A";

            Assert.Equal("A", primitivePropertyConfiguration.ColumnName);

            var propertyMappingConfiguration = 
                new PropertyMappingConfiguration(primitivePropertyConfiguration);
            propertyMappingConfiguration.HasColumnName("B");

            Assert.Equal("B", primitivePropertyConfiguration.ColumnName);
        }
    }
}
