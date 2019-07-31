// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class FunctionImportEntityTypeMappingConditionValueTests
    {
        [Fact]
        public void Cannot_create_with_null_columnName()
        {
            Assert.Equal(
                "columnName",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportEntityTypeMappingConditionValue(
                        null, new object())).ParamName);
        }

        [Fact]
        public void Cannot_create_with_null_value()
        {
            Assert.Equal(
                "value",
                Assert.Throws<ArgumentNullException>(
                    () => new FunctionImportEntityTypeMappingConditionValue(
                        "C", null)).ParamName);
        }

        [Fact]
        public void Can_create_and_retrieve_ColumnName_and_Value()
        {
            var mappingCondition = new FunctionImportEntityTypeMappingConditionValue("C", 4);

            Assert.Equal("C", mappingCondition.ColumnName);
            Assert.Equal(4, mappingCondition.Value);
        }

        [Fact]
        public void GetConditionValue_returns_null_if_type_does_not_match()
        {
            var mappingCondition = new FunctionImportEntityTypeMappingConditionValue("C", 4);
            var actionId = 0;

            var value = mappingCondition.GetConditionValue(
                typeof(string),
                handleTypeNotComparable: () => { actionId = 1; },
                handleInvalidConditionValue: () => { actionId = 2; });

            Assert.Equal(null, value);
            Assert.Equal(2, actionId);
        }

        [Fact]
        public void GetConditionValue_returns_value_if_type_matches()
        {
            var mappingCondition = new FunctionImportEntityTypeMappingConditionValue("C", 4);
            var actionId = 0;

            var value = mappingCondition.GetConditionValue(
                typeof(int),
                handleTypeNotComparable: () => { actionId = 1; },
                handleInvalidConditionValue: () => { actionId = 2; });

            Assert.Equal(4, value);
            Assert.Equal(0, actionId);
        }
    }
}
