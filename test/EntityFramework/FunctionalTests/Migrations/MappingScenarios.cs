// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
#if NET452
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
#endif
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class MappingScenarios : DbTestCase
    {
        public MappingScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        private class WeirdNamesContext : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Order>()
                    .ToTable("[orders'\"]", "[bar'\"]")
                    .MapToStoredProcedures(
                        m => m.Insert(i => i.HasName("[foo.bar'Baz\"]")));

                modelBuilder
                    .Entity<OrderLine>()
                    .ToTable("[[[order\"lines..]]]]..].ba'z")
                    .Property(p => p.OrderId).HasColumnName("[foo.bar'Baz\"]");
            }
        }

        [MigrationsTheory]
        public void Can_specify_table_and_schema_with_dot_in_name()
        {
            ResetDatabase();

            var migrator = CreateMigrator<WeirdNamesContext>();

            migrator.Update();
        }

        private class MappingScenariosContext : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsEmployee>()
                    .HasOptional(e => e.Manager)
                    .WithMany(m => m.DirectReports);
            }
        }

        [MigrationsTheory]
        public void Can_migrate_model_with_advanced_mapping_scenarios()
        {
            ResetDatabase();

            var migrator = CreateMigrator<MappingScenariosContext>();

            migrator.Update();
        }
    }
}
