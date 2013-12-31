// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Xunit;

    public class MaxLengthConfigurationTests
    {
        [Fact]
        public void GetAttributeBody_returns_body()
        {
            var configuration = new MaxLengthConfiguration { MaxLength = 30 };
            var code = new CSharpCodeHelper();

            Assert.Equal("MaxLength(30)", configuration.GetAttributeBody(code));
        }

        [Fact]
        public void GetMethodChain_returns_chain()
        {
            var configuration = new MaxLengthConfiguration { MaxLength = 30 };
            var code = new CSharpCodeHelper();

            Assert.Equal(".HasMaxLength(30)", configuration.GetMethodChain(code));
        }
    }
}
