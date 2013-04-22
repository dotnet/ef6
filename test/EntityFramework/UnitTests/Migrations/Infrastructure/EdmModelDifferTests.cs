// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.Infrastructure.FunctionsModel;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Migrations.UserRoles_v1;
    using System.Data.Entity.Migrations.UserRoles_v2;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;
    using Order = System.Data.Entity.Migrations.Order;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    public class EdmModelDifferTests : DbTestCase
    {
        [MigrationsTheory]
        public void System_operations_are_ignored_by_default()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo).GetModel();

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>();

            var model2 = modelBuilder.Build(ProviderInfo).GetModel();
            model2.Descendants().Each(e => e.SetAttributeValue(EdmXNames.IsSystemName, true));

            var operations = new EdmModelDiffer().Diff(model1, model2);

            Assert.Equal(0, operations.Count());
        }

        [MigrationsTheory]
        public void System_operations_are_included_when_requested()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo).GetModel();

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>();

            var model2 = modelBuilder.Build(ProviderInfo).GetModel();
            model2.Descendants().Each(e => e.SetAttributeValue(EdmXNames.IsSystemName, true));

            var operations = new EdmModelDiffer().Diff(model1, model2, includeSystemOperations: true);

            Assert.True(operations.All(o => o.IsSystem));

            var createTableOperation
                = operations.OfType<CreateTableOperation>().First();

            Assert.True(createTableOperation.IsSystem);

            operations = new EdmModelDiffer().Diff(model2, model1, includeSystemOperations: true);

            var dropTableOperation
                = operations.OfType<DropTableOperation>().First();

            Assert.True(dropTableOperation.IsSystem);
        }

        [MigrationsTheory]
        public void Can_diff_identical_models_at_different_edm_versions_and_no_diffs_produced()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<OrderLine>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V5_0);

            modelBuilder.Entity<OrderLine>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }

        [MigrationsTheory]
        public void Can_diff_different_models_at_different_edm_versions_and_diffs_produced()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<OrderLine>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V5_0);

            modelBuilder.Entity<OrderLine>().ToTable("Foos");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());
        }

        [MigrationsTheory]
        public void Bug_47549_crash_when_many_to_many_end_renamed_in_ospace()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<User>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<User2>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(13, operations.Count());
        }

        [MigrationsTheory]
        public void Can_detect_changed_primary_keys()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OrderLine>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OrderLine>().HasKey(
                ol => new
                          {
                              ol.Id,
                              ol.OrderId
                          });

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(3, operations.Count());

            var addPrimaryKeyOperation = operations.OfType<AddPrimaryKeyOperation>().Single();

            Assert.Equal("dbo.OrderLines", addPrimaryKeyOperation.Table);
            Assert.Equal("Id", addPrimaryKeyOperation.Columns.First());
            Assert.Equal("OrderId", addPrimaryKeyOperation.Columns.Last());

            var dropPrimaryKeyOperation = operations.OfType<DropPrimaryKeyOperation>().Single();

            Assert.Equal("dbo.OrderLines", dropPrimaryKeyOperation.Table);
            Assert.Equal("Id", dropPrimaryKeyOperation.Columns.Single());
        }

        [MigrationsTheory]
        public void Should_not_detect_pk_change_when_pk_column_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OrderLine>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OrderLine>().Property(ol => ol.Id).HasColumnName("pk_ID");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());
            Assert.True(operations.Single() is RenameColumnOperation);
        }

        [MigrationsTheory]
        public void Can_detect_changed_primary_key_when_column_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OrderLine>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OrderLine>().HasKey(
                ol => new
                          {
                              ol.Id,
                              ol.OrderId
                          });
            modelBuilder.Entity<OrderLine>().Property(ol => ol.Id).HasColumnName("pk_ID");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(3, operations.Count());

            var addPrimaryKeyOperation = operations.OfType<AddPrimaryKeyOperation>().Single();

            Assert.Equal("dbo.OrderLines", addPrimaryKeyOperation.Table);
            Assert.Equal("pk_ID", addPrimaryKeyOperation.Columns.First());
            Assert.Equal("OrderId", addPrimaryKeyOperation.Columns.Last());

            var dropPrimaryKeyOperation = operations.OfType<DropPrimaryKeyOperation>().Single();

            Assert.Equal("dbo.OrderLines", dropPrimaryKeyOperation.Table);
            Assert.Equal("Id", dropPrimaryKeyOperation.Columns.Single());
        }

        [MigrationsTheory]
        public void Can_detect_changed_primary_key_when_table_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OrderLine>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OrderLine>()
                        .HasKey(
                            ol => new
                                      {
                                          ol.Id,
                                          ol.OrderId
                                      })
                        .ToTable("tbl_OrderLines");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(4, operations.Count());

            var addPrimaryKeyOperation = operations.OfType<AddPrimaryKeyOperation>().Single();

            Assert.Equal("dbo.tbl_OrderLines", addPrimaryKeyOperation.Table);
            Assert.Equal("Id", addPrimaryKeyOperation.Columns.First());
            Assert.Equal("OrderId", addPrimaryKeyOperation.Columns.Last());

            var dropPrimaryKeyOperation = operations.OfType<DropPrimaryKeyOperation>().Single();

            Assert.Equal("dbo.OrderLines", dropPrimaryKeyOperation.Table);
            Assert.Equal("Id", dropPrimaryKeyOperation.Columns.Single());
        }

        [MigrationsTheory]
        public void Cross_provider_diff_should_be_clean_when_same_model()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();

            var model1 = modelBuilder.Build(new DbProviderInfo(DbProviders.Sql, "2008"));

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();

            var model2 = modelBuilder.Build(new DbProviderInfo(DbProviders.SqlCe, "4"));

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }

        [MigrationsTheory]
        public void Can_detect_dropped_columns()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>().Ignore(o => o.Version);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var dropColumnOperation = operations.OfType<DropColumnOperation>().Single();

            Assert.Equal("ordering.Orders", dropColumnOperation.Table);
            Assert.Equal("Version", dropColumnOperation.Name);

            var inverse = (AddColumnOperation)dropColumnOperation.Inverse;

            Assert.NotNull(inverse);
            Assert.Equal("ordering.Orders", inverse.Table);
            Assert.Equal("Version", inverse.Column.Name);
        }

        [MigrationsTheory]
        public void Can_detect_timestamp_columns()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();

            var model2 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>().Ignore(o => o.Version);

            var model1 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var column = operations.OfType<AddColumnOperation>().Single().Column;

            Assert.True(column.IsTimestamp);
        }

        [MigrationsTheory]
        public void Should_not_detect_identity_when_not_valid_identity_type_for_ddl()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            WhenSqlCe(
                () =>
                    {
                        modelBuilder.Entity<MigrationsStore>().Ignore(e => e.Location);
                        modelBuilder.Entity<MigrationsStore>().Ignore(e => e.FloorPlan);
                    });

            modelBuilder
                .Entity<MigrationsStore>()
                .Property(s => s.Name)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var column = operations.OfType<CreateTableOperation>().Single().Columns.Single(c => c.Name == "Name");

            Assert.False(column.IsIdentity);
        }

        [MigrationsTheory]
        public void Can_detect_changed_columns()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>().Property(c => c.FullName).HasMaxLength(25).IsUnicode(false);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());
            var column = operations.OfType<AlterColumnOperation>().Single().Column;

            Assert.Equal(25, column.MaxLength);

            if (DatabaseProvider != DatabaseProvider.SqlServerCe)
            {
                Assert.False(column.IsUnicode.Value);
            }

            var inverse = (AlterColumnOperation)operations.OfType<AlterColumnOperation>().Single().Inverse;

            Assert.NotNull(inverse);
            Assert.Equal("FullName", inverse.Column.Name);

            if (DatabaseProvider != DatabaseProvider.SqlServerCe)
            {
                Assert.Null(inverse.Column.MaxLength);
            }
            else
            {
                Assert.Equal(4000, inverse.Column.MaxLength);
            }

            Assert.Null(inverse.Column.IsUnicode);
        }

        [MigrationsTheory] // CodePlex 726
        public void Can_handle_max_length_set_to_MAX_in_SSDL()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>().Property(c => c.Photo).HasMaxLength(100);
            modelBuilder.Entity<MigrationsCustomer>().Property(c => c.FullName).HasMaxLength(100);
            var sourceModel = modelBuilder.Build(ProviderInfo).GetModel();

            modelBuilder.Entity<MigrationsCustomer>().Property(c => c.Photo).IsMaxLength();
            modelBuilder.Entity<MigrationsCustomer>().Property(c => c.FullName).IsMaxLength();
            var targetModel = modelBuilder.Build(ProviderInfo).GetModel();

            // Artificially add MaxLength=MAX to a couple of properties
            var customerEntity = targetModel
                .Elements().First()
                .Elements().First()
                .Elements().Single(e => e.Name.LocalName == "StorageModels")
                .Elements().Single(e => e.Name.LocalName == "Schema")
                .Elements().Single(e => e.Name.LocalName == "EntityType" && e.Attributes("Name").Any(a => a.Value == "MigrationsCustomer"));

            customerEntity.Elements().Single(e => e.Name.LocalName == "Property" && e.Attributes("Name").Any(a => a.Value == "FullName"))
                          .Add(new XAttribute("MaxLength", "Max"));
            customerEntity.Elements().Single(e => e.Name.LocalName == "Property" && e.Attributes("Name").Any(a => a.Value == "Photo"))
                          .Add(new XAttribute("MaxLength", "MAX"));

            DbProviderInfo providerInfo;
            var storageMappingItemCollection = sourceModel.GetStorageMappingItemCollection(out providerInfo);

            var sourceMetadata = new EdmModelDiffer.ModelMetadata
                                     {
                                         Model = sourceModel,
                                         StoreItemCollection = storageMappingItemCollection.StoreItemCollection,
                                         StorageEntityContainerMapping
                                             = storageMappingItemCollection.GetItems<StorageEntityContainerMapping>().Single(),
                                         ProviderManifest = GetProviderManifest(providerInfo),
                                         ProviderInfo = providerInfo
                                     };

            var targetMetadata = new EdmModelDiffer.ModelMetadata
                                     {
                                         Model = targetModel,
                                         // Use the source model here since it doesn't effect the test and the SQL Server provider
                                         // won't load the target model
                                         StoreItemCollection = storageMappingItemCollection.StoreItemCollection,
                                         StorageEntityContainerMapping
                                             = storageMappingItemCollection.GetItems<StorageEntityContainerMapping>().Single(),
                                         ProviderManifest = GetProviderManifest(providerInfo),
                                         ProviderInfo = providerInfo
                                     };

            var operations = new EdmModelDiffer().Diff(sourceMetadata, targetMetadata, false, null, null);

            Assert.Equal(2, operations.Count());
            operations.OfType<AlterColumnOperation>().Each(
                o =>
                    {
                        Assert.Null(o.Column.MaxLength);
                        Assert.Equal(100, ((AlterColumnOperation)o.Inverse).Column.MaxLength);
                    });
        }

        private static DbProviderManifest GetProviderManifest(DbProviderInfo providerInfo)
        {
            return DbConfiguration.GetService<DbProviderFactory>(providerInfo.ProviderInvariantName)
                                  .GetProviderServices()
                                  .GetProviderManifest(providerInfo.ProviderManifestToken);
        }

        [MigrationsTheory]
        public void Can_populate_table_model_for_added_tables()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.OfType<AddForeignKeyOperation>().Count());
            var createTableOperation = operations.OfType<CreateTableOperation>().Single(t => t.Name == "ordering.Orders");

            Assert.Equal(4, createTableOperation.Columns.Count());
            Assert.Equal(1, createTableOperation.PrimaryKey.Columns.Count());
            Assert.Equal("OrderId", createTableOperation.PrimaryKey.Columns.Single());

            var customerIdColumn = createTableOperation.Columns.Single(c => c.Name == "MigrationsCustomer_Id");

            Assert.Equal(PrimitiveTypeKind.Int32, customerIdColumn.Type);
            Assert.Null(customerIdColumn.IsNullable);

            var orderIdColumn = createTableOperation.Columns.Single(c => c.Name == "OrderId");

            Assert.True(orderIdColumn.IsIdentity);
        }

        [MigrationsTheory]
        public void Can_detect_added_tables()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<OrderLine>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());
            Assert.Equal(1, operations.OfType<CreateTableOperation>().Count());
        }

        [MigrationsTheory]
        public void Can_detect_added_modification_functions()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            var model2 = new TestContext();

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel());

            var createProcedureOperations
                = new EdmModelDiffer()
                    .Diff(
                        model1.GetModel(), 
                        model2.GetModel(), 
                        false, 
                        commandTreeGenerator, 
                        new SqlServerMigrationSqlGenerator())
                    .OfType<CreateProcedureOperation>()
                    .ToList();

            Assert.Equal(14, createProcedureOperations.Count);
            Assert.True(createProcedureOperations.All(c => c.Name.Any()));
            Assert.True(createProcedureOperations.All(c => c.BodySql.Any()));
        }

        [MigrationsTheory]
        public void Can_detect_removed_modification_functions()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<OrderLine>().MapToStoredProcedures();

            var model2 = new TestContext();

            var dropProcedureOperations
                = new EdmModelDiffer().Diff(model2.GetModel(), model1.GetModel())
                                      .OfType<DropProcedureOperation>()
                                      .ToList();

            Assert.Equal(14, dropProcedureOperations.Count);
            Assert.True(dropProcedureOperations.All(c => c.Name.Any()));
        }

        [MigrationsTheory]
        public void Can_detect_custom_store_type()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<OrderLine>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            var createTableOperation = operations.OfType<CreateTableOperation>().Single();

            var column = createTableOperation.Columns.Single(c => c.Name == "Total");

            Assert.Equal("money", column.StoreType);

            createTableOperation.Columns.Except(new[] { column }).Each(c => Assert.Null(c.StoreType));
        }

        [MigrationsTheory]
        public void Can_detect_added_columns()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OrderLine>();

            var model2 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<OrderLine>().Ignore(ol => ol.OrderId);

            var model1 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());
            var addColumnOperation = operations.OfType<AddColumnOperation>().Single();

            Assert.Equal("dbo.OrderLines", addColumnOperation.Table);
            Assert.Equal("OrderId", addColumnOperation.Column.Name);
            Assert.Equal(PrimitiveTypeKind.Int32, addColumnOperation.Column.Type);
            Assert.False(addColumnOperation.Column.IsNullable.Value);
        }

        [MigrationsTheory]
        public void Can_detect_added_foreign_keys()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<Order>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(4, operations.Count());
            Assert.Equal(2, operations.OfType<CreateTableOperation>().Count());
            Assert.Equal(1, operations.OfType<CreateIndexOperation>().Count());

            // create fk indexes first
            Assert.True(
                operations.Select(
                    (o, i) => new
                                  {
                                      o,
                                      i
                                  }).Single(a => a.o is CreateIndexOperation).i <
                operations.Select(
                    (o, i) => new
                                  {
                                      o,
                                      i
                                  }).Single(a => a.o is AddForeignKeyOperation).i);

            var addForeignKeyOperation = operations.OfType<AddForeignKeyOperation>().Single();

            Assert.Equal("ordering.Orders", addForeignKeyOperation.PrincipalTable);
            Assert.Equal("OrderId", addForeignKeyOperation.PrincipalColumns.Single());
            Assert.Equal("dbo.OrderLines", addForeignKeyOperation.DependentTable);
            Assert.Equal("OrderId", addForeignKeyOperation.DependentColumns.Single());
            Assert.True(addForeignKeyOperation.CascadeDelete);
        }

        [MigrationsTheory]
        public void Can_detect_changed_foreign_keys_when_cascade()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithOptional().WillCascadeOnDelete(false);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(4, operations.Count());
            Assert.Equal(1, operations.OfType<DropForeignKeyOperation>().Count());
            Assert.Equal(1, operations.OfType<DropIndexOperation>().Count());
            Assert.Equal(1, operations.OfType<CreateIndexOperation>().Count());
            var addForeignKeyOperation = operations.OfType<AddForeignKeyOperation>().Single();

            Assert.Equal("ordering.Orders", addForeignKeyOperation.PrincipalTable);
            Assert.Equal("OrderId", addForeignKeyOperation.PrincipalColumns.Single());
            Assert.Equal("dbo.OrderLines", addForeignKeyOperation.DependentTable);
            Assert.Equal("OrderId", addForeignKeyOperation.DependentColumns.Single());
            Assert.False(addForeignKeyOperation.CascadeDelete);
        }

        [MigrationsTheory]
        public void Should_not_detect_changed_foreign_keys_when_multiplicity()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithRequired();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }

        [MigrationsTheory]
        public void Can_detect_removed_foreign_keys()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<Order>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model2.GetModel(), model1.GetModel());

            Assert.Equal(4, operations.Count());
            Assert.Equal(2, operations.OfType<DropTableOperation>().Count());
            Assert.Equal(1, operations.OfType<DropIndexOperation>().Count());

            // drop fks before indexes
            Assert.True(
                operations.Select(
                    (o, i) => new
                                  {
                                      o,
                                      i
                                  }).Single(a => a.o is DropForeignKeyOperation).i <
                operations.Select(
                    (o, i) => new
                                  {
                                      o,
                                      i
                                  }).Single(a => a.o is DropIndexOperation).i);

            var dropForeignKeyOperation = operations.OfType<DropForeignKeyOperation>().Single();

            Assert.Equal("ordering.Orders", dropForeignKeyOperation.PrincipalTable);
            Assert.Equal("dbo.OrderLines", dropForeignKeyOperation.DependentTable);
            Assert.Equal("OrderId", dropForeignKeyOperation.DependentColumns.Single());

            var inverse = (AddForeignKeyOperation)dropForeignKeyOperation.Inverse;

            Assert.Equal("ordering.Orders", inverse.PrincipalTable);
            Assert.Equal("OrderId", inverse.PrincipalColumns.Single());
            Assert.Equal("dbo.OrderLines", inverse.DependentTable);
            Assert.Equal("OrderId", inverse.DependentColumns.Single());
        }

        [MigrationsTheory]
        public void Can_detect_removed_tables()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OrderLine>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());
            var inverse = (CreateTableOperation)operations.OfType<DropTableOperation>().Single().Inverse;

            Assert.NotNull(inverse);
            Assert.Equal("dbo.OrderLines", inverse.Name);
            Assert.Equal(8, inverse.Columns.Count());
        }

        [MigrationsTheory]
        public void Can_detect_moved_tables()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>().ToTable("MigrationsCustomers", "foo");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());
            var moveTableOperation = operations.OfType<MoveTableOperation>().Single();

            Assert.Equal("dbo.MigrationsCustomers", moveTableOperation.Name);
            Assert.Equal("foo", moveTableOperation.NewSchema);
        }

        [MigrationsTheory]
        public void Can_detect_moved_system_tables()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo).GetModel();

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>().ToTable("MigrationsCustomer", "foo");

            var model2 = modelBuilder.Build(ProviderInfo).GetModel();
            model2.Descendants().Each(e => e.SetAttributeValue(EdmXNames.IsSystemName, true));

            var operations = new EdmModelDiffer().Diff(model1, model2, includeSystemOperations: true);

            var moveTableOperation
                = operations.OfType<MoveTableOperation>().Single();

            Assert.True(moveTableOperation.IsSystem);
            Assert.NotNull(moveTableOperation.CreateTableOperation);
            Assert.Equal("dbo.MigrationsCustomer", moveTableOperation.Name);
            Assert.Equal("foo.MigrationsCustomer", moveTableOperation.CreateTableOperation.Name);
        }

        [MigrationsTheory]
        public void Can_detect_simple_table_rename()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>().ToTable("Customer");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());
            var tableRename = operations.OfType<RenameTableOperation>().Single();

            Assert.Equal("dbo.MigrationsCustomers", tableRename.Name);
            Assert.Equal("Customer", tableRename.NewName);
        }

        [MigrationsTheory]
        public void Can_detect_simple_column_rename()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>().Property(p => p.Name).HasColumnName("col_Name");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var columnRename = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("dbo.MigrationsCustomers", columnRename.Table);
            Assert.Equal("Name", columnRename.Name);
            Assert.Equal("col_Name", columnRename.NewName);
        }

        [MigrationsTheory]
        public void Can_detect_simple_column_rename_when_entity_split()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>()
                        .Map(
                            mc =>
                                {
                                    mc.Properties(
                                        c => new
                                                 {
                                                     c.Id,
                                                     c.FullName,
                                                     c.HomeAddress,
                                                     c.WorkAddress
                                                 });
                                    mc.ToTable("MigrationsCustomers");
                                })
                        .Map(
                            mc =>
                                {
                                    mc.Properties(
                                        c => new
                                                 {
                                                     c.Name
                                                 });
                                    mc.ToTable("Customers_Split");
                                });

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>().Property(p => p.Name).HasColumnName("col_Name");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var columnRename = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("dbo.Customers_Split", columnRename.Table);
            Assert.Equal("Name", columnRename.Name);
            Assert.Equal("col_Name", columnRename.NewName);
        }

        [MigrationsTheory]
        public void Can_detect_complex_column_rename()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>().Property(p => p.HomeAddress.City).HasColumnName("HomeCity");
            modelBuilder.Entity<MigrationsCustomer>().Property(p => p.WorkAddress.City).HasColumnName("WorkCity");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var columnRename = operations.OfType<RenameColumnOperation>().ElementAt(0);

            Assert.Equal("dbo.MigrationsCustomers", columnRename.Table);
            Assert.Equal("HomeAddress_City", columnRename.Name);
            Assert.Equal("HomeCity", columnRename.NewName);

            columnRename = operations.OfType<RenameColumnOperation>().ElementAt(1);

            Assert.Equal("dbo.MigrationsCustomers", columnRename.Table);
            Assert.Equal("WorkAddress_City", columnRename.Name);
            Assert.Equal("WorkCity", columnRename.NewName);
        }

        [MigrationsTheory]
        public void Can_detect_ia_column_rename()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();
            modelBuilder.Entity<MigrationsCustomer>().HasKey(
                p => new
                         {
                             p.Id,
                             p.Name
                         });

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>()
                        .HasMany(p => p.Orders)
                        .WithOptional()
                        .Map(c => c.MapKey("CustomerId", "CustomerName"));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var columnRename = operations.OfType<RenameColumnOperation>().ElementAt(0);

            Assert.Equal("ordering.Orders", columnRename.Table);
            Assert.Equal("MigrationsCustomer_Id", columnRename.Name);
            Assert.Equal("CustomerId", columnRename.NewName);

            columnRename = operations.OfType<RenameColumnOperation>().ElementAt(1);

            Assert.Equal("ordering.Orders", columnRename.Table);
            Assert.Equal("MigrationsCustomer_Name", columnRename.Name);
            Assert.Equal("CustomerName", columnRename.NewName);
        }

        [MigrationsTheory]
        public void Should_only_detect_single_column_rename_when_fk_association()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<OrderLine>().Property(ol => ol.OrderId).HasColumnName("Order_Id");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var columnRename = operations.OfType<RenameColumnOperation>().ElementAt(0);

            Assert.Equal("dbo.OrderLines", columnRename.Table);
            Assert.Equal("OrderId", columnRename.Name);
            Assert.Equal("Order_Id", columnRename.NewName);
        }

        [MigrationsTheory]
        public void Should_only_detect_single_column_rename_when_ia_to_fk_association()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Comment>().Ignore(c => c.MigrationsBlogId); // IA

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Comment>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var columnRename = operations.OfType<RenameColumnOperation>().ElementAt(0);

            Assert.Equal("dbo.Comments", columnRename.Table);
            Assert.Equal("Blog_MigrationsBlogId", columnRename.Name);
            Assert.Equal("MigrationsBlogId", columnRename.NewName);
        }

        [MigrationsTheory]
        public void Should_detect_fk_drop_create_when_ia_to_fk_association_and_cascade_changes()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Comment>().Ignore(c => c.MigrationsBlogId); // IA

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Comment>().HasOptional(c => c.Blog).WithMany().WillCascadeOnDelete();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(5, operations.Count());

            var dropForeignKeyOperation = operations.OfType<DropForeignKeyOperation>().Single();

            Assert.Equal("dbo.Comments", dropForeignKeyOperation.DependentTable);
            Assert.Equal("Blog_MigrationsBlogId", dropForeignKeyOperation.DependentColumns.Single());

            var dropForeignKeyOperationInverse = (AddForeignKeyOperation)dropForeignKeyOperation.Inverse;

            Assert.Equal("dbo.Comments", dropForeignKeyOperationInverse.DependentTable);
            Assert.Equal("Blog_MigrationsBlogId", dropForeignKeyOperationInverse.DependentColumns.Single());

            var dropIndexOperation = operations.OfType<DropIndexOperation>().Single();

            Assert.Equal("dbo.Comments", dropIndexOperation.Table);
            Assert.Equal("Blog_MigrationsBlogId", dropIndexOperation.Columns.Single());

            var dropIndexOperationInverse = (CreateIndexOperation)dropIndexOperation.Inverse;

            Assert.Equal("dbo.Comments", dropIndexOperationInverse.Table);
            Assert.Equal("Blog_MigrationsBlogId", dropIndexOperationInverse.Columns.Single());
        }

        [MigrationsTheory]
        public void Can_detect_discriminator_column_rename()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<MigrationsCustomer>()
                .Map(
                    c =>
                        {
                            c.Requires("disc0").HasValue("2");
                            c.Requires("disc1").HasValue("PC");
                        });

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<MigrationsCustomer>()
                .Map(
                    c =>
                        {
                            c.Requires("new_disc1").HasValue("PC");
                            c.Requires("new_disc0").HasValue("2");
                        });

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var columnRename = operations.OfType<RenameColumnOperation>().ElementAt(0);

            Assert.Equal("dbo.MigrationsCustomers", columnRename.Table);
            Assert.Equal("disc0", columnRename.Name);
            Assert.Equal("new_disc0", columnRename.NewName);

            columnRename = operations.OfType<RenameColumnOperation>().ElementAt(1);

            Assert.Equal("dbo.MigrationsCustomers", columnRename.Table);
            Assert.Equal("disc1", columnRename.Name);
            Assert.Equal("new_disc1", columnRename.NewName);
        }

        [MigrationsTheory]
        public void Should_not_detect_diffs_when_models_are_identical()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(
                model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }
    }
}
