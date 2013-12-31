// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Xunit;

    public class TimestampConfigurationTests
    {
        [Fact]
        public void GetAttributeBody_returns_body()
        {
            var configuration = new TimestampConfiguration();
            var code = new CSharpCodeHelper();

            Assert.Equal("Timestamp", configuration.GetAttributeBody(code));
        }

        [Fact]
        public void GetMethodChain_returns_chain()
        {
            var configuration = new TimestampConfiguration();
            var code = new CSharpCodeHelper();

            Assert.Equal(".IsRowVersion()", configuration.GetMethodChain(code));
        }
    }
}
