// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Xunit;

    public class PrecisionDecimalConfigurationTests
    {
        [Fact]
        public void GetMethodChain_returns_chain()
        {
            var configuration = new PrecisionDecimalConfiguration { Precision = 8, Scale = 2 };
            var code = new CSharpCodeHelper();

            Assert.Equal(".HasPrecision(8, 2)", configuration.GetMethodChain(code));
        }
    }
}
