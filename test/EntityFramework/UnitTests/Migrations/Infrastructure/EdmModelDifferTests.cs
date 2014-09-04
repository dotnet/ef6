// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Infrastructure.FunctionsModel;
    using System.Data.Entity.Migrations.Infrastructure.Tph_to_tpt_v1;
    using System.Data.Entity.Migrations.Infrastructure.Tph_to_tpt_v2;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.OSpaceRenames_v1;
    using System.Data.Entity.Migrations.OSpaceRenames_v2;
    using System.Data.Entity.Migrations.UserRoles_v1;
    using System.Data.Entity.Migrations.UserRoles_v2;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;
    using Order = System.Data.Entity.Migrations.Order;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    public class EdmModelDifferTests : DbTestCase
    {
        #region Table Renames

        public class ManyManySelfRef
        {
            public int Id { get; set; }
            public ICollection<ManyManySelfRef> From { get; set; }
            public ICollection<ManyManySelfRef> To { get; set; }
        }

        [MigrationsTheory]
        public void Can_detect_renamed_many_to_many_table()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ManyManySelfRef>();

            var model1 = modelBuilder.Build(ProviderInfo);
            
            modelBuilder.Entity<ManyManySelfRef>().ToTable("Renamed");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var tableRename = (RenameTableOperation)operations.Single();

            Assert.Equal("dbo.ManyManySelfRefs", tableRename.Name);
            Assert.Equal("Renamed", tableRename.NewName);
        }

        [MigrationsTheory]
        public void Can_detect_simple_table_rename()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>().ToTable("Customer");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var tableRename = (RenameTableOperation)operations.Single();

            Assert.Equal("dbo.MigrationsCustomers", tableRename.Name);
            Assert.Equal("Customer", tableRename.NewName);
        }

        [MigrationsTheory]
        public void Table_rename_does_not_trigger_index_drop_and_create()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>()
                .Property(e => e.Id)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella")));

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>()
                .ToTable("Customer")
                .Property(e => e.Id)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella")));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations= new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.IsType<RenameTableOperation>(operations.Single());
        }

        public class Foo
        {
            public int Id { get; set; }
        }

        public class Bar
        {
            public int Id { get; set; }
        }

        public class Baz
        {
            public int Id { get; set; }
        }

        [MigrationsTheory]
        public void Should_introduce_temp_table_renames_when_transitive_dependencies()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Foo>().ToTable("Foo1");
            modelBuilder.Entity<Bar>().ToTable("Bar1");
            modelBuilder.Entity<Baz>().ToTable("Baz1");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Foo>().ToTable("Baz1");
            modelBuilder.Entity<Bar>().ToTable("Foo1");
            modelBuilder.Entity<Baz>().ToTable("Bar1");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(5, operations.Count());

            var renameOperations
                = operations.OfType<RenameTableOperation>().ToList();

            Assert.Equal(5, renameOperations.Count());

            var renameTableOperation = renameOperations.ElementAt(0);

            Assert.Equal("dbo.Foo1", renameTableOperation.Name);
            Assert.Equal("__mig_tmp__0", renameTableOperation.NewName);

            renameTableOperation = renameOperations.ElementAt(1);

            Assert.Equal("dbo.Bar1", renameTableOperation.Name);
            Assert.Equal("__mig_tmp__1", renameTableOperation.NewName);

            renameTableOperation = renameOperations.ElementAt(2);

            Assert.Equal("dbo.Baz1", renameTableOperation.Name);
            Assert.Equal("Bar1", renameTableOperation.NewName);

            renameTableOperation = renameOperations.ElementAt(3);

            Assert.Equal("__mig_tmp__0", renameTableOperation.Name);
            Assert.Equal("Baz1", renameTableOperation.NewName);

            renameTableOperation = renameOperations.ElementAt(4);

            Assert.Equal("__mig_tmp__1", renameTableOperation.Name);
            Assert.Equal("Foo1", renameTableOperation.NewName);
        }

        [MigrationsTheory]
        public void Should_not_introduce_temp_table_renames_when_tables_in_different_schemas()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Foo>().ToTable("Foo1", "foo");
            modelBuilder.Entity<Bar>().ToTable("Bar1", "bar");
            modelBuilder.Entity<Baz>().ToTable("Baz1", "baz");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Foo>().ToTable("Baz1", "foo");
            modelBuilder.Entity<Bar>().ToTable("Foo1", "bar");
            modelBuilder.Entity<Baz>().ToTable("Bar1", "baz");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(3, operations.Count());

            var renameOperations
                = operations.OfType<RenameTableOperation>().ToList();

            Assert.Equal(3, renameOperations.Count());

            var renameTableOperation = renameOperations.ElementAt(0);

            Assert.Equal("foo.Foo1", renameTableOperation.Name);
            Assert.Equal("Baz1", renameTableOperation.NewName);

            renameTableOperation = renameOperations.ElementAt(1);

            Assert.Equal("bar.Bar1", renameTableOperation.Name);
            Assert.Equal("Foo1", renameTableOperation.NewName);

            renameTableOperation = renameOperations.ElementAt(2);

            Assert.Equal("baz.Baz1", renameTableOperation.Name);
            Assert.Equal("Bar1", renameTableOperation.NewName);
        }

        public class B
        {
            public int Id { get; set; }
            public A A { get; set; }
        }

        public class A
        {
            public int Id { get; set; }
            public B B { get; set; }
        }

        [MigrationsTheory]
        public void Should_not_detect_table_rename_when_sets_have_different_names()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<A>().ToTable("Foos");
            modelBuilder.Entity<B>().ToTable("Foos");
            modelBuilder.Entity<B>().HasRequired(b => b.A).WithRequiredPrincipal(a => a.B);

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<B>().ToTable("Foos");
            modelBuilder.Entity<A>().ToTable("Foos");
            modelBuilder.Entity<B>().HasRequired(b => b.A).WithRequiredPrincipal(a => a.B);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer()
                    .Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }

        [MigrationsTheory]
        public void Can_detect_table_rename_when_ospace_type_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRename1>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRename2>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var renameTableOperation = (RenameTableOperation)operations.Single();

            Assert.Equal("dbo.TableRename1", renameTableOperation.Name);
            Assert.Equal("TableRename2", renameTableOperation.NewName);
        }

        [MigrationsTheory]
        public void Should_not_detect_table_rename_when_ospace_type_renamed_when_table_name_unchanged()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRename1>().ToTable("Foo");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRename2>().ToTable("Foo");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }

        [MigrationsTheory]
        public void Can_detect_table_rename_when_ospace_type_renamed_on_one_side_of_many_to_many()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<TableRenameManyManyLeft1>()
                .ToTable("Left")
                .HasMany(l => l.Rights)
                .WithMany(r => r.Lefts)
                .Map(
                    m =>
                    {
                        m.MapLeftKey("LeftId");
                        m.MapRightKey("RightId");
                    });

            modelBuilder
                .Entity<OSpaceRenames_v1.TableRenameManyManyRight>()
                .ToTable("Right");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<TableRenameManyManyLeft2>()
                .ToTable("Left")
                .HasMany(l => l.Rights)
                .WithMany(r => r.Lefts)
                .Map(
                    m =>
                    {
                        m.MapLeftKey("LeftId");
                        m.MapRightKey("RightId");
                    });

            modelBuilder
                .Entity<OSpaceRenames_v2.TableRenameManyManyRight>()
                .ToTable("Right");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var renameTableOperation = (RenameTableOperation)operations.Single();

            Assert.Equal("dbo.TableRenameManyManyLeft1TableRenameManyManyRight", renameTableOperation.Name);
            Assert.Equal("TableRenameManyManyLeft2TableRenameManyManyRight", renameTableOperation.NewName);
        }

        [MigrationsTheory]
        public void Should_not_detect_table_rename_when_ospace_type_renamed_on_one_side_of_many_to_many_when_join_table_unchanged()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<TableRenameManyManyLeft1>()
                .ToTable("Left")
                .HasMany(l => l.Rights)
                .WithMany(r => r.Lefts)
                .Map(
                    m =>
                    {
                        m.MapLeftKey("LeftId");
                        m.MapRightKey("RightId");
                        m.ToTable("Join");
                    });

            modelBuilder
                .Entity<OSpaceRenames_v1.TableRenameManyManyRight>()
                .ToTable("Right");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<TableRenameManyManyLeft2>()
                .ToTable("Left")
                .HasMany(l => l.Rights)
                .WithMany(r => r.Lefts)
                .Map(
                    m =>
                    {
                        m.MapLeftKey("LeftId");
                        m.MapRightKey("RightId");
                        m.ToTable("Join");
                    });

            modelBuilder
                .Entity<OSpaceRenames_v2.TableRenameManyManyRight>()
                .ToTable("Right");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }

        [MigrationsTheory]
        public void Can_detect_table_rename_when_ospace_type_renamed_tpt()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRename1>().ToTable("Base");
            modelBuilder.Entity<TableRenameDerived1>().ToTable("TableRenameDerived1");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRename2>().ToTable("Base");
            modelBuilder.Entity<TableRenameDerived2>().ToTable("TableRenameDerived2");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var renameTableOperation = operations.OfType<RenameTableOperation>().Single();

            Assert.Equal("dbo.TableRenameDerived1", renameTableOperation.Name);
            Assert.Equal("TableRenameDerived2", renameTableOperation.NewName);
        }

        [MigrationsTheory]
        public void Should_not_detect_table_rename_when_ospace_type_renamed_when_table_name_unchanged_tpt()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRename1>().ToTable("Base");
            modelBuilder.Entity<TableRenameDerived1>().ToTable("Derived");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRename2>().ToTable("Base");
            modelBuilder.Entity<TableRenameDerived2>().ToTable("Derived");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }

        [MigrationsTheory]
        public void Can_detect_table_renames_when_ospace_type_renamed_entity_splitting()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<TableRenameEntitySplit1>()
                .Map(
                    m =>
                    {
                        m.Properties(p => new { p.Id });
                        m.ToTable("TableRenameEntitySplit1");
                    }
                ).Map(
                    m =>
                    {
                        m.Properties(p => new { p.Member });
                        m.ToTable("Split1");
                    }
                );

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<TableRenameEntitySplit2>()
                .Map(
                    m =>
                    {
                        m.Properties(p => new { p.Id });
                        m.ToTable("TableRenameEntitySplit2");
                    }
                ).Map(
                    m =>
                    {
                        m.Properties(p => new { p.Member });
                        m.ToTable("Split2");
                    }
                );

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());
            Assert.Equal(2, operations.OfType<RenameTableOperation>().Count());
        }

        [MigrationsTheory]
        public void Should_not_detect_table_renames_when_ospace_type_renamed_entity_splitting_when_table_names_unchanged()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<TableRenameEntitySplit1>()
                .Map(
                    m =>
                    {
                        m.Properties(p => new { p.Id });
                        m.ToTable("TableRenameEntitySplit1");
                    }
                ).Map(
                    m =>
                    {
                        m.Properties(p => new { p.Member });
                        m.ToTable("Split1");
                    }
                );

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<TableRenameEntitySplit2>()
                .Map(
                    m =>
                    {
                        m.Properties(p => new { p.Id });
                        m.ToTable("TableRenameEntitySplit1");
                    }
                ).Map(
                    m =>
                    {
                        m.Properties(p => new { p.Member });
                        m.ToTable("Split1");
                    }
                );

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.OfType<RenameTableOperation>().Count());
        }

        [MigrationsTheory]
        public void Can_detect_table_rename_when_ospace_type_renamed_table_splitting()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRenameTableSplit1>();
            modelBuilder.Entity<TableRenameTableSplit1>().HasRequired(t => t.Payload).WithRequiredPrincipal();
            modelBuilder.Entity<TableRenameTableSplitPayload1>().ToTable("TableRenameTableSplit1");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRenameTableSplit2>();
            modelBuilder.Entity<TableRenameTableSplit2>().HasRequired(t => t.Payload).WithRequiredPrincipal();
            modelBuilder.Entity<TableRenameTableSplitPayload2>().ToTable("TableRenameTableSplit2");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var renameTableOperation = (RenameTableOperation)operations.Single();

            Assert.Equal("dbo.TableRenameTableSplit1", renameTableOperation.Name);
            Assert.Equal("TableRenameTableSplit2", renameTableOperation.NewName);
        }

        [MigrationsTheory]
        public void Should_not_detect_table_rename_when_ospace_type_renamed_table_splitting_when_table_name_unchanged()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRenameTableSplit1>().ToTable("TableRenameTableSplit");
            modelBuilder.Entity<TableRenameTableSplit1>().HasRequired(t => t.Payload).WithRequiredPrincipal();
            modelBuilder.Entity<TableRenameTableSplitPayload1>().ToTable("TableRenameTableSplit");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRenameTableSplit2>().ToTable("TableRenameTableSplit");
            modelBuilder.Entity<TableRenameTableSplit2>().HasRequired(t => t.Payload).WithRequiredPrincipal();
            modelBuilder.Entity<TableRenameTableSplitPayload2>().ToTable("TableRenameTableSplit");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }

        public class A1
        {
            public int Id { get; set; }
            public ICollection<B1> Bs { get; set; }
        }

        public class B1
        {
            public int Id { get; set; }
            public ICollection<A1> As { get; set; }
        }

        [MigrationsTheory]
        public void Should_detect_table_rename_when_associations_have_different_names_and_table_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<A1>();
            modelBuilder.Entity<B1>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<B1>();
            modelBuilder.Entity<A1>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(3, operations.Count());

            var tableRename 
                = operations.OfType<RenameTableOperation>().Single();

            Assert.Equal("dbo.B1A1", tableRename.Name);
            Assert.Equal("A1B1", tableRename.NewName);
        }

        public class Customer
        {
            public int Id { get; set; }
        }

        public class GoldCustomer : Customer
        {
            public string GoldStatus { get; set; }
        }

        [MigrationsTheory]
        public void Should_not_detect_table_rename_when_from_table_exists_in_target()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<GoldCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<GoldCustomer>().ToTable("GoldCustomers");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(5, operations.Count());
            Assert.False(operations.OfType<RenameTableOperation>().Any());
            Assert.True(operations.OfType<CreateTableOperation>().Any());
            Assert.Equal(2, operations.OfType<DropColumnOperation>().Count());
        }

        [MigrationsTheory]
        public void Should_not_detect_table_rename_when_to_table_exists_in_target()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<GoldCustomer>().ToTable("GoldCustomers");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<GoldCustomer>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(5, operations.Count());
            Assert.False(operations.OfType<RenameTableOperation>().Any());
            Assert.True(operations.OfType<DropTableOperation>().Any());
            Assert.Equal(2, operations.OfType<AddColumnOperation>().Count());
        }

        #endregion
        
        #region Table Moves
        
        [MigrationsTheory]
        public void Can_detect_moved_tables()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>().ToTable("MigrationsCustomers", "foo");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var moveTableOperation 
                = operations.OfType<MoveTableOperation>().Single();

            Assert.Equal("dbo.MigrationsCustomers", moveTableOperation.Name);
            Assert.Equal("foo", moveTableOperation.NewSchema);
        }

        [MigrationsTheory]
        public void Moved_tables_should_include_create_table_operation()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>().ToTable("MigrationsCustomer", "foo");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var moveTableOperation
                = operations.OfType<MoveTableOperation>().Single();

            Assert.NotNull(moveTableOperation.CreateTableOperation);
            Assert.Equal("dbo.MigrationsCustomer", moveTableOperation.Name);
            Assert.Equal("foo.MigrationsCustomer", moveTableOperation.CreateTableOperation.Name);
        }

        [MigrationsTheory]
        public void Can_detect_table_move_when_ospace_type_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRename1>().ToTable("foo.TableRename1");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableRename2>().ToTable("bar.TableRename1");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var moveTableOperation = (MoveTableOperation)operations.Single();

            Assert.Equal("foo.TableRename1", moveTableOperation.Name);
            Assert.Equal("bar", moveTableOperation.NewSchema);
        }

        [MigrationsTheory]
        public void Can_detect_table_move_when_ospace_type_renamed_on_one_side_of_many_to_many()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<TableRenameManyManyLeft1>()
                .ToTable("Left")
                .HasMany(l => l.Rights)
                .WithMany(r => r.Lefts)
                .Map(
                    m =>
                    {
                        m.MapLeftKey("LeftId");
                        m.MapRightKey("RightId");
                        m.ToTable("Join", "foo");
                    });

            modelBuilder
                .Entity<OSpaceRenames_v1.TableRenameManyManyRight>()
                .ToTable("Right");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<TableRenameManyManyLeft2>()
                .ToTable("Left")
                .HasMany(l => l.Rights)
                .WithMany(r => r.Lefts)
                .Map(
                    m =>
                    {
                        m.MapLeftKey("LeftId");
                        m.MapRightKey("RightId");
                        m.ToTable("Join", "bar");
                    });

            modelBuilder
                .Entity<OSpaceRenames_v2.TableRenameManyManyRight>()
                .ToTable("Right");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var moveTableOperation = (MoveTableOperation)operations.Single();

            Assert.Equal("foo.Join", moveTableOperation.Name);
            Assert.Equal("bar", moveTableOperation.NewSchema);
        }
        
        #endregion

        #region Added Tables

        public class PrecisionScaleEntity
        {
            public string Id { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime DatetimeColumn { get; set; }
        }

        [MigrationsTheory]
        public void Should_not_set_precision_scale_when_default_for_type()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<PrecisionScaleEntity>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var createTableOperation
                = operations.OfType<CreateTableOperation>().Single();

            Assert.True(createTableOperation.Columns.All(c => c.Precision == null));
            Assert.True(createTableOperation.Columns.All(c => c.Scale == null));
        }

        [MigrationsTheory]
        public void Can_detect_added_tables()
        {
            var modelBuilder = new DbModelBuilder();
            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<OrderLine>()
                .HasTableAnnotation("AT1", "VT1")
                .ToTable("[foo.[]]].bar")
                .Property(e => e.OrderId)
                .HasColumnAnnotation("AP1", "VP1")
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella")));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var createTableOperation = operations.OfType<CreateTableOperation>().Single();

            Assert.Equal("[foo.[]]].bar", createTableOperation.Name);
            Assert.Equal("VT1", createTableOperation.Annotations["AT1"]);

            var column = createTableOperation.Columns.Single(c => c.Name == "OrderId");
            Assert.Null(column.Annotations["AP1"].OldValue);
            Assert.Equal("VP1", column.Annotations["AP1"].NewValue);

            var createIndexOperation = operations.OfType<CreateIndexOperation>().Single();
            Assert.Equal("Bella", createIndexOperation.Name);
            Assert.Equal("[foo.[]]].bar", createIndexOperation.Table);
            Assert.Equal(new List<string> { "OrderId" }, createIndexOperation.Columns);
        }

        [MigrationsTheory]
        public void Can_populate_table_model_for_added_tables()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer()
                    .Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(7, operations.Count());
            Assert.Equal(2, operations.OfType<AddForeignKeyOperation>().Count());

            var ordersCreateTableOperation 
                = operations.OfType<CreateTableOperation>()
                    .Single(t => t.Name == "ordering.Orders");

            Assert.Equal(4, ordersCreateTableOperation.Columns.Count());
            Assert.Equal(1, ordersCreateTableOperation.PrimaryKey.Columns.Count());
            Assert.Equal("OrderId", ordersCreateTableOperation.PrimaryKey.Columns.Single());

            var customerIdColumn
                = ordersCreateTableOperation.Columns
                    .Single(c => c.Name == "MigrationsCustomer_Id");

            Assert.Equal(PrimitiveTypeKind.Int32, customerIdColumn.Type);
            Assert.Null(customerIdColumn.IsNullable);
            Assert.Null(customerIdColumn.StoreType);
            Assert.Null(customerIdColumn.Precision);
            Assert.Null(customerIdColumn.Scale);
            Assert.Null(customerIdColumn.MaxLength);
            Assert.Null(customerIdColumn.IsFixedLength);
            Assert.False(customerIdColumn.IsIdentity);
            Assert.Null(customerIdColumn.IsUnicode);
            Assert.False(customerIdColumn.IsTimestamp);

            var orderIdColumn
                = ordersCreateTableOperation.Columns
                    .Single(c => c.Name == "OrderId");

            Assert.True(orderIdColumn.IsIdentity);
            Assert.Equal(false, orderIdColumn.IsNullable);

            var versionColumn
                = ordersCreateTableOperation.Columns
                    .Single(c => c.Name == "Version");

            Assert.True(versionColumn.IsTimestamp);

            var typeColumn
                = ordersCreateTableOperation.Columns
                    .Single(c => c.Name == "Type");

            Assert.Equal(
                DatabaseProvider != DatabaseProvider.SqlServerCe
                    ? (int?)null
                    : 4000,
                typeColumn.MaxLength);

            var orderLinesCreateTableOperation
               = operations.OfType<CreateTableOperation>()
                   .Single(t => t.Name == "dbo.OrderLines");

            var totalColumn
                = orderLinesCreateTableOperation.Columns
                    .Single(c => c.Name == "Total");

            Assert.Equal("money", totalColumn.StoreType);

            var skuColumn
                = orderLinesCreateTableOperation.Columns
                    .Single(c => c.Name == "Sku");

            Assert.Equal(128, skuColumn.MaxLength);

            var priceColumn
                = orderLinesCreateTableOperation.Columns
                    .Single(c => c.Name == "Price");

            Assert.Null(priceColumn.StoreType);
            Assert.Equal<byte?>(18, priceColumn.Precision);
            Assert.Equal<byte?>(2, priceColumn.Scale);
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
        public void Can_detect_custom_store_type()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<OrderLine>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var createTableOperation = operations.OfType<CreateTableOperation>().Single();

            var column = createTableOperation.Columns.Single(c => c.Name == "Total");

            Assert.Equal("money", column.StoreType);

            createTableOperation.Columns.Except(new[] { column }).Each(c => Assert.Null(c.StoreType));
        }

        #endregion

        #region Added table annotations

        [MigrationsTheory]
        public void Can_detect_altered_table_annotations()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<OrderLine>()
                .HasTableAnnotation("AT1", "VT1")
                .Property(e => e.OrderId)
                .HasColumnAnnotation("AP1", "VP1");
            
            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<OrderLine>()
                .HasTableAnnotation("AT1", "VT2")
                .Property(e => e.OrderId)
                .HasColumnAnnotation("AP1", "VP1");
            
            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var operation = operations.OfType<AlterTableOperation>().Single();

            Assert.Equal("VT1", operation.Annotations["AT1"].OldValue);
            Assert.Equal("VT2", operation.Annotations["AT1"].NewValue);

            var column = operation.Columns.Single(c => c.Name == "OrderId");
            Assert.Equal("VP1", column.Annotations["AP1"].OldValue);
            Assert.Equal("VP1", column.Annotations["AP1"].NewValue);
        }

        [MigrationsTheory]
        public void Can_detect_altered_table_and_column_annotations()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<OrderLine>()
                .HasTableAnnotation("AT1", "VT1")
                .Property(e => e.OrderId)
                .HasColumnAnnotation("AP1", "VP1");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<OrderLine>()
                .HasTableAnnotation("AT1", "VT2")
                .Property(e => e.OrderId)
                .HasColumnAnnotation("AP1", "VP2");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var alterColumnOperation = operations.OfType<AlterColumnOperation>().Single();

            Assert.Equal("VP1", alterColumnOperation.Column.Annotations["AP1"].OldValue);
            Assert.Equal("VP2", alterColumnOperation.Column.Annotations["AP1"].NewValue);

            var alterTableAnnotationsOperation = operations.OfType<AlterTableOperation>().Single();

            Assert.Equal("VT1", alterTableAnnotationsOperation.Annotations["AT1"].OldValue);
            Assert.Equal("VT2", alterTableAnnotationsOperation.Annotations["AT1"].NewValue);

            var column = alterTableAnnotationsOperation.Columns.Single(c => c.Name == "OrderId");
            Assert.Equal("VP2", column.Annotations["AP1"].OldValue);
            Assert.Equal("VP2", column.Annotations["AP1"].NewValue);
        }

        #endregion

        #region Removed Tables

        [MigrationsTheory]
        public void Can_detect_removed_tables()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<OrderLine>()
                .HasTableAnnotation("AT1", "VT1")
                .Property(e => e.OrderId)
                .HasColumnAnnotation("AP1", "VP1")
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella")));

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var dropTableOperation = operations.OfType<DropTableOperation>().Single();
            Assert.Equal("VT1", dropTableOperation.RemovedAnnotations["AT1"]);
            Assert.Equal("VP1", dropTableOperation.RemovedColumnAnnotations["OrderId"]["AP1"]);

            var inverse = (CreateTableOperation)dropTableOperation.Inverse;
            Assert.NotNull(inverse);
            Assert.Equal("dbo.OrderLines", inverse.Name);
            Assert.Equal(8, inverse.Columns.Count());

            var dropIndexOperation = operations.OfType<DropIndexOperation>().Single();
            Assert.Equal("Bella", dropIndexOperation.Name);
            Assert.Equal("dbo.OrderLines", dropIndexOperation.Table);
            Assert.Equal(new List<string> { "OrderId" }, dropIndexOperation.Columns);
        }

        #endregion

        #region Modification Functions

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
                        new Lazy<ModificationCommandTreeGenerator>(() => commandTreeGenerator),
                        new SqlServerMigrationSqlGenerator())
                    .OfType<CreateProcedureOperation>()
                    .ToList();

            Assert.Equal(23, createProcedureOperations.Count);
            Assert.True(createProcedureOperations.All(c => c.Name.Any()));
            Assert.True(createProcedureOperations.All(c => c.BodySql.Any()));
        }

        [MigrationsTheory]
        public void Can_detect_changed_modification_functions()
        {
            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel());

            var targetModel = new TestContext_v2().GetModel();

            var alterProcedureOperations
                = new EdmModelDiffer()
                    .Diff(
                        new TestContext().GetModel(),
                        targetModel,
                        new Lazy<ModificationCommandTreeGenerator>(() => commandTreeGenerator),
                        new SqlServerMigrationSqlGenerator())
                    .OfType<AlterProcedureOperation>()
                    .ToList();

            Assert.Equal(3, alterProcedureOperations.Count);
            Assert.True(alterProcedureOperations.All(c => c.BodySql.Any()));
            Assert.Equal(1, alterProcedureOperations.Count(c => c.Parameters.Any(p => p.Name == "key_for_update2")));
            Assert.Equal(1, alterProcedureOperations.Count(c => c.Parameters.Any(p => p.Name == "affected_rows")));
        }

        [MigrationsTheory]
        public void Can_detect_changed_modification_functions_when_column_change_affects_parameter()
        {
            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel());

            var targetModel = new TestContext_v2c().GetModel();

            var alterProcedureOperations
                = new EdmModelDiffer()
                    .Diff(
                        new TestContext().GetModel(),
                        targetModel,
                        new Lazy<ModificationCommandTreeGenerator>(() => commandTreeGenerator),
                        new SqlServerMigrationSqlGenerator())
                    .OfType<AlterProcedureOperation>()
                    .ToList();

            Assert.Equal(2, alterProcedureOperations.Count);
            Assert.True(alterProcedureOperations.All(c => c.BodySql.Any()));
            Assert.True(alterProcedureOperations
                .SelectMany(a => a.Parameters).Any(p => p.Name == "Name" && p.MaxLength == 42));
        }

        [MigrationsTheory]
        public void Can_detect_changed_modification_functions_when_many_to_many()
        {
            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel());

            var targetModel = new TestContext_v2b().GetModel();

            var alterProcedureOperations
                = new EdmModelDiffer()
                    .Diff(
                        new TestContext().GetModel(),
                        targetModel,
                        new Lazy<ModificationCommandTreeGenerator>(() => commandTreeGenerator),
                        new SqlServerMigrationSqlGenerator())
                    .OfType<AlterProcedureOperation>()
                    .ToList();

            Assert.Equal(2, alterProcedureOperations.Count);
            Assert.Equal(1, alterProcedureOperations.Count(ap => ap.Parameters.Any(p => p.Name == "order_thing_id")));
            Assert.Equal(1, alterProcedureOperations.Count(ap => ap.Parameters.Any(p => p.Name == "order_id")));
        }

        [MigrationsTheory]
        public void Can_detect_moved_modification_functions()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>().MapToStoredProcedures();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder
                .Entity<MigrationsCustomer>()
                .MapToStoredProcedures(
                    m =>
                    {
                        m.Insert(c => c.HasName("MigrationsCustomer_Insert", "foo"));
                        m.Update(c => c.HasName("delete_it", "foo"));
                    });

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(3, operations.Count());

            var moveProcedureOperations
                = operations.OfType<MoveProcedureOperation>();

            Assert.True(moveProcedureOperations.All(mpo => mpo.NewSchema == "foo"));
        }

        [MigrationsTheory]
        public void Can_detect_moved_modification_functions_when_many_to_many()
        {
            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel());

            var targetModel = new TestContext_v2b().GetModel();

            var moveProcedureOperations
                = new EdmModelDiffer()
                    .Diff(
                        new TestContext().GetModel(),
                        targetModel,
                        new Lazy<ModificationCommandTreeGenerator>(() => commandTreeGenerator),
                        new SqlServerMigrationSqlGenerator())
                    .OfType<MoveProcedureOperation>()
                    .ToList();

            Assert.Equal(2, moveProcedureOperations.Count);
            Assert.Equal(1, moveProcedureOperations.Count(c => c.NewSchema == "foo"));
            Assert.Equal(1, moveProcedureOperations.Count(c => c.NewSchema == "bar"));
        }

        [MigrationsTheory]
        public void Can_detect_renamed_modification_functions()
        {
            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel());

            var targetModel = new TestContext_v2().GetModel();

            var renameProcedureOperations
                = new EdmModelDiffer()
                    .Diff(
                        new TestContext().GetModel(),
                        targetModel,
                        new Lazy<ModificationCommandTreeGenerator>(() => commandTreeGenerator),
                        new SqlServerMigrationSqlGenerator())
                    .OfType<RenameProcedureOperation>()
                    .ToList();

            Assert.Equal(1, renameProcedureOperations.Count);
            Assert.Equal(1, renameProcedureOperations.Count(c => c.NewName == "sproc_A"));
        }

        [MigrationsTheory]
        public void Can_detect_renamed_modification_functions_when_many_to_many()
        {
            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel());

            var targetModel = new TestContext_v2b().GetModel();

            var renameProcedureOperations
                = new EdmModelDiffer()
                    .Diff(
                        new TestContext().GetModel(),
                        targetModel,
                        new Lazy<ModificationCommandTreeGenerator>(() => commandTreeGenerator),
                        new SqlServerMigrationSqlGenerator())
                    .OfType<RenameProcedureOperation>()
                    .ToList();

            Assert.Equal(1, renameProcedureOperations.Count);
            Assert.Equal(1, renameProcedureOperations.Count(c => c.NewName == "m2m_insert"));
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

            Assert.Equal(23, dropProcedureOperations.Count);
            Assert.True(dropProcedureOperations.All(c => c.Name.Any()));
        }

        #endregion

        #region Column Renames

        public class OneManySelfRef
        {
            public int Id { get; set; }
            public OneManySelfRef Parent { get; set; }
            public ICollection<OneManySelfRef> Children { get; set; }
        }

        [MigrationsTheory]
        public void Can_detect_renamed_one_to_many_ia_fk()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OneManySelfRef>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder
                .Entity<OneManySelfRef>()
                .HasMany(o => o.Children)
                .WithOptional(o => o.Parent)
                .Map(m => m.MapKey("Renamed"));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var columnRename = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("dbo.OneManySelfRefs", columnRename.Table);
            Assert.Equal("Parent_Id", columnRename.Name);
            Assert.Equal("Renamed", columnRename.NewName);
        }

        [MigrationsTheory]
        public void Can_detect_simple_column_rename()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder
                .Entity<MigrationsCustomer>()
                .Property(p => p.Name)
                .HasColumnName("col_Name")
                .HasMaxLength(23);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var renameColumnOperation = (RenameColumnOperation)operations.First();

            Assert.Equal("dbo.MigrationsCustomers", renameColumnOperation.Table);
            Assert.Equal("Name", renameColumnOperation.Name);
            Assert.Equal("col_Name", renameColumnOperation.NewName);
            
            var alterColumnOperation = (AlterColumnOperation)operations.Last();

            Assert.Equal("dbo.MigrationsCustomers", alterColumnOperation.Table);
            Assert.Equal("col_Name", alterColumnOperation.Column.Name);
            Assert.Equal(23, alterColumnOperation.Column.MaxLength);
        }

        [MigrationsTheory]
        public void Column_rename_does_not_trigger_index_drop_and_create()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>()
                .Property(p => p.CustomerNumber)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella")));

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder
                .Entity<MigrationsCustomer>()
                .Property(p => p.CustomerNumber)
                .HasColumnName("CusNum")
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella")));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.IsType<RenameColumnOperation>(operations.Single());
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

            modelBuilder
                .Entity<MigrationsCustomer>()
                .Property(p => p.Name)
                .HasColumnName("col_Name");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

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

            modelBuilder
                .Entity<MigrationsCustomer>()
                .Property(p => p.HomeAddress.City)
                .HasColumnName("HomeCity");

            modelBuilder
                .Entity<MigrationsCustomer>()
                .Property(p => p.WorkAddress.City)
                .HasColumnName("WorkCity");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(3, operations.Count());

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
        public void Can_detect_column_rename_when_ospace_type_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ColumnRename1>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<ColumnRename2>()
                .Property(c => c.Member)
                .HasColumnName("NewName");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var renameColumnOperation = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("dbo.ColumnRename", renameColumnOperation.Table);
            Assert.Equal("Member", renameColumnOperation.Name);
            Assert.Equal("NewName", renameColumnOperation.NewName);
        }

        [MigrationsTheory]
        public void Can_detect_complex_column_rename_when_ospace_type_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ColumnRename1>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<ColumnRename2>()
                .Property(c => c.ComplexMember.Member)
                .HasColumnName("NewName");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var renameColumnOperation = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("dbo.ColumnRename", renameColumnOperation.Table);
            Assert.Equal("ComplexMember_Member", renameColumnOperation.Name);
            Assert.Equal("NewName", renameColumnOperation.NewName);
        }

        [MigrationsTheory]
        public void Can_detect_ia_fk_column_rename_when_ospace_type_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ColumnRename1>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<ColumnRename2>()
                .HasRequired(c => c.Parent)
                .WithMany()
                .Map(m => m.MapKey("NewName"));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var renameColumnOperation = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("dbo.ColumnRename", renameColumnOperation.Table);
            Assert.Equal("Parent_Id", renameColumnOperation.Name);
            Assert.Equal("NewName", renameColumnOperation.NewName);
        }
        
        [MigrationsTheory]
        public void Can_detect_discriminator_column_rename_when_ospace_type_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<ColumnRename1>()
                .Map(m => m.Requires("disc0").HasValue(23));

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<ColumnRename2>()
                .Map(m => m.Requires("disc1").HasValue(23));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var renameColumnOperation = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("dbo.ColumnRename", renameColumnOperation.Table);
            Assert.Equal("disc0", renameColumnOperation.Name);
            Assert.Equal("disc1", renameColumnOperation.NewName);
        }

        public class RenameColumnEntity
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }

        [MigrationsTheory]
        public void Should_not_detect_column_name_case_change_as_alter_column()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<RenameColumnEntity>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<RenameColumnEntity>()
                .Property(e => e.Title)
                .HasColumnName("tITLE");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer()
                    .Diff(model1.GetModel(), model2.GetModel())
                    .ToList();

            Assert.Equal(0, operations.Count());
        }

        public class TypeBase
        {
            public int Id { get; set; }
        }

        public class TypeOne : TypeBase
        {
            public RelatedType Related { get; set; }
        }

        public class TypeTwo : TypeBase
        {
            public RelatedType Related { get; set; }
        }

        public class RelatedType
        {
            public int Id { get; set; }
        }

        [MigrationsTheory]
        public void Should_detect_renamed_columns_when_swapped()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TypeWithRenames>().Ignore(t => t.Bazz);

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TypeWithRenames>().Property(t => t.Barz).HasColumnName("Bazz");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var renameColumnOperation = (RenameColumnOperation)operations.First();

            Assert.Equal("Barz", renameColumnOperation.Name);
            Assert.Equal("Bazz", renameColumnOperation.NewName);


            var addColumnOperation = (AddColumnOperation)operations.Last();

            Assert.Equal("Bazz1", addColumnOperation.Column.Name);
        }

        [MigrationsTheory]
        public void Should_detect_renamed_fk_columns_when_swapped()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TypeBase>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TypeBase>();

            modelBuilder
                .Entity<TypeOne>()
                .HasOptional(t => t.Related)
                .WithMany()
                .Map(m => m.MapKey("Related_Id1"));

            modelBuilder
                .Entity<TypeTwo>()
                .HasOptional(t => t.Related)
                .WithMany()
                .Map(m => m.MapKey("Related_Id"));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(3, operations.OfType<RenameColumnOperation>().Count());
        }

        public class TypeWithRenames
        {
            public int Id { get; set; }
            public int Fooz { get; set; }
            public int Barz { get; set; }
            public int Bazz { get; set; }
        }

        [MigrationsTheory]
        public void Should_drop_correct_constraint_when_ia_fk_name_component_changed() // #2338
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Renamed_ia_pk_v1.Dependent>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Renamed_ia_pk_v2.Dependent>();
            
            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(8, operations.Count());
            Assert.Equal("Principal_Id", operations.OfType<DropForeignKeyOperation>().First().DependentColumns.Single());
        }
        
        [MigrationsTheory]
        public void Should_introduce_temp_column_renames_when_transitive_dependencies()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TypeWithRenames>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TypeWithRenames>().Property(t => t.Fooz).HasColumnName("Bazz");
            modelBuilder.Entity<TypeWithRenames>().Property(t => t.Barz).HasColumnName("Fooz");
            modelBuilder.Entity<TypeWithRenames>().Property(t => t.Bazz).HasColumnName("Barz");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer()
                    .Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(5, operations.Count());

            var renameOperations
                = operations.OfType<RenameColumnOperation>().ToList();

            Assert.Equal(5, renameOperations.Count());

            var renameColumnOperation = renameOperations.ElementAt(0);

            Assert.Equal("Fooz", renameColumnOperation.Name);
            Assert.Equal("__mig_tmp__0", renameColumnOperation.NewName);

            renameColumnOperation = renameOperations.ElementAt(1);

            Assert.Equal("Barz", renameColumnOperation.Name);
            Assert.Equal("__mig_tmp__1", renameColumnOperation.NewName);

            renameColumnOperation = renameOperations.ElementAt(2);

            Assert.Equal("Bazz", renameColumnOperation.Name);
            Assert.Equal("Barz", renameColumnOperation.NewName);

            renameColumnOperation = renameOperations.ElementAt(3);

            Assert.Equal("__mig_tmp__0", renameColumnOperation.Name);
            Assert.Equal("Bazz", renameColumnOperation.NewName);

            renameColumnOperation = renameOperations.ElementAt(4);

            Assert.Equal("__mig_tmp__1", renameColumnOperation.Name);
            Assert.Equal("Fooz", renameColumnOperation.NewName);
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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

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

        public class SelfRef
        {
            public int Id { get; set; }
            public int Fk { get; set; }
            public ICollection<SelfRef> Children { get; set; }
        }

        [MigrationsTheory]
        public void Can_detect_fk_column_rename_when_self_ref()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<SelfRef>()
                .HasMany(s => s.Children)
                .WithOptional()
                .HasForeignKey(s => s.Fk);

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder
                .Entity<SelfRef>()
                .Property(s => s.Fk)
                .HasColumnName("changed");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var renameColumnOperation = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("Fk", renameColumnOperation.Name);
            Assert.Equal("changed", renameColumnOperation.NewName);
        }

        [MigrationsTheory]
        public void Should_only_detect_single_column_rename_when_fk_association()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<OrderLine>().Property(ol => ol.OrderId).HasColumnName("Order_Id");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var columnRename = operations.OfType<RenameColumnOperation>().Single();

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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var columnRename = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("dbo.Comments", columnRename.Table);
            Assert.Equal("Blog_MigrationsBlogId", columnRename.Name);
            Assert.Equal("MigrationsBlogId", columnRename.NewName);
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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(4, operations.Count());
        }

        #endregion

        #region Added Columns

        [MigrationsTheory]
        public void Can_detect_added_columns()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<OrderLine>()
                .Property(e => e.OrderId)
                .HasColumnAnnotation("A1", "V1")
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella")));
            
            var model2 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<OrderLine>().Ignore(ol => ol.OrderId);
            var model1 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());
            var addColumnOperation = operations.OfType<AddColumnOperation>().Single();

            Assert.Equal("dbo.OrderLines", addColumnOperation.Table);
            Assert.Equal("OrderId", addColumnOperation.Column.Name);
            Assert.Equal(PrimitiveTypeKind.Int32, addColumnOperation.Column.Type);
            Assert.Equal(false, addColumnOperation.Column.IsNullable);
            Assert.Null(addColumnOperation.Column.Annotations["A1"].OldValue);
            Assert.Equal("V1", addColumnOperation.Column.Annotations["A1"].NewValue);

            var createIndexOperation = operations.OfType<CreateIndexOperation>().Single();
            Assert.Equal("Bella", createIndexOperation.Name);
            Assert.Equal("dbo.OrderLines", createIndexOperation.Table);
            Assert.Equal(new List<string> { "OrderId" }, createIndexOperation.Columns);
        }

        [MigrationsTheory]
        public void Can_detect_column_add_when_ospace_type_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ColumnRename1>().Ignore(t => t.Member);

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ColumnRename2>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var addColumnOperation = (AddColumnOperation)operations.Single();

            Assert.Equal("Member", addColumnOperation.Column.Name);
        }

        [MigrationsTheory]
        public void Can_detect_added_timestamp_columns()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();

            var model2 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>().Ignore(o => o.Version);

            var model1 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());

            var column = operations.OfType<AddColumnOperation>().Single().Column;

            Assert.True(column.IsTimestamp);
        }

        public class ArubaTask
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ArubaRun
        {
            public int Id { get; set; }
            public ICollection<ArubaTask> Tasks { get; set; }
        }

        [MigrationsTheory]
        public void Should_generate_add_column_operation_when_shared_pk_fk_moved_to_ia()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ArubaRun>();
            modelBuilder.Entity<ArubaTask>().HasKey(k => new { k.Id, k.Name });

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<ArubaRun>().HasMany(r => r.Tasks).WithRequired().Map(m => { });

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(5, operations.Count());
            Assert.True(operations.Any(o => o is AddColumnOperation));
        }

        #endregion

        #region Removed Columns

        [MigrationsTheory]
        public void Can_detect_column_drop_when_ospace_type_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ColumnRename1>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ColumnRename2>().Ignore(t => t.Member);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var dropColumnOperation = (DropColumnOperation)operations.Single();

            Assert.Equal("Member", dropColumnOperation.Name);
        }

        [MigrationsTheory]
        public void Can_detect_dropped_columns()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Order>()
                .Property(e => e.Version)
                .HasColumnAnnotation("A1", "V1")
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella")));

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Order>().Ignore(o => o.Version);
            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());
            Assert.Equal(2, operations.Count);
            
            var dropColumnOperation = operations.OfType<DropColumnOperation>().Single();

            Assert.Equal("ordering.Orders", dropColumnOperation.Table);
            Assert.Equal("Version", dropColumnOperation.Name);

            var inverse = (AddColumnOperation)dropColumnOperation.Inverse;

            Assert.NotNull(inverse);
            Assert.Equal("ordering.Orders", inverse.Table);
            Assert.Equal("Version", inverse.Column.Name);
            Assert.Equal("V1", dropColumnOperation.RemovedAnnotations["A1"]);

            var dropIndexOperation = operations.OfType<DropIndexOperation>().Single();
            Assert.Equal("Bella", dropIndexOperation.Name);
            Assert.Equal("ordering.Orders", dropIndexOperation.Table);
            Assert.Equal(new List<string> { "Version" }, dropIndexOperation.Columns);
        }

        #endregion

        #region Altered Columns

        [MigrationsTheory]
        public void Can_detect_changed_columns()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder
                .Entity<MigrationsCustomer>()
                .Property(c => c.FullName)
                .HasMaxLength(25)
                .IsUnicode(false);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var alterColumnOperation = (AlterColumnOperation)operations.Single();

            Assert.Equal(25, alterColumnOperation.Column.MaxLength);

            if (DatabaseProvider != DatabaseProvider.SqlServerCe)
            {
                Assert.Equal(false, alterColumnOperation.Column.IsUnicode);
            }

            var inverseAlterColumnOperation
                = (AlterColumnOperation)alterColumnOperation.Inverse;

            Assert.Equal("FullName", inverseAlterColumnOperation.Column.Name);

            Assert.Equal(
                DatabaseProvider != DatabaseProvider.SqlServerCe
                    ? (int?)null
                    : 4000,
                inverseAlterColumnOperation.Column.MaxLength);

            Assert.Null(inverseAlterColumnOperation.Column.IsUnicode);
        }

        [MigrationsTheory]
        public void Can_detect_changed_column_annotations()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.FullName)
                .HasColumnAnnotation("A1", "V1");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.FullName)
                .HasColumnAnnotation("A1", "V2");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var alterColumnOperation = (AlterColumnOperation)operations.Single();

            Assert.Equal("V1", alterColumnOperation.Column.Annotations["A1"].OldValue);
            Assert.Equal("V2", alterColumnOperation.Column.Annotations["A1"].NewValue);
        }

        [MigrationsTheory]
        public void Can_detect_added_column_annotations()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.FullName);

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.FullName)
                .HasColumnAnnotation("A1", "V2");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var alterColumnOperation = (AlterColumnOperation)operations.Single();

            Assert.Null(alterColumnOperation.Column.Annotations["A1"].OldValue);
            Assert.Equal("V2", alterColumnOperation.Column.Annotations["A1"].NewValue);
        }

        [MigrationsTheory]
        public void Can_detect_removed_column_annotations()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.FullName)
                .HasColumnAnnotation("A1", "V1");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.FullName);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var alterColumnOperation = (AlterColumnOperation)operations.Single();

            Assert.Equal("V1", alterColumnOperation.Column.Annotations["A1"].OldValue);
            Assert.Null(alterColumnOperation.Column.Annotations["A1"].NewValue);
        }

        [MigrationsTheory]
        public void Can_detect_changed_index()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.CustomerNumber)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella")));

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.CustomerNumber)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella") { IsUnique = true }));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel()).ToArray();

            Assert.Equal(2, operations.Length);

            var dropIndexOperation = (DropIndexOperation)operations[0];
            Assert.Equal("Bella", dropIndexOperation.Name);
            Assert.Equal("dbo.MigrationsCustomers", dropIndexOperation.Table);
            Assert.Equal(new List<string> { "CustomerNumber" }, dropIndexOperation.Columns);

            var createIndexOperation = (CreateIndexOperation)operations[1];
            Assert.Equal("Bella", createIndexOperation.Name);
            Assert.Equal("dbo.MigrationsCustomers", createIndexOperation.Table);
            Assert.True(createIndexOperation.IsUnique);
            Assert.Equal(new List<string> { "CustomerNumber" }, createIndexOperation.Columns);
        }

        [MigrationsTheory]
        public void Can_detect_added_index()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.CustomerNumber);

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.CustomerNumber)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella")));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var createIndexOperation = (CreateIndexOperation)operations.Single();
            Assert.Equal("Bella", createIndexOperation.Name);
            Assert.Equal("dbo.MigrationsCustomers", createIndexOperation.Table);
            Assert.Equal(new List<string> { "CustomerNumber" }, createIndexOperation.Columns);
        }

        [MigrationsTheory]
        public void Can_detect_removed_index()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.CustomerNumber)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("Bella")));

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MigrationsCustomer>()
                .Property(c => c.CustomerNumber);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var dropIndexOperation = (DropIndexOperation)operations.Single();
            Assert.Equal("Bella", dropIndexOperation.Name);
            Assert.Equal("dbo.MigrationsCustomers", dropIndexOperation.Table);
            Assert.Equal(new List<string> { "CustomerNumber" }, dropIndexOperation.Columns);
        }
        
        [MigrationsTheory]
        public void Can_detect_changed_columns_when_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder
                .Entity<MigrationsCustomer>()
                .Property(c => c.FullName)
                .HasMaxLength(25)
                .HasColumnName("Foo");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());

            var alterColumnOperation
                = (AlterColumnOperation)operations.Last();

            Assert.Equal("Foo", alterColumnOperation.Column.Name);

            var inverseAlterColumnOperation
                = (AlterColumnOperation)alterColumnOperation.Inverse;

            Assert.Equal("Foo", inverseAlterColumnOperation.Column.Name);
        }

        #endregion

        #region Added FKs

        [MigrationsTheory]
        public void Can_detect_added_foreign_keys()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<Order>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

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

        [Table("Contract")]
        public class Contract
        {
            public long Id { get; set; }
            public DateTime X { get; set; }
        }

        [Table("InitialContract")]
        public class ContractRevision : Contract
        {
        }

        [Table("InitialContract")]
        public class InitialContract : Contract
        {
        }

        [MigrationsTheory]
        public void Should_not_detect_duplicate_added_foreign_keys()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<Contract>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.OfType<AddForeignKeyOperation>().Count());
        }

        #endregion

        #region Removed FKs

        [MigrationsTheory]
        public void Can_detect_removed_foreign_keys()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<Order>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model2.GetModel(), model1.GetModel());

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
        public void Should_not_detect_duplicate_dropped_foreign_keys()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<Contract>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model2.GetModel(), model1.GetModel());

            Assert.Equal(1, operations.OfType<DropForeignKeyOperation>().Count());
        }

        #endregion

        #region Changed FKs

        [MigrationsTheory]
        public void Should_detect_fk_drop_create_when_ia_to_fk_association_and_cascade_changes()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Comment>().Ignore(c => c.MigrationsBlogId); // IA

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Comment>().HasOptional(c => c.Blog).WithMany().WillCascadeOnDelete();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(4, operations.Count());

            var dropForeignKeyOperation = operations.OfType<DropForeignKeyOperation>().Single();

            Assert.Equal("dbo.Comments", dropForeignKeyOperation.DependentTable);
            Assert.Equal("Blog_MigrationsBlogId", dropForeignKeyOperation.DependentColumns.Single());

            var dropForeignKeyOperationInverse = (AddForeignKeyOperation)dropForeignKeyOperation.Inverse;

            Assert.Equal("dbo.Comments", dropForeignKeyOperationInverse.DependentTable);
            Assert.Equal("Blog_MigrationsBlogId", dropForeignKeyOperationInverse.DependentColumns.Single());

            var dropIndexOperation = operations.OfType<RenameIndexOperation>().Single();

            Assert.Equal("dbo.Comments", dropIndexOperation.Table);
            Assert.Equal("IX_Blog_MigrationsBlogId", dropIndexOperation.Name);
            Assert.Equal("IX_MigrationsBlogId", dropIndexOperation.NewName);
        }

        [MigrationsTheory]
        public void Can_detect_changed_foreign_keys_when_cascade()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithOptional().WillCascadeOnDelete(false);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(2, operations.Count());
            Assert.Equal(1, operations.OfType<DropForeignKeyOperation>().Count());
            
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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }

        #endregion

        #region Changed PKs

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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(4, operations.Count());

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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(4, operations.Count());

            var addPrimaryKeyOperation = operations.OfType<AddPrimaryKeyOperation>().Single();

            Assert.Equal("dbo.tbl_OrderLines", addPrimaryKeyOperation.Table);
            Assert.Equal("Id", addPrimaryKeyOperation.Columns.First());
            Assert.Equal("OrderId", addPrimaryKeyOperation.Columns.Last());

            var dropPrimaryKeyOperation = operations.OfType<DropPrimaryKeyOperation>().Single();

            Assert.Equal("dbo.tbl_OrderLines", dropPrimaryKeyOperation.Table);
            Assert.Equal("Id", dropPrimaryKeyOperation.Columns.Single());
        }

        public class PkEntity1
        {
            public string Id { get; set; }
            public ICollection<PkEntity2> PkEntity2s { get; set; }
        }

        public class PkEntity2
        {
            public string Id { get; set; }
            public string PkEntity1Id { get; set; }
        }


        [MigrationsTheory]
        public void Can_detect_changed_primary_key_when_pk_column_facets_changed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PkEntity1>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PkEntity1>().Property(e => e.Id).HasMaxLength(42);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(8, operations.Count());

            Assert.Equal(
                new[]
                {
                    typeof(DropForeignKeyOperation),
                    typeof(DropIndexOperation),
                    typeof(DropPrimaryKeyOperation),
                    typeof(AlterColumnOperation),
                    typeof(AlterColumnOperation),
                    typeof(AddPrimaryKeyOperation),
                    typeof(CreateIndexOperation),
                    typeof(AddForeignKeyOperation)
                },
                operations.Select(o => o.GetType()));
        }

        #endregion

        #region Indexes

        public class IndexRename
        {
            public int Id { get; set; }

            [Index]
            public string Indexed { get; set; }
        }

        [MigrationsTheory]
        public void Can_detect_renamed_indexes()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<IndexRename>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder
                .Entity<IndexRename>()
                .Property(p => p.Indexed)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("NewName")));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            var renameIndexOperation = (RenameIndexOperation)operations.Single();

            Assert.Equal("dbo.IndexRenames", renameIndexOperation.Table);
            Assert.Equal("IX_Indexed", renameIndexOperation.Name);
            Assert.Equal("NewName", renameIndexOperation.NewName);
        }

        public class IndexColumnRenameParent
        {
            public int Id { get; set; }
            public ICollection<IndexColumnRenameChild> Children { get; set; }
        }

        public class IndexColumnRenameChild
        {
            public int Id { get; set; }
            public int IndexColumnRenameParentId { get; set; }
        }

        [MigrationsTheory]
        public void Indexes_have_correct_columns_when_column_being_renamed_and_altered()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<IndexColumnRenameParent>();
            modelBuilder.Entity<IndexColumnRenameChild>().Ignore(c => c.IndexColumnRenameParentId);

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<IndexColumnRenameParent>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(6, operations.Count);

            var dropIndexOperation = operations.OfType<DropIndexOperation>().Single();

            Assert.Equal("IndexColumnRenameParent_Id", dropIndexOperation.Columns.Single());
            Assert.Equal("IndexColumnRenameParent_Id", ((CreateIndexOperation)dropIndexOperation.Inverse).Columns.Single());

        }

        #endregion

        #region Misc.

        [MigrationsTheory]
        public void Should_not_detect_diffs_when_upgrading_to_6_1()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();
            modelBuilder.Conventions.Remove<ForeignKeyIndexConvention>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel(), sourceModelVersion: "6.0.0");

            Assert.Equal(0, operations.Count());
        }

        public class Developer
        {
            public int CostCenter { get; set; }
            public string Alias { get; set; }
            public ICollection<Checkin> Checkins { get; set; }
        }

        public class Checkin
        {
            public int Id { get; set; }
        }

        [MigrationsTheory]
        public void Should_not_detect_diffs_when_upgrading_model_with_entity_having_composite_key_to_6_1()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Developer>().HasKey(d => new { d.CostCenter, d.Alias });
            modelBuilder.Entity<Checkin>();
            modelBuilder.Conventions.Remove<ForeignKeyIndexConvention>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Developer>().HasKey(d => new { d.CostCenter, d.Alias });
            modelBuilder.Entity<Checkin>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel(), sourceModelVersion: "6.0.0");

            Assert.Equal(0, operations.Count());
        }

        [MigrationsTheory]
        public void Should_detect_diffs_when_upgrading_to_6_1_and_new_user_index()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();
            modelBuilder.Conventions.Remove<ForeignKeyIndexConvention>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<MigrationsCustomer>()
                .Property(c => c.Name)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute()));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel(), sourceModelVersion: "5.0.0");

            var createIndexOperation
                = operations.OfType<CreateIndexOperation>().Single();

            Assert.Equal("IX_Name", createIndexOperation.Name);
        }

        [MigrationsTheory]
        public void Should_detect_diffs_when_upgrading_to_6_1_and_fk_index_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();
            modelBuilder.Conventions.Remove<ForeignKeyIndexConvention>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order>();
            modelBuilder
                .Entity<OrderLine>()
                .Property(c => c.OrderId)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("NewName")));

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel(), sourceModelVersion: "4.3.0");

            var renameIndexOperation
                = operations.OfType<RenameIndexOperation>().Single();

            Assert.Equal("IX_OrderId", renameIndexOperation.Name);
            Assert.Equal("NewName", renameIndexOperation.NewName);
        }

        [MigrationsTheory]
        public void Should_not_detect_diffs_when_models_are_identical()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MigrationsCustomer>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder.Entity<MigrationsCustomer>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }

        public class CodePlex951Context : DbContext
        {
            public DbSet<Parent> Entities { get; set; }

            static CodePlex951Context()
            {
                Database.SetInitializer<CodePlex951Context>(null);
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Child1>().Map(
                    m =>
                    {
                        m.Requires("Disc1").HasValue(true);
                        m.Requires("Disc2").HasValue(false);
                    });

                modelBuilder.Entity<Child2>().Map(
                    m =>
                    {
                        m.Requires("Disc2").HasValue(true);
                        m.Requires("Disc1").HasValue(false);
                    });
            }
        }

        public abstract class Parent
        {
            public int Id { get; set; }
        }

        public class Child1 : Parent
        {
        }

        public class Child2 : Parent
        {
        }

        [MigrationsTheory]
        public void CodePlex951_should_not_detect_discriminator_column_diffs()
        {
            XDocument model;
            using (var context = new CodePlex951Context())
            {
                model = context.GetModel();
            }

            var operations = new EdmModelDiffer().Diff(model, model);

            Assert.Equal(0, operations.Count());
        }

        public class ClassA
        {
            public string Id { get; set; }
        }

        public class ClassB
        {
            public int Id { get; set; }
            public string ClassAId { get; set; }
            public ClassA ClassA { get; set; }
        }

        [MigrationsTheory]
        public void Can_detect_orphaned_columns()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<ClassB>()
                .HasOptional(b => b.ClassA)
                .WithMany()
                .Map(_ => { }); // IA

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ClassB>();

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(
                DatabaseProvider == DatabaseProvider.SqlClient ? 5 : 3, // SQL column's max length changes too
                operations.Count());

            var dropColumnOperation = operations.OfType<DropColumnOperation>().Single();

            Assert.Equal("dbo.ClassBs", dropColumnOperation.Table);
            Assert.Equal("ClassAId", dropColumnOperation.Name);

            var renameColumnOperation = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("dbo.ClassBs", renameColumnOperation.Table);
            Assert.Equal("ClassA_Id", renameColumnOperation.Name);
            Assert.Equal("ClassAId", renameColumnOperation.NewName);
        }

        [MigrationsTheory]
        public void Can_detect_orphaned_columns_when_ospace_rename()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<OrphanedColumn1>()
                .HasOptional(b => b.OrphanedColumnParent)
                .WithMany()
                .Map(_ => { }); // IA

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<OrphanedColumn2>()
                .ToTable("OrphanedColumn1");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(
                DatabaseProvider == DatabaseProvider.SqlClient ? 5 : 3, // SQL column's max length changes too
                operations.Count());

            var dropColumnOperation = operations.OfType<DropColumnOperation>().Single();

            Assert.Equal("dbo.OrphanedColumn1", dropColumnOperation.Table);
            Assert.Equal("OrphanedColumnParentId", dropColumnOperation.Name);

            var renameColumnOperation = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("dbo.OrphanedColumn1", renameColumnOperation.Table);
            Assert.Equal("OrphanedColumnParent_Id", renameColumnOperation.Name);
            Assert.Equal("OrphanedColumnParentId", renameColumnOperation.NewName);
        }

        [MigrationsTheory]
        public void Can_detect_orphaned_columns_when_table_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<ClassB>()
                .HasOptional(b => b.ClassA)
                .WithMany()
                .Map(_ => { }); // IA

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ClassB>().ToTable("Renamed");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(
                DatabaseProvider == DatabaseProvider.SqlClient ? 6 : 4, // SQL column's max length changes too
                operations.Count());

            var dropColumnOperation = operations.OfType<DropColumnOperation>().Single();

            Assert.Equal("dbo.Renamed", dropColumnOperation.Table);
            Assert.Equal("ClassAId", dropColumnOperation.Name);

            var renameColumnOperation = operations.OfType<RenameColumnOperation>().Single();

            Assert.Equal("dbo.Renamed", renameColumnOperation.Table);
            Assert.Equal("ClassA_Id", renameColumnOperation.Name);
            Assert.Equal("ClassAId", renameColumnOperation.NewName);
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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(1, operations.Count());
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

            var operations 
                = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count());
        }

        public class SwagBag
        {
            public int Id { get; set; }
            public virtual Hat Hat { get; set; }
            public virtual Bracelet Bracelet { get; set; }
        }
        public class Bracelet
        {
            public int Id { get; set; }
            public virtual SwagBag SwagBag { get; set; }
        }

        public class Hat
        {
            public int Id { get; set; }
            public virtual SwagBag SwagBag { get; set; }
        }

        public class SwagBag2
        {
            public int Id { get; set; }
            public virtual Hat2 Hat { get; set; }
            public virtual Bracelet2 Bracelet { get; set; }
        }
        
        public class Bracelet2
        {
            public int Id { get; set; }
            public virtual SwagBag2 SwagBag { get; set; }
        }

        public class Hat2
        {
            public int Id { get; set; }
            public virtual SwagBag2 SwagBag { get; set; }
        }

        [MigrationsTheory]
        public void Should_not_produce_duplicate_indexes_when_multiple_fks()
        {
            var modelBuilder = new DbModelBuilder();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SwagBag>()
                .HasRequired(c => c.Hat)
                .WithOptional(p => p.SwagBag);

            modelBuilder.Entity<SwagBag>()
                .HasRequired(c => c.Bracelet)
                .WithOptional(l => l.SwagBag);

            modelBuilder.Entity<SwagBag2>()
                .HasRequired(c => c.Hat)
                .WithOptional(p => p.SwagBag);

            modelBuilder.Entity<SwagBag2>()
                .HasRequired(c => c.Bracelet)
                .WithOptional(l => l.SwagBag);

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer()
                    .Diff(model1.GetModel(), model2.GetModel())
                    .ToList();

            Assert.Equal(2, operations.OfType<CreateIndexOperation>().Count());
            Assert.Equal(4, operations.OfType<AddForeignKeyOperation>().Count());
        }

        [Fact]
        public void GetAnnotations_returns_only_annotations_with_annotation_prefix_and_strips_prefix()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Empty(EdmModelDiffer.GetAnnotations(entityType));

            entityType.AddAnnotation("name", new object());

            Assert.Empty(EdmModelDiffer.GetAnnotations(entityType));

            entityType.AddAnnotation(XmlConstants.CustomAnnotationPrefix + "First", "Amy");
            entityType.AddAnnotation(XmlConstants.CustomAnnotationPrefix + "Last", "Winehouse");

            Assert.Equal(2, EdmModelDiffer.GetAnnotations(entityType).Count);
            Assert.Same("Amy", EdmModelDiffer.GetAnnotations(entityType)["First"]);
            Assert.Same("Winehouse", EdmModelDiffer.GetAnnotations(entityType)["Last"]);

            entityType.AddAnnotation(XmlConstants.IndexAnnotationWithPrefix, new IndexAnnotation(new IndexAttribute()));

            Assert.Equal(2, EdmModelDiffer.GetAnnotations(entityType).Count);
        }

        [Fact] // CodePlex 1901
        public void BuildColumnModel_handles_unbounded_values()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String);
            var typeUsage =
                TypeUsage.Create(
                    primitiveType,
                    new[]
                    {
                        CreateIntFacet(DbProviderManifest.MaxLengthFacetName, EdmConstants.Unbounded.Instance),
                        CreateByteFacet(DbProviderManifest.PrecisionFacetName, EdmConstants.Unbounded.Instance),
                        CreateByteFacet(DbProviderManifest.ScaleFacetName, EdmConstants.Unbounded.Instance),
                        CreateBoolFacet(DbProviderManifest.NullableFacetName, true),
                    });

            var property = new EdmProperty("P", typeUsage);

            var model = EdmModelDiffer.BuildColumnModel(property, new TypeUsage(), typeUsage, null);

            Assert.Null(model.MaxLength);
            Assert.Null(model.Precision);
            Assert.Null(model.Scale);
        }

        private static Facet CreateIntFacet(string facetName, object value)
        {
            return Facet.Create(
                new FacetDescription(facetName, PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), 0, 1, 0, false, null),
                value);
        }

        private static Facet CreateByteFacet(string facetName, object value)
        {
            return Facet.Create(
                new FacetDescription(facetName, PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Byte), 0, 1, (byte)0, false, null),
                value);
        }

        private static Facet CreateBoolFacet(string facetName, object value)
        {
            return Facet.Create(
                new FacetDescription(facetName, PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Boolean), 0, 1, 0, true, null),
                value);
        }

        #endregion

        #region Bugs

        // issue #2433
        [MigrationsTheory]
        public void Sequence_contains_no_elements_when_nav_props_renamed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Bug2433_v1.Foob>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();
            
            modelBuilder.Entity<Bug2433_v2.Foob>();
            
            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(4, operations.Count());
        }

        // issue 2157
        [MigrationsTheory]
        public void Diffing_TPH_models_with_discriminator_based_on_nullability_rather_than_value_works()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Foo2157>();
            modelBuilder.Entity<EnhancedFoo2157>().Map(m => m.Requires(o => o.XColumn).HasValue());

            var model1 = modelBuilder.Build(ProviderInfo);

            // making this change to force the actual diff
            modelBuilder.Entity<EnhancedFoo2157>().Property(e => e.Name).HasMaxLength(20);
            var model2 = modelBuilder.Build(ProviderInfo);

            var operations = new EdmModelDiffer().Diff(model1.GetModel(), model2.GetModel());
            var alterColumnOperation = (AlterColumnOperation)operations.Single();
            Assert.Equal("Name", alterColumnOperation.Column.Name);
        }

        public class Foo2157
        {
            public Guid Foo2157Id { get; set; }
        }

        public class EnhancedFoo2157 : Foo2157
        {
            public Int32 XColumn { get; set; }
            public string Name { get; set; }
        }

        // issue 2103
        [MigrationsTheory]
        public void Tph_to_tpt()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<AttachedFile>()
                .HasRequired(t => t.Tender)
                .WithMany(t => t.AttachedFiles)
                .HasForeignKey(d => d.TenderId);
            modelBuilder
                .Entity<AttachedFile>()
                .ToTable("AttachedFile");

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<AttachedFileBase>();
            modelBuilder
                .Entity<TenderAttachedFile>()
                .HasRequired(t => t.Tender)
                .WithMany(t => t.AttachedFiles)
                .HasForeignKey(d => d.TenderId);
            modelBuilder
                .Entity<TenderAttachedFile>()
                .ToTable("TenderAttachedFile");

            var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer()
                    .Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(7, operations.Count);
        }

        // issue 2391
        [MigrationsTheory]
        public void Nav_prop_rename()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Nav_prop_rename_v1.Table>();

            var model1 = modelBuilder.Build(ProviderInfo);

            modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Nav_prop_rename_v2.Table>();

           var model2 = modelBuilder.Build(ProviderInfo);

            var operations
                = new EdmModelDiffer()
                    .Diff(model1.GetModel(), model2.GetModel());

            Assert.Equal(0, operations.Count);
        }

        #endregion
    }

    #region Fixtures

    namespace Bug2433_v1
    {
        public class Foob
        {
            public int Id { get; set; }

            public Barb Bar1 { get; set; }
            public Barb Bar2 { get; set; }
        }

        public class Barb
        {
            public int Id { get; set; }
        }
    }

    namespace Bug2433_v2
    {
        public class Foob
        {
            public int Id { get; set; }

            public Barb Bars1 { get; set; }
            public Barb Bars2 { get; set; }
        }

        public class Barb
        {
            public int Id { get; set; }
        }
    }

    namespace Nav_prop_rename_v1
    {
        public class Table
        {
            public Guid Id { get; set; }

            public int? ReturnedBy { get; set; }
            public int? ReceivedBy { get; set; }

            [ForeignKey("ReturnedBy")]
            public User User1 { get; set; }

            [ForeignKey("ReceivedBy")]
            public User User2 { get; set; }
        }

        public class User
        {
            public int Id { get; set; }
        }
    }

    namespace Nav_prop_rename_v2
    {
        public class Table
        {
            public Guid Id { get; set; }

            public int? ReturnedBy { get; set; }
            public int? ReceivedBy { get; set; }

            [ForeignKey("ReturnedBy")]
            public User ReturnedByUser { get; set; }

            [ForeignKey("ReceivedBy")]
            public User ReceivedByUser { get; set; }
        }

        public class User
        {
            public int Id { get; set; }
        }
    }

    namespace Tph_to_tpt_v1
    {
        public class Tender
        {
            public int Id { get; set; }
            public ICollection<AttachedFile> AttachedFiles { get; set; }
        }

        public class AttachedFile
        {
            public int Id { get; set; }
            public int TenderId { get; set; }
            public Tender Tender { get; set; }
        }
    }

    namespace Tph_to_tpt_v2
    {
        public class Tender
        {
            public int Id { get; set; }
            public ICollection<TenderAttachedFile> AttachedFiles { get; set; }
        }

        public abstract class AttachedFileBase
        {
            public int Id { get; set; }
        }

        public class TenderAttachedFile : AttachedFileBase
        {
            public int TenderId { get; set; }
            public Tender Tender { get; set; }
        }
    }

    namespace Renamed_ia_pk_v1
    {
        public class Dependent
        {
            public int Id { get; set; }
            public virtual Principal Principal { get; set; }
        }

        public class Principal
        {
            public int Id { get; set; }
        }
    }

    namespace Renamed_ia_pk_v2
    {
        public class Dependent
        {
            public int Id { get; set; }
            public virtual Principal Principal { get; set; }
        }

        public class Principal
        {
            public int PrincipalId { get; set; }
        }
    }

    #endregion
}
