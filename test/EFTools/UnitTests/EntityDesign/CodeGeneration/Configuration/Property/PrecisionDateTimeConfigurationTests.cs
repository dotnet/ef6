// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Xunit;

    public class PrecisionDateTimeConfigurationTests
    {
        [Fact]
        public void GetMethodChain_returns_chain()
        {
            var configuration = new PrecisionDateTimeConfiguration { Precision = 4 };
            var code = new CSharpCodeHelper();

            Assert.Equal(".HasPrecision(4)", configuration.GetMethodChain(code));
        }
    }
}
