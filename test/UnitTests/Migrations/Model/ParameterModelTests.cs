// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class ParameterModelTests
    {
        [Fact]
        public void Can_get_and_set_properties()
        {
            var parameterModel
                = new ParameterModel(PrimitiveTypeKind.Guid)
                      {
                          Name = "C",
                          IsFixedLength = true,
                          IsUnicode = true,
                          MaxLength = 42,
                          Precision = 23,
                          Scale = 1,
                          StoreType = "goobar"
                      };

            Assert.Equal("C", parameterModel.Name);
            Assert.Equal(PrimitiveTypeKind.Guid, parameterModel.Type);
            Assert.True(parameterModel.IsFixedLength.Value);
            Assert.True(parameterModel.IsUnicode.Value);
            Assert.Equal(42, parameterModel.MaxLength);
            Assert.Equal((byte)23, parameterModel.Precision);
            Assert.Equal((byte)1, parameterModel.Scale);
            Assert.Equal("goobar", parameterModel.StoreType);
        }
    }
}
