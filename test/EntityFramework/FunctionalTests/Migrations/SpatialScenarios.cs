// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Spatial;
    using System.Data.SqlClient;
    using Xunit;

    /// <summary>
    /// Note that AutoAndGenerateScenarios.cs also has some spatial tests.
    /// </summary>
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class SpatialScenarios : DbTestCase
    {
        private class AlterSpatialColumnWithDefaultMigration : DbMigration
        {
            public override void Up()
            {
                AlterColumn("WithSpatials", "Location", c => c.String());
            }
        }

        [MigrationsTheory] // CodePlex 478
        public void Changing_spatial_column_to_some_other_column_type_throws_from_SQL_provider()
        {
            var migrator = CreateMigrator<AlterSpatialContext>();

            migrator.Update();

            migrator = CreateMigrator<AlterSpatialContext>(new AlterSpatialColumnWithDefaultMigration());

            var message = Assert.Throws<SqlException>(() => migrator.Update()).Message;

            // Message comes from SQL so not verifying full message
            Assert.Contains("geography", message);
            Assert.Contains("nvarchar(max)", message);
        }

        public class AlterSpatialContext : DbContext
        {
            public DbSet<WithSpatials> WithSpatials { get; set; }
        }

        public class WithSpatials
        {
            public int Id { get; set; }
            public DbGeography Location { get; set; }
        }
    }
}
