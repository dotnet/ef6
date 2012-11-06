// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class StorageScalarPropertyMappingTests
    {
        [Fact]
        public void Can_get_and_set_column_property()
        {
            var column = new EdmProperty("C");
            var scalarPropertyMapping = new StorageScalarPropertyMapping(new EdmProperty("P"), column);

            Assert.Same(column, scalarPropertyMapping.ColumnProperty);
        
            column = new EdmProperty("C'");

            scalarPropertyMapping.ColumnProperty = column;

            Assert.Same(column, scalarPropertyMapping.ColumnProperty);
        }
    }
}