// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Infrastructure;
    using Xunit;

    public sealed class DbProviderInfoExtensionsTests
    {
        [Fact]
        public void IsSqlCe_returns_true_when_provider_any_version_of_ce_provider()
        {
            Assert.True(new DbProviderInfo("System.Data.SqlServerCe.3.0", "3.0").IsSqlCe());
            Assert.True(new DbProviderInfo("System.Data.SqlServerCe.4.0", "4.0").IsSqlCe());
            Assert.True(new DbProviderInfo("System.Data.SqlServerCe.5.0", "5.0").IsSqlCe());
        }
    }
}
