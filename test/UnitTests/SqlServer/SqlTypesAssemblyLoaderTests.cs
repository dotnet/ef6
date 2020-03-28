// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.SqlServer.Resources;
    using Moq;
    using Xunit;

    public class SqlTypesAssemblyLoaderTests
    {
        private const string AssemblyNameStart
            = "Microsoft.SqlServer.Types, Version=";

        private const string AssemblyNameEnd
            = ", Culture=neutral, PublicKeyToken=89845dcd8080cc91";

        private static bool IsSqlTypesAssemblyName(string assemblyName)
        {
            return assemblyName != null
                   && assemblyName.StartsWith(AssemblyNameStart, StringComparison.Ordinal)
                   && assemblyName.EndsWith(AssemblyNameEnd, StringComparison.Ordinal);
        }

        [Fact]
        public void TryGetSqlTypesAssembly_on_dev_machine_returns_assembly_for_SQL_native_types()
        {
            var assemblyName = new SqlTypesAssemblyLoader().TryGetSqlTypesAssembly().SqlGeographyType.Assembly.FullName;

            Assert.True(IsSqlTypesAssemblyName(assemblyName));
        }

        [Fact]
        public void TryGetSqlTypesAssembly_returns_null_if_native_types_are_not_available()
        {
            Assert.Null(new SqlTypesAssemblyLoader(new[] { "SomeMissingAssembly" }).TryGetSqlTypesAssembly());
        }

        [Fact]
        public void GetSqlTypesAssembly_on_dev_machine_returns_assembly_for_SQL_native_types()
        {
            var assemblyName = new SqlTypesAssemblyLoader().GetSqlTypesAssembly().SqlGeographyType.Assembly.FullName;

            Assert.True(IsSqlTypesAssemblyName(assemblyName));
        }

        [Fact]
        public void GetSqlTypesAssembly_returns_specified_assembly()
        {
            var availableAssemblyName = new SqlTypesAssemblyLoader().TryGetSqlTypesAssembly().SqlGeographyType.Assembly.FullName;
            
            SqlProviderServices.SqlServerTypesAssemblyName = availableAssemblyName;
            try
            {
                var assemblyName = new SqlTypesAssemblyLoader().GetSqlTypesAssembly().SqlGeographyType.Assembly.FullName;

                Assert.True(assemblyName == availableAssemblyName, assemblyName);
            }
            finally
            {
                SqlProviderServices.SqlServerTypesAssemblyName = null;
            }
        }

        [Fact]
        public void GetSqlTypesAssembly_throws_if_native_types_are_not_available()
        {
            Assert.Equal(
                Strings.SqlProvider_SqlTypesAssemblyNotFound,
                Assert.Throws<InvalidOperationException>(
                    () => new SqlTypesAssemblyLoader(new[] { "SomeMissingAssembly" }).GetSqlTypesAssembly()).Message);
        }

        [Fact]
        public void SqlTypes_returns_the_assembly_set_in_the_constructor()
        {
            var sqlTypesAssembly = new Mock<SqlTypesAssembly>().Object;

            Assert.Same(sqlTypesAssembly, new SqlTypesAssemblyLoader(sqlTypesAssembly).TryGetSqlTypesAssembly());
        }
    }
}
