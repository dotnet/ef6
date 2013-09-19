// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class DefaultValueScenarios : DbTestCase
    {
        private class ColumnClashMigration : DbMigration
        {
            public override void Up()
            {
                CreateTable(
                    "TableA",
                    c => new
                             {
                                 c = c.Int()
                             });

                CreateTable(
                    "TableB",
                    c => new
                             {
                                 c = c.Int()
                             });

                AlterColumn("TableA", "c", c => c.Int(defaultValue: 42));
                AlterColumn("TableB", "c", c => c.Int(defaultValue: 42));
            }
        }

        [MigrationsTheory]
        public void Can_create_constraints_and_names_are_table_qualified()
        {
            ResetDatabase();

            var migrator = CreateMigrator<BlankSlate>(new ColumnClashMigration());

            migrator.Update();
        }

        private class AlterDefaultMigration : DbMigration
        {
            public override void Up()
            {
                CreateTable(
                    "TableA",
                    c => new
                    {
                        c = c.Int(defaultValue: 42)
                    });

                AlterColumn("TableA", "c", c => c.Int(defaultValue: 43));
            }
        }

        [MigrationsTheory]
        public void Can_alter_default_constraint()
        {
            ResetDatabase();

            var migrator = CreateMigrator<BlankSlate>(new AlterDefaultMigration());

            migrator.Update();
        }

        private class DefaultValuesMigration : DbMigration
        {
            public override void Up()
            {
                CreateTable(
                    "DefaultValues",
                    c => new
                             {
                                 Binary = c.Binary(defaultValue: new byte[] { }),
                                 Boolean = c.Boolean(defaultValue: true),
                                 Byte = c.Byte(defaultValue: 42),
                                 DateTime = c.DateTime(defaultValue: new DateTime()),
                                 DateTimeOffset = c.DateTimeOffset(defaultValue: new DateTimeOffset()),
                                 Decimal = c.Decimal(defaultValue: 42.23m),
                                 Double = c.Double(defaultValue: 123.45),
                                 Guid = c.Guid(defaultValue: new Guid()),
                                 Int = c.Int(defaultValue: 0),
                                 Long = c.Long(defaultValue: 3456789),
                                 Short = c.Short(defaultValue: 256),
                                 Single = c.Single(defaultValue: 234.999f),
                                 String = c.String(defaultValue: string.Empty),
                                 Time = c.Time(defaultValue: TimeSpan.Zero)
                             });
            }
        }

        [MigrationsTheory]
        public void Can_create_columns_with_default_values_for_all_types()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            var defaultValuesMigration = new DefaultValuesMigration();

            var createTableOperation
                = (CreateTableOperation)defaultValuesMigration.GetOperations().Single();

            WhenSqlCe(
                () =>
                    {
                        createTableOperation.Columns.Remove(createTableOperation.Columns.Single(c => c.Name == "DateTimeOffset"));
                        createTableOperation.Columns.Remove(createTableOperation.Columns.Single(c => c.Name == "Time"));
                    });

            migrator = CreateMigrator<ShopContext_v1>(defaultValuesMigration);

            migrator.Update();

            var table = Info.Tables.Single(t => t.Name == "DefaultValues");
            Assert.True(table.Columns.Any(c => c.Name == "Binary" && c.Default.Contains("0x")));
            Assert.True(table.Columns.Any(c => c.Name == "Boolean" && c.Default.Contains("1")));
            Assert.True(table.Columns.Any(c => c.Name == "Byte" && c.Default.Contains("42")));
            Assert.True(table.Columns.Any(c => c.Name == "DateTime" && c.Default.Contains("'0001-01-01T00:00:00.000'")));
            Assert.True(table.Columns.Any(c => c.Name == "Decimal" && c.Default.Contains("42.23")));
            Assert.True(table.Columns.Any(c => c.Name == "Double" && c.Default.Contains("123.45")));
            Assert.True(table.Columns.Any(c => c.Name == "Guid" && c.Default.Contains("'00000000-0000-0000-0000-000000000000'")));
            Assert.True(table.Columns.Any(c => c.Name == "Int" && c.Default.Contains("0")));
            Assert.True(table.Columns.Any(c => c.Name == "Long" && c.Default.Contains("3456789")));
            Assert.True(table.Columns.Any(c => c.Name == "Short" && c.Default.Contains("256")));
            Assert.True(table.Columns.Any(c => c.Name == "Single" && c.Default.Contains("234.999")));
            Assert.True(table.Columns.Any(c => c.Name == "String" && c.Default.Contains("''")));

            WhenNotSqlCe(
                () =>
                    {
                        Assert.True(table.Columns.Any(c => c.Name == "DateTimeOffset" && c.Default == "('0001-01-01T00:00:00.000+00:00')"));
                        Assert.True(table.Columns.Any(c => c.Name == "Time" && c.Default == "('00:00:00')"));
                    });
        }

        private class DefaultValueSqlMigration : DbMigration
        {
            public override void Up()
            {
                CreateTable(
                    "DefaultValueSql",
                    c => new
                             {
                                 Binary = c.Binary(defaultValueSql: "CONVERT([binary],'123')"),
                                 Boolean = c.Boolean(defaultValueSql: "CONVERT([bit],'1')"),
                                 Byte = c.Byte(defaultValueSql: "CONVERT([tinyint],'123')"),
                                 DateTime = c.DateTime(defaultValueSql: "CONVERT([datetime],'1947/08/15 03:33:20')"),
                                 DateTimeOffset = c.DateTimeOffset(defaultValueSql: "CONVERT([datetimeoffset],'1947/08/15 03:33:20')"),
                                 Decimal = c.Decimal(defaultValueSql: "CONVERT([money],'123')"),
                                 Double = c.Double(defaultValueSql: "CONVERT([float],'123')"),
                                 Guid = c.Guid(defaultValueSql: "CONVERT([uniqueidentifier],'123')"),
                                 Int = c.Int(defaultValueSql: "CONVERT([int],'123')"),
                                 Long = c.Long(defaultValueSql: "CONVERT([bigint],'123')"),
                                 Short = c.Short(defaultValueSql: "CONVERT([smallint],'123')"),
                                 Single = c.Single(defaultValueSql: "CONVERT([real],'123')"),
                                 String = c.String(defaultValueSql: "CONVERT([nvarchar](100),(123))"),
                                 Time = c.Time(defaultValueSql: "CONVERT([time],'03:33:20')")
                             });
            }
        }

        [MigrationsTheory]
        public void Can_create_columns_with_default_value_expressions_for_all_types()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            var defaultValuesMigration = new DefaultValueSqlMigration();

            var createTableOperation
                = (CreateTableOperation)defaultValuesMigration.GetOperations().Single();

            WhenSqlCe(
                () =>
                    {
                        createTableOperation.Columns.Remove(createTableOperation.Columns.Single(c => c.Name == "DateTimeOffset"));
                        createTableOperation.Columns.Remove(createTableOperation.Columns.Single(c => c.Name == "Time"));
                    });

            migrator = CreateMigrator<ShopContext_v1>(defaultValuesMigration);

            migrator.Update();

            var table = Info.Tables.Single(t => t.Name == "DefaultValueSql");
            Assert.True(table.Columns.Any(c => c.Name == "Binary" && c.Default.Contains("CONVERT([binary],'123'")));
            Assert.True(table.Columns.Any(c => c.Name == "Boolean" && c.Default.Contains("CONVERT([bit],'1'")));
            Assert.True(table.Columns.Any(c => c.Name == "Byte" && c.Default.Contains("CONVERT([tinyint],'123'")));
            Assert.True(table.Columns.Any(c => c.Name == "DateTime" && c.Default.Contains("CONVERT([datetime],'1947/08/15 03:33:20'")));
            Assert.True(table.Columns.Any(c => c.Name == "Decimal" && c.Default.Contains("CONVERT([money],'123'")));
            Assert.True(table.Columns.Any(c => c.Name == "Double" && c.Default.Contains("CONVERT([float],'123'")));
            Assert.True(table.Columns.Any(c => c.Name == "Guid" && c.Default.Contains("CONVERT([uniqueidentifier],'123'")));
            Assert.True(table.Columns.Any(c => c.Name == "Int" && c.Default.Contains("CONVERT([int],'123'")));
            Assert.True(table.Columns.Any(c => c.Name == "Long" && c.Default.Contains("CONVERT([bigint],'123'")));
            Assert.True(table.Columns.Any(c => c.Name == "Short" && c.Default.Contains("CONVERT([smallint],'123'")));
            Assert.True(table.Columns.Any(c => c.Name == "Single" && c.Default.Contains("CONVERT([real],'123'")));
            Assert.True(table.Columns.Any(c => c.Name == "String" && c.Default.Contains("CONVERT([nvarchar](100),(123)")));

            WhenNotSqlCe(
                () =>
                    {
                        Assert.True(
                            table.Columns.Any(
                                c => c.Name == "DateTimeOffset" && c.Default.StartsWith("(CONVERT([datetimeoffset],'1947/08/15 03:33:20'")));
                        Assert.True(table.Columns.Any(c => c.Name == "Time" && c.Default.StartsWith("(CONVERT([time],'03:33:20'")));
                    });
        }
    }
}
