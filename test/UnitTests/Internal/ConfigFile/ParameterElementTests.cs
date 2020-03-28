// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using Xunit;

    public class ParameterElementTests : TestBase
    {
        [Fact]
        public void ParameterElement_converts_to_valid_type()
        {
            var param = new ParameterElement(0)
            {
                ValueString = "2",
                TypeName = "System.Int32"
            };

            Assert.Equal(2, param.GetTypedParameterValue());
        }

        [Fact]
        public void ParameterElement_throws_converting_to_invalid_type()
        {
            var param = new ParameterElement(0)
            {
                ValueString = "MyValue",
                TypeName = "Not.A.Type"
            };

            Assert.True(Assert.Throws<TypeLoadException>(() => param.GetTypedParameterValue()).Message.Contains("Not.A.Type"));
        }

        [Fact]
        public void ParameterElement_throws_converting_to_incompatible_type()
        {
            var param = new ParameterElement(0)
            {
                ValueString = "MyValue",
                TypeName = "System.Int32"
            };

            Assert.Throws<FormatException>(() => param.GetTypedParameterValue());
        }
    }
}
