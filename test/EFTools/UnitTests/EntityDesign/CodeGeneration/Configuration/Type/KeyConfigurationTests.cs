// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class KeyConfigurationTests
    {
        [Fact]
        public void GetMethodChain_returns_chain_when_one_key_property()
        {
            var configuration = new KeyConfiguration { KeyProperties = { new EdmProperty("Id") } };
            var code = new CSharpCodeHelper();

            Assert.Equal(".HasKey(e => e.Id)", configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chain_when_more_than_one_key_property()
        {
            var configuration = new KeyConfiguration
                {
                    KeyProperties = { new EdmProperty("Id1"), new EdmProperty("Id2") }
                };
            var code = new CSharpCodeHelper();

            Assert.Equal(".HasKey(e => new { e.Id1, e.Id2 })", configuration.GetMethodChain(code));
        }
    }
}
