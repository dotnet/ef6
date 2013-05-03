// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;
    using Xunit;

    #region AutoAndGenerateScenarios

    public class AutoAndGenerateScenarios_Empty :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_Empty.V1, AutoAndGenerateScenarios_Empty.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
        }

        public class V2 : AutoAndGenerateContext_v2
        {
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(0, migrationOperations.Count());
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(0, migrationOperations.Count());
        }
    }

    #endregion

    #region TableScenarios

    public class AutoAndGenerateScenarios_AddTable :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddTable.V1, AutoAndGenerateScenarios_AddTable.V2>
    {
        public AutoAndGenerateScenarios_AddTable()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().ToTable("MigrationsStores", "my");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var createTableOperation =
                migrationOperations.OfType<CreateTableOperation>().SingleOrDefault(o => o.Name == "my.MigrationsStores");
            Assert.NotNull(createTableOperation);

            Assert.True(createTableOperation.PrimaryKey.Columns.Count == 1);
            Assert.Equal("Id", createTableOperation.PrimaryKey.Columns.Single());
            Assert.Equal(7, createTableOperation.Columns.Count);

            var idColumn = createTableOperation.Columns.SingleOrDefault(c => c.Name == "Id");
            Assert.NotNull(idColumn);

            Assert.Equal(PrimitiveTypeKind.Int32, idColumn.Type);
            Assert.False(idColumn.IsNullable.Value);
            Assert.True(idColumn.IsIdentity);

            var nameColumn = createTableOperation.Columns.SingleOrDefault(c => c.Name == "Name");
            Assert.NotNull(nameColumn);

            Assert.Equal(PrimitiveTypeKind.String, nameColumn.Type);
            Assert.Null(nameColumn.IsNullable);
            Assert.Null(nameColumn.IsFixedLength);
            Assert.Null(nameColumn.MaxLength);

            var addressCityColumn = createTableOperation.Columns.SingleOrDefault(c => c.Name == "Address_City");
            Assert.NotNull(addressCityColumn);

            Assert.Equal(PrimitiveTypeKind.String, addressCityColumn.Type);
            Assert.Null(addressCityColumn.IsNullable);
            Assert.Null(addressCityColumn.IsFixedLength);
            Assert.Equal(150, addressCityColumn.MaxLength);

            var rowVersionColumn = createTableOperation.Columns.SingleOrDefault(c => c.Name == "RowVersion");
            Assert.NotNull(rowVersionColumn);

            Assert.Equal(PrimitiveTypeKind.Binary, rowVersionColumn.Type);
            Assert.False(rowVersionColumn.IsNullable.Value);
            Assert.True(rowVersionColumn.IsFixedLength.Value);
            Assert.Null(rowVersionColumn.MaxLength);
            Assert.True(rowVersionColumn.IsTimestamp);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropTableOperation = migrationOperations.OfType<DropTableOperation>().SingleOrDefault(
                o => o.Name == "my.MigrationsStores");
            Assert.NotNull(dropTableOperation);
        }
    }

    public class AutoAndGenerateScenarios_AddTableWithGuidKey :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddTableWithGuidKey.V1, AutoAndGenerateScenarios_AddTableWithGuidKey.V2>
    {
        public AutoAndGenerateScenarios_AddTableWithGuidKey()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<WithGuidKey>();
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var createTableOperation = migrationOperations.OfType<CreateTableOperation>().SingleOrDefault(o => o.Name == "dbo.WithGuidKeys");
            Assert.NotNull(createTableOperation);

            Assert.True(createTableOperation.PrimaryKey.Columns.Count == 1);
            Assert.Equal("Id", createTableOperation.PrimaryKey.Columns.Single());
            Assert.Equal(2, createTableOperation.Columns.Count);

            var idColumn = createTableOperation.Columns.SingleOrDefault(c => c.Name == "Id");
            Assert.NotNull(idColumn);
            Assert.Equal(PrimitiveTypeKind.Guid, idColumn.Type);
            Assert.False(idColumn.IsNullable.Value);
            Assert.True(idColumn.IsIdentity);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropTableOperation = migrationOperations.OfType<DropTableOperation>().SingleOrDefault(o => o.Name == "dbo.WithGuidKeys");
            Assert.NotNull(dropTableOperation);
        }
    }

    public class AutoAndGenerateScenarios_RemoveTable :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_RemoveTable.V1, AutoAndGenerateScenarios_RemoveTable.V2>
    {
        public AutoAndGenerateScenarios_RemoveTable()
        {
            UpDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropTableOperation = migrationOperations.OfType<DropTableOperation>().SingleOrDefault(o => o.Name == "dbo.MigrationsStores");
            Assert.NotNull(dropTableOperation);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var createTableOperation =
                migrationOperations.OfType<CreateTableOperation>().SingleOrDefault(o => o.Name == "dbo.MigrationsStores");
            Assert.NotNull(createTableOperation);

            Assert.Equal(7, createTableOperation.Columns.Count);
        }
    }

    public class AutoAndGenerateScenarios_ChangeTableSchema :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_ChangeTableSchema.V1, AutoAndGenerateScenarios_ChangeTableSchema.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().ToTable("MigrationsStores", "new");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var moveTableOperation = migrationOperations.OfType<MoveTableOperation>().SingleOrDefault(o => o.Name == "dbo.MigrationsStores");
            Assert.NotNull(moveTableOperation);

            Assert.Equal("new", moveTableOperation.NewSchema);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var moveTableOperation =
                migrationOperations.OfType<MoveTableOperation>().SingleOrDefault(o => o.Name == "new.MigrationsStores");
            Assert.NotNull(moveTableOperation);

            Assert.Equal("dbo", moveTableOperation.NewSchema);
        }
    }

    public class AutoAndGenerateScenarios_ChangeTableName :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_ChangeTableName.V1, AutoAndGenerateScenarios_ChangeTableName.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().ToTable("Renamed");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var renameTableOperation =
                migrationOperations.OfType<RenameTableOperation>().SingleOrDefault(o => o.Name == "dbo.MigrationsStores");
            Assert.NotNull(renameTableOperation);

            Assert.Equal("Renamed", renameTableOperation.NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var renameTableOperation =
                migrationOperations.OfType<RenameTableOperation>().SingleOrDefault(o => o.Name == "dbo.Renamed");
            Assert.NotNull(renameTableOperation);

            Assert.Equal("MigrationsStores", renameTableOperation.NewName);
        }
    }

    public class AutoAndGenerateScenarios_ChangeTablePrimaryKey :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_ChangeTablePrimaryKey.V1, AutoAndGenerateScenarios_ChangeTablePrimaryKey.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                // Compensate for convention differences
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).IsRequired().HasMaxLength(128);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().HasKey(s => s.Name);

                // Compensate for convention differences
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            Assert.True(migrationOperations.First() is DropPrimaryKeyOperation);
            Assert.True(migrationOperations.Last() is AddPrimaryKeyOperation);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            Assert.True(migrationOperations.First() is DropPrimaryKeyOperation);
            Assert.True(migrationOperations.Last() is AddPrimaryKeyOperation);
        }
    }

    #endregion

    #region  ForeignKeyScenarios

    public class AutoAndGenerateScenarios_AddForeignKey :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddForeignKey.V1, AutoAndGenerateScenarios_AddForeignKey.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().Ignore(o => o.OrderLines);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithRequired().HasForeignKey(ol => ol.OrderId);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var createIndexOperation =
                migrationOperations.OfType<CreateIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(createIndexOperation);

            var addForeignKeyOperation =
                migrationOperations.OfType<AddForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(addForeignKeyOperation);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var dropIndexOperation =
                migrationOperations.OfType<DropIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(dropIndexOperation);

            var dropForeignKeyOperation =
                migrationOperations.OfType<DropForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(dropForeignKeyOperation);
        }
    }

    public class AutoAndGenerateScenarios_AddPromotedForeignKey :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddPromotedForeignKey.V1, AutoAndGenerateScenarios_AddPromotedForeignKey.V2>
    {
        public AutoAndGenerateScenarios_AddPromotedForeignKey()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithRequired().HasForeignKey(ol => ol.OrderId).
                    WillCascadeOnDelete(false);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(3, migrationOperations.Count());

            var createTableOperation =
                migrationOperations.OfType<CreateTableOperation>().SingleOrDefault(o => o.Name == "dbo.Orders");
            Assert.NotNull(createTableOperation);

            var createIndexOperation =
                migrationOperations.OfType<CreateIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(createIndexOperation);

            var addForeignKeyOperation =
                migrationOperations.OfType<AddForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(addForeignKeyOperation);

            Assert.False(addForeignKeyOperation.CascadeDelete);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(3, migrationOperations.Count());

            var dropTableOperation = migrationOperations.OfType<DropTableOperation>().SingleOrDefault(o => o.Name == "dbo.Orders");
            Assert.NotNull(dropTableOperation);

            var dropIndexOperation =
                migrationOperations.OfType<DropIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(dropIndexOperation);

            var dropForeignKeyOperation =
                migrationOperations.OfType<DropForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(dropForeignKeyOperation);
        }
    }

    public class AutoAndGenerateScenarios_RemoveForeignKey :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_RemoveForeignKey.V1, AutoAndGenerateScenarios_RemoveForeignKey.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithRequired().HasForeignKey(ol => ol.OrderId);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().Ignore(o => o.OrderLines);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var dropIndexOperation =
                migrationOperations.OfType<DropIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(dropIndexOperation);

            var dropForeignKeyOperation =
                migrationOperations.OfType<DropForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(dropForeignKeyOperation);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            var createIndexOperation =
                migrationOperations.OfType<CreateIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(createIndexOperation);

            var addForeignKeyOperation =
                migrationOperations.OfType<AddForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(addForeignKeyOperation);
        }
    }

    public class AutoAndGenerateScenarios_ChangeForeignKeyOnDeleteAction :
        AutoAndGenerateTestCase
            <AutoAndGenerateScenarios_ChangeForeignKeyOnDeleteAction.V1, AutoAndGenerateScenarios_ChangeForeignKeyOnDeleteAction.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithRequired().HasForeignKey(ol => ol.OrderId);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>().ToTable("Orders");
                modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithRequired().HasForeignKey(ol => ol.OrderId).
                    WillCascadeOnDelete(false);
                modelBuilder.Entity<OrderLine>().ToTable("OrderLines");
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(4, migrationOperations.Count());

            var dropIndexOperation =
                migrationOperations.OfType<DropIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(dropIndexOperation);

            var dropForeignKeyOperation =
                migrationOperations.OfType<DropForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(dropForeignKeyOperation);

            var createIndexOperation =
                migrationOperations.OfType<CreateIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(createIndexOperation);

            var addForeignKeyOperation =
                migrationOperations.OfType<AddForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(addForeignKeyOperation);

            Assert.False(addForeignKeyOperation.CascadeDelete);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(4, migrationOperations.Count());

            var dropIndexOperation =
                migrationOperations.OfType<DropIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(dropIndexOperation);

            var dropForeignKeyOperation =
                migrationOperations.OfType<DropForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(dropForeignKeyOperation);

            var createIndexOperation =
                migrationOperations.OfType<CreateIndexOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Columns.Count == 1 && o.Columns.Single() == "OrderId");
            Assert.NotNull(createIndexOperation);

            var addForeignKeyOperation =
                migrationOperations.OfType<AddForeignKeyOperation>().SingleOrDefault(
                    o =>
                    o.PrincipalTable == "dbo.Orders" && o.DependentTable == "dbo.OrderLines" && o.DependentColumns.Count == 1
                    && o.DependentColumns.Single() == "OrderId");
            Assert.NotNull(addForeignKeyOperation);

            Assert.True(addForeignKeyOperation.CascadeDelete);
        }
    }

    #endregion

    #region ColumnScenarios

    public class AutoAndGenerateScenarios_AddColumn :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AddColumn.V1, AutoAndGenerateScenarios_AddColumn.V2>
    {
        public AutoAndGenerateScenarios_AddColumn()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Ignore(s => s.Name);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var addColumnOperation =
                migrationOperations.OfType<AddColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(addColumnOperation);

            Assert.Null(addColumnOperation.Column.IsFixedLength);
            Assert.Null(addColumnOperation.Column.IsNullable);
            Assert.Null(addColumnOperation.Column.MaxLength);
            Assert.Equal(PrimitiveTypeKind.String, addColumnOperation.Column.Type);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropColumnOperation =
                migrationOperations.OfType<DropColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");
            Assert.NotNull(dropColumnOperation);
        }
    }

    public class AutoAndGenerateScenarios_RemoveColumn :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_RemoveColumn.V1, AutoAndGenerateScenarios_RemoveColumn.V2>
    {
        public AutoAndGenerateScenarios_RemoveColumn()
        {
            UpDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Ignore(s => s.Name);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var dropColumnOperation =
                migrationOperations.OfType<DropColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");
            Assert.NotNull(dropColumnOperation);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var addColumnOperation =
                migrationOperations.OfType<AddColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(addColumnOperation);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnName :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnName.V1, AutoAndGenerateScenarios_AlterColumnName.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).HasColumnName("Renamed");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var renameColumnOperation =
                migrationOperations.OfType<RenameColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Name");
            Assert.NotNull(renameColumnOperation);

            Assert.Equal("Renamed", renameColumnOperation.NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var renameColumnOperation =
                migrationOperations.OfType<RenameColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Name == "Renamed");
            Assert.NotNull(renameColumnOperation);

            Assert.Equal("Name", renameColumnOperation.NewName);
        }
    }

    public class AutoAndGenerateScenarios_AlterSpatialColumnNames :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterSpatialColumnNames.V1, AutoAndGenerateScenarios_AlterSpatialColumnNames.V2>
    {
        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Location).HasColumnName("Locomotion");
                modelBuilder.Entity<MigrationsStore>().Property(s => s.FloorPlan).HasColumnName("PoorPlan");

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            Assert.Equal(
                "Locomotion",
                migrationOperations.OfType<RenameColumnOperation>()
                    .Single(o => o.Table == "dbo.MigrationsStores" && o.Name == "Location")
                    .NewName);

            Assert.Equal(
                "PoorPlan",
                migrationOperations.OfType<RenameColumnOperation>()
                    .Single(o => o.Table == "dbo.MigrationsStores" && o.Name == "FloorPlan")
                    .NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count());

            Assert.Equal(
                "Location",
                migrationOperations.OfType<RenameColumnOperation>()
                    .Single(o => o.Table == "dbo.MigrationsStores" && o.Name == "Locomotion")
                    .NewName);

            Assert.Equal(
                "FloorPlan",
                migrationOperations.OfType<RenameColumnOperation>()
                    .Single(o => o.Table == "dbo.MigrationsStores" && o.Name == "PoorPlan")
                    .NewName);
        }
    }

    public abstract class AutoAndGenerateScenarios_AlterColumnType<TContextV1, TContextV2>
        : AutoAndGenerateTestCase<TContextV1, TContextV2>
        where TContextV1 : DbContext
        where TContextV2 : DbContext
    {
        private readonly string _columnName;

        protected AutoAndGenerateScenarios_AlterColumnType(string columnName)
        {
            _columnName = columnName;

            IsDownDataLoss = true;
        }

        public abstract class BaseV1 : AutoAndGenerateContext_v1
        {
            private readonly string _columnName;

            protected BaseV1(string columnName)
            {
                _columnName = columnName;
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TypeCasts>().IgnoreAllBut("Id", _columnName);

                switch (_columnName)
                {
                    case "Decimal15ToDouble":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.Decimal15ToDouble).HasPrecision(15, 2);
                        break;

                    case "Decimal6ToDouble":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.Decimal6ToDouble).HasPrecision(6, 2);
                        break;

                    case "Decimal6ToSingle":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.Decimal6ToSingle).HasPrecision(6, 2);
                        break;

                    case "Decimal7ToSingle":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.Decimal7ToSingle).HasPrecision(7, 2);
                        break;
                }
            }
        }

        public abstract class BaseV2 : AutoAndGenerateContext_v2
        {
            private readonly string _columnName;

            protected BaseV2(string columnName)
            {
                _columnName = columnName;
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TypeCasts>().IgnoreAllBut("Id", _columnName + "_v2");

                switch (_columnName)
                {
                    case "DoubleToDecimal16":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.DoubleToDecimal16_v2).HasPrecision(16, 2);
                        break;

                    case "SingleToDecimal11":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.SingleToDecimal11_v2).HasPrecision(11, 2);
                        break;

                    case "SingleToDecimal16":
                        modelBuilder.Entity<TypeCasts>().Property(tc => tc.SingleToDecimal16_v2).HasPrecision(16, 2);
                        break;
                }
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.TypeCasts" && o.Column.Name == _columnName);
            Assert.NotNull(alterColumnOperation);

            PrimitiveTypeKind toType;
            var toTypeName = _columnName.Substring(_columnName.IndexOf("To") + 2);

            byte precision;
            var isDecimal = IsDecimal(ref toTypeName, out precision);

            Assert.True(Enum.TryParse(toTypeName, out toType));

            Assert.Equal(toType, alterColumnOperation.Column.Type);

            if (isDecimal)
            {
                Assert.Equal(precision, alterColumnOperation.Column.Precision);
            }
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.TypeCasts" && o.Column.Name == _columnName);
            Assert.NotNull(alterColumnOperation);
            PrimitiveTypeKind fromType;
            var fromTypeName = _columnName.Substring(0, _columnName.IndexOf("To"));

            byte precision;
            var isDecimal = IsDecimal(ref fromTypeName, out precision);

            Assert.True(Enum.TryParse(fromTypeName, out fromType));

            Assert.Equal(fromType, alterColumnOperation.Column.Type);

            if (isDecimal)
            {
                Assert.Equal(precision, alterColumnOperation.Column.Precision);
            }
        }

        private static bool IsDecimal(ref string typeName, out byte precision)
        {
            if (!typeName.StartsWith("Decimal"))
            {
                precision = 0;

                return false;
            }

            Assert.True(Byte.TryParse(typeName.Substring(7), out precision));
            typeName = "Decimal";

            return true;
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_Decimal15ToDouble
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_Decimal15ToDouble.V1,
                AutoAndGenerateScenarios_AlterColumnType_Decimal15ToDouble.V2>
    {
        private const string ColumnName = "Decimal15ToDouble";

        public AutoAndGenerateScenarios_AlterColumnType_Decimal15ToDouble()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal16
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal16.V1,
                AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal16.V2>
    {
        private const string ColumnName = "SingleToDecimal16";

        public AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal16()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_SingleToDouble
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_SingleToDouble.V1, AutoAndGenerateScenarios_AlterColumnType_SingleToDouble.V2>
    {
        private const string ColumnName = "SingleToDouble";

        public AutoAndGenerateScenarios_AlterColumnType_SingleToDouble()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal11
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal11.V1,
                AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal11.V2>
    {
        private const string ColumnName = "SingleToDecimal11";

        public AutoAndGenerateScenarios_AlterColumnType_SingleToDecimal11()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_Decimal6ToDouble
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_Decimal6ToDouble.V1, AutoAndGenerateScenarios_AlterColumnType_Decimal6ToDouble.V2>
    {
        private const string ColumnName = "Decimal6ToDouble";

        public AutoAndGenerateScenarios_AlterColumnType_Decimal6ToDouble()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_Int32ToInt64
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_Int32ToInt64.V1, AutoAndGenerateScenarios_AlterColumnType_Int32ToInt64.V2>
    {
        private const string ColumnName = "Int32ToInt64";

        public AutoAndGenerateScenarios_AlterColumnType_Int32ToInt64()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_Int16ToInt64
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_Int16ToInt64.V1, AutoAndGenerateScenarios_AlterColumnType_Int16ToInt64.V2>
    {
        private const string ColumnName = "Int16ToInt64";

        public AutoAndGenerateScenarios_AlterColumnType_Int16ToInt64()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_Int16ToInt32
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_Int16ToInt32.V1, AutoAndGenerateScenarios_AlterColumnType_Int16ToInt32.V2>
    {
        private const string ColumnName = "Int16ToInt32";

        public AutoAndGenerateScenarios_AlterColumnType_Int16ToInt32()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_ByteToInt64
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_ByteToInt64.V1, AutoAndGenerateScenarios_AlterColumnType_ByteToInt64.V2>
    {
        private const string ColumnName = "ByteToInt64";

        public AutoAndGenerateScenarios_AlterColumnType_ByteToInt64()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_ByteToInt32
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_ByteToInt32.V1, AutoAndGenerateScenarios_AlterColumnType_ByteToInt32.V2>
    {
        private const string ColumnName = "ByteToInt32";

        public AutoAndGenerateScenarios_AlterColumnType_ByteToInt32()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnType_ByteToInt16
        :
            AutoAndGenerateScenarios_AlterColumnType
                <AutoAndGenerateScenarios_AlterColumnType_ByteToInt16.V1, AutoAndGenerateScenarios_AlterColumnType_ByteToInt16.V2>
    {
        private const string ColumnName = "ByteToInt16";

        public AutoAndGenerateScenarios_AlterColumnType_ByteToInt16()
            : base(ColumnName)
        {
        }

        public class V1 : BaseV1
        {
            public V1()
                : base(ColumnName)
            {
            }
        }

        public class V2 : BaseV2
        {
            public V2()
                : base(ColumnName)
            {
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnFixedLength :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnFixedLength.V1, AutoAndGenerateScenarios_AlterColumnFixedLength.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnFixedLength()
        {
            UpDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).IsFixedLength();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.True(alterColumnOperation.Column.IsFixedLength.Value);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Null(alterColumnOperation.Column.IsFixedLength);
        }
    }

    public abstract class AutoAndGenerateScenarios_AlterColumnMaxLength<TContextV1, TContextV2>
        : AutoAndGenerateTestCase<TContextV1, TContextV2>
        where TContextV1 : DbContext
        where TContextV2 : DbContext
    {
        private readonly int? _newMaxLength;

        protected AutoAndGenerateScenarios_AlterColumnMaxLength(int? newMaxLength)
        {
            _newMaxLength = newMaxLength;

            IsDownDataLoss = true;
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(_newMaxLength, alterColumnOperation.Column.MaxLength);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal(256, alterColumnOperation.Column.MaxLength);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnMaxLength_Max
        :
            AutoAndGenerateScenarios_AlterColumnMaxLength
                <AutoAndGenerateScenarios_AlterColumnMaxLength_Max.V1, AutoAndGenerateScenarios_AlterColumnMaxLength_Max.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnMaxLength_Max()
            : base(null)
        {
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).HasMaxLength(256);

                // Prevent convention override
                modelBuilder.Conventions.Remove<SqlCePropertyMaxLengthConvention>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).HasMaxLength(null);

                // Prevent convention override
                modelBuilder.Conventions.Remove<SqlCePropertyMaxLengthConvention>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnMaxLength_512
        :
            AutoAndGenerateScenarios_AlterColumnMaxLength
                <AutoAndGenerateScenarios_AlterColumnMaxLength_512.V1, AutoAndGenerateScenarios_AlterColumnMaxLength_512.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnMaxLength_512()
            : base(512)
        {
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).HasMaxLength(256);

                // Prevent convention override
                modelBuilder.Conventions.Remove<SqlCePropertyMaxLengthConvention>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).HasMaxLength(512);

                // Prevent convention override
                modelBuilder.Conventions.Remove<SqlCePropertyMaxLengthConvention>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnNullable :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnNullable.V1, AutoAndGenerateScenarios_AlterColumnNullable.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnNullable()
        {
            UpDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).IsRequired();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.False(alterColumnOperation.Column.IsNullable.Value);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Null(alterColumnOperation.Column.IsNullable);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnPrecision :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnPrecision.V1, AutoAndGenerateScenarios_AlterColumnPrecision.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnPrecision()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>().Property(ol => ol.Price).HasPrecision(9, 0);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>().Property(ol => ol.Price).HasPrecision(18, 0);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal((byte?)18, alterColumnOperation.Column.Precision);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal((byte?)9, alterColumnOperation.Column.Precision);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnScale :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnScale.V1, AutoAndGenerateScenarios_AlterColumnScale.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnScale()
        {
            IsDownDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>().Property(ol => ol.Price).HasPrecision(18, 0);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderLine>().Property(ol => ol.Price).HasPrecision(18, 2);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal((byte?)2, alterColumnOperation.Column.Scale);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.OrderLines" && o.Column.Name == "Price");
            Assert.NotNull(alterColumnOperation);

            Assert.Equal((byte?)0, alterColumnOperation.Column.Scale);
        }
    }

    public class AutoAndGenerateScenarios_AlterColumnUnicode :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterColumnUnicode.V1, AutoAndGenerateScenarios_AlterColumnUnicode.V2>
    {
        public AutoAndGenerateScenarios_AlterColumnUnicode()
        {
            IsDownDataLoss = true;
            UpDataLoss = true;
        }

        public class V1 : AutoAndGenerateContext_v1
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MigrationsStore>().Property(s => s.Name).IsUnicode(false);

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.False(alterColumnOperation.Column.IsUnicode.Value);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(1, migrationOperations.Count());

            var alterColumnOperation =
                migrationOperations.OfType<AlterColumnOperation>().SingleOrDefault(
                    o => o.Table == "dbo.MigrationsStores" && o.Column.Name == "Name");
            Assert.NotNull(alterColumnOperation);

            Assert.Null(alterColumnOperation.Column.IsUnicode);
        }
    }

    #endregion

    #region Modification Functions

    public class AutoAndGenerateScenarios_RenameProcedure :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_RenameProcedure.V1, AutoAndGenerateScenarios_RenameProcedure.V2>
    {
        public AutoAndGenerateScenarios_RenameProcedure()
        {
            IsDownDataLoss = false;
        }

        public class V1 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures(
                        m =>
                            {
                                m.Insert(c => c.HasName("ins_customer"));
                                m.Update(c => c.HasName("upd_customer"));
                                m.Delete(c => c.HasName("del_customer"));
                            });

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(3, migrationOperations.Count(o => o is RenameProcedureOperation));

            var renameProcedureOperation
                = migrationOperations
                    .OfType<RenameProcedureOperation>()
                    .Single(o => o.Name == "dbo.MigrationsCustomer_Insert");

            Assert.Equal("ins_customer", renameProcedureOperation.NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(3, migrationOperations.Count(o => o is RenameProcedureOperation));

            var renameProcedureOperation
                = migrationOperations
                    .OfType<RenameProcedureOperation>()
                    .Single(o => o.Name == "dbo.ins_customer");

            Assert.Equal("MigrationsCustomer_Insert", renameProcedureOperation.NewName);
        }
    }

    public class AutoAndGenerateScenarios_AlterProcedure :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_AlterProcedure.V1, AutoAndGenerateScenarios_AlterProcedure.V2>
    {
        public AutoAndGenerateScenarios_AlterProcedure()
        {
            IsDownNotSupported = true;
        }

        public class V1 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures(
                        m =>
                            {
                                m.Insert(c => c.Parameter(mc => mc.Age, "old"));
                                m.Update(c => c.Parameter(mc => mc.HomeAddress.City, "addr_city"));
                                m.Delete(c => c.Parameter(mc => mc.Id, "key"));
                            });

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyMigrationsException(MigrationsException migrationsException)
        {
            migrationsException.ValidateMessage("AutomaticStaleFunctions");
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(3, migrationOperations.Count(o => o is AlterProcedureOperation));

            var alterProcedureOperation
                = migrationOperations
                    .OfType<AlterProcedureOperation>()
                    .Single(o => o.Name == "dbo.MigrationsCustomer_Insert");

            Assert.True(alterProcedureOperation.Parameters.Any(p => p.Name == "old"));
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(0, migrationOperations.Count(o => o is AlterProcedureOperation));
        }
    }

    public class AutoAndGenerateScenarios_MoveProcedure :
        AutoAndGenerateTestCase<AutoAndGenerateScenarios_MoveProcedure.V1, AutoAndGenerateScenarios_MoveProcedure.V2>
    {
        public AutoAndGenerateScenarios_MoveProcedure()
        {
            IsDownDataLoss = false;
        }

        public class V1 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures();

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        public class V2 : AutoAndGenerateContext_v2
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<MigrationsCustomer>()
                    .MapToStoredProcedures(
                        m =>
                            {
                                m.Insert(c => c.HasName("MigrationsCustomer_Insert", "foo"));
                                m.Update(c => c.HasName("upd_customer", "bar"));
                            });

                this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
            }
        }

        protected override void VerifyUpOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count(o => o is MoveProcedureOperation));
            Assert.Equal(1, migrationOperations.Count(o => o is RenameProcedureOperation));

            var moveProcedureOperation
                = migrationOperations
                    .OfType<MoveProcedureOperation>()
                    .Single(o => o.Name == "dbo.MigrationsCustomer_Update");

            Assert.Equal("bar", moveProcedureOperation.NewSchema);

            var renameProcedureOperation
                = migrationOperations
                    .OfType<RenameProcedureOperation>()
                    .Single(o => o.Name == "bar.MigrationsCustomer_Update");

            Assert.Equal("upd_customer", renameProcedureOperation.NewName);
        }

        protected override void VerifyDownOperations(IEnumerable<MigrationOperation> migrationOperations)
        {
            Assert.Equal(2, migrationOperations.Count(o => o is MoveProcedureOperation));
            Assert.Equal(1, migrationOperations.Count(o => o is RenameProcedureOperation));
        }
    }

    #endregion
}
