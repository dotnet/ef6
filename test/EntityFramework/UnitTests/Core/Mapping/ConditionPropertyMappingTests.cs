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
            var column1 = new EdmProperty("C");
            var conditionPropertyMapping
                = new ConditionPropertyMapping(null, column1, 42, null);

            Assert.Same(column1, conditionPropertyMapping.Column);

            var column2 = new EdmProperty("C");

            conditionPropertyMapping.ColumnProperty = column2;

            Assert.Same(column2, conditionPropertyMapping.Column);
        }        

        [Fact]
        public void Can_get_and_set_Value()
        {
            var conditionPropertyMapping
                = new ConditionPropertyMapping(null, new EdmProperty("C"), 42, null);
            
            Assert.Equal(42, conditionPropertyMapping.Value);
            Assert.Null(conditionPropertyMapping.IsNull);
        }

        [Fact]
        public void Can_get_and_set_IsNull()
        {
            var conditionPropertyMapping
                = new ConditionPropertyMapping(null, new EdmProperty("C"), null, false);

            Assert.Null(conditionPropertyMapping.Value);
            Assert.False((bool)conditionPropertyMapping.IsNull);
        }
    }
}
