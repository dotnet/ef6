// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class EnumTypeExtensionsTests
    {
        [Fact]
        public void Should_be_able_to_get_and_set_clr_type()
        {
            var enumType = new EnumType();

            Assert.Null(enumType.GetClrType());

            enumType.SetClrType(typeof(object));

            Assert.Equal(typeof(object), enumType.GetClrType());
        }
    }
}
