// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Transactions;
    using FunctionalTests.Model;
    using Xunit;

    #region Fixtures

    public abstract class ITFoo
    {
        public int Id { get; set; }
    }

    public class ITBar : ITFoo
    {
    }

    public class ITBaz
    {
        public int Id { get; set; }
        public ICollection<ITFoo> ITFoos { get; set; }
    }

    public abstract class A1
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int Age1 { get; set; }
        public string Name1 { get; set; }
    }

    public class A2 : A1
    {
        public int Age2 { get; set; }
        public string Name2 { get; set; }
    }

    public class A3 : A2
    {
        public int Age3 { get; set; }
        public string Name3 { get; set; }
    }

    public class A4 : A1
    {
        public int Age4 { get; set; }
        public string Name4 { get; set; }
    }

    public class B1
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int Age1 { get; set; }
        public string Name1 { get; set; }
    }

    public abstract class B2 : B1
    {
        public int Age2 { get; set; }
        public string Name2 { get; set; }
    }

    public class B3 : B2
    {
        public int Age3 { get; set; }
        public string Name3 { get; set; }
    }

    public class ITOffice
    {
        public int ITOfficeId { get; set; }
        public string Name { get; set; }
    }

    public class ITEmployee
    {
        public int ITEmployeeId { get; set; }
        public string Name { get; set; }
    }

    public class ITOnSiteEmployee : ITEmployee
    {
        public ITOffice ITOffice { get; set; }
    }

    public class ITOffSiteEmployee : ITEmployee
    {
        public string SiteName { get; set; }
    }

    public class IT_Office
    {
        public int IT_OfficeId { get; set; }
        public string Name { get; set; }
    }

    public class IT_Employee
    {
        public int IT_EmployeeId { get; set; }
        public string Name { get; set; }
    }

    public class IT_OnSiteEmployee : IT_Employee
    {
        public int IT_OfficeId { get; set; }
        public IT_Office IT_Office { get; set; }
    }

    public class IT_OffSiteEmployee : IT_Employee
    {
        public string SiteName { get; set; }
    }

    public class IT_Context : DbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IT_Office>();
            modelBuilder.Entity<IT_Employee>().ToTable("Employees");
            modelBuilder.Entity<IT_OffSiteEmployee>().ToTable("OffSiteEmployees");
            modelBuilder.Entity<IT_OnSiteEmployee>().ToTable("OnSiteEmployees");
            modelBuilder.Entity<IT_OnSiteEmployee>()
                .HasRequired(e => e.IT_Office);
        }

        public DbSet<IT_Office> Offices { get; set; }
        public DbSet<IT_Employee> Employees { get; set; }
    }

    public abstract class D1
    {
        public int D1Id { get; set; }
    }

    public class DiscontinueD1 : D1
    {
        public DateTime DiscontinuedOn { get; set; }
    }

    public class C1
    {
        public int Id { get; set; }
        public int DiscontinueD1Id { get; set; }
        public DiscontinueD1 DiscontinueD1 { get; set; }
    }

    public class BaseEntityDuplicateProps
    {
        public int ID { get; set; }
        public string Title { get; set; }
    }

    public class Entity1DuplicateProps : BaseEntityDuplicateProps
    {
        public string SomeProperty { get; set; }
        public int Entity2ID { get; set; }
        public Entity2DuplicateProps Entity2 { get; set; }
    }

    public class Entity2DuplicateProps : BaseEntityDuplicateProps
    {
        public string SomeProperty { get; set; }
    }

    public class Base_195898
    {
        public int Id { get; set; }
        public Complex_195898 Complex { get; set; }
    }

    public class Derived_195898 : Base_195898
    {
    }

    public class Complex_195898
    {
        public string Foo { get; set; }
    }

    public abstract class BaseDependent_165027
    {
        public decimal? BaseProperty { get; set; }
        public float? Key1 { get; set; }
        public decimal? Key2 { get; set; }
    }

    public class Dependent_165027 : BaseDependent_165027
    {
    }

    #endregion

    public sealed class InheritanceScenarioTests : TestBase
    {
        [Fact]
        public void Orphaned_configured_table_should_throw()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<BaseDependent_165027>().HasKey(
                e => new
                         {
                             e.Key1,
                             e.Key2,
                         });
            modelBuilder.Entity<Dependent_165027>()
                .Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("Dependent");
                        });
            modelBuilder.Entity<BaseDependent_165027>()
                .Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("BaseDependent");
                        });

            Assert.Equal(
                Strings.OrphanedConfiguredTableDetected("BaseDependent"),
                Assert.Throws<InvalidOperationException>(
                    () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo)).Message);
        }

        [Fact]
        public void Orphaned_unconfigured_table_should_be_removed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<BaseDependent_165027>().HasKey(
                e => new
                         {
                             e.Key1,
                             e.Key2,
                         });
            modelBuilder.Entity<Dependent_165027>()
                .Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("Dependent");
                        });
            modelBuilder.Entity<BaseDependent_165027>()
                .Map(mapping => mapping.MapInheritedProperties());

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(1, databaseMapping.Database.Schemas.Single().Tables.Count());
        }

        [Fact]
        public void Should_be_able_configure_base_properties_via_derived_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Base_195898>().ToTable("Base");
            modelBuilder.Entity<Base_195898>().Property(b => b.Id).HasColumnName("base_c");
            modelBuilder.Entity<Base_195898>().Property(b => b.Complex.Foo).HasColumnName("base_foo");
            modelBuilder.Entity<Derived_195898>().ToTable("Derived");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Id).HasColumnName("derived_c");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Complex.Foo).HasColumnName("derived_foo");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            modelBuilder.Entity<Base_195898>().Property(b => b.Id).HasColumnName("base_c");
            modelBuilder.Entity<Base_195898>().Property(b => b.Complex.Foo).HasColumnName("base_foo");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Id).HasColumnName("derived_c");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Complex.Foo).HasColumnName("derived_foo");
        }

        [Fact]
        public void Should_be_able_configure_base_properties_via_derived_type_reverse()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Derived_195898>().ToTable("Derived");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Id).HasColumnName("derived_c");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Complex.Foo).HasColumnName("derived_foo");
            modelBuilder.Entity<Base_195898>().ToTable("Base");
            modelBuilder.Entity<Base_195898>().Property(b => b.Id).HasColumnName("base_c");
            modelBuilder.Entity<Base_195898>().Property(b => b.Complex.Foo).HasColumnName("base_foo");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            modelBuilder.Entity<Base_195898>().Property(b => b.Id).HasColumnName("base_c");
            modelBuilder.Entity<Base_195898>().Property(b => b.Complex.Foo).HasColumnName("base_foo");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Id).HasColumnName("derived_c");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Complex.Foo).HasColumnName("derived_foo");
        }

        [Fact]
        public void Should_be_able_configure_derived_property_and_base_property_is_not_configured()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Base_195898>().ToTable("Base");
            modelBuilder.Entity<Derived_195898>().ToTable("Derived");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Id).HasColumnName("derived_c");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Complex.Foo).HasColumnName("derived_foo");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            modelBuilder.Entity<Base_195898>().Property(b => b.Id).HasColumnName("Id");
            modelBuilder.Entity<Base_195898>().Property(b => b.Complex.Foo).HasColumnName("Foo");
            modelBuilder.Entity<Derived_195898>().Property(b => b.Id).HasColumnName("derived_c");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Complex.Foo).HasColumnName("derived_foo");
        }

        [Fact]
        public void Should_be_able_configure_base_property_and_derived_property_inherits_configuration()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Base_195898>().ToTable("Base");
            modelBuilder.Entity<Base_195898>().Property(d => d.Id).HasColumnName("base_c");
            modelBuilder.Entity<Derived_195898>().ToTable("Derived");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            modelBuilder.Entity<Base_195898>().Property(b => b.Id).HasColumnName("base_c");
            modelBuilder.Entity<Base_195898>().Property(b => b.Complex.Foo).HasColumnName("base_foo");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Id).HasColumnName("base_c");
            modelBuilder.Entity<Derived_195898>().Property(d => d.Complex.Foo).HasColumnName("base_foo");
        }

        [Fact]
        public void Columns_should_get_preferred_names_when_distinct_in_target_table()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<BaseEntityDuplicateProps>().ToTable("BaseEntities");
            modelBuilder.Entity<Entity1DuplicateProps>().ToTable("Entity1s");
            modelBuilder.Entity<Entity2DuplicateProps>().ToTable("Entity2s");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Entity1DuplicateProps>(e => e.SomeProperty).DbEqual("SomeProperty", c => c.Name);
            databaseMapping.Assert<Entity2DuplicateProps>(e => e.SomeProperty).DbEqual("SomeProperty", c => c.Name);
        }

        [Fact]
        public void Columns_should_get_configured_names_when_distinct_in_target_table()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<BaseEntityDuplicateProps>().ToTable("BaseEntities");
            modelBuilder.Entity<Entity1DuplicateProps>().ToTable("Entity1s");
            modelBuilder.Entity<Entity2DuplicateProps>().ToTable("Entity2s");
            modelBuilder.Entity<Entity1DuplicateProps>().Property(e => e.SomeProperty).HasColumnName("Foo");
            modelBuilder.Entity<Entity2DuplicateProps>().Property(e => e.SomeProperty).HasColumnName("Foo");
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Entity1DuplicateProps>(e => e.SomeProperty).DbEqual("Foo", c => c.Name);
            databaseMapping.Assert<Entity2DuplicateProps>(e => e.SomeProperty).DbEqual("Foo", c => c.Name);
        }

        [Fact]
        public void Build_model_for_simple_tpt()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<ColoredProduct>().ToTable("ColoredProducts");
            modelBuilder.Entity<StyledProduct>().ToTable("StyledProducts");

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_tpt_tph()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<DiscontinuedProduct>().ToTable("DiscontinuedProducts");
            modelBuilder.Entity<StyledProduct>()
                .Map(
                    m =>
                        {
                            m.Requires("disc").HasValue("S");
                            m.ToTable("StyledProducts");
                        });
            modelBuilder.Entity<ColoredProduct>()
                .Map(
                    m =>
                        {
                            m.Requires("disc").HasValue("C");
                            m.ToTable("StyledProducts");
                        });

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_split_tpt_tph()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<DiscontinuedProduct>();
            modelBuilder.Entity<StyledProduct>().ToTable("StyledProducts");
            modelBuilder.Entity<ColoredProduct>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_tpc_with_default_tph_in_part_of_tree()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<DiscontinuedProduct>();
            modelBuilder.Entity<StyledProduct>()
                .Map(
                    m =>
                        {
                            m.MapInheritedProperties();
                            m.ToTable("StyledProducts");
                        });

            Assert.Throws<NotSupportedException>(
                () => modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void Build_model_for_three_level_abstract_types_tpt()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<AbstractType1>().HasKey(a => a.Property1_ID).ToTable("AbstractType1");
            modelBuilder.Entity<AbstractType1_1>().ToTable("AbstractType1_1");
            modelBuilder.Entity<ConcreteType1_1_1>().ToTable("ConcreteType1_1_1");
            modelBuilder.Entity<ConcreteType1_2>().ToTable("ConcreteType1_2");

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_tree_containing_only_abstract_types()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<AbstractType1>().HasKey(a => a.Property1_ID);
            modelBuilder.Entity<AbstractType1_1>().ToTable("AbstractType1_1");
            modelBuilder.IgnoreAll();

            Assert.Throws<InvalidOperationException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void Build_model_for_entity_splitting()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Vendor>()
                .Map(
                    m =>
                        {
                            m.Properties(
                                v1 => new
                                          {
                                              v1.VendorID,
                                              v1.Name,
                                              v1.PreferredVendorStatus,
                                              v1.AccountNumber,
                                              v1.ActiveFlag,
                                              v1.CreditRating
                                          });
                            m.ToTable("Vendor");
                        })
                .Map(
                    m =>
                        {
                            m.Properties(
                                v2 => new
                                          {
                                              v2.VendorID,
                                              v2.ModifiedDate,
                                              v2.PurchasingWebServiceURL
                                          });
                            m.ToTable("VendorDetails");
                        });

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_entity_splitting_excluding_key()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Vendor>()
                .Map(
                    m =>
                        {
                            m.Properties(
                                v1 => new
                                          {
                                              v1.VendorID,
                                              v1.Name,
                                              v1.PreferredVendorStatus,
                                              v1.AccountNumber,
                                              v1.ActiveFlag,
                                              v1.CreditRating
                                          });
                            m.ToTable("Vendor");
                        })
                .Map(
                    m =>
                        {
                            m.Properties(
                                v2 => new
                                          {
                                              v2.ModifiedDate,
                                              v2.PurchasingWebServiceURL
                                          });
                            m.ToTable("VendorDetails");
                        });

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_entity_splitting_with_complex_properties()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ProductDescription>();
            modelBuilder.ComplexType<RowDetails>();

            modelBuilder.Entity<ProductDescription>()
                .Map(
                    m =>
                        {
                            m.Properties(
                                pd1 => new
                                           {
                                               pd1.ProductDescriptionID,
                                               pd1.RowDetails.rowguid
                                           });
                            m.ToTable("ProductDescription");
                        })
                .Map(
                    m =>
                        {
                            m.Properties(
                                pd2 => new
                                           {
                                               pd2.ProductDescriptionID,
                                               pd2.Description,
                                               pd2.RowDetails.ModifiedDate
                                           });
                            m.ToTable("ProductDescriptionExtended");
                        });

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Base_type_discovered_by_reachability_is_mapped()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<ITBar>();
            modelBuilder.Entity<ITBaz>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(2, databaseMapping.Model.Containers.Single().EntitySets.Count);
            Assert.Equal(
                "ITFoo",
                databaseMapping.Model.Containers.Single().EntitySets.Single(es => es.Name == "ITFoos").
                    ElementType.Name);
            Assert.Equal(3, databaseMapping.Model.Namespaces.Single().EntityTypes.Count);

            //Base type with 1 prop
            Assert.Equal(
                1,
                databaseMapping.Model.Namespaces.Single().EntityTypes.Single(et => et.Name == "ITFoo").
                    DeclaredProperties.Count);
            Assert.Equal(
                1,
                databaseMapping.Model.Namespaces.Single().EntityTypes.Single(et => et.Name == "ITFoo").
                    Properties.Count());

            //Derived type with 1 prop, 0 declared
            Assert.Equal(
                0,
                databaseMapping.Model.Namespaces.Single().EntityTypes.Single(et => et.Name == "ITBar").
                    DeclaredProperties.Count);
            Assert.Equal(
                1,
                databaseMapping.Model.Namespaces.Single().EntityTypes.Single(et => et.Name == "ITBar").
                    Properties.Count());
        }

        [Fact]
        public void Abstract_type_at_base_of_TPH_gets_IsTypeOf_mapping()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<A1>().Map<A2>(m => { m.Requires("disc").HasValue("A2"); }).Map<A3>(
                m => { m.Requires("disc").HasValue("A3"); }).Map<A4>(m => { m.Requires("disc").HasValue("A4"); }).
                ToTable("A1");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(1, databaseMapping.EntityContainerMappings[0].EntitySetMappings.Count);
            Assert.Equal(4, databaseMapping.EntityContainerMappings[0].EntitySetMappings[0].EntityTypeMappings.Count);
            Assert.True(
                databaseMapping.EntityContainerMappings[0].EntitySetMappings[0].EntityTypeMappings.Single(
                    x => x.EntityType.Name == "A1").IsHierarchyMapping);
            Assert.False(
                databaseMapping.EntityContainerMappings[0].EntitySetMappings[0].EntityTypeMappings.Single(
                    x => x.EntityType.Name == "A2").IsHierarchyMapping);
            Assert.False(
                databaseMapping.EntityContainerMappings[0].EntitySetMappings[0].EntityTypeMappings.Single(
                    x => x.EntityType.Name == "A3").IsHierarchyMapping);
            Assert.False(
                databaseMapping.EntityContainerMappings[0].EntitySetMappings[0].EntityTypeMappings.Single(
                    x => x.EntityType.Name == "A4").IsHierarchyMapping);
        }

        //[Fact]
        // Still fails, investigating issue
        public void Abstract_type_in_middle_of_TPH_gets_IsTypeOf_mapping()
        {
            var modelBuilder = new DbModelBuilder();
            //modelBuilder.Entity<B1>().Map<B2>(m =>
            //{
            //    m.Requires("disc").HasValue("B2");
            //}).Map<B3>(m =>
            //{
            //    m.Requires("disc").HasValue("B3");
            //}).ToTable("B1");
            modelBuilder.Entity<B1>();
            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(1, databaseMapping.EntityContainerMappings[0].EntitySetMappings.Count);
            Assert.Equal(3, databaseMapping.EntityContainerMappings[0].EntitySetMappings[0].EntityTypeMappings.Count);
            Assert.False(
                databaseMapping.EntityContainerMappings[0].EntitySetMappings[0].EntityTypeMappings.Single(
                    x => x.EntityType.Name == "B1").IsHierarchyMapping);
            Assert.True(
                databaseMapping.EntityContainerMappings[0].EntitySetMappings[0].EntityTypeMappings.Single(
                    x => x.EntityType.Name == "B2").IsHierarchyMapping);
            Assert.False(
                databaseMapping.EntityContainerMappings[0].EntitySetMappings[0].EntityTypeMappings.Single(
                    x => x.EntityType.Name == "B3").IsHierarchyMapping);
        }

        [Fact]
        public void Mapping_IA_FK_to_derived_type_puts_FK_in_correct_TPT_table()
        {
            var modelBuilder = new DbModelBuilder();

            // Map to TPT
            modelBuilder.Entity<ITOffice>();
            modelBuilder.Entity<ITEmployee>().ToTable("Employees");
            modelBuilder.Entity<ITOffSiteEmployee>().ToTable("OffSiteEmployees");
            modelBuilder.Entity<ITOnSiteEmployee>().ToTable("OnSiteEmployees");
            modelBuilder.Entity<ITOnSiteEmployee>()
                .HasRequired(e => e.ITOffice);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<ITEmployee>().DbEqual("Employees", t => t.DatabaseIdentifier);
            databaseMapping.Assert<ITOffSiteEmployee>().DbEqual("OffSiteEmployees", t => t.DatabaseIdentifier);
            databaseMapping.Assert<ITOnSiteEmployee>().DbEqual("OnSiteEmployees", t => t.DatabaseIdentifier);

            // IA FK was properly moved
            databaseMapping.Assert<ITEmployee>().HasNoForeignKeyColumn("ITOffice_ITOfficeId");
            databaseMapping.Assert<ITOnSiteEmployee>().HasForeignKeyColumn("ITOffice_ITOfficeId");

            // AssociationSet mapping updated properly
            Assert.Equal(
                "OnSiteEmployees",
                databaseMapping.EntityContainerMappings[0].AssociationSetMappings[0].Table.DatabaseIdentifier);
            Assert.Equal(
                "ITOffice_ITOfficeId",
                databaseMapping.EntityContainerMappings[0].AssociationSetMappings[0].SourceEndMapping.
                    PropertyMappings[0].Column.Name);
        }

        [Fact]
        public void Mapping_FK_to_derived_type_puts_FK_in_correct_TPT_table()
        {
            var modelBuilder = new DbModelBuilder();

            // Map to TPT
            modelBuilder.Entity<IT_Office>();
            modelBuilder.Entity<IT_Employee>().ToTable("Employees");
            modelBuilder.Entity<IT_OffSiteEmployee>().ToTable("OffSiteEmployees");
            modelBuilder.Entity<IT_OnSiteEmployee>().ToTable("OnSiteEmployees");
            modelBuilder.Entity<IT_OnSiteEmployee>()
                .HasRequired(e => e.IT_Office);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<IT_Employee>().DbEqual("Employees", t => t.DatabaseIdentifier);
            databaseMapping.Assert<IT_OffSiteEmployee>().DbEqual("OffSiteEmployees", t => t.DatabaseIdentifier);
            databaseMapping.Assert<IT_OnSiteEmployee>().DbEqual("OnSiteEmployees", t => t.DatabaseIdentifier);

            databaseMapping.Assert<IT_Employee>().HasNoForeignKeyColumn("IT_OfficeId");
            databaseMapping.Assert<IT_OnSiteEmployee>().HasForeignKeyColumn("IT_OfficeId");
            Assert.Equal(0, databaseMapping.EntityContainerMappings[0].AssociationSetMappings.Count);
        }

        [Fact]
        public void Mapping_association_to_subtype_by_convention_and_TPH_uses_correct_entity_sets()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<C1>();
            modelBuilder.Entity<D1>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal("C1", databaseMapping.Model.Containers[0].AssociationSets[0].SourceSet.Name);
            Assert.Equal("D1", databaseMapping.Model.Containers[0].AssociationSets[0].TargetSet.Name);
        }

        [Fact]
        public void Mapping_association_to_subtype_by_configuration_and_TPH_uses_correct_entity_sets()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<C1>().HasRequired(g => g.DiscontinueD1).WithOptional();
            modelBuilder.Entity<D1>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal("C1", databaseMapping.Model.Containers[0].AssociationSets[0].SourceSet.Name);
            Assert.Equal("D1", databaseMapping.Model.Containers[0].AssociationSets[0].TargetSet.Name);
        }

        [Fact]
        public void TPT_model_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<ColoredProduct>().ToTable("ColoredProducts");
            modelBuilder.Entity<StyledProduct>().ToTable("StyledProducts");

            SetDerivedEntityColumnNames(modelBuilder);

            ValidateTPTOrTPCWithRenamedColumns(modelBuilder);
        }

        [Fact]
        public void TPT_model_using_Map_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<ColoredProduct>().Map(m => m.ToTable("ColoredProducts"));
            modelBuilder.Entity<StyledProduct>().Map(m => m.ToTable("StyledProducts"));

            SetDerivedEntityColumnNames(modelBuilder);

            ValidateTPTOrTPCWithRenamedColumns(modelBuilder);
        }

        [Fact]
        public void TPT_model_with_HasColumnName_done_before_ToTable_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            SetDerivedEntityColumnNames(modelBuilder);

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<ColoredProduct>().ToTable("ColoredProducts");
            modelBuilder.Entity<StyledProduct>().ToTable("StyledProducts");

            ValidateTPTOrTPCWithRenamedColumns(modelBuilder);
        }

        [Fact]
        public void TPC_model_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("Products");
                    });
            modelBuilder.Entity<ColoredProduct>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("ColoredProducts");
                    });
            modelBuilder.Entity<StyledProduct>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("StyledProducts");
                    });

            SetDerivedEntityColumnNames(modelBuilder);

            ValidateTPTOrTPCWithRenamedColumns(modelBuilder);
        }

        [Fact]
        public void TPC_model_with_HasColumnName_done_before_ToTable_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            SetDerivedEntityColumnNames(modelBuilder);

            modelBuilder.Entity<Product>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("Products");
                    });
            modelBuilder.Entity<ColoredProduct>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("ColoredProducts");
                    });
            modelBuilder.Entity<StyledProduct>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("StyledProducts");
                    });

            ValidateTPTOrTPCWithRenamedColumns(modelBuilder);
        }

        private void SetDerivedEntityColumnNames(AdventureWorksModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().Property(p => p.ProductID).HasColumnName("base_product_id");
            modelBuilder.Entity<ColoredProduct>().Property(p => p.ProductID).HasColumnName("colored_product_id");
            modelBuilder.Entity<StyledProduct>().Property(p => p.ProductID).HasColumnName("styled_product_id");
        }

        private void ValidateTPTOrTPCWithRenamedColumns(AdventureWorksModelBuilder modelBuilder)
        {
            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);

            databaseMapping.Assert<Product>(p => p.ProductID).DbEqual("base_product_id", c => c.Name);
            databaseMapping.Assert<ColoredProduct>(p => p.ProductID).DbEqual("colored_product_id", c => c.Name);
            databaseMapping.Assert<StyledProduct>(p => p.ProductID).DbEqual("styled_product_id", c => c.Name);
        }

        [Fact]
        public void TPT_model_using_Table_attributes_can_map_PK_property_to_different_columns_in_different_tables()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<TPTHorse>().Property(e => e.Id).HasColumnName("horse_id");
            modelBuilder.Entity<TPTUnicorn>().Property(e => e.Id).HasColumnName("unicorn_id");
            modelBuilder.Entity<TPTHornedPegasus>().Property(e => e.Id).HasColumnName("pegasus_id");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);

            databaseMapping.Assert<TPTHorse>(p => p.Id).DbEqual("horse_id", c => c.Name);
            databaseMapping.Assert<TPTUnicorn>(p => p.Id).DbEqual("unicorn_id", c => c.Name);
            databaseMapping.Assert<TPTHornedPegasus>(p => p.Id).DbEqual("pegasus_id", c => c.Name);
        }

        [Fact]
        public void TPT_model_with_PK_property_to_different_columns_in_different_tables_roundtrips()
        {
            TPT_or_TPC_model_with_PK_property_to_different_columns_in_different_tables_roundtrips<ContextForPkNamingTPT>();
        }

        [Fact]
        public void TPC_model_with_PK_property_to_different_columns_in_different_tables_roundtrips()
        {
            TPT_or_TPC_model_with_PK_property_to_different_columns_in_different_tables_roundtrips<ContextForPkNamingTPC>();
        }

        private void TPT_or_TPC_model_with_PK_property_to_different_columns_in_different_tables_roundtrips<TContext>()
            where TContext : BaseContextForPkNaming, new()
        {
            using (var context = new TContext())
            {
                context.Database.Initialize(force: false);

                using (new TransactionScope())
                {
                    var baseEntity = context.Bases.Add(
                        new BaseForPKNaming
                            {
                                Id = 1,
                                Foo = "Foo1"
                            });
                    var derivedEntity =
                        context.Deriveds.Add(
                            new DerivedForPKNaming
                                {
                                    Id = 2,
                                    Foo = "Foo2",
                                    Bar = "Bar2"
                                });

                    context.SaveChanges();

                    context.Entry(baseEntity).State = EntityState.Detached;
                    context.Entry(derivedEntity).State = EntityState.Detached;

                    var foundBase = context.Bases.Where(e => e.Id == baseEntity.Id).Single();
                    var foundDerived = context.Deriveds.Where(e => e.Id == derivedEntity.Id).Single();

                    Assert.Equal("Foo1", foundBase.Foo);
                    Assert.Equal("Foo2", foundDerived.Foo);
                    Assert.Equal("Bar2", foundDerived.Bar);

                    Assert.True(context.Database.SqlQuery<int>("select base_id from base_table").Any());
                    Assert.True(context.Database.SqlQuery<int>("select derived_id from derived_table").Any());

                    if (typeof(TContext)
                        == typeof(ContextForPkNamingTPC))
                    {
                        Assert.True(context.Database.SqlQuery<string>("select base_foo from base_table").Any());
                        Assert.True(context.Database.SqlQuery<string>("select derived_foo from derived_table").Any());
                    }
                }
            }
        }
    }

    #region Bug DevDiv#223284

    namespace Bug223284
    {
        public class ITEmployee : FunctionalTests.ITEmployee
        {
            public ITOffice ITOffice { get; set; }
        }

        public class IT_Context : DbContext
        {
            public DbSet<ITEmployee> Employees { get; set; }
        }

        public sealed class Bug223284Test
        {
            [Fact]
            public void Duplicate_entity_name_different_namespace_should_throw()
            {
                var context = new IT_Context();

                Assert.Equal(
                    Strings.InvalidEntityType("FunctionalTests.Bug223284.ITEmployee"),
                    Assert.Throws<InvalidOperationException>(() => context.Employees.Add(new ITEmployee())).
                        Message);
            }
        }
    }

    #endregion

    #region Bug DevDiv#175804

    namespace Bug175804
    {
        public class Dependent
        {
            public int principalnavigationkey1 { get; set; }
            public int Id { get; set; }
            public Principal PrincipalNavigation { get; set; }
        }

        public class Principal : BasePrincipal
        {
        }

        public class DerivedDependent : Dependent
        {
            public decimal DependentDerivedProperty1 { get; set; }
        }

        public class BasePrincipal
        {
            public System.DateTime BaseProperty { get; set; }
            public int Key1 { get; set; }
        }

        public sealed class Bug175804Test
        {
            [Fact]
            public void TPC_Ordering_Of_Configuration_Between_Related_Types()
            {
                var builder = new DbModelBuilder();

                builder.Entity<Dependent>().HasRequired(e => e.PrincipalNavigation);
                builder.Entity<BasePrincipal>().HasKey(e => e.Key1);

                builder.Entity<DerivedDependent>().Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("DerivedDependent");
                        });
                builder.Entity<Principal>().Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("Principal");
                        });

                builder.Entity<Dependent>().Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("Dependent");
                        });
                builder.Entity<BasePrincipal>().Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("BasePrincipal");
                        });

                var databaseMapping = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;

                databaseMapping.AssertValid();
                //databaseMapping.ShellEdmx("Res3.xml");
                var derivedTypeMappings =
                    databaseMapping.EntityContainerMappings[0].EntitySetMappings.Where(
                        es => es.EntitySet.Name.Contains("Dependent")).First().EntityTypeMappings;

                Assert.Equal(
                    "Principal",
                    derivedTypeMappings[0].TypeMappingFragments[0].Table.ForeignKeyConstraints[0].
                        PrincipalTable.Name);
                Assert.Equal(
                    "Principal",
                    derivedTypeMappings[1].TypeMappingFragments[0].Table.ForeignKeyConstraints[0].
                        PrincipalTable.Name);
            }
        }
    }

    #endregion

    #region BugDevDiv#178590

    namespace BugDevDiv_178590
    {
        public abstract class A
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
        }

        public abstract class B : A
        {
            public virtual int? Y { get; set; }
        }

        public class C : B
        {
            public virtual int? Z { get; set; }
        }

        #region Subclasses have no extra properties

        public abstract class D
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
        }

        public class E : D
        {
        }

        public class F : E
        {
        }

        #endregion

        public sealed class Bug178590Test
        {
            [Fact]
            public void AbstractClasses_TPC()
            {
                var builder = new DbModelBuilder();

                // add .ToTable("B", "dbo") as workaround
                builder.Entity<A>();
                builder
                    .Entity<B>()
                    .Map(mapping => mapping.MapInheritedProperties())
                    ;

                builder
                    .Entity<C>()
                    .Map(mapping => mapping.MapInheritedProperties())
                    .ToTable("C", "dbo")
                    ;

                var databaseMapping = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;
                databaseMapping.AssertValid();

                var typeMappings = databaseMapping.EntityContainerMappings[0].EntitySetMappings[0].EntityTypeMappings;

                Assert.Equal(1, typeMappings.Count);
                Assert.Equal("C", typeMappings[0].EntityType.Name);
                Assert.Equal(1, typeMappings[0].TypeMappingFragments.Count);
                Assert.Equal(4, typeMappings[0].TypeMappingFragments[0].PropertyMappings.Count);
            }

            [Fact]
            public void AbstractClasses_TPT()
            {
                var builder = new DbModelBuilder();

                builder.Entity<A>();
                builder
                    .Entity<B>()
                    .Map(mapping => mapping.MapInheritedProperties());

                builder
                    .Entity<C>()
                    .ToTable("C", "dbo");

                var databaseMapping = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;
                databaseMapping.AssertValid();
            }

            [Fact]
            public void SubClasses_NoProperties()
            {
                var builder = new DbModelBuilder();

                builder.Entity<D>();
                builder.Entity<E>();
                builder.Entity<F>();

                var databaseMapping = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;
                databaseMapping.AssertValid();
            }
        }
    }

    #endregion

    #region Bug165027

    namespace Bug165027
    {
        public class Dependent1
        {
            public System.DateTimeOffset key1 { get; set; }
            public Principal1 PrincipalNavigation { get; set; }
        }

        public abstract class Principal1 : BasePrincipal1
        {
            public Dependent1 DependentNavigation { get; set; }
        }

        public class BasePrincipal1
        {
            public short BaseProperty { get; set; }
            public DateTimeOffset? Key1 { get; set; }
        }

        public class DerivedPrincipal1 : Principal1
        {
            public decimal PrincipalDerivedProperty1 { get; set; }
        }

        public sealed class Bug165027Repro
        {
            [Fact]
            public void Bug165027Test()
            {
                var builder = new DbModelBuilder();

                builder.Entity<BasePrincipal1>().ToTable("BasePrincipal");
                builder.Entity<BasePrincipal1>().HasKey(e => e.Key1);
                builder.Entity<Principal1>().HasOptional(e => e.DependentNavigation).WithRequired(
                    e => e.PrincipalNavigation);

                builder.Entity<DerivedPrincipal1>().Map(
                    mapping =>
                        {
                            mapping.MapInheritedProperties();
                            mapping.ToTable("DerivedPrincipal");
                        });

                builder.Entity<Dependent1>().HasKey(e => e.key1);

                var databaseMapping = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;
                databaseMapping.AssertValid();
            }
        }
    }

    #endregion

    #region Bug178568

    namespace Bug178568
    {
        public abstract class A
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
        }

        public class B : A
        {
            public virtual int? Y { get; set; }
        }

        public class C
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
            public virtual int? Y { get; set; }
        }

        public sealed class Bug178568Repro : TestBase
        {
            [Fact]
            public void TPC_Identity_ShouldPropagate()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<A>()
                    .HasKey(a => a.Id);

                modelBuilder
                    .Entity<A>()
                    .Property(a => a.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

                modelBuilder
                    .Entity<B>()
                    .Map(mapping => mapping.MapInheritedProperties())
                    .ToTable("B", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("TPC_Identity_ShouldPropagate.xml");
                databaseMapping.AssertValid();

                databaseMapping.Assert<B>("B")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.Identity,
                        c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void TPC_IdentityNotExplicit_ShouldNotPropagate()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<A>();

                modelBuilder
                    .Entity<B>()
                    .Map(mapping => mapping.MapInheritedProperties())
                    .ToTable("B", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("TPC_IdentityNotExplicit_ShouldNotPropagate.xml");
                databaseMapping.AssertValid();

                databaseMapping.Assert<B>("B")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.None,
                        c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void TPT_Identity_ShouldNotPropagate()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<A>()
                    .HasKey(a => a.Id);

                modelBuilder
                    .Entity<A>()
                    .Property(a => a.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

                modelBuilder
                    .Entity<B>()
                    .ToTable("B", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("TPT_Identity_ShouldNotPropagate.xml");
                databaseMapping.AssertValid();

                databaseMapping.Assert<A>("A")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.Identity,
                        c => c.StoreGeneratedPattern);
                databaseMapping.Assert<B>("B")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.None,
                        c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void EntitySplitting_Identity_ShouldNotPropagate()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<C>()
                    .HasKey(a => a.Id);

                modelBuilder
                    .Entity<C>()
                    .Property(a => a.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

                modelBuilder.Entity<C>()
                    .Map(
                        m =>
                            {
                                m.Properties(
                                    c => new
                                             {
                                                 c.Id,
                                                 c.X
                                             });
                                m.ToTable("CX");
                            })
                    .Map(
                        m =>
                            {
                                m.Properties(
                                    c => new
                                             {
                                                 c.Id,
                                                 c.Y
                                             });
                                m.ToTable("CY");
                            });

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("EntitySplitting_Identity_ShouldNotPropagate.xml");
                databaseMapping.AssertValid();

                databaseMapping.Assert<C>("CX")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.Identity,
                        c => c.StoreGeneratedPattern);
                databaseMapping.Assert<C>("CY")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.None,
                        c => c.StoreGeneratedPattern);
            }
        }
    }

    #endregion

    #region Bug336566

    namespace Bug336566
    {
        public class A
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
        }

        public class B : A
        {
            public virtual int? Y { get; set; }
        }

        public class C
        {
            public virtual int Id { get; set; }
            public virtual int? X { get; set; }
            public virtual int? Y { get; set; }
        }

        public sealed class Bug336566Repro : TestBase
        {
            [Fact]
            public void TPC_IdentityNotExplicit_ShouldNotPropagate()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<A>();

                modelBuilder
                    .Entity<B>()
                    .Map(mapping => mapping.MapInheritedProperties())
                    .ToTable("B", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("Bug336566Repro.xml");
                databaseMapping.AssertValid();

                databaseMapping.Assert<A>("A")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.None,
                        c => c.StoreGeneratedPattern);

                databaseMapping.Assert<B>("B")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.None,
                        c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void NoIdentityExplicit()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<A>()
                    .Property(a => a.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("NoIdentityExplicit.xml");
                databaseMapping.AssertValid();

                databaseMapping.Assert<A>("A")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.None,
                        c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void TPT_Identity_ShouldKickIn()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<A>();

                modelBuilder
                    .Entity<B>()
                    .ToTable("B", "dbo");

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("TPT_Identity_ShouldKickIn.xml");
                databaseMapping.AssertValid();

                databaseMapping.Assert<A>("A")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.Identity,
                        c => c.StoreGeneratedPattern);
                databaseMapping.Assert<B>("B")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.None,
                        c => c.StoreGeneratedPattern);
            }

            [Fact]
            public void EntitySplitting_Identity_ShouldKickIn()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder
                    .Entity<C>()
                    .HasKey(a => a.Id);

                modelBuilder.Entity<C>()
                    .Map(
                        m =>
                            {
                                m.Properties(
                                    c => new
                                             {
                                                 c.Id,
                                                 c.X
                                             });
                                m.ToTable("CX");
                            })
                    .Map(
                        m =>
                            {
                                m.Properties(
                                    c => new
                                             {
                                                 c.Id,
                                                 c.Y
                                             });
                                m.ToTable("CY");
                            });

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("EntitySplitting_Identity_ShouldNotPropagate.xml");
                databaseMapping.AssertValid();

                databaseMapping.Assert<C>("CX")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.Identity,
                        c => c.StoreGeneratedPattern);
                databaseMapping.Assert<C>("CY")
                    .Column("Id").DbEqual(
                        System.Data.Entity.Edm.Db.DbStoreGeneratedPattern.None,
                        c => c.StoreGeneratedPattern);
            }
        }
    }

    #endregion

    #region Contexts for TPT/TPC with different PK column names

    public abstract class BaseContextForPkNaming : DbContext
    {
        public DbSet<BaseForPKNaming> Bases { get; set; }
        public DbSet<DerivedForPKNaming> Deriveds { get; set; }
    }

    public class ContextForPkNamingTPC : BaseContextForPkNaming
    {
        public ContextForPkNamingTPC()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ContextForPkNamingTPC>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseForPKNaming>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("base_table");
                    });
            modelBuilder.Entity<DerivedForPKNaming>().Map(
                m =>
                    {
                        m.MapInheritedProperties();
                        m.ToTable("derived_table");
                    });

            modelBuilder.Entity<BaseForPKNaming>().Property(e => e.Id).HasColumnName("base_id");
            modelBuilder.Entity<DerivedForPKNaming>().Property(e => e.Id).HasColumnName("derived_id");
            modelBuilder.Entity<BaseForPKNaming>().Property(e => e.Foo).HasColumnName("base_foo");
            modelBuilder.Entity<DerivedForPKNaming>().Property(e => e.Foo).HasColumnName("derived_foo");
        }
    }

    public class ContextForPkNamingTPT : BaseContextForPkNaming
    {
        public ContextForPkNamingTPT()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ContextForPkNamingTPT>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseForPKNaming>().ToTable("base_table");
            modelBuilder.Entity<DerivedForPKNaming>().ToTable("derived_table");

            modelBuilder.Entity<BaseForPKNaming>().Property(e => e.Id).HasColumnName("base_id");
            modelBuilder.Entity<DerivedForPKNaming>().Property(e => e.Id).HasColumnName("derived_id");
        }
    }

    public class BaseForPKNaming
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Foo { get; set; }
    }

    public class DerivedForPKNaming : BaseForPKNaming
    {
        public string Bar { get; set; }
    }

    [Table("Horses")]
    public class TPTHorse
    {
        public int Id { get; set; }
    }

    [Table("Unicorns")]
    public class TPTUnicorn : TPTHorse
    {
        public int HornLength { get; set; }
    }

    [Table("HornedPegasuses")]
    public class TPTHornedPegasus : TPTUnicorn
    {
        public int Wingspan { get; set; }
    }

    #endregion

    #region Bug335965

    namespace Bug335965
    {
        public class A
        {
            public int Id { get; set; }
            public string X { get; set; }
        }

        public abstract class B : A
        {
            public string Y { get; set; }
        }

        public class C : B
        {
            public string Z { get; set; }
        }

        public class A2
        {
            public int Id { get; set; }
            public string X { get; set; }
        }

        public abstract class B2 : A2
        {
            public string Y { get; set; }
        }

        public class C2 : B2
        {
            public string Z { get; set; }
        }

        public class D2 : B2
        {
            public string W { get; set; }
        }

        public sealed class Bug335965Repro : TestBase
        {
            [Fact]
            public void ExplicitDiscriminatorShouldNotBeNullable()
            {
                var modelBuilder = new DbModelBuilder();

                // Adding this configuration makes the discriminator nullable.
                modelBuilder.Entity<B>();

                modelBuilder.Entity<A>().Map(
                    m =>
                        {
                            // Adding IsRequired() does not help.
                            m.Requires("Disc").HasValue(17);
                        });
                modelBuilder.Entity<C>().Map(m => { m.Requires("Disc").HasValue(7); });

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("DiscriminatorShouldNotBeNull.xml");
                databaseMapping.AssertValid();

                databaseMapping.Assert<A>("A")
                    .Column("Disc").DbEqual(false, c => c.IsNullable);
            }

            [Fact]
            public void InvalidMappingSubtypeHasNoDiscriminatorCondition()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<A2>().Map(m => { m.Requires("Disc").HasValue(17); });
                modelBuilder.Entity<C2>().Map(m => { m.Requires("Disc").HasValue(7); });

                modelBuilder.Entity<B2>();
                modelBuilder.Entity<D2>();

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("ExplicitDiscriminatorShouldNotBeNull.xml");
                Assert.Throws<MappingException>(() => databaseMapping.AssertValid(true));
            }

            [Fact]
            public void ImplicitDiscriminatorShouldNotBeNullable()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<A>();
                modelBuilder.Entity<C>();
                modelBuilder.Entity<B>();

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("ImplicitDiscriminatorShouldNotBeNull.xml");
                databaseMapping.AssertValid();

                databaseMapping.Assert<A>("A")
                    .Column("Discriminator").DbEqual(false, c => c.IsNullable);
            }
        }
    }

    #endregion

    #region Bug339467

    namespace Bug339467
    {
        public class A
        {
            public int Id { set; get; }
            public string Name { set; get; }
        }

        public abstract class B : A
        {
        }

        public class C : B
        {
            public string CName { set; get; }
        }

        public sealed class Bug339467Repro : TestBase
        {
            [Fact]
            public void SimpleTPTWithAbstractWithNoPropertiesInBetween()
            {
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<A>();
                modelBuilder.Entity<C>().ToTable("C");

                var databaseMapping = BuildMapping(modelBuilder);
                //databaseMapping.ShellEdmx("SimpleTest.xml");
                databaseMapping.AssertValid();
            }
        }
    }

    #endregion

    #region Bug336706

    namespace Bug336706
    {
        public class Dependent : BaseDependent
        {
            public int PrincipalNavigationId { get; set; }
            public BaseDependent PrincipalNavigation { get; set; }
        }

        public abstract class BaseDependent
        {
            public int Id { get; set; }
            public ICollection<Dependent> DependentNavigation { get; set; }

            public BaseDependent()
            {
                DependentNavigation = new List<Dependent>();
            }
        }

        public sealed class Bug336706Repro : TestBase
        {
            [Fact]
            public void TPH_with_self_ref_FK_on_derived_type_has_non_nullable_FK_when_base_type_is_abstract()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<Dependent>().HasRequired(e => e.PrincipalNavigation).WithMany(
                    e => e.DependentNavigation)
                    .WillCascadeOnDelete(false);
                modelBuilder.Entity<Dependent>().Map(mapping => { mapping.Requires("DiscriminatorValue").HasValue(1); });

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.Assert<Dependent>("BaseDependents")
                    .HasForeignKeyColumn("PrincipalNavigationId")
                    .DbEqual(false, t => t.Columns.Single(c => c.Name == "PrincipalNavigationId").IsNullable);

                //databaseMapping.ShellEdmx("TPH_with_self_ref_FK_on_derived_type_has_non_nullable_FK_when_base_type_is_abstract.xml");
                databaseMapping.AssertValid();
            }
        }
    }

    #endregion
}
