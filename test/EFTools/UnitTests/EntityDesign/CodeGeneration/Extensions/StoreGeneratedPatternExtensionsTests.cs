// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Extensions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class StoreGeneratedPatternExtensionsTests
    {
        [Fact]
        public void ToDatabaseGeneratedOption_converts_to_DatabaseGeneratedOption()
        {
            Assert.Equal(DatabaseGeneratedOption.Computed, StoreGeneratedPattern.Computed.ToDatabaseGeneratedOption());
            Assert.Equal(DatabaseGeneratedOption.Identity, StoreGeneratedPattern.Identity.ToDatabaseGeneratedOption());
            Assert.Equal(DatabaseGeneratedOption.None, StoreGeneratedPattern.None.ToDatabaseGeneratedOption());
        }

        [Fact]
        public void ToDatabaseGeneratedOption_returns_none_when_unknown()
        {
            Assert.Equal(DatabaseGeneratedOption.None, ((StoreGeneratedPattern)42).ToDatabaseGeneratedOption());
        }
    }
}
