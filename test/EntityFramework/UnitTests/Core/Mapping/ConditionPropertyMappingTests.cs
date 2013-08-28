// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class ConditionPropertyMappingTests
    {
        [Fact]
        public void Can_get_and_set_column_property()
        {
            var columnMember1 = new EdmProperty("C");
            var conditionPropertyMapping
                = new ConditionPropertyMapping(null, columnMember1, 42, null);

            Assert.Same(columnMember1, conditionPropertyMapping.ColumnProperty);

            var columnMember2 = new EdmProperty("C");

            conditionPropertyMapping.ColumnProperty = columnMember2;

            Assert.Same(columnMember2, conditionPropertyMapping.ColumnProperty);
        }

        public void Can_get_and_set_Value()
        {
            var conditionPropertyMapping
                = new ConditionPropertyMapping(null, new EdmProperty("C"), 42, null);
            
            Assert.Equal(42, conditionPropertyMapping.Value);
            Assert.Null(conditionPropertyMapping.IsNull);
        }

        public void Can_get_and_set_IsNull()
        {
            var conditionPropertyMapping
                = new ConditionPropertyMapping(null, new EdmProperty("C"), null, false);

            Assert.Null(conditionPropertyMapping.Value);
            Assert.False((bool)conditionPropertyMapping.IsNull);
        }
    }
}
