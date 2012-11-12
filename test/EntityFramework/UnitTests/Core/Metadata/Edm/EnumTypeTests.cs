// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class EnumTypeTests
    {
        [Fact]
        public void Can_set_and_get_underlying_type()
        {
            var enumType = new EnumType();

            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64);

            enumType.UnderlyingType = primitiveType;

            Assert.Same(primitiveType, enumType.UnderlyingType);
        }
    }
}
