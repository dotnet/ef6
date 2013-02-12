// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Resources;
    using System.Linq;
    using ConcurrencyModel;
    using Xunit;

    public class FunctionsScenarioTests
    {
        public class ModificationFunctions
        {
            public class MetadataGeneration : TestBase
            {
                [Fact]
                public void Map_to_functions_by_convention()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<OrderLine>().MapToFunctions();

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.Equal(3, databaseMapping.Database.Functions.Count());

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .Single()
                            .ModificationFunctionMappings
                            .Single();

                    Assert.NotNull(functionMapping.InsertFunctionMapping);
                    Assert.NotNull(functionMapping.UpdateFunctionMapping);
                    Assert.NotNull(functionMapping.DeleteFunctionMapping);
                }

                [Fact]
                public void Map_to_functions_by_convention_when_complex_type()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Building>().MapToFunctions();

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.Equal(3, databaseMapping.Database.Functions.Count());

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .Single()
                            .ModificationFunctionMappings
                            .Single();

                    Assert.NotNull(functionMapping.InsertFunctionMapping);
                    Assert.NotNull(functionMapping.UpdateFunctionMapping);
                    Assert.NotNull(functionMapping.DeleteFunctionMapping);
                }

                [Fact]
                public void Map_to_functions_by_convention_when_inheritance()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<MigrationsCustomer>().MapToFunctions();

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.Equal(9, databaseMapping.Database.Functions.Count());

                    var functionMappings
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings);

                    foreach (var functionMapping in functionMappings)
                    {
                        Assert.NotNull(functionMapping.InsertFunctionMapping);
                        Assert.NotNull(functionMapping.UpdateFunctionMapping);
                        Assert.NotNull(functionMapping.DeleteFunctionMapping);
                    }
                }

                [Fact]
                public void Map_to_functions_by_convention_when_inheritance_base_type_not_mapped_to_functions()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<MigrationsCustomer>();
                    modelBuilder.Entity<GoldCustomer>().MapToFunctions();

                    Assert.Equal(
                        Strings.BaseTypeNotMappedToFunctions(
                            typeof(MigrationsCustomer).FullName,
                            typeof(GoldCustomer).FullName),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Map_to_functions_by_convention_when_ias()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Order>().MapToFunctions();
                    modelBuilder.Entity<OrderLine>()
                                .MapToFunctions()
                                .Ignore(ol => ol.OrderId);

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.Equal(6, databaseMapping.Database.Functions.Count());

                    var functionMappings
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings);

                    foreach (var functionMapping in functionMappings)
                    {
                        Assert.NotNull(functionMapping.InsertFunctionMapping);
                        Assert.NotNull(functionMapping.UpdateFunctionMapping);
                        Assert.NotNull(functionMapping.DeleteFunctionMapping);
                    }
                }

                [Fact]
                public void Map_to_functions_by_convention_when_many_to_many()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Tag>().MapToFunctions();
                    modelBuilder.Entity<ProductA>().MapToFunctions();

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.Equal(8, databaseMapping.Database.Functions.Count());

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .AssociationSetMappings
                            .Select(asm => asm.ModificationFunctionMapping)
                            .Single();

                    Assert.NotNull(functionMapping.InsertFunctionMapping);
                    Assert.NotNull(functionMapping.DeleteFunctionMapping);
                }

                [Fact]
                public void Map_to_functions_by_convention_when_concurrency()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Engine>().MapToFunctions();
                    modelBuilder.Ignore<Team>();

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.Equal(3, databaseMapping.Database.Functions.Count());

                    var functionMappings
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings);

                    foreach (var functionMapping in functionMappings)
                    {
                        Assert.NotNull(functionMapping.InsertFunctionMapping);
                        Assert.NotNull(functionMapping.UpdateFunctionMapping);
                        Assert.NotNull(functionMapping.DeleteFunctionMapping);
                    }
                }
            }

            public class Apis : TestBase
            {
                [Fact]
                public void Can_configure_function_names()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToFunctions(
                            map =>
                                {
                                    map.InsertFunction(f => f.HasName("insert_order_line"));
                                    map.UpdateFunction(f => f.HasName("update_order_line"));
                                    map.DeleteFunction(f => f.HasName("delete_order_line"));
                                });

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .Single()
                            .ModificationFunctionMappings
                            .Single();

                    Assert.Equal("insert_order_line", functionMapping.InsertFunctionMapping.Function.Name);
                    Assert.Equal("update_order_line", functionMapping.UpdateFunctionMapping.Function.Name);
                    Assert.Equal("delete_order_line", functionMapping.DeleteFunctionMapping.Function.Name);
                }

                [Fact]
                public void Can_configure_parameter_names()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Building>()
                        .MapToFunctions(
                            map =>
                                {
                                    map.InsertFunction(f => f.Parameter(b => b.Address.Line1).HasName("ins_line1"));
                                    map.UpdateFunction(f => f.Parameter(b => b.Id).HasName("upd_id"));
                                    map.DeleteFunction(f => f.Parameter(b => b.Id).HasName("del_id"));
                                });

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .Single()
                            .ModificationFunctionMappings
                            .Single();

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "ins_line1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "upd_id"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "del_id"));
                }

                [Fact]
                public void Can_configure_original_value_column_names_when_update()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Engine>()
                        .MapToFunctions(
                            map => map.UpdateFunction(
                                f =>
                                    {
                                        f.Parameter(e => e.Name).HasName("name_cur");
                                        f.Parameter(e => e.Name, originalValue: true).HasName("name_orig");
                                        f.Parameter(e => e.StorageLocation.Latitude).HasName("lat_cur");
                                        f.Parameter(e => e.StorageLocation.Latitude, originalValue: true).HasName("lat_orig");
                                    }));
                    modelBuilder.Ignore<Team>();

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var function
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Select(fm => fm.UpdateFunctionMapping.Function)
                            .Single();

                    Assert.NotNull(function.Parameters.Single(p => p.Name == "name_cur"));
                    Assert.NotNull(function.Parameters.Single(p => p.Name == "name_orig"));
                    Assert.NotNull(function.Parameters.Single(p => p.Name == "lat_cur"));
                    Assert.NotNull(function.Parameters.Single(p => p.Name == "lat_orig"));
                }

                [Fact]
                public void Configuring_original_value_for_non_concurrency_token_should_throw()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Engine>()
                        .MapToFunctions(
                            map => map.UpdateFunction(
                                f => f.Parameter(e => e.Id, originalValue: true).HasName("boom")));
                    modelBuilder.Ignore<Team>();

                    Assert.Equal(
                        Strings.ModificationFunctionParameterNotFoundOriginal("Id", "Engine_Update"),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Configuring_parameter_when_not_valid_for_operation_should_throw()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToFunctions(
                            map => map.DeleteFunction(
                                f =>
                                    {
                                        f.HasName("del_ol");
                                        f.Parameter(e => e.IsShipped).HasName("boom");
                                    }));

                    Assert.Equal(
                        Strings.ModificationFunctionParameterNotFound("IsShipped", "del_ol"),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Can_configure_result_binding_column_names()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Order>()
                        .MapToFunctions(
                            map =>
                                {
                                    map.InsertFunction(f => f.BindResult(o => o.OrderId, "order_id"));
                                    map.UpdateFunction(f => f.BindResult(o => o.Version, "timestamp"));
                                });

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single();

                    Assert.NotNull(functionMapping.InsertFunctionMapping.ResultBindings.Single(rb => rb.ColumnName == "order_id"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.ResultBindings.Single(rb => rb.ColumnName == "timestamp"));
                }

                [Fact]
                public void Configuring_binding_for_complex_property_should_throw()
                {
                    var modelBuilder = new DbModelBuilder();

                    Assert.Equal(
                        Strings.InvalidPropertyExpression("e => e.StorageLocation.Latitude"),
                        Assert.Throws<InvalidOperationException>(
                            () => modelBuilder
                                      .Entity<Engine>()
                                      .MapToFunctions(
                                          map => map.UpdateFunction(
                                              f => f.BindResult(e => e.StorageLocation.Latitude, "boom")))).Message);
                }

                [Fact]
                public void Configuring_binding_for_missing_property_should_throw()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Order>()
                        .MapToFunctions(
                            map => map.InsertFunction(f => f.BindResult(o => o.Type, "boom")));

                    Assert.Equal(
                        Strings.ResultBindingNotFound("Type", "Order_Insert"),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Can_configure_rows_affected_column_name()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Order>()
                        .MapToFunctions(
                            map =>
                                {
                                    map.UpdateFunction(f => f.RowsAffectedParameter("rows_affected1"));
                                    map.DeleteFunction(f => f.RowsAffectedParameter("rows_affected2"));
                                });

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single();

                    Assert.Equal("rows_affected1", functionMapping.UpdateFunctionMapping.RowsAffectedParameter.Name);
                    Assert.Equal("rows_affected2", functionMapping.DeleteFunctionMapping.RowsAffectedParameter.Name);
                }

                [Fact]
                public void Configuring_missing_rows_affected_parameter_should_throw()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToFunctions(
                            map => map.UpdateFunction(f => f.RowsAffectedParameter("rows_affected")));

                    Assert.Equal(
                        Strings.NoRowsAffectedParameter("OrderLine_Update"),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }
            }

            public class AdvancedMapping : AdvancedMappingScenarioTests
            {
                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entities().Configure(c => c.MapToFunctions());
                }
            }

            public class Associations : AssociationScenarioTests
            {
                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entities().Configure(c => c.MapToFunctions());
                }
            }

            public class BasicMapping : BasicMappingScenarioTests
            {
                public override void Abstract_in_middle_of_hierarchy_with_TPC()
                {
                    //TODO: Bug #842
                }

                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entities().Configure(c => c.MapToFunctions());
                }
            }

            public class ComplexTypes : ComplexTypeScenarioTests
            {
                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entities().Configure(c => c.MapToFunctions());
                }
            }

            public class Configuration : ConfigurationScenarioTests
            {
                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entities().Configure(c => c.MapToFunctions());
                }
            }

            public class Conventions : ConventionsScenarioTests
            {
                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entities().Configure(c => c.MapToFunctions());
                }
            }

            public class DataAnnotations : DataAnnotationScenarioTests
            {
                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entities().Configure(c => c.MapToFunctions());
                }
            }

            public class Enums : EnumsScenarioTests
            {
                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entities().Configure(c => c.MapToFunctions());
                }
            }

            public class Inheritance : InheritanceScenarioTests
            {
                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entities().Configure(c => c.MapToFunctions());
                }
            }

            public class PropertyConfiguration : PropertyConfigurationScenarioTests
            {
                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entities().Configure(c => c.MapToFunctions());
                }
            }

            public class Spatial : SpatialScenarioTests
            {
                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entities().Configure(c => c.MapToFunctions());
                }
            }
        }
    }
}
