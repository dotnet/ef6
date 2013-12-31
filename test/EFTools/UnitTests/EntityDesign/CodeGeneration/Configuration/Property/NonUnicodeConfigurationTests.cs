// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Xunit;

    public class NonUnicodeConfigurationTests
    {
        [Fact]
        public void GetMethodChain_returns_chain()
        {
            var configuration = new NonUnicodeConfiguration();
            var code = new CSharpCodeHelper();

            Assert.Equal(".IsUnicode(false)", configuration.GetMethodChain(code));
        }
    }
}
