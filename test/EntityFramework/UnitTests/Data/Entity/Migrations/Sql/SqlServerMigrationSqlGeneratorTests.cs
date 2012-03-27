namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Metadata.Edm;
    using System.Data.Spatial;
    using System.Diagnostics;
    using Xunit;

    public class SqlServerMigrationSqlGeneratorTests
    {
        [Fact]
        public void Generate_can_output_add_timestamp_column_operation()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var addColumnOperation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Binary)
                    {
                        IsNullable = false,
                        Name = "C",
                        IsTimestamp = true
                    });

            var sql = migrationProvider.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("ALTER TABLE [T] ADD [C] rowversion NOT NULL"));
        }

        [Fact]
        public void Generate_can_output_add_rowversion_store_type_column_operation()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var addColumnOperation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Binary)
                    {
                        IsNullable = false,
                        Name = "C",
                        StoreType = "RowVersion"
                    });

            var sql = migrationProvider.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("ALTER TABLE [T] ADD [C] [RowVersion] NOT NULL"));
        }

        [Fact]
        public void Generate_can_output_add_timestamp_store_type_column_operation()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var addColumnOperation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Binary)
                    {
                        IsNullable = false,
                        Name = "C",
                        StoreType = "timestamp"
                    });

            var sql = migrationProvider.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("ALTER TABLE [T] ADD [C] [timestamp] NOT NULL"));
        }

        [Fact]
        public void Generate_can_output_drop_primary_key_operation()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation { Table = "T" };

            var sql = migrationProvider.Generate(new[] { dropPrimaryKeyOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("ALTER TABLE [T] DROP CONSTRAINT [PK_T]"));
        }

        [Fact]
        public void Generate_can_output_add_primary_key_operation()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation { Table = "T" };

            addPrimaryKeyOperation.Columns.Add("c1");
            addPrimaryKeyOperation.Columns.Add("c2");

            var sql = migrationProvider.Generate(new[] { addPrimaryKeyOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("ALTER TABLE [T] ADD CONSTRAINT [PK_T] PRIMARY KEY ([c1], [c2])"));
        }

        [Fact]
        public void Generate_can_output_drop_column()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var dropColumnOperation = new DropColumnOperation("Customers", "Foo");

            var sql = migrationProvider.Generate(new[] { dropColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("ALTER TABLE [Customers] DROP COLUMN [Foo]"));
        }

        [Fact]
        public void Generate_can_output_timestamp_column()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var createTableOperation = new CreateTableOperation("Customers");
            var column = new ColumnModel(PrimitiveTypeKind.Binary) { Name = "Version", IsTimestamp = true };
            createTableOperation.Columns.Add(column);

            var sql = migrationProvider.Generate(new[] { createTableOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains(@"[Version] rowversion"));
        }

        [Fact]
        public void Generate_can_output_custom_sql_operation()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var sql = migrationProvider.Generate(new[] { new SqlOperation("insert into foo") }, "2008").Join(
                s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains(@"insert into foo"));
        }

        [Fact]
        public void Generate_can_output_create_table_statement()
        {
            var createTableOperation = new CreateTableOperation("foo.Customers");
            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32) { Name = "Id", IsNullable = true, IsIdentity = true };
            createTableOperation.Columns.Add(idColumn);
            createTableOperation.Columns.Add(new ColumnModel(PrimitiveTypeKind.String) { Name = "Name", IsNullable = false });
            createTableOperation.PrimaryKey = new AddPrimaryKeyOperation();
            createTableOperation.PrimaryKey.Columns.Add(idColumn.Name);

            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var sql = migrationProvider.Generate(new[] { createTableOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(
                sql.Contains(
                    @"IF schema_id('foo') IS NULL
    EXECUTE('CREATE SCHEMA [foo]')
CREATE TABLE [foo].[Customers] (
    [Id] [int] IDENTITY,
    [Name] [nvarchar](max) NOT NULL,
    CONSTRAINT [PK_foo.Customers] PRIMARY KEY ([Id])
)"));
        }

        [Fact]
        public void Generate_can_output_create_table_as_system_object_statement()
        {
            var createTableOperation = new CreateTableOperation("Customers", new { IsMSShipped = true });
            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32) { Name = "Id", IsNullable = true, IsIdentity = true };
            createTableOperation.Columns.Add(idColumn);
            createTableOperation.Columns.Add(new ColumnModel(PrimitiveTypeKind.String) { Name = "Name", IsNullable = false });

            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var sql = migrationProvider.Generate(new[] { createTableOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(
                sql.Contains(
                    @"CREATE TABLE [Customers] (
    [Id] [int] IDENTITY,
    [Name] [nvarchar](max) NOT NULL
)
BEGIN TRY
    EXEC sp_MS_marksystemobject 'Customers'
END TRY
BEGIN CATCH
END CATCH"));
        }

        [Fact]
        public void Generate_can_output_create_index_statement()
        {
            var createTableOperation = new CreateTableOperation("Customers");
            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32) { Name = "Id", IsNullable = true, IsIdentity = true };
            createTableOperation.Columns.Add(idColumn);
            createTableOperation.Columns.Add(new ColumnModel(PrimitiveTypeKind.String) { Name = "Name", IsNullable = false });
            createTableOperation.PrimaryKey = new AddPrimaryKeyOperation();
            createTableOperation.PrimaryKey.Columns.Add(idColumn.Name);

            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var createIndexOperation = new CreateIndexOperation
                {
                    Table = createTableOperation.Name,
                    IsUnique = true
                };

            createIndexOperation.Columns.Add(idColumn.Name);

            var sql
                = migrationProvider.Generate(
                    new[]
                            {
                                createIndexOperation
                            },
                    "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(
                sql.Contains(
                    @"CREATE UNIQUE INDEX [IX_Id] ON [Customers]([Id])"));
        }

        [Fact]
        public void Generate_can_output_add_fk_statement()
        {
            var addForeignKeyOperation = new AddForeignKeyOperation
                {
                    PrincipalTable = "Customers",
                    DependentTable = "Orders",
                    CascadeDelete = true
                };
            addForeignKeyOperation.PrincipalColumns.Add("CustomerId");
            addForeignKeyOperation.DependentColumns.Add("CustomerId");

            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var sql = migrationProvider.Generate(new[] { addForeignKeyOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(
                sql.Contains(
                    @"ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE"));
        }

        [Fact]
        public void Generate_can_output_drop_table_statement()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var sql = migrationProvider.Generate(new[] { new DropTableOperation("Customers") }, "2008").Join(
                s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("DROP TABLE [Customers]"));
        }

        [Fact]
        public void Generate_can_output_insert_history_statement()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var insertHistoryOperation
                = new InsertHistoryOperation(
                    "Foo",
                    "Migration1",
                    new byte[] { 0xBE, 0xEF }
                    );

            var sql =
                migrationProvider.Generate(
                    new[] { insertHistoryOperation },
                    "2008").Join(s => s.Sql, Environment.NewLine);

            var expectedVersion = FileVersionInfo.GetVersionInfo(typeof(DbMigrator).Assembly.Location).ProductVersion;

            Assert.True(
                sql.Contains(
                    "INSERT INTO [Foo] ([MigrationId], [Model], [ProductVersion]) VALUES ('Migration1', 0xBEEF, '" + expectedVersion + "')"));
        }

        [Fact]
        public void Generate_can_output_add_column_statement()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Guid) { Name = "Bar", IsIdentity = true };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationProvider.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("ALTER TABLE [Foo] ADD [Bar] [uniqueidentifier] DEFAULT newid()"));
        }

        [Fact]
        public void Generate_can_output_add_column_statement_with_custom_store_type()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.String) { Name = "Bar", StoreType = "varchar", MaxLength = 15 };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationProvider.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("ALTER TABLE [Foo] ADD [Bar] [varchar](15)"));
        }

        [Fact]
        public void Generate_can_output_add_geography_column_operation_with_default_value()
        {
            var operation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geography)
                    {
                        IsNullable = false,
                        Name = "C",
                        DefaultValue = DbGeography.FromText("POINT (6 7)")
                    });

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal("ALTER TABLE [T] ADD [C] [geography] NOT NULL DEFAULT 'SRID=4326;POINT (6 7)'", sql);
        }

        [Fact]
        public void Generate_can_output_add_geometry_column_operation_with_default_value()
        {
            var operation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geometry)
                    {
                        IsNullable = false,
                        Name = "C",
                        DefaultValue = DbGeometry.FromText("POINT (8 9)")
                    });

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal("ALTER TABLE [T] ADD [C] [geometry] NOT NULL DEFAULT 'SRID=0;POINT (8 9)'", sql);
        }

        [Fact]
        public void Generate_can_output_add_geography_column_operation_with_implicit_default_value()
        {
            var operation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geography)
                    {
                        IsNullable = false,
                        Name = "C"
                    });

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal("ALTER TABLE [T] ADD [C] [geography] NOT NULL DEFAULT 'SRID=4326;POINT (0 0)'", sql);
        }

        [Fact]
        public void Generate_can_output_add_geometry_column_operation_with_implicit_default_value()
        {
            var operation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geometry)
                    {
                        IsNullable = false,
                        Name = "C"
                    });

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal("ALTER TABLE [T] ADD [C] [geometry] NOT NULL DEFAULT 'SRID=0;POINT (0 0)'", sql);
        }

        [Fact]
        public void Generate_can_output_add_geography_column_operation_with_SQL_default_value()
        {
            var operation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geography)
                    {
                        IsNullable = false,
                        Name = "C",
                        DefaultValueSql = "'POINT (6 7)'"
                    });

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal("ALTER TABLE [T] ADD [C] [geography] NOT NULL DEFAULT 'POINT (6 7)'", sql);
        }

        [Fact]
        public void Generate_can_output_add_geometry_column_operation_with_SQL_default_value()
        {
            var operation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geometry)
                    {
                        IsNullable = false,
                        Name = "C",
                        DefaultValueSql = "'POINT (8 9)'"
                    });

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal("ALTER TABLE [T] ADD [C] [geometry] NOT NULL DEFAULT 'POINT (8 9)'", sql);
        }

        [Fact]
        public void Generate_can_output_alter_geography_column_operation_with_default_value()
        {
            var operation
                = new AlterColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geography)
                    {
                        IsNullable = false,
                        Name = "C",
                        DefaultValue = DbGeography.FromText("POINT (6 7)")
                    },
                    isDestructiveChange: false);

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(
                @"ALTER TABLE [T] ADD CONSTRAINT DF_C DEFAULT 'SRID=4326;POINT (6 7)' FOR [C]
ALTER TABLE [T] ALTER COLUMN [C] [geography] NOT NULL", sql);
        }

        [Fact]
        public void Generate_can_output_alter_geometry_column_operation_with_default_value()
        {
            var operation
                = new AlterColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geometry)
                    {
                        IsNullable = false,
                        Name = "C",
                        DefaultValue = DbGeometry.FromText("POINT (8 9)")
                    },
                    isDestructiveChange: false);

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(
                @"ALTER TABLE [T] ADD CONSTRAINT DF_C DEFAULT 'SRID=0;POINT (8 9)' FOR [C]
ALTER TABLE [T] ALTER COLUMN [C] [geometry] NOT NULL",sql);
        }

        [Fact]
        public void Generate_can_output_alter_geography_column_operation_with_SQL_default_value()
        {
            var operation
                = new AlterColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geography)
                    {
                        IsNullable = false,
                        Name = "C",
                        DefaultValueSql = "'POINT (6 7)'"
                    },
                    isDestructiveChange: false);

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(
                @"ALTER TABLE [T] ADD CONSTRAINT DF_C DEFAULT 'POINT (6 7)' FOR [C]
ALTER TABLE [T] ALTER COLUMN [C] [geography] NOT NULL", sql);
        }

        [Fact]
        public void Generate_can_output_alter_geometry_column_operation_with_SQL_default_value()
        {
            var operation
                = new AlterColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geometry)
                    {
                        IsNullable = false,
                        Name = "C",
                        DefaultValueSql = "'POINT (8 9)'"
                    },
                    isDestructiveChange: false);

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(
                @"ALTER TABLE [T] ADD CONSTRAINT DF_C DEFAULT 'POINT (8 9)' FOR [C]
ALTER TABLE [T] ALTER COLUMN [C] [geometry] NOT NULL", sql);
        }

        [Fact]
        public void Generate_can_output_alter_geography_column_operation_with_no_default_value()
        {
            var operation
                = new AlterColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geography)
                    {
                        IsNullable = false,
                        Name = "C",
                    },
                    isDestructiveChange: false);

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(
                @"ALTER TABLE [T] ALTER COLUMN [C] [geography] NOT NULL", sql);
        }

        [Fact]
        public void Generate_can_output_alter_geometry_column_operation_with_no_default_value()
        {
            var operation
                = new AlterColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Geometry)
                    {
                        IsNullable = false,
                        Name = "C",
                    },
                    isDestructiveChange: false);

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(
                @"ALTER TABLE [T] ALTER COLUMN [C] [geometry] NOT NULL", sql);
        }

        [Fact]
        public void Generate_can_output_add_table_with_spatial_columns_that_have_differently_specified_or_no_default_values()
        {
            var operation = new CreateTableOperation("T");

            new[]
            {
                new ColumnModel(PrimitiveTypeKind.Geography)
                {
                    IsNullable = false,
                    Name = "A",
                    DefaultValue = DbGeography.FromText("POINT (6 7)")
                },
                new ColumnModel(PrimitiveTypeKind.Geometry)
                {
                    IsNullable = false,
                    Name = "B",
                    DefaultValue = DbGeometry.FromText("POINT (8 9)")
                },
                new ColumnModel(PrimitiveTypeKind.Geography)
                {
                    IsNullable = false,
                    Name = "C",
                    DefaultValueSql = "'POINT (6 7)'"
                },
                new ColumnModel(PrimitiveTypeKind.Geometry)
                {
                    IsNullable = false,
                    Name = "D",
                    DefaultValueSql = "'POINT (8 9)'"
                },
                new ColumnModel(PrimitiveTypeKind.Geography)
                {
                    IsNullable = false,
                    Name = "E",
                },
                new ColumnModel(PrimitiveTypeKind.Geometry)
                {
                    IsNullable = false,
                    Name = "F",
                }
            }.Each(c => operation.Columns.Add(c));

            var sql = new SqlServerMigrationSqlGenerator().Generate(new[] { operation }, "2008")
                .Join(s => s.Sql,Environment.NewLine);

            Assert.Equal(@"CREATE TABLE [T] (
    [A] [geography] NOT NULL DEFAULT 'SRID=4326;POINT (6 7)',
    [B] [geometry] NOT NULL DEFAULT 'SRID=0;POINT (8 9)',
    [C] [geography] NOT NULL DEFAULT 'POINT (6 7)',
    [D] [geometry] NOT NULL DEFAULT 'POINT (8 9)',
    [E] [geography] NOT NULL,
    [F] [geometry] NOT NULL
)", sql);
        }

        [Fact]
        public void Generate_can_output_add_column_statement_with_explicit_default_value()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Guid) { Name = "Bar", IsNullable = false, DefaultValue = 42 };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationProvider.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("ALTER TABLE [Foo] ADD [Bar] [uniqueidentifier] NOT NULL DEFAULT 42"));
        }

        [Fact]
        public void Generate_can_output_add_column_statement_with_explicit_default_value_sql()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Guid) { Name = "Bar", IsNullable = false, DefaultValueSql = "42" };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationProvider.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("ALTER TABLE [Foo] ADD [Bar] [uniqueidentifier] NOT NULL DEFAULT 42"));
        }

        [Fact]
        public void Generate_can_output_add_column_statement_when_non_nullable_and_no_default_provided()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Int32) { Name = "Bar", IsNullable = false };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationProvider.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.True(sql.Contains("ALTER TABLE [Foo] ADD [Bar] [int] NOT NULL DEFAULT 0"));
        }
    }
}
