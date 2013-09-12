// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using ConcurrencyModel;
    using Xunit;

    public class FunctionsScenarioTests
    {
        [ComplexType]
        public class Address
        {
            public string Line1 { get; set; }
            public string Line2 { get; set; }
        }

        public class CTEmployee
        {
            public int CTEmployeeId { get; set; }
            public Address HomeAddress { get; set; }
        }

        public class OffSiteEmployee : CTEmployee
        {
            public Address WorkAddress { get; set; }
        }

        public class Building : IBuilding
        {
            public int Id { get; set; }
            public Address Address { get; set; }
        }

        public interface IBuilding
        {
            int Id { get; set; }
            Address Address { get; set; }
        }

        public class ModificationFunctions
        {
            public class ProductA
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public ICollection<Tag> Tags { get; set; }
            }

            public class Tag
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public ICollection<ProductA> Products { get; set; }
            }

            public class MetadataGeneration : TestBase
            {
                [Fact]
                public void Map_to_functions_by_convention()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<OrderLine>().MapToStoredProcedures();

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

                    modelBuilder.Entity<Building>().MapToStoredProcedures();

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

                    modelBuilder.Entity<MigrationsCustomer>().MapToStoredProcedures();

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
                    modelBuilder.Entity<GoldCustomer>().MapToStoredProcedures();

                    Assert.Equal(
                        Strings.BaseTypeNotMappedToFunctions(
                            typeof(MigrationsCustomer).Name,
                            typeof(GoldCustomer).Name),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Map_to_functions_by_convention_when_ias()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Order>().MapToStoredProcedures();
                    modelBuilder.Entity<OrderLine>()
                        .MapToStoredProcedures()
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

                    modelBuilder
                        .Entity<Tag>()
                        .HasMany(t => t.Products)
                        .WithMany(p => p.Tags)
                        .MapToStoredProcedures();

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.Equal(2, databaseMapping.Database.Functions.Count());

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .AssociationSetMappings
                            .Select(asm => asm.ModificationFunctionMapping)
                            .Single();

                    Assert.NotNull(functionMapping.InsertFunctionMapping);
                    Assert.Equal("TagProductA_Insert", functionMapping.InsertFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.NotNull(functionMapping.DeleteFunctionMapping);
                    Assert.Equal("TagProductA_Delete", functionMapping.DeleteFunctionMapping.Function.StoreFunctionNameAttribute);
                }

                [Fact]
                public void Map_to_functions_by_convention_when_concurrency()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Engine>().MapToStoredProcedures();
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

                [Fact]
                public void Parameter_names_are_uniquified_when_column_name_configured()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures()
                        .Property(o => o.IsShipped).HasColumnName("Price");

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

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "Price1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "Price1"));
                }

                [Fact]
                public void Parameter_names_are_uniquified_when_parameter_name_configured()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures(map => map.Insert(f => f.Parameter(ol => ol.IsShipped, "Price")));

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

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "Price1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "IsShipped"));
                }

                [Fact]
                public void Parameter_names_are_uniquified_when_parameter_name_configured_via_property_configuration()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures()
                        .Property(ol => ol.IsShipped)
                        .HasParameterName("Price");

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

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "Price1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "Price1"));
                }

                [Fact]
                public void Parameter_names_are_uniquified_when_rows_affected_parameter_configured()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Engine>()
                        .MapToStoredProcedures(map => map.Update(f => f.RowsAffectedParameter("Name")));

                    modelBuilder.Ignore<Team>();

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single();

                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "Name1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "Name"));
                }

                [Fact]
                public void Entity_function_names_are_uniquified_when_name_configured()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures(map => map.Insert(f => f.HasName("OrderLine_Update")));

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

                    Assert.Equal("OrderLine_Update", functionMapping.InsertFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("OrderLine_Update1", functionMapping.UpdateFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("OrderLine_Delete", functionMapping.DeleteFunctionMapping.Function.StoreFunctionNameAttribute);
                }

                [Fact]
                public void Association_function_names_are_uniquified_when_name_configured()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Tag>()
                        .HasMany(t => t.Products)
                        .WithMany(p => p.Tags)
                        .MapToStoredProcedures(map => map.Insert(f => f.HasName("TagProductA_Delete")));

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .AssociationSetMappings
                            .Single()
                            .ModificationFunctionMapping;

                    Assert.Equal("TagProductA_Delete", functionMapping.InsertFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("TagProductA_Delete1", functionMapping.DeleteFunctionMapping.Function.StoreFunctionNameAttribute);
                }
            }

            public class Item
            {
                public int Id { get; set; }
                public int Name { get; set; }
                public virtual Item ParentItem { get; set; }
                public virtual ICollection<Item> ChildrenItems { get; set; }
            }

            public class ConfigurationApis : TestBase
            {
                [Fact]
                public void Can_configure_function_names()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures(
                            map =>
                                {
                                    map.Insert(f => f.HasName("insert_order_line"));
                                    map.Update(f => f.HasName("update_order_line", "foo"));
                                    map.Delete(f => f.HasName("delete_order_line", "bar"));
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

                    Assert.Equal("insert_order_line", functionMapping.InsertFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("update_order_line", functionMapping.UpdateFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("delete_order_line", functionMapping.DeleteFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("foo", functionMapping.UpdateFunctionMapping.Function.Schema);
                    Assert.Equal("bar", functionMapping.DeleteFunctionMapping.Function.Schema);
                }

                [Fact]
                public void Can_configure_parameter_names()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Building>()
                        .MapToStoredProcedures(
                            map =>
                                {
                                    map.Insert(f => f.Parameter(b => b.Address.Line1, "ins_line1"));
                                    map.Update(f => f.Parameter(b => b.Id, "upd_id"));
                                    map.Delete(f => f.Parameter(b => b.Id, "del_id"));
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
                        .MapToStoredProcedures(
                            map => map.Update(
                                f =>
                                    {
                                        f.Parameter(e => e.Name, "name_cur", "name_orig");
                                        f.Parameter(e => e.StorageLocation.Latitude, "lat_cur", "lat_orig");
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
                        .MapToStoredProcedures(
                            map => map.Update(
                                f => f.Parameter(e => e.Id, "id", "boom")));
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
                        .MapToStoredProcedures(
                            map => map.Delete(
                                f =>
                                    {
                                        f.HasName("del_ol");
                                        f.Parameter(e => e.IsShipped, "boom");
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
                        .MapToStoredProcedures(
                            map =>
                                {
                                    map.Insert(f => f.Result(o => o.OrderId, "order_id"));
                                    map.Update(f => f.Result(o => o.Version, "timestamp"));
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
                                      .MapToStoredProcedures(
                                          map => map.Update(
                                              f => f.Result(e => e.StorageLocation.Latitude, "boom")))).Message);
                }

                [Fact]
                public void Configuring_binding_for_missing_property_should_throw()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Order>()
                        .MapToStoredProcedures(
                            map => map.Insert(f => f.Result(o => o.Type, "boom")));

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
                        .MapToStoredProcedures(
                            map =>
                                {
                                    map.Update(f => f.RowsAffectedParameter("rows_affected1"));
                                    map.Delete(f => f.RowsAffectedParameter("rows_affected2"));
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
                public void Configuring_rows_affected_parameter_should_introduce_parameter()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures(
                            map => map.Update(f => f.RowsAffectedParameter("rows_affected")));

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single();

                    Assert.Equal("rows_affected", functionMapping.UpdateFunctionMapping.RowsAffectedParameterName);
                }

                [Fact]
                public void Can_configure_many_to_many_modification_functions()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Tag>()
                        .HasMany(t => t.Products)
                        .WithMany(p => p.Tags)
                        .MapToStoredProcedures(
                            map =>
                                {
                                    map.Insert(
                                        f =>
                                            {
                                                f.HasName("ins_product_tag");
                                                f.LeftKeyParameter(t => t.Id, "tag_id");
                                                f.RightKeyParameter(p => p.Id, "product_id");
                                            });
                                    map.Delete(
                                        f =>
                                            {
                                                f.HasName("del_product_tag", "bar");
                                                f.LeftKeyParameter(t => t.Id, "tag_id");
                                                f.RightKeyParameter(p => p.Id, "product_id");
                                            });
                                });

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.Equal(2, databaseMapping.Database.Functions.Count());

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .AssociationSetMappings
                            .Select(asm => asm.ModificationFunctionMapping)
                            .Single();

                    Assert.Equal("ins_product_tag", functionMapping.InsertFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("del_product_tag", functionMapping.DeleteFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "tag_id"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "tag_id"));
                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "product_id"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "product_id"));
                    Assert.Equal("bar", functionMapping.DeleteFunctionMapping.Function.Schema);
                }

                [Fact]
                public void Can_configure_many_to_many_modification_functions_from_both_ends()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Tag>()
                        .HasMany(t => t.Products)
                        .WithMany(p => p.Tags)
                        .MapToStoredProcedures(
                            map => map.Insert(
                                f =>
                                    {
                                        f.HasName("ins_product_tag");
                                        f.LeftKeyParameter(t => t.Id, "tag_id");
                                    }));

                    modelBuilder
                        .Entity<ProductA>()
                        .HasMany(p => p.Tags)
                        .WithMany(t => t.Products)
                        .MapToStoredProcedures(
                            map => map.Delete(
                                f =>
                                    {
                                        f.HasName("del_product_tag");
                                        f.LeftKeyParameter(p => p.Id, "product_id");
                                        f.RightKeyParameter(t => t.Id, "tag_id");
                                    }));

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.Equal(2, databaseMapping.Database.Functions.Count());

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .AssociationSetMappings
                            .Select(asm => asm.ModificationFunctionMapping)
                            .Single();

                    Assert.Equal("ins_product_tag", functionMapping.InsertFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("del_product_tag", functionMapping.DeleteFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "tag_id"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "tag_id"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "product_id"));
                }

                public class Person
                {
                    public int Id { get; set; }
                    public string Name { get; set; }
                    public ICollection<Person> Children { get; set; }
                    public ICollection<Person> Parents { get; set; }
                }

                [Fact]
                public void Can_configure_many_to_many_self_ref()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Person>()
                        .HasMany(p => p.Children)
                        .WithMany(p => p.Parents)
                        .MapToStoredProcedures(
                            m =>
                                {
                                    m.Insert(
                                        f => f.LeftKeyParameter(l => l.Id, "insert_left")
                                                 .RightKeyParameter(r => r.Id, "insert_right"));
                                    m.Delete(
                                        f => f.RightKeyParameter(r => r.Id, "delete_right"));
                                });

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.Equal(2, databaseMapping.Database.Functions.Count());

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .AssociationSetMappings
                            .Select(asm => asm.ModificationFunctionMapping)
                            .Single();

                    Assert.Equal("PersonPerson_Insert", functionMapping.InsertFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("PersonPerson_Delete", functionMapping.DeleteFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("insert_left", functionMapping.InsertFunctionMapping.Function.Parameters.First().Name);
                    Assert.Equal("insert_right", functionMapping.InsertFunctionMapping.Function.Parameters.Last().Name);
                    Assert.Equal("Person_Id", functionMapping.DeleteFunctionMapping.Function.Parameters.First().Name);
                    Assert.Equal("delete_right", functionMapping.DeleteFunctionMapping.Function.Parameters.Last().Name);
                }

                [Fact]
                public void Configuring_parameter_when_not_valid_for_many_to_many_should_throw()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Tag>()
                        .HasMany(t => t.Products)
                        .WithMany(p => p.Tags)
                        .MapToStoredProcedures(
                            map => map.Insert(
                                f => f.LeftKeyParameter(t => t.Name, "tag_id")));

                    Assert.Equal(
                        Strings.ModificationFunctionParameterNotFound("Name", "TagProductA_Insert"),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Configuring_parameter_when_conflicting_configuration_for_many_to_many_should_throw()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Tag>()
                        .HasMany(t => t.Products)
                        .WithMany(p => p.Tags)
                        .MapToStoredProcedures(
                            map => map.Insert(f => f.HasName("ins_product_tag")));

                    modelBuilder
                        .Entity<ProductA>()
                        .HasMany(p => p.Tags)
                        .WithMany(t => t.Products)
                        .MapToStoredProcedures(
                            map => map.Insert(f => f.HasName("boom")));

                    Assert.Equal(
                        Strings.ConflictingFunctionsMapping(
                            "Tags", "FunctionalTests.FunctionsScenarioTests+ModificationFunctions+ProductA"),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Can_configure_composite_ia_fk_parameters_from_nav_prop_on_principal()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Order>()
                        .HasKey(
                            o => new
                                     {
                                         o.OrderId,
                                         o.Type
                                     });

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures(
                            map =>
                                {
                                    map.Insert(
                                        f => f.Navigation<Order>(
                                            o => o.OrderLines,
                                            a =>
                                                {
                                                    a.Parameter(o => o.OrderId, "order_id1");
                                                    a.Parameter(o => o.Type, "the_type1");
                                                }));
                                    map.Update(
                                        f => f.Navigation<Order>(
                                            o => o.OrderLines,
                                            a =>
                                                {
                                                    a.Parameter(o => o.OrderId, "order_id2");
                                                    a.Parameter(o => o.Type, "the_type2");
                                                }));
                                    map.Delete(
                                        f => f.Navigation<Order>(
                                            o => o.OrderLines,
                                            a =>
                                                {
                                                    a.Parameter(o => o.OrderId, "order_id3");
                                                    a.Parameter(o => o.Type, "the_type3");
                                                }));
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

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "order_id1"));
                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "the_type1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "order_id2"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "the_type2"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "order_id3"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "the_type3"));
                }

                [Fact]
                public void Can_configure_composite_ia_fk_parameters_from_nav_prop_on_dependent_and_last_wins()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Ignore<Chassis>()
                        .Ignore<Sponsor>()
                        .Ignore<TestDriver>()
                        .Ignore<Gearbox>()
                        .Ignore<Engine>();

                    modelBuilder
                        .Entity<Team>()
                        .HasKey(
                            o => new
                                     {
                                         o.Id,
                                         o.Name
                                     });

                    modelBuilder
                        .Entity<Driver>()
                        .MapToStoredProcedures(
                            map =>
                                {
                                    map.Insert(
                                        f =>
                                            {
                                                f.Navigation<Team>(
                                                    t => t.Drivers,
                                                    a =>
                                                        {
                                                            a.Parameter(t => t.Id, "team_id0");
                                                            a.Parameter(t => t.Name, "team_name0");
                                                        });
                                                f.Parameter(d => d.Team.Id, "team_id1");
                                                f.Parameter(d => d.Team.Name, "team_name1");
                                            });
                                    map.Update(
                                        f =>
                                            {
                                                f.Parameter(d => d.Team.Id, "team_id2");
                                                f.Parameter(d => d.Team.Name, "team_name2");
                                            });
                                    map.Delete(
                                        f =>
                                            {
                                                f.Parameter(d => d.Team.Id, "team_id3");
                                                f.Parameter(d => d.Team.Name, "team_name3");
                                            });
                                })
                        .Ignore(d => d.Name);

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single();

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "team_id1"));
                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "team_name1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "team_id2"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "team_name2"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "team_id3"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "team_name3"));
                }

                [Fact]
                public void Can_configure_ia_fk_self_ref_parameters_from_nav_prop_on_dependent()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Item>()
                        .MapToStoredProcedures(
                            map =>
                                {
                                    map.Insert(f => f.Parameter(i => i.ParentItem.Id, "item_id1"));
                                    map.Update(f => f.Parameter(i => i.ParentItem.Id, "item_id2"));
                                    map.Delete(f => f.Parameter(i => i.ParentItem.Id, "item_id3"));
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

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "item_id1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "item_id2"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "item_id3"));
                }

                [Fact]
                public void Can_configure_ia_fk_self_ref_parameters_from_nav_prop_on_principal()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Item>()
                        .MapToStoredProcedures(
                            map =>
                                {
                                    map.Insert(f => f.Navigation<Item>(o => o.ChildrenItems, a => a.Parameter(i => i.Id, "item_id1")));
                                    map.Update(
                                        f =>
                                            {
                                                f.Parameter(i => i.Id, "id2");
                                                f.Navigation<Item>(o => o.ChildrenItems, a => a.Parameter(i => i.Id, "item_id2"));
                                            });
                                    map.Delete(
                                        f =>
                                            {
                                                f.Navigation<Item>(o => o.ChildrenItems, a => a.Parameter(i => i.Id, "item_id3"));
                                                f.Parameter(i => i.Id, "id3");
                                            });
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

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "item_id1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "id2"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "item_id2"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "id3"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "item_id3"));
                }

                public void Column_configuration_is_propagated_to_parameters()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Order>()
                        .HasMany(o => o.OrderLines)
                        .WithRequired()
                        .Map(m => m.MapKey("order_id"));

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures()
                        .Property(ol => ol.IsShipped).HasColumnName("is_shipped");

                    modelBuilder
                        .Entity<OrderLine>()
                        .Ignore(ol => ol.OrderId);

                    modelBuilder
                        .Entity<OrderLine>()
                        .Property(ol => ol.Quantity).HasColumnType("int");

                    modelBuilder
                        .Entity<OrderLine>()
                        .Property(ol => ol.Id).HasColumnName("the_id");

                    modelBuilder
                        .Entity<Building>()
                        .MapToStoredProcedures()
                        .Property(b => b.Address.Line2).HasColumnName("bar");

                    modelBuilder
                        .ComplexType<Address>()
                        .Property(a => a.Line1).HasColumnName("foomatic");

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var orderLineFunctionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single(mfm => mfm.EntityType.Name == "OrderLine");

                    Assert.NotNull(orderLineFunctionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "order_id"));
                    Assert.NotNull(orderLineFunctionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "is_shipped"));
                    Assert.NotNull(
                        orderLineFunctionMapping.InsertFunctionMapping.Function.Parameters
                            .Single(p => p.Name == "Quantity" && p.TypeName == "int"));

                    Assert.NotNull(orderLineFunctionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "the_id"));
                    Assert.NotNull(orderLineFunctionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "order_id"));
                    Assert.NotNull(orderLineFunctionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "is_shipped"));
                    Assert.NotNull(
                        orderLineFunctionMapping.UpdateFunctionMapping.Function.Parameters
                            .Single(p => p.Name == "Quantity" && p.TypeName == "int"));

                    Assert.NotNull(orderLineFunctionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "the_id"));
                    Assert.NotNull(orderLineFunctionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "order_id"));

                    var buildingFunctionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single(mfm => mfm.EntityType.Name == "Building");

                    Assert.NotNull(buildingFunctionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "bar"));
                    Assert.NotNull(buildingFunctionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "foomatic"));
                    Assert.NotNull(buildingFunctionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "bar"));
                    Assert.NotNull(buildingFunctionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "foomatic"));
                }

                [Fact]
                public void Should_throw_when_conflicting_parameter_names_configured()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures(
                            map => map.Insert(
                                f =>
                                    {
                                        f.Parameter(ol => ol.IsShipped, "Price");
                                        f.Parameter(ol => ol.Price, "Price");
                                    }));

                    Assert.Throws<ModelValidationException>(() => BuildMapping(modelBuilder));
                }

                [Fact]
                public void Can_configure_parameter_names_via_property_configuration()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures()
                        .Property(ol => ol.IsShipped).HasParameterName("is_shipped");

                    modelBuilder
                        .ComplexType<Address>()
                        .Property(a => a.Line1).HasParameterName("foomatic");

                    modelBuilder
                        .Entity<CTEmployee>()
                        .MapToStoredProcedures()
                        .Property(b => b.HomeAddress.Line2).HasParameterName("bar");

                    modelBuilder
                        .Entity<OffSiteEmployee>()
                        .Property(b => b.HomeAddress.Line1).HasParameterName("baz");

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var orderLineFunctionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single(mfm => mfm.EntityType.Name == "OrderLine");

                    Assert.NotNull(orderLineFunctionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "is_shipped"));
                    Assert.NotNull(orderLineFunctionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "is_shipped"));

                    var employeeFunctionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single(mfm => mfm.EntityType.Name == "CTEmployee");

                    Assert.NotNull(employeeFunctionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "bar"));
                    Assert.NotNull(employeeFunctionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "foomatic"));
                    Assert.NotNull(employeeFunctionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "bar"));
                    Assert.NotNull(employeeFunctionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "foomatic"));

                    var offsiteEmployeeFunctionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single(mfm => mfm.EntityType.Name == "OffSiteEmployee");

                    Assert.NotNull(offsiteEmployeeFunctionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "bar"));
                    Assert.NotNull(offsiteEmployeeFunctionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "baz"));
                    Assert.NotNull(offsiteEmployeeFunctionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "bar"));
                    Assert.NotNull(offsiteEmployeeFunctionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "baz"));
                }

                [Fact]
                public void Only_current_value_column_names_updated_via_property_configuration()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Engine>()
                        .MapToStoredProcedures()
                        .Property(e => e.Name)
                        .HasParameterName("my_name");

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

                    Assert.NotNull(function.Parameters.Single(p => p.Name == "my_name"));
                    Assert.NotNull(function.Parameters.Single(p => p.Name == "Name_Original"));
                }

                public class Blog
                {
                    public int Id { get; set; }
                }

                [Fact]
                public void Can_configure_in_multiple_statements_and_configs_are_merged()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Blog>()
                        .MapToStoredProcedures(s => s.Update(u => u.HasName("modify_blog")));

                    modelBuilder
                        .Entity<Blog>()
                        .MapToStoredProcedures(s => s.Delete(d => d.HasName("delete_blog")));

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();
                }
            }

            public class LightweightConventions : TestBase
            {
                [Fact]
                public void Can_configure_function_names()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<OrderLine>();

                    modelBuilder
                        .Types()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map =>
                                    {
                                        map.Insert(f => f.HasName("insert_order_line"));
                                        map.Update(f => f.HasName("update_order_line", "foo"));
                                        map.Delete(f => f.HasName("delete_order_line", "bar"));
                                    }));

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

                    Assert.Equal("insert_order_line", functionMapping.InsertFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("update_order_line", functionMapping.UpdateFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("delete_order_line", functionMapping.DeleteFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("foo", functionMapping.UpdateFunctionMapping.Function.Schema);
                    Assert.Equal("bar", functionMapping.DeleteFunctionMapping.Function.Schema);
                }

                [Fact]
                public void Can_configure_function_names_when_type_specified()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Order>();
                    modelBuilder.Entity<OrderLine>();

                    modelBuilder
                        .Types<OrderLine>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map =>
                                    {
                                        map.Insert(f => f.HasName("insert_order_line"));
                                        map.Update(f => f.HasName("update_order_line", "foo"));
                                        map.Delete(f => f.HasName("delete_order_line", "bar"));
                                    }));

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single();

                    Assert.Equal("insert_order_line", functionMapping.InsertFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("update_order_line", functionMapping.UpdateFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("delete_order_line", functionMapping.DeleteFunctionMapping.Function.StoreFunctionNameAttribute);
                    Assert.Equal("foo", functionMapping.UpdateFunctionMapping.Function.Schema);
                    Assert.Equal("bar", functionMapping.DeleteFunctionMapping.Function.Schema);
                }

                [Fact]
                public void Can_configure_parameter_names()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<OrderLine>();

                    modelBuilder
                        .Types()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map =>
                                    {
                                        map.Insert(f => f.Parameter("OrderId", "ins_order_id"));
                                        map.Update(f => f.Parameter(typeof(OrderLine).GetDeclaredProperty("Id"), "upd_id"));
                                        map.Delete(f => f.Parameter("Id", "del_id"));
                                    }));

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

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "ins_order_id"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "upd_id"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "del_id"));
                }

                [Fact]
                public void Can_configure_parameter_names_when_type_specified()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Building>();

                    modelBuilder
                        .Types<IBuilding>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map =>
                                    {
                                        map.Insert(f => f.Parameter(b => b.Address.Line1, "ins_line1"));
                                        map.Update(f => f.Parameter(b => b.Id, "upd_id"));
                                        map.Delete(f => f.Parameter(b => b.Id, "del_id"));
                                    }));

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

                    modelBuilder.Entity<Engine>();

                    modelBuilder
                        .Types()
                        .Where(t => t == typeof(Engine))
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map => map.Update(
                                    f => f.Parameter("Name", "name_cur", "name_orig"))));
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
                }

                [Fact]
                public void Can_configure_original_value_column_names_when_update_and_type_specified()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Engine>();

                    modelBuilder
                        .Types<Engine>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map => map.Update(
                                    f =>
                                        {
                                            f.Parameter(e => e.Name, "name_cur", "name_orig");
                                            f.Parameter(e => e.StorageLocation.Latitude, "lat_cur", "lat_orig");
                                        })));
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

                    modelBuilder.Entity<Engine>();

                    modelBuilder
                        .Types()
                        .Where(t => t == typeof(Engine))
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map => map.Update(
                                    f => f.Parameter("Id", "id", "boom"))));
                    modelBuilder.Ignore<Team>();

                    Assert.Equal(
                        Strings.ModificationFunctionParameterNotFoundOriginal("Id", "Engine_Update"),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Configuring_original_value_for_non_concurrency_token_should_throw_when_type_specified()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Engine>();

                    modelBuilder
                        .Types<Engine>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map => map.Update(
                                    f => f.Parameter(e => e.Id, "id", "boom"))));
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

                    modelBuilder.Entity<OrderLine>();

                    modelBuilder
                        .Types()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map => map.Delete(
                                    f =>
                                        {
                                            f.HasName("del_ol");
                                            f.Parameter("IsShipped", "boom");
                                        })));

                    Assert.Equal(
                        Strings.ModificationFunctionParameterNotFound("IsShipped", "del_ol"),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Can_configure_result_binding_column_names()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Order>();

                    modelBuilder
                        .Types()
                        .Where(t => t == typeof(Order))
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map =>
                                    {
                                        map.Insert(f => f.Result("OrderId", "order_id"));
                                        map.Update(f => f.Result("Version", "timestamp"));
                                    }));

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
                public void Can_configure_result_binding_column_names_when_type_specified()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Order>();

                    modelBuilder
                        .Types<Order>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map =>
                                    {
                                        map.Insert(f => f.Result(o => o.OrderId, "order_id"));
                                        map.Update(f => f.Result(o => o.Version, "timestamp"));
                                    }));

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
                public void Configuring_binding_for_complex_property_should_throw_when_type_specified()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Engine>();

                    modelBuilder
                        .Types<Engine>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map => map.Update(
                                    f => f.Result(e => e.StorageLocation.Latitude, "boom"))));

                    Assert.Equal(
                        Strings.InvalidPropertyExpression("e => e.StorageLocation.Latitude"),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Configuring_binding_for_missing_property_should_throw()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Order>();

                    modelBuilder
                        .Types()
                        .Where(t => t == typeof(Order))
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map => map.Insert(f => f.Result("Type", "boom"))));

                    Assert.Equal(
                        Strings.ResultBindingNotFound("Type", "Order_Insert"),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Configuring_binding_for_missing_property_should_throw_when_type_specified()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Order>();

                    modelBuilder
                        .Types<Order>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map => map.Insert(f => f.Result(o => o.Type, "boom"))));

                    Assert.Equal(
                        Strings.ResultBindingNotFound("Type", "Order_Insert"),
                        Assert.Throws<InvalidOperationException>(
                            () => BuildMapping(modelBuilder)).Message);
                }

                [Fact]
                public void Can_configure_rows_affected_column_name()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Order>();

                    modelBuilder
                        .Types()
                        .Where(t => t == typeof(Order))
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map =>
                                    {
                                        map.Update(f => f.RowsAffectedParameter("rows_affected1"));
                                        map.Delete(f => f.RowsAffectedParameter("rows_affected2"));
                                    }));

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
                public void Can_configure_rows_affected_column_name_when_type_specified()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Order>();

                    modelBuilder
                        .Types<Order>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map =>
                                    {
                                        map.Update(f => f.RowsAffectedParameter("rows_affected1"));
                                        map.Delete(f => f.RowsAffectedParameter("rows_affected2"));
                                    }));

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
                public void Can_configure_composite_ia_fk_parameters_from_nav_prop_on_principal()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<Order>()
                        .HasKey(
                            o => new
                                     {
                                         o.OrderId,
                                         o.Type
                                     });

                    modelBuilder
                        .Types<OrderLine>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map =>
                                    {
                                        map.Insert(
                                            f => f.Navigation<Order>(
                                                o => o.OrderLines,
                                                a =>
                                                    {
                                                        a.Parameter(o => o.OrderId, "order_id1");
                                                        a.Parameter(o => o.Type, "the_type1");
                                                    }));
                                        map.Update(
                                            f => f.Navigation<Order>(
                                                o => o.OrderLines,
                                                a =>
                                                    {
                                                        a.Parameter(o => o.OrderId, "order_id2");
                                                        a.Parameter(o => o.Type, "the_type2");
                                                    }));
                                        map.Delete(
                                            f => f.Navigation<Order>(
                                                o => o.OrderLines,
                                                a =>
                                                    {
                                                        a.Parameter(o => o.OrderId, "order_id3");
                                                        a.Parameter(o => o.Type, "the_type3");
                                                    }));
                                    }));

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single();

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "order_id1"));
                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "the_type1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "order_id2"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "the_type2"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "order_id3"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "the_type3"));
                }

                [Fact]
                public void Can_configure_composite_ia_fk_parameters_from_nav_prop_on_dependent_and_last_wins()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Ignore<Chassis>()
                        .Ignore<Sponsor>()
                        .Ignore<TestDriver>()
                        .Ignore<Gearbox>()
                        .Ignore<Engine>();

                    modelBuilder
                        .Entity<Team>()
                        .HasKey(
                            o => new
                                     {
                                         o.Id,
                                         o.Name
                                     });

                    modelBuilder.Entity<Driver>().Ignore(d => d.Name);

                    modelBuilder
                        .Types<Driver>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map =>
                                    {
                                        map.Insert(
                                            f =>
                                                {
                                                    f.Navigation<Team>(
                                                        t => t.Drivers,
                                                        a =>
                                                            {
                                                                a.Parameter(t => t.Id, "team_id0");
                                                                a.Parameter(t => t.Name, "team_name0");
                                                            });
                                                    f.Parameter(d => d.Team.Id, "team_id1");
                                                    f.Parameter(d => d.Team.Name, "team_name1");
                                                });
                                        map.Update(
                                            f =>
                                                {
                                                    f.Parameter(d => d.Team.Id, "team_id2");
                                                    f.Parameter(d => d.Team.Name, "team_name2");
                                                });
                                        map.Delete(
                                            f =>
                                                {
                                                    f.Parameter(d => d.Team.Id, "team_id3");
                                                    f.Parameter(d => d.Team.Name, "team_name3");
                                                });
                                    }));

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single();

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "team_id1"));
                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "team_name1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "team_id2"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "team_name2"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "team_id3"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "team_name3"));
                }

                [Fact]
                public void Can_configure_ia_fk_self_ref_parameters_from_nav_prop_on_dependent()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Item>();

                    modelBuilder
                        .Types<Item>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map =>
                                    {
                                        map.Insert(f => f.Parameter(i => i.ParentItem.Id, "item_id1"));
                                        map.Update(f => f.Parameter(i => i.ParentItem.Id, "item_id2"));
                                        map.Delete(f => f.Parameter(i => i.ParentItem.Id, "item_id3"));
                                    }));

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var functionMapping
                        = databaseMapping
                            .EntityContainerMappings
                            .Single()
                            .EntitySetMappings
                            .SelectMany(esm => esm.ModificationFunctionMappings)
                            .Single();

                    Assert.NotNull(functionMapping.InsertFunctionMapping.Function.Parameters.Single(p => p.Name == "item_id1"));
                    Assert.NotNull(functionMapping.UpdateFunctionMapping.Function.Parameters.Single(p => p.Name == "item_id2"));
                    Assert.NotNull(functionMapping.DeleteFunctionMapping.Function.Parameters.Single(p => p.Name == "item_id3"));
                }

                [Fact]
                public void Should_throw_when_conflicting_parameter_names_configured()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<OrderLine>();

                    modelBuilder
                        .Types()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map => map.Insert(
                                    f =>
                                        {
                                            f.Parameter("IsShipped", "Price");
                                            f.Parameter("Price", "Price");
                                        })));

                    Assert.Throws<ModelValidationException>(() => BuildMapping(modelBuilder));
                }

                [Fact]
                public void Should_throw_when_conflicting_parameter_names_configured_when_type_specified()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<OrderLine>();

                    modelBuilder
                        .Types<OrderLine>()
                        .Configure(
                            c => c.MapToStoredProcedures(
                                map => map.Insert(
                                    f =>
                                        {
                                            f.Parameter(ol => ol.IsShipped, "Price");
                                            f.Parameter(ol => ol.Price, "Price");
                                        })));

                    Assert.Throws<ModelValidationException>(() => BuildMapping(modelBuilder));
                }

                [Fact]
                public void Can_configure_parameter_name_via_properties()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures();

                    modelBuilder
                        .Properties()
                        .Configure(c => c.HasParameterName("i_" + c.ClrPropertyInfo.Name));

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var integerParameters
                        = databaseMapping
                            .Database
                            .Functions
                            .SelectMany(f => f.Parameters)
                            .Where(p => p.TypeName == "int")
                            .ToList();

                    Assert.Equal(6, integerParameters.Count());
                    Assert.True(integerParameters.All(p => p.Name.StartsWith("i_")));
                }

                [Fact]
                public void Can_configure_parameter_name_via_properties_of_type()
                {
                    var modelBuilder = new DbModelBuilder();

                    modelBuilder
                        .Entity<OrderLine>()
                        .MapToStoredProcedures();

                    modelBuilder
                        .Properties<int>()
                        .Configure(c => c.HasParameterName("i_" + c.ClrPropertyInfo.Name));

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    var integerParameters
                        = databaseMapping
                            .Database
                            .Functions
                            .SelectMany(f => f.Parameters)
                            .Where(p => p.TypeName == "int")
                            .ToList();

                    Assert.Equal(6, integerParameters.Count());
                    Assert.True(integerParameters.All(p => p.Name.StartsWith("i_")));
                }
            }
        }

        public class AdvancedMapping : AdvancedMappingScenarioTests
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }

        public class Associations : AssociationScenarioTests
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
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
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }

        public class ComplexTypes : ComplexTypeScenarioTests
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }

        public class Configuration : ConfigurationScenarioTests
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }

        public class ConventionTests : ConventionsScenarioTests.ConventionTests
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }

        public class LightweightTypeConventions : ConventionsScenarioTests.LightweightTypeConventions
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }

        public class LightweightPropertyConventions : ConventionsScenarioTests.LightweightPropertyConventions
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }

        public class DataAnnotations : DataAnnotationScenarioTests
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }

        public class Enums : EnumsScenarioTests
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }

        public class Inheritance : InheritanceScenarioTests
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }

        public class PropertyConfiguration : PropertyConfigurationScenarioTests
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }

        public class Spatial : SpatialScenarioTests
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Types().Configure(c => c.MapToStoredProcedures());
            }
        }
    }
}
