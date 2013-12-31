// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class DatabaseGeneratedConfigurationTests
    {
        [Fact]
        public void GetAttributeBody_returns_body()
        {
            var configuration = new DatabaseGeneratedConfiguration
                {
                    StoreGeneratedPattern = StoreGeneratedPattern.Computed
                };
            var code = new CSharpCodeHelper();

            Assert.Equal("DatabaseGenerated(DatabaseGeneratedOption.Computed)", configuration.GetAttributeBody(code));
        }

        [Fact]
        public void GetMethodChain_returns_chain()
        {
            var configuration = new DatabaseGeneratedConfiguration
                {
                    StoreGeneratedPattern = StoreGeneratedPattern.Computed
                };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                ".HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed)",
                configuration.GetMethodChain(code));
        }
    }
}
