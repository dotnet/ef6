// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Moq;
    using Xunit;

    public class SqlCeMigrationSqlGeneratorTests
    {
        [Fact]
        public void Generate_can_handle_update_database_operations()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();
            var providerInvariantName = ProviderRegistry.SqlCe4_ProviderInfo.ProviderInvariantName;

            var historyRepository
                = new HistoryRepository(
                    new SqlCeConnectionFactory(providerInvariantName)
                        .CreateConnection("Foo").ConnectionString,
                    DbProviderFactories.GetFactory(providerInvariantName),
                    "MyKey",
                    null,
                    HistoryContext.DefaultFactory);

            var updateDatabaseOperation
                = new UpdateDatabaseOperation(historyRepository.CreateDiscoveryQueryTrees().ToList());

            updateDatabaseOperation.AddMigration("M1", new []{ new DropColumnOperation("Customers", "Foo") });

            var sql = migrationSqlGenerator.Generate(new[] { updateDatabaseOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(@"ALTER TABLE [Customers] DROP COLUMN [Foo]", sql);
        }

        [Fact]
        public void Generate_should_throw_when_column_rename()
        {
            var migrationProvider = new SqlCeMigrationSqlGenerator();

            var renameColumnOperation = new RenameColumnOperation("T", "c", "c'");

            Assert.Equal(
                Strings.SqlCeColumnRenameNotSupported,
                Assert.Throws<MigrationsException>(() => migrationProvider.Generate(new[] { renameColumnOperation }, "4.0").ToList()).
                    Message);
        }

        [Fact]
        public void Generate_throws_when_operation_unknown()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();
            var unknownOperation = new Mock<MigrationOperation>(null).Object;

            var ex = Assert.Throws<InvalidOperationException>(
                () => migrationSqlGenerator.Generate(new[] { unknownOperation }, "4.0"));

            Assert.Equal(
                Strings.SqlServerMigrationSqlGenerator_UnknownOperation(
                    typeof(SqlCeMigrationSqlGenerator).Name, unknownOperation.GetType().FullName),
                ex.Message);
        }

        [Fact]
        public void Generate_can_output_add_column_statement_for_GUID_and_uses_newid()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Guid)
                             {
                                 Name = "Bar",
                                 IsIdentity = true
                             };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Foo] ADD [Bar] [uniqueidentifier] DEFAULT newid()", sql);
        }

        [Fact]
        public void Generate_can_output_alter_column_with_default_constraint()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Int32)
                             {
                                 Name = "Bar",
                                 DefaultValue = 42
                             };

            var alterColumnOperation = new AlterColumnOperation("Foo", column, false);

            var sql = migrationSqlGenerator
                .Generate(new[] { alterColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(@"ALTER TABLE [Foo] ALTER COLUMN [Bar] [int]
ALTER TABLE [Foo] ALTER COLUMN [Bar] DROP DEFAULT
ALTER TABLE [Foo] ALTER COLUMN [Bar] SET DEFAULT 42", sql);
        }

        [Fact]
        public void Generate_should_output_invariant_decimals_when_non_invariant_culture()
        {
            var migrationProvider = new SqlCeMigrationSqlGenerator();

            var addColumnOperation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Binary)
                        {
                            Name = "C",
                            DefaultValue = 123.45m
                        });

            var lastCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("nl-NL");

                var sql = migrationProvider.Generate(new[] { addColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

                Assert.Contains("ALTER TABLE [T] ADD [C] [image] DEFAULT 123.45", sql);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = lastCulture;
            }
        }

        [Fact]
        public void Generate_can_output_add_timestamp_column_operation()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var addColumnOperation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Binary)
                        {
                            IsNullable = false,
                            Name = "C",
                            IsTimestamp = true
                        });

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] ADD [C] rowversion NOT NULL", sql);
        }

        [Fact]
        public void Generate_can_output_add_rowversion_store_type_column_operation()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var addColumnOperation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Binary)
                        {
                            IsNullable = false,
                            Name = "C",
                            StoreType = "RowVersion"
                        });

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] ADD [C] [RowVersion] NOT NULL", sql);
        }

        [Fact]
        public void Generate_can_output_add_timestamp_store_type_column_operation()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var addColumnOperation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Binary)
                        {
                            IsNullable = false,
                            Name = "C",
                            StoreType = "timestamp"
                        });

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] ADD [C] [timestamp] NOT NULL", sql);
        }

        [Fact]
        public void Generate_can_output_drop_primary_key_operation()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation
                                              {
                                                  Table = "T"
                                              };

            var sql = migrationSqlGenerator.Generate(new[] { dropPrimaryKeyOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] DROP CONSTRAINT [PK_T]", sql);
        }

        [Fact]
        public void Generate_can_output_add_primary_key_operation()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation
                                             {
                                                 Table = "T",
                                                 IsClustered = true
                                             };

            addPrimaryKeyOperation.Columns.Add("c1");
            addPrimaryKeyOperation.Columns.Add("c2");

            var sql = migrationSqlGenerator.Generate(new[] { addPrimaryKeyOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] ADD CONSTRAINT [PK_T] PRIMARY KEY ([c1], [c2])", sql);
        }

        [Fact]
        public void Generate_can_output_non_clustered_add_primary_key_operation()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation
                                             {
                                                 Table = "T",
                                                 IsClustered = false
                                             };

            addPrimaryKeyOperation.Columns.Add("c1");
            addPrimaryKeyOperation.Columns.Add("c2");

            var sql = migrationSqlGenerator.Generate(new[] { addPrimaryKeyOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] ADD CONSTRAINT [PK_T] PRIMARY KEY NONCLUSTERED ([c1], [c2])", sql);
        }

        [Fact]
        public void Generate_can_output_drop_column()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var dropColumnOperation = new DropColumnOperation("Customers", "Foo");

            var sql = migrationSqlGenerator.Generate(new[] { dropColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Customers] DROP COLUMN [Foo]", sql);
        }

        [Fact]
        public void Generate_can_output_timestamp_column()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var createTableOperation = new CreateTableOperation("Customers");
            var column = new ColumnModel(PrimitiveTypeKind.Binary)
                             {
                                 Name = "Version",
                                 IsTimestamp = true
                             };
            createTableOperation.Columns.Add(column);

            var sql = migrationSqlGenerator.Generate(new[] { createTableOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(@"[Version] rowversion", sql);
        }

        [Fact]
        public void Generate_can_output_custom_sql_operation()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { new SqlOperation("insert into foo") }, "4.0").Join(
                s => s.Sql, Environment.NewLine);

            Assert.Contains(@"insert into foo", sql);
        }

        [Fact]
        public void Generate_can_output_create_table_statement()
        {
            var createTableOperation = new CreateTableOperation("foo.Customers");
            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32)
                               {
                                   Name = "Id",
                                   IsNullable = true,
                                   IsIdentity = true
                               };
            createTableOperation.Columns.Add(idColumn);
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.String)
                    {
                        Name = "Name",
                        IsNullable = false
                    });

            createTableOperation.PrimaryKey = new AddPrimaryKeyOperation();

            createTableOperation.PrimaryKey.Columns.Add(idColumn.Name);

            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { createTableOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(
                @"CREATE TABLE [Customers] (
    [Id] [int] IDENTITY,
    [Name] [ntext] NOT NULL,
    CONSTRAINT [PK_foo.Customers] PRIMARY KEY ([Id])
)", sql);
        }

        [Fact]
        public void Generate_can_output_create_table_statement_with_non_clustered_pk()
        {
            var createTableOperation = new CreateTableOperation("foo.Customers");
            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32)
                               {
                                   Name = "Id",
                                   IsNullable = true,
                                   IsIdentity = true
                               };
            createTableOperation.Columns.Add(idColumn);
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.String)
                    {
                        Name = "Name",
                        IsNullable = false
                    });

            createTableOperation.PrimaryKey
                = new AddPrimaryKeyOperation
                      {
                          IsClustered = false
                      };

            createTableOperation.PrimaryKey.Columns.Add(idColumn.Name);

            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { createTableOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(
                @"CREATE TABLE [Customers] (
    [Id] [int] IDENTITY,
    [Name] [ntext] NOT NULL,
    CONSTRAINT [PK_foo.Customers] PRIMARY KEY NONCLUSTERED ([Id])
)", sql);
        }

        [Fact]
        public void Generate_can_output_create_index_statement()
        {
            var createTableOperation = new CreateTableOperation("Customers");
            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32)
                               {
                                   Name = "Id",
                                   IsNullable = true,
                                   IsIdentity = true
                               };
            createTableOperation.Columns.Add(idColumn);
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.String)
                    {
                        Name = "Name",
                        IsNullable = false
                    });
            createTableOperation.PrimaryKey = new AddPrimaryKeyOperation();
            createTableOperation.PrimaryKey.Columns.Add(idColumn.Name);

            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var createIndexOperation = new CreateIndexOperation
                                           {
                                               Table = createTableOperation.Name,
                                               IsUnique = true
                                           };

            createIndexOperation.Columns.Add(idColumn.Name);

            var sql
                = migrationSqlGenerator.Generate(
                    new[]
                        {
                            createIndexOperation
                        },
                    "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(
                @"CREATE UNIQUE INDEX [IX_Id] ON [Customers]([Id])", sql);
        }

        [Fact]
        public void Generate_can_output_create_index_statement_clustered()
        {
            var createTableOperation = new CreateTableOperation("Customers");
            var idColumn = new ColumnModel(PrimitiveTypeKind.Int32)
                               {
                                   Name = "Id",
                                   IsNullable = true,
                                   IsIdentity = true
                               };
            createTableOperation.Columns.Add(idColumn);
            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.String)
                    {
                        Name = "Name",
                        IsNullable = false
                    });
            createTableOperation.PrimaryKey = new AddPrimaryKeyOperation();
            createTableOperation.PrimaryKey.Columns.Add(idColumn.Name);

            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var createIndexOperation = new CreateIndexOperation
                                           {
                                               Table = createTableOperation.Name,
                                               IsUnique = true,
                                               IsClustered = true
                                           };

            createIndexOperation.Columns.Add(idColumn.Name);

            var sql
                = migrationSqlGenerator.Generate(
                    new[]
                        {
                            createIndexOperation
                        },
                    "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(
                @"CREATE UNIQUE CLUSTERED INDEX [IX_Id] ON [Customers]([Id])", sql);
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

            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { addForeignKeyOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(
                @"ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE",
                sql);
        }

        [Fact]
        public void Generate_can_output_drop_table_statement()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { new DropTableOperation("Customers") }, "4.0").Join(
                s => s.Sql, Environment.NewLine);

            Assert.Contains("DROP TABLE [Customers]", sql);
        }

        [Fact]
        public void Generate_can_output_insert_history_statement()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            using (var historyContext = new HistoryContext())
            {
                historyContext.History.Add(
                    new HistoryRow
                        {
                            MigrationId = "House Lannister",
                            ContextKey = "The pointy end",
                            Model = new byte[0],
                            ProductVersion = "Awesomeness"
                        });

                using (var commandTracer = new CommandTracer(historyContext))
                {
                    historyContext.SaveChanges();

                    var insertHistoryOperation
                        = new HistoryOperation(commandTracer.CommandTrees.OfType<DbModificationCommandTree>().ToList());

                    var sql
                        = migrationSqlGenerator
                            .Generate(new[] { insertHistoryOperation }, "4.0")
                            .Single();

                    Assert.Equal(@"INSERT [__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
VALUES (N'House Lannister', N'The pointy end',  0x , N'Awesomeness')", sql.Sql.Trim());
                }
            }
        }

        [Fact]
        public void Generate_can_output_delete_history_statement()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            using (var historyContext = new HistoryContext())
            {
                var historyRow
                    = new HistoryRow
                          {
                              MigrationId = "House Lannister",
                              ContextKey = "The pointy end"
                          };

                historyContext.History.Attach(historyRow);
                historyContext.History.Remove(historyRow);

                using (var commandTracer = new CommandTracer(historyContext))
                {
                    historyContext.SaveChanges();

                    var deleteHistoryOperation
                        = new HistoryOperation(commandTracer.CommandTrees.OfType<DbModificationCommandTree>().ToList());

                    var sql
                        = migrationSqlGenerator
                            .Generate(new[] { deleteHistoryOperation }, "4.0")
                            .Single();

                    Assert.Equal(@"DELETE [__MigrationHistory]
WHERE (([MigrationId] = N'House Lannister') AND ([ContextKey] = N'The pointy end'))", sql.Sql.Trim());
                }
            }
        }

        [Fact]
        public void Generate_can_output_add_column_statement_for_GUID()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Guid)
                             {
                                 Name = "Bar",
                                 IsIdentity = true
                             };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "4.0")
                .Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(string.Format("ALTER TABLE [Foo] ADD [Bar] [uniqueidentifier] DEFAULT {0}", "newid()"), sql);
        }

        [Fact]
        public void Generate_can_output_add_column_statement_with_custom_store_type()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.String)
                             {
                                 Name = "Bar",
                                 StoreType = "nvarchar"
                             };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Foo] ADD [Bar] [nvarchar](4000)", sql);
        }

        [Fact]
        public void Generate_can_output_add_column_statement_with_custom_store_type_and_maxLength()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.String)
            {
                Name = "Bar",
                StoreType = "nvarchar",
                MaxLength = 15
            };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Foo] ADD [Bar] [nvarchar](15)", sql);
        }

        [Fact]
        public void Generate_can_output_add_column_statement_with_explicit_default_value()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Guid)
                             {
                                 Name = "Bar",
                                 IsNullable = false,
                                 DefaultValue = 42
                             };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Foo] ADD [Bar] [uniqueidentifier] NOT NULL DEFAULT 42", sql);
        }

        [Fact]
        public void Generate_can_output_add_column_statement_with_explicit_default_value_sql()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Guid)
                             {
                                 Name = "Bar",
                                 IsNullable = false,
                                 DefaultValueSql = "42"
                             };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Foo] ADD [Bar] [uniqueidentifier] NOT NULL DEFAULT 42", sql);
        }

        [Fact]
        public void Generate_can_output_add_column_statement_when_non_nullable_and_no_default_provided()
        {
            var migrationSqlGenerator = new SqlCeMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Int32)
                             {
                                 Name = "Bar",
                                 IsNullable = false
                             };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "4.0").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Foo] ADD [Bar] [int] NOT NULL DEFAULT 0", sql);
        }
    }
}
