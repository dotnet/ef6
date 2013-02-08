// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class FunctionParameterTests
    {
        [Fact]
        public void Can_get_type_name()
        {
            var function
                = new FunctionParameter(
                    "P",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    ParameterMode.InOut);

            Assert.Equal("String", function.TypeName);
        }
    }
}
