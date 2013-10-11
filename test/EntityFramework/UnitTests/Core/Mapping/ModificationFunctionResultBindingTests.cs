// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class ModificationFunctionResultBindingTests
    {
        [Fact]
        public void Cannot_create_with_null_argument()
        {
            var property = EdmProperty.CreatePrimitive(
                "P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            Assert.Equal(
                "columnName",
                Assert.Throws<ArgumentNullException>(
                    () => new ModificationFunctionResultBinding(
                        null, property)).ParamName);

            Assert.Equal(
                "property",
                Assert.Throws<ArgumentNullException>(
                    () => new ModificationFunctionResultBinding(
                        "C", null)).ParamName);            
        }

        [Fact]
        public void Can_retrieve_properties()
        {
            var property = EdmProperty.CreatePrimitive(
                "P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var resultBinding = new ModificationFunctionResultBinding("C", property);

            Assert.Equal("C", resultBinding.ColumnName);
            Assert.Same(property, resultBinding.Property);
        }
    }
}
