// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.SqlServer;
    using Moq;
    using Xunit;

    public class SpatialServicesLoaderTests
    {
        [Fact]
        public void SpatialServicesLoader_uses_resolver_to_obtain_spatial_services()
        {
            var mockSpatialServices = new Mock<DbSpatialServices>();
            mockSpatialServices.Setup(m => m.NativeTypesAvailable).Returns(true);

            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(m => m.GetService(typeof(DbSpatialServices), null)).Returns(mockSpatialServices.Object);

            Assert.Same(mockSpatialServices.Object, new SpatialServicesLoader(mockResolver.Object).LoadDefaultServices());
        }

        [Fact]
        public void SpatialServicesLoader_uses_SQL_Server_spatial_types_if_resolver_doesnt_provide_services()
        {
            Assert.Same(
                SqlSpatialServices.Instance,
                new SpatialServicesLoader(DbConfiguration.DependencyResolver).LoadDefaultServices());
        }

        [Fact]
        public void SpatialServicesLoader_uses_default_spatial_services_if_SQL_types_are_not_available()
        {
            Assert.Same(
                DefaultSpatialServices.Instance, new SpatialServicesLoader(new Mock<IDbDependencyResolver>().Object).LoadDefaultServices());
        }

        [Fact]
        public void SpatialServicesLoader_uses_default_spatial_services_if_SQL_spatial_types_are_not_available()
        {
            var mockSpatialServices = new Mock<DbSpatialServices>();
            mockSpatialServices.Setup(m => m.NativeTypesAvailable).Returns(false);

            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(m => m.GetService(typeof(DbSpatialServices), new DbProviderInfo("System.Data.SqlClient", "2012")))
                        .Returns(mockSpatialServices.Object);

            Assert.Same(DefaultSpatialServices.Instance, new SpatialServicesLoader(mockResolver.Object).LoadDefaultServices());
        }
    }
}
