// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Data.Entity.Migrations.Sql;
    using System.Linq;
    using Xunit;

    public class MigrationsConfigurationResolverTests : TestBase
    {
        [Fact]
        public void SQL_Server_and_SQL_compact_generators_are_registered_by_default()
        {
            var resolver = new MigrationsConfigurationResolver();

            Assert.IsType<SqlServerMigrationSqlGenerator>(resolver.GetService<MigrationSqlGenerator>("System.Data.SqlClient"));
            Assert.IsType<SqlCeMigrationSqlGenerator>(resolver.GetService<MigrationSqlGenerator>("System.Data.SqlServerCe.4.0"));
        }

        [Fact]
        public void A_new_instance_is_returned_each_time_GetService_is_called()
        {
            var resolver = new MigrationsConfigurationResolver();

            Assert.NotSame(
                resolver.GetService<MigrationSqlGenerator>("System.Data.SqlClient"),
                resolver.GetService<MigrationSqlGenerator>("System.Data.SqlClient"));

            Assert.NotSame(
                resolver.GetService<MigrationSqlGenerator>("System.Data.SqlServerCe.4.0"),
                resolver.GetService<MigrationSqlGenerator>("System.Data.SqlServerCe.4.0"));
        }

        [Fact]
        public void Release_does_not_throw()
        {
            new MigrationsConfigurationResolver().Release(new object());
        }

        /// <summary>
        ///     This test makes calls from multiple threads such that we have at least some chance of finding threading
        ///     issues. As with any test of this type just because the test passes does not mean that the code is
        ///     correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
        ///     be investigated. DON'T just re-run and think things are okay if the test then passes.
        /// </summary>
        [Fact]
        public void GetService_can_be_accessed_from_multiple_threads_concurrently()
        {
            for (var i = 0; i < 30; i++)
            {
                var bag = new ConcurrentBag<MigrationSqlGenerator>();
                var resolver = new MigrationsConfigurationResolver();

                ExecuteInParallel(() => bag.Add(resolver.GetService<MigrationSqlGenerator>("System.Data.SqlClient")));

                Assert.Equal(20, bag.Count);
                foreach (var generator in bag)
                {
                    Assert.IsType<SqlServerMigrationSqlGenerator>(generator);
                    Assert.Equal(19, bag.Count(c => generator != c));
                }
            }
        }
    }
}
