// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class PropertyMappingTests
    {
        [Fact]
        public void Can_get_set_property()
        {
            var property = EdmProperty.CreatePrimitive("p", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));

            Assert.Same(
                property,
                new PropertyMappingFake(property).Property);
        }

        private class PropertyMappingFake : PropertyMapping
        {
            public PropertyMappingFake(EdmProperty property)
                : base(property)
            {
            }
        }
    }
}
