// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class DefaultServicesTests
    {
        [Fact]
        public void Use_of_default_spatial_services_for_non_trival_operations_throws()
        {
            Assert.Equal(
                Strings.SpatialProviderNotUsable,
                Assert.Throws<NotImplementedException>(() => DefaultSpatialServices.Instance.GeographyLineFromBinary(null, 0)).Message);
        }
    }
}
