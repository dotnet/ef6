// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact
{
    using System.Data.Entity.Core;
    using System.Data.Entity.SqlServerCompact.Resources;
    using Xunit;

    public class SqlCeProviderServicesTests : TestBase
    {
        [Fact]
        public void GetProviderManifest_throws_when_empty()
        {
            var ex = Assert.Throws<ProviderIncompatibleException>(
                () => new SqlCeProviderServices().GetProviderManifest(string.Empty));

            // NOTE: Verifying base exception since DbProviderServices wraps errors
            var baseException = ex.GetBaseException();

            Assert.IsType<ArgumentException>(baseException);
            Assert.Equal(Strings.UnableToDetermineStoreVersion, baseException.Message);
        }
    }
}
