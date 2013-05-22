// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Infrastructure.FunctionsModel;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Moq;
    using Xunit;

    public class SqlServerMigrationSqlGeneratorTests
    {
        [Fact]
        public void Generate_should_output_invariant_decimals_when_non_invariant_culture()
        {
            var migrationProvider = new SqlServerMigrationSqlGenerator();

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

                var sql = migrationProvider.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

                Assert.Contains("ALTER TABLE [T] ADD [C] [varbinary](max) DEFAULT 123.45", sql);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = lastCulture;
            }
        }

        [Fact]
        public void Generate_can_output_add_timestamp_column_operation()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var addColumnOperation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Binary)
                        {
                            IsNullable = false,
                            Name = "C",
                            IsTimestamp = true
                        });

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] ADD [C] rowversion NOT NULL", sql);
        }

        [Fact]
        public void Generate_can_output_add_rowversion_store_type_column_operation()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var addColumnOperation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Binary)
                        {
                            IsNullable = false,
                            Name = "C",
                            StoreType = "RowVersion"
                        });

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] ADD [C] [RowVersion] NOT NULL", sql);
        }

        [Fact]
        public void Generate_can_output_add_timestamp_store_type_column_operation()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var addColumnOperation
                = new AddColumnOperation(
                    "T",
                    new ColumnModel(PrimitiveTypeKind.Binary)
                        {
                            IsNullable = false,
                            Name = "C",
                            StoreType = "timestamp"
                        });

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] ADD [C] [timestamp] NOT NULL", sql);
        }

        [Fact]
        public void Generate_can_output_drop_primary_key_operation()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var dropPrimaryKeyOperation = new DropPrimaryKeyOperation
                                              {
                                                  Table = "T"
                                              };

            var sql = migrationSqlGenerator.Generate(new[] { dropPrimaryKeyOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] DROP CONSTRAINT [PK_T]", sql);
        }

        [Fact]
        public void Generate_can_output_add_primary_key_operation()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation
                                             {
                                                 Table = "T",
                                                 IsClustered = true
                                             };

            addPrimaryKeyOperation.Columns.Add("c1");
            addPrimaryKeyOperation.Columns.Add("c2");

            var sql = migrationSqlGenerator.Generate(new[] { addPrimaryKeyOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] ADD CONSTRAINT [PK_T] PRIMARY KEY ([c1], [c2])", sql);
        }

        [Fact]
        public void Generate_can_output_non_clustered_add_primary_key_operation()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation
                                             {
                                                 Table = "T",
                                                 IsClustered = false
                                             };

            addPrimaryKeyOperation.Columns.Add("c1");
            addPrimaryKeyOperation.Columns.Add("c2");

            var sql = migrationSqlGenerator.Generate(new[] { addPrimaryKeyOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [T] ADD CONSTRAINT [PK_T] PRIMARY KEY NONCLUSTERED ([c1], [c2])", sql);
        }

        [Fact]
        public void Generate_can_output_drop_column()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var dropColumnOperation = new DropColumnOperation("Customers", "Foo");

            var sql = migrationSqlGenerator.Generate(new[] { dropColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Customers] DROP COLUMN [Foo]", sql);
        }

        [Fact]
        public void Generate_can_output_timestamp_column()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var createTableOperation = new CreateTableOperation("Customers");
            var column = new ColumnModel(PrimitiveTypeKind.Binary)
                             {
                                 Name = "Version",
                                 IsTimestamp = true
                             };
            createTableOperation.Columns.Add(column);

            var sql = migrationSqlGenerator.Generate(new[] { createTableOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(@"[Version] rowversion", sql);
        }

        [Fact]
        public void Generate_can_output_custom_sql_operation()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { new SqlOperation("insert into foo") }, "2008").Join(
                s => s.Sql, Environment.NewLine);

            Assert.Contains(@"insert into foo", sql);
        }

        [Fact]
        public void Generate_can_output_create_procedure_statements()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var model2 = new TestContext();

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel());

            var createProcedureOperation
                = new EdmModelDiffer()
                    .Diff(
                        model1.GetModel(),
                        model2.GetModel(),
                        commandTreeGenerator,
                        new SqlServerMigrationSqlGenerator())
                    .OfType<CreateProcedureOperation>()
                    .Single(c => c.Name == "dbo.ExtraSpecialOrder_Update");

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { createProcedureOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(
                @"CREATE PROCEDURE [dbo].[ExtraSpecialOrder_Update]
    @xid [int],
    @key_for_update [uniqueidentifier],
    @Code [nvarchar](128),
    @Signature [varbinary](128),
    @Name [nvarchar](max),
    @Name_Original [nvarchar](max),
    @Address_Street [nvarchar](max),
    @Address_City [nvarchar](max),
    @Address_Country_Name [nvarchar](max),
    @OrderGroupId [int],
    @RowVersion_Original [rowversion],
    @OtherAddress_Street [nvarchar](max),
    @OtherAddress_City [nvarchar](max),
    @OtherAddress_Country_Name [nvarchar](max),
    @TheSpecialist [int],
    @Customer_CustomerId [int],
    @OtherCustomer_CustomerId [int],
    @RowsAffected [int] OUT
AS
BEGIN
    UPDATE [dbo].[Orders]
    SET [Name] = @Name, [Address_Street] = @Address_Street, [Address_City] = @Address_City, [Address_Country_Name] = @Address_Country_Name, [OrderGroupId] = @OrderGroupId, [Customer_CustomerId] = @Customer_CustomerId
    WHERE ((((((([order_id] = @xid) AND ([Key] = @key_for_update)) AND ([Code] = @Code)) AND ([Signature] = @Signature)) AND (([Name] = @Name_Original) OR ([Name] IS NULL AND @Name_Original IS NULL))) AND (([RowVersion] = @RowVersion_Original) OR ([RowVersion] IS NULL AND @RowVersion_Original IS NULL))) AND (([Customer_CustomerId] = @Customer_CustomerId) OR ([Customer_CustomerId] IS NULL AND @Customer_CustomerId IS NULL)))
    
    UPDATE [dbo].[special_orders]
    SET [OtherCustomer_CustomerId] = @OtherCustomer_CustomerId, [OtherAddress_Street] = @OtherAddress_Street, [OtherAddress_City] = @OtherAddress_City, [OtherAddress_Country_Name] = @OtherAddress_Country_Name
    WHERE ((((([order_id] = @xid) AND ([so_key] = @key_for_update)) AND ([Code] = @Code)) AND ([Signature] = @Signature)) AND (([OtherCustomer_CustomerId] = @OtherCustomer_CustomerId) OR ([OtherCustomer_CustomerId] IS NULL AND @OtherCustomer_CustomerId IS NULL)))
    AND @@ROWCOUNT > 0
    
    UPDATE [dbo].[xspecial_orders]
    SET [TheSpecialist] = @TheSpecialist
    WHERE (((([xid] = @xid) AND ([so_key] = @key_for_update)) AND ([Code] = @Code)) AND ([Signature] = @Signature))
    AND @@ROWCOUNT > 0
    
    SELECT t0.[OrderNo] AS order_fu, t0.[RowVersion], t1.[MagicOrderToken], t2.[FairyDust]
    FROM [dbo].[Orders] AS t0
    JOIN [dbo].[special_orders] AS t1 ON t1.[order_id] = t0.[order_id] AND t1.[so_key] = t0.[Key] AND t1.[Code] = t0.[Code] AND t1.[Signature] = t0.[Signature]
    JOIN [dbo].[xspecial_orders] AS t2 ON t2.[xid] = t0.[order_id] AND t2.[so_key] = t0.[Key] AND t2.[Code] = t0.[Code] AND t2.[Signature] = t0.[Signature]
    WHERE @@ROWCOUNT > 0 AND t0.[order_id] = @xid AND t0.[Key] = @key_for_update AND t0.[Code] = @Code AND t0.[Signature] = @Signature
    
    SET @RowsAffected = @@ROWCOUNT
END", sql);
        }

        [Fact]
        public void Generate_can_output_alter_procedure_statements()
        {
            var model1 = new TestContext();
            var model2 = new TestContext_v2();

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel());

            var alterProcedureOperation
                = new EdmModelDiffer()
                    .Diff(
                        model1.GetModel(),
                        model2.GetModel(),
                        commandTreeGenerator,
                        new SqlServerMigrationSqlGenerator())
                    .OfType<AlterProcedureOperation>()
                    .Single(c => c.Name == "dbo.Order_Update");

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { alterProcedureOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(
                @"ALTER PROCEDURE [dbo].[Order_Update]
    @order_id [int],
    @key_for_update2 [uniqueidentifier],
    @Code [nvarchar](128),
    @Signature [varbinary](128),
    @Name [nvarchar](max),
    @Name_Original [nvarchar](max),
    @Address_Street [nvarchar](max),
    @Address_City [nvarchar](max),
    @Address_Country_Name [nvarchar](max),
    @OrderGroupId [int],
    @RowVersion_Original [rowversion],
    @Customer_CustomerId [int],
    @RowsAffected [int] OUT
AS
BEGIN
    UPDATE [dbo].[Orders]
    SET [Name] = @Name, [Address_Street] = @Address_Street, [Address_City] = @Address_City, [Address_Country_Name] = @Address_Country_Name, [OrderGroupId] = @OrderGroupId, [Customer_CustomerId] = @Customer_CustomerId
    WHERE ((((((([order_id] = @order_id) AND ([Key] = @key_for_update2)) AND ([Code] = @Code)) AND ([Signature] = @Signature)) AND (([Name] = @Name_Original) OR ([Name] IS NULL AND @Name_Original IS NULL))) AND (([RowVersion] = @RowVersion_Original) OR ([RowVersion] IS NULL AND @RowVersion_Original IS NULL))) AND (([Customer_CustomerId] = @Customer_CustomerId) OR ([Customer_CustomerId] IS NULL AND @Customer_CustomerId IS NULL)))
    
    SELECT t0.[OrderNo], t0.[RowVersion]
    FROM [dbo].[Orders] AS t0
    WHERE @@ROWCOUNT > 0 AND t0.[order_id] = @order_id AND t0.[Key] = @key_for_update2 AND t0.[Code] = @Code AND t0.[Signature] = @Signature
    
    SET @RowsAffected = @@ROWCOUNT
END", sql);
        }

        [Fact]
        public void Generate_can_output_rename_procedure_statements()
        {
            var model1 = new TestContext();
            var model2 = new TestContext_v2();

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel());

            var renameProcedureOperation
                = new EdmModelDiffer()
                    .Diff(
                        model1.GetModel(),
                        model2.GetModel(),
                        commandTreeGenerator,
                        new SqlServerMigrationSqlGenerator())
                    .OfType<RenameProcedureOperation>()
                    .Single();

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { renameProcedureOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(@"EXECUTE sp_rename @objname = N'dbo.Order_Insert', @newname = N'sproc_A', @objtype = N'OBJECT'", sql);
        }

        [Fact]
        public void Generate_can_output_drop_procedure_statement()
        {
            var dropModificationFunctionsOperation
                = new DropProcedureOperation("Customer_Insert");

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var sql
                = migrationSqlGenerator
                    .Generate(new[] { dropModificationFunctionsOperation }, "2008")
                    .Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(
                @"DROP PROCEDURE [Customer_Insert]", sql);
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

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { createTableOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(
                @"IF schema_id('foo') IS NULL
    EXECUTE('CREATE SCHEMA [foo]')
CREATE TABLE [foo].[Customers] (
    [Id] [int] IDENTITY,
    [Name] [nvarchar](max) NOT NULL,
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

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { createTableOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(
                @"IF schema_id('foo') IS NULL
    EXECUTE('CREATE SCHEMA [foo]')
CREATE TABLE [foo].[Customers] (
    [Id] [int] IDENTITY,
    [Name] [nvarchar](max) NOT NULL,
    CONSTRAINT [PK_foo.Customers] PRIMARY KEY NONCLUSTERED ([Id])
)", sql);
        }

        [Fact]
        public void Generate_can_output_move_procedure_statement()
        {
            var moveProcedureOperation
                = new MoveProcedureOperation("dbo.History", "foo");

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { moveProcedureOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(
                @"IF schema_id('foo') IS NULL
    EXECUTE('CREATE SCHEMA [foo]')
ALTER SCHEMA [foo] TRANSFER [dbo].[History]", sql);
        }

        [Fact]
        public void Generate_can_output_move_table_as_system_object_statement()
        {
            var createTableOperation
                = new CreateTableOperation("dbo.History");

            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.Int32)
                    {
                        Name = "Id",
                        IsNullable = false
                    });

            createTableOperation.Columns.Add(
                new ColumnModel(PrimitiveTypeKind.String)
                    {
                        Name = "Name",
                        IsNullable = false
                    });

            var moveTableOperation
                = new MoveTableOperation("dbo.History", "foo")
                      {
                          IsSystem = true,
                          ContextKey = "MyKey",
                          CreateTableOperation = createTableOperation
                      };

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { moveTableOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(
                @"IF schema_id('foo') IS NULL
    EXECUTE('CREATE SCHEMA [foo]')
IF object_id('dbo.History') IS NULL BEGIN
    CREATE TABLE [dbo].[History] (
        [Id] [int] NOT NULL,
        [Name] [nvarchar](max) NOT NULL
    )
END
INSERT INTO [dbo].[History]
SELECT * FROM [dbo].[History]
WHERE [ContextKey] = 'MyKey'
DELETE [dbo].[History]
WHERE [ContextKey] = 'MyKey'
IF NOT EXISTS(SELECT * FROM [dbo].[History])
    DROP TABLE [dbo].[History]", sql);
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

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

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
                    "2008").Join(s => s.Sql, Environment.NewLine);

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

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

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
                    "2008").Join(s => s.Sql, Environment.NewLine);

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

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { addForeignKeyOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(
                @"ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE",
                sql);
        }

        [Fact]
        public void Generate_can_output_drop_table_statement()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var sql = migrationSqlGenerator.Generate(new[] { new DropTableOperation("Customers") }, "2008").Join(
                s => s.Sql, Environment.NewLine);

            Assert.Contains("DROP TABLE [Customers]", sql);
        }

        [Fact]
        public void Generate_can_output_insert_history_statement()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

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
                        = new HistoryOperation(commandTracer.CommandTrees.OfType<DbModificationCommandTree>());

                    var sql
                        = migrationSqlGenerator
                            .Generate(new[] { insertHistoryOperation }, "2008")
                            .Single();

                    Assert.Equal(@"INSERT [dbo].[__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
VALUES (N'House Lannister', N'The pointy end',  0x , N'Awesomeness')", sql.Sql.Trim());
                }
            }
        }

        [Fact]
        public void Generate_can_output_delete_history_statement()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

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
                        = new HistoryOperation(commandTracer.CommandTrees.OfType<DbModificationCommandTree>());

                    var sql
                        = migrationSqlGenerator
                            .Generate(new[] { deleteHistoryOperation }, "2008")
                            .Single();

                    Assert.Equal(@"DELETE [dbo].[__MigrationHistory]
WHERE (([MigrationId] = N'House Lannister') AND ([ContextKey] = N'The pointy end'))", sql.Sql.Trim());
                }
            }
        }

        [Fact]
        public void Generate_can_output_add_column_statement_for_GUID_on_SQL_Server_2008()
        {
            Generate_can_output_add_column_statement_for_GUID("2008", "newsequentialid()");
        }

        [Fact]
        public void Generate_can_output_add_column_statement_for_GUID_on_SQL_Server_2000()
        {
            Generate_can_output_add_column_statement_for_GUID("2000", "newid()");
        }

        [Fact]
        public void Generate_can_output_add_column_statement_for_GUID_on_SQL_Server_2012()
        {
            Generate_can_output_add_column_statement_for_GUID("2012", "newsequentialid()");
        }

        [Fact]
        public void Generate_can_output_add_column_statement_for_GUID_on_SQL_Azure()
        {
            Generate_can_output_add_column_statement_for_GUID("2012.Azure", "newid()");
        }

        public void Generate_can_output_add_column_statement_for_GUID(string providerManifestToken, string expectedGuidDefault)
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Guid)
                             {
                                 Name = "Bar",
                                 IsIdentity = true
                             };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, providerManifestToken)
                                           .Join(s => s.Sql, Environment.NewLine);

            Assert.Contains(string.Format("ALTER TABLE [Foo] ADD [Bar] [uniqueidentifier] DEFAULT {0}", expectedGuidDefault), sql);
        }

        [Fact]
        public void Generate_can_output_add_column_statement_with_custom_store_type()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.String)
                             {
                                 Name = "Bar",
                                 StoreType = "varchar",
                                 MaxLength = 15
                             };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Foo] ADD [Bar] [varchar](15)", sql);
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
                @"DECLARE @var0 nvarchar(128)
SELECT @var0 = name
FROM sys.default_constraints
WHERE parent_object_id = object_id(N'T')
AND col_name(parent_object_id, parent_column_id) = 'C';
IF @var0 IS NOT NULL
    EXECUTE('ALTER TABLE [T] DROP CONSTRAINT ' + @var0)
ALTER TABLE [T] ADD CONSTRAINT DF_T_C DEFAULT 'SRID=4326;POINT (6 7)' FOR [C]
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
                @"DECLARE @var0 nvarchar(128)
SELECT @var0 = name
FROM sys.default_constraints
WHERE parent_object_id = object_id(N'T')
AND col_name(parent_object_id, parent_column_id) = 'C';
IF @var0 IS NOT NULL
    EXECUTE('ALTER TABLE [T] DROP CONSTRAINT ' + @var0)
ALTER TABLE [T] ADD CONSTRAINT DF_T_C DEFAULT 'SRID=0;POINT (8 9)' FOR [C]
ALTER TABLE [T] ALTER COLUMN [C] [geometry] NOT NULL", sql);
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
                @"DECLARE @var0 nvarchar(128)
SELECT @var0 = name
FROM sys.default_constraints
WHERE parent_object_id = object_id(N'T')
AND col_name(parent_object_id, parent_column_id) = 'C';
IF @var0 IS NOT NULL
    EXECUTE('ALTER TABLE [T] DROP CONSTRAINT ' + @var0)
ALTER TABLE [T] ADD CONSTRAINT DF_T_C DEFAULT 'POINT (6 7)' FOR [C]
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
                @"DECLARE @var0 nvarchar(128)
SELECT @var0 = name
FROM sys.default_constraints
WHERE parent_object_id = object_id(N'T')
AND col_name(parent_object_id, parent_column_id) = 'C';
IF @var0 IS NOT NULL
    EXECUTE('ALTER TABLE [T] DROP CONSTRAINT ' + @var0)
ALTER TABLE [T] ADD CONSTRAINT DF_T_C DEFAULT 'POINT (8 9)' FOR [C]
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
                                                          .Join(s => s.Sql, Environment.NewLine);

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
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Guid)
                             {
                                 Name = "Bar",
                                 IsNullable = false,
                                 DefaultValue = 42
                             };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Foo] ADD [Bar] [uniqueidentifier] NOT NULL DEFAULT 42", sql);
        }

        [Fact]
        public void Generate_can_output_add_column_statement_with_explicit_default_value_sql()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Guid)
                             {
                                 Name = "Bar",
                                 IsNullable = false,
                                 DefaultValueSql = "42"
                             };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Foo] ADD [Bar] [uniqueidentifier] NOT NULL DEFAULT 42", sql);
        }

        [Fact]
        public void Generate_can_output_add_column_statement_when_non_nullable_and_no_default_provided()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var column = new ColumnModel(PrimitiveTypeKind.Int32)
                             {
                                 Name = "Bar",
                                 IsNullable = false
                             };
            var addColumnOperation = new AddColumnOperation("Foo", column);

            var sql = migrationSqlGenerator.Generate(new[] { addColumnOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Contains("ALTER TABLE [Foo] ADD [Bar] [int] NOT NULL DEFAULT 0", sql);
        }

        [Fact]
        public void Generate_throws_when_operation_unknown()
        {
            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();
            var unknownOperation = new Mock<MigrationOperation>(null).Object;

            var ex = Assert.Throws<InvalidOperationException>(
                () => migrationSqlGenerator.Generate(new[] { unknownOperation }, "2008"));

            Assert.Equal(
                Strings.SqlServerMigrationSqlGenerator_UnknownOperation(
                    typeof(SqlServerMigrationSqlGenerator).Name, unknownOperation.GetType().FullName),
                ex.Message);
        }

        [Fact]
        public void Has_ProviderInvariantNameAttribute()
        {
            Assert.Equal(
                "System.Data.SqlClient",
                DbProviderNameAttribute.GetFromType(typeof(SqlServerMigrationSqlGenerator)).Single().Name);
        }

        [Fact]
        public static void Generate_can_output_statement_to_upgrade_primary_key_of_history_table()
        {
            const string expectedSql =
@"CREATE TABLE [dbo].[__MigrationHistory2] (
    [MigrationId] [nvarchar](255) NOT NULL,
    [ContextKey] [nvarchar](512) NOT NULL,
    [Model] [varbinary](max) NOT NULL,
    [ProductVersion] [nvarchar](32) NOT NULL,
    CONSTRAINT [PK_dbo.__MigrationHistory2] PRIMARY KEY ([MigrationId], [ContextKey])
)
INSERT INTO [dbo].[__MigrationHistory2]
SELECT [MigrationId], 'DefaultContextKey', [Model], [ProductVersion] FROM [dbo].[__MigrationHistory]
DROP TABLE [dbo].[__MigrationHistory]
EXECUTE sp_rename @objname = N'dbo.__MigrationHistory2', @newname = N'__MigrationHistory', @objtype = N'OBJECT'";

            var migrationSqlGenerator = new SqlServerMigrationSqlGenerator();

            var operations = Create_operations_to_upgrade_primary_key_of_history_table();
            var sql = migrationSqlGenerator.Generate(operations, SqlProviderManifest.TokenAzure11)
                .Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(expectedSql, sql);

            operations = Create_operations_to_upgrade_primary_key_of_history_table();
            sql = migrationSqlGenerator.Generate(operations, SqlProviderManifest.TokenSql11)
                .Join(s => s.Sql, Environment.NewLine);

            Assert.Equal(expectedSql, sql);
        }

        private static MigrationOperation[] Create_operations_to_upgrade_primary_key_of_history_table()
        {
            var tableName = "dbo." + HistoryContext.DefaultTableName;

            CreateTableOperation createTableOperation;
            using (var context = new HistoryContext())
            {
                var emptyModel = new DbModelBuilder()
                    .Build(context.Database.Connection).GetModel();
                createTableOperation = (CreateTableOperation)
                    new EdmModelDiffer().Diff(emptyModel, context.GetModel()).Single();
            }

            var addColumnOperation =
                new AddColumnOperation(
                    tableName,
                    new ColumnModel(PrimitiveTypeKind.String)
                        {
                            MaxLength = 512,
                            Name = "ContextKey",
                            IsNullable = false,
                            DefaultValue = "DefaultContextKey"
                        });

            var dropPrimaryKeyOperation
                = new DropPrimaryKeyOperation
                      {
                          Table = tableName,
                          CreateTableOperation = createTableOperation
                      };

            dropPrimaryKeyOperation.Columns.Add("MigrationId");

            var addPrimaryKeyOperation
                = new AddPrimaryKeyOperation
                      {
                          Table = tableName
                      };

            addPrimaryKeyOperation.Columns.Add("MigrationId");
            addPrimaryKeyOperation.Columns.Add("ContextKey");

            return 
                new MigrationOperation[] 
                    { 
                        addColumnOperation,
                        dropPrimaryKeyOperation, 
                        addPrimaryKeyOperation 
                    };
        }
    }
}
