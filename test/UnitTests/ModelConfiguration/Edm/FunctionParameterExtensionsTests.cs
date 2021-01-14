// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class FunctionParameterExtensionsTests
    {
        [Fact]
        public void Can_get_and_set_configuration_annotation()
        {
            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            property.SetConfiguration(42);

            Assert.Equal(42, property.GetConfiguration());
        }
    }
}
