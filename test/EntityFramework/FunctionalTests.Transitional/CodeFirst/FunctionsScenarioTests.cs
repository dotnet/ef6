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

                    databaseMapping.ShellEdmx();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .Single()
                            .ModificationFunctionMappings
                            .Single();

                    Assert.True(functionMapping.InsertFunctionMapping.Function.Parameters.Any(p => p.Name == "ins_line1"));
                    Assert.True(functionMapping.UpdateFunctionMapping.Function.Parameters.Any(p => p.Name == "upd_id"));
                    Assert.True(functionMapping.DeleteFunctionMapping.Function.Parameters.Any(p => p.Name == "del_id"));
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
