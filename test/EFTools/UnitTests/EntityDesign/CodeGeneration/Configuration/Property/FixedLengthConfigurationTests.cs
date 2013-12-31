// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Xunit;

    public class FixedLengthConfigurationTests
    {
        [Fact]
        public void GetMethodChain_returns_chain()
        {
            var configuration = new FixedLengthConfiguration();
            var code = new CSharpCodeHelper();

            Assert.Equal(".IsFixedLength()", configuration.GetMethodChain(code));
        }
    }
}
