// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class AlterColumnScenarios : DbTestCase
    {
        private class AlterColumnWithDefault : DbMigration
        {
            public override void Up()
            {
                AlterColumn("MigrationsCustomers", "Name", c => c.String(defaultValue: "Bill"));
            }
        }

        [MigrationsTheory]
        public void Can_change_column_to_have_default_value()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnWithDefault());

            migrator.Update();

            var column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "Name");
            Assert.True(column.Default.Contains("'Bill'"));
        }

        private class AlterColumnWithIdentityMigration : DbMigration
        {
            public override void Up()
            {
                AlterColumn("MigrationsCustomers", "CustomerNumber", c => c.Long(identity: true));
            }
        }

        [MigrationsTheory]
        public void Can_change_column_to_identity_column_when_no_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnWithIdentityMigration());

            migrator.Update();
        }

        private class AlterColumnMigration : DbMigration
        {
            public override void Up()
            {
                AlterColumn("MigrationsCustomers", "Name", c => c.String(nullable: false));
            }
        }

        [MigrationsTheory]
        public void Can_change_column_to_non_nullable_column_when_no_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnMigration());

            migrator.Update();

            var column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "Name");
            Assert.Equal("NO", column.IsNullable);
        }

        private class AlterColumnWithDefaultMigration : DbMigration
        {
            public override void Up()
            {
                AlterColumn("MigrationsCustomers", "Name", c => c.String(nullable: false, defaultValue: string.Empty));
            }
        }

        // UNDONE: Can't handle this yet (table rebuild)
        // [MigrationsTheory]
        public void Can_change_colum_to_non_nullable_column_with_default_value_when_data_present()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            using (var context = CreateContext<ShopContext_v1>())
            {
                context.Customers.Add(
                    new MigrationsCustomer
                        {
                            HomeAddress = new MigrationsAddress(),
                            WorkAddress = new MigrationsAddress(),
                            DateOfBirth = DateTime.Now
                        });
                context.SaveChanges();
            }

            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnWithDefaultMigration());

            migrator.Update();

            var column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "Name");
            Assert.Equal("NO", column.IsNullable);
            Assert.True(column.Default.Contains("''"));
        }

        private class AlterColumnWithAddedAnnotationMigration : DbMigration
        {
            public override void Up()
            {
                AlterColumn(
                    "MigrationsCustomers",
                    "Name",
                    c => c.String(
                        annotations: new Dictionary<string, AnnotationValues>
                        {
                            {
                                CollationAttribute.AnnotationName,
                                new AnnotationValues(null, new CollationAttribute("Finnish_Swedish_CS_AS"))
                            }
                        }));
            }
        }

        [MigrationsTheory]
        [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)] // No collation on add column in CE
        public void Can_alter_column_when_custom_annotation_added()
        {
            ResetDatabase();

            var sqlGenerators = new[]
            {
                Tuple.Create<string, MigrationSqlGenerator>(
                    SqlProviderServices.ProviderInvariantName,
                    new SqlServerMigrationSqlGeneratorWtihCollations()),
            };

            var migrator = CreateMigrator<ShopContext_v1>(sqlGenerators: sqlGenerators);
            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnWithAddedAnnotationMigration(), sqlGenerators);
            migrator.Update();

            var column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "Name");
            Assert.Equal("Finnish_Swedish_CS_AS", column.Collation);
        }

        private class AlterColumnWithRemovedAnnotationMigration : DbMigration
        {
            public override void Up()
            {
                AlterColumn(
                    "MigrationsCustomers",
                    "Name",
                    c => c.String(
                        annotations: new Dictionary<string, AnnotationValues>
                        {
                            {
                                CollationAttribute.AnnotationName,
                                new AnnotationValues(new CollationAttribute("Finnish_Swedish_CS_AS"), null)
                            }
                        }));
            }
        }

        [MigrationsTheory]
        [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)] // No collation on add column in CE
        public void Can_alter_column_when_custom_annotation_removed()
        {
            ResetDatabase();

            var sqlGenerators = new[]
            {
                Tuple.Create<string, MigrationSqlGenerator>(
                    SqlProviderServices.ProviderInvariantName,
                    new SqlServerMigrationSqlGeneratorWtihCollations()),
            };

            var migrator = CreateMigrator<ShopContext_v1>(sqlGenerators: sqlGenerators);
            migrator.Update();

            var column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "Name");
            var defaultCollation = column.Collation;

            // Make sure the column has non-default collation before we start
            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnWithAddedAnnotationMigration(), sqlGenerators);
            migrator.Update();

            column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "Name");
            Assert.Equal("Finnish_Swedish_CS_AS", column.Collation);

            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnWithRemovedAnnotationMigration(), sqlGenerators);
            migrator.Update();

            column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "Name");
            Assert.Equal(defaultCollation, column.Collation);
        }

        private class AlterColumnWithChangedAnnotationMigration : DbMigration
        {
            public override void Up()
            {
                AlterColumn(
                    "MigrationsCustomers",
                    "Name",
                    c => c.String(
                        annotations: new Dictionary<string, AnnotationValues>
                        {
                            {
                                CollationAttribute.AnnotationName,
                                new AnnotationValues(
                                    new CollationAttribute("Finnish_Swedish_CS_AS"), 
                                    new CollationAttribute("Icelandic_CS_AS"))
                            }
                        }));
            }
        }

        [MigrationsTheory]
        [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)] // No collation on add column in CE
        public void Can_alter_column_when_custom_annotation_changed()
        {
            ResetDatabase();

            var sqlGenerators = new[]
            {
                Tuple.Create<string, MigrationSqlGenerator>(
                    SqlProviderServices.ProviderInvariantName,
                    new SqlServerMigrationSqlGeneratorWtihCollations()),
            };

            var migrator = CreateMigrator<ShopContext_v1>(sqlGenerators: sqlGenerators);
            migrator.Update();

            // Make sure the column has non-default collation before we start
            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnWithAddedAnnotationMigration(), sqlGenerators);
            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new AlterColumnWithChangedAnnotationMigration(), sqlGenerators);
            migrator.Update();

            var column = Info.Columns.Single(c => c.TableName == "MigrationsCustomers" && c.Name == "Name");
            Assert.Equal("Icelandic_CS_AS", column.Collation);
        }

        protected override void ModifyMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            configuration.CodeGenerator.AnnotationGenerators[CollationAttribute.AnnotationName] = () => new CollationCSharpCodeGenerator();
        }
    }
}
