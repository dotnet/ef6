// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using Moq;
    using Xunit;

    public class SqlSpatialServicesForConversionsTests
    {
        [Fact]
        public void SqlTypes_returns_the_assembly_set_in_the_constructor()
        {
            var sqlTypesAssembly = new Mock<SqlTypesAssembly>().Object;

            Assert.Same(sqlTypesAssembly, new SqlSpatialServicesForConversions(sqlTypesAssembly).SqlTypes);
        }
    }
}
