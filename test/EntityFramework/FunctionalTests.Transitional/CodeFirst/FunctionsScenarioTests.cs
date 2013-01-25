// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Resources;
    using System.Linq;
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
