// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class EdmTypeExtensionsTests
    {
        [Fact]
        public void GetClrType_returns_CLR_type_annotation_for_EntityType()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Null(((EdmType)entityType).GetClrType());

            entityType.Annotations.SetClrType(typeof(Random));

            Assert.Same(typeof(Random), ((EdmType)entityType).GetClrType());
        }

        [Fact]
        public void GetClrType_returns_CLR_type_annotation_for_ComplexType()
        {
            var complexType = new ComplexType("C", "N", DataSpace.CSpace);

            Assert.Null(((EdmType)complexType).GetClrType());

            complexType.Annotations.SetClrType(typeof(Random));

            Assert.Same(typeof(Random), ((EdmType)complexType).GetClrType());
        }

        [Fact]
        public void GetClrType_returns_CLR_type_annotation_for_EnumType()
        {
            var enumType = new EnumType();

            Assert.Null(((EdmType)enumType).GetClrType());

            enumType.Annotations.SetClrType(typeof(Random));

            Assert.Same(typeof(Random), ((EdmType)enumType).GetClrType());
        }
    }
}
