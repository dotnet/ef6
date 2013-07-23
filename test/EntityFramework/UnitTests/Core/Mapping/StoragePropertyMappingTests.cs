// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class StoragePropertyMappingTests
    {
        [Fact]
        public void Can_get_set_property()
        {
            var property = EdmProperty.CreatePrimitive("p", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));

            Assert.Same(
                property,
                new StoragePropertyMappingFake(property).EdmProperty);
        }

        private class StoragePropertyMappingFake : StoragePropertyMapping
        {
            public StoragePropertyMappingFake(EdmProperty property)
                : base(property)
            {
            }
        }
    }
}
