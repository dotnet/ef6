// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class ForeignKeyConfigurationTests
    {
        [Fact]
        public void GetMethodChain_returns_chain_when_one_property()
        {
            var configuration = new ForeignKeyConfiguration { Properties = { new EdmProperty("EntityId") } };
            var code = new CSharpCodeHelper();

            Assert.Equal(".HasForeignKey(e => e.EntityId)", configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chain_when_more_than_one_property()
        {
            var configuration = new ForeignKeyConfiguration
                {
                    Properties = { new EdmProperty("EntityId1"), new EdmProperty("EntityId2") }
                };
            var code = new CSharpCodeHelper();

            Assert.Equal(".HasForeignKey(e => new { e.EntityId1, e.EntityId2 })", configuration.GetMethodChain(code));
        }
    }
}
