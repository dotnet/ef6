// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using FunctionalTests.Model;
    using Xunit;

    public sealed class AssociationScenarioTests : TestBase
    {
        [Fact]
        public void FK_attribute_with_inverse_property_should_create_fk_association()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Dependent_172949>();
            modelBuilder.Entity<Principal_172949>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<Dependent_172949>().HasForeignKey(
                new[] { "DependentForeignKeyPropertyNotFromConvention1" }, "Principal_172949");
        }

        [Fact]
        public void Required_on_dependent_nav_prop_with_foreign_key_attribute_on_fk()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Dependent_159001a>().HasKey(e => e.DependentForeignKeyPropertyNotFromConvention1);
            modelBuilder.Entity<Principal_159001>().HasKey(e => e.Key1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var associationType = databaseMapping.Model.Namespaces.Single().AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, associationType.TargetEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Required_on_dependent_nav_prop_with_foreign_key_attribute_on_nav()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Dependent_159001b>().HasKey(e => e.DependentForeignKeyPropertyNotFromConvention1);
            modelBuilder.Entity<Principal_159001>().HasKey(e => e.Key1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var associationType = databaseMapping.Model.Namespaces.Single().AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, associationType.TargetEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Required_on_principal_and_dependent_nav_prop_with_foreign_key_attribute_on_fk()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DependentWithNav_159001a>().HasKey(e => e.DependentForeignKeyPropertyNotFromConvention1);
            modelBuilder.Entity<PrincipalWithNav_159001a>().HasKey(e => e.Key1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var associationType = databaseMapping.Model.Namespaces.Single().AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.One, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, associationType.TargetEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Should_not_detect_one_to_one_fk_that_is_not_the_dependent_pk()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Principal_181909>();
            modelBuilder.Entity<Dependent_181909>().HasKey(
                l => new
                         {
                             l.Key
                         });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Dependent_181909>().HasForeignKey(new[] { "Order_Id" }, "Principal_181909");
        }

        [Fact]
        public void Can_detect_overlapping_key_by_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order_181909>().HasKey(
                o => new
                         {
                             o.OrderId,
                             o.CustomerId
                         });
            modelBuilder.Entity<OrderLine_181909>().HasKey(
                l => new
                         {
                             l.OrderLineId,
                             l.CustomerId
                         });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<OrderLine_181909>().HasForeignKey(new[] { "OrderId", "CustomerId" }, "Order_181909");
        }

        [Fact]
        public void Can_detect_overlapping_key_by_convention_when_pk_covers_fk()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order_181909>().HasKey(
                o => new
                         {
                             o.OrderId,
                             o.CustomerId
                         });
            modelBuilder.Entity<OrderLine_181909>().HasKey(
                l => new
                         {
                             l.OrderLineId,
                             l.CustomerId,
                             l.OrderId
                         });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<OrderLine_181909>().HasForeignKey(new[] { "OrderId", "CustomerId" }, "Order_181909");
        }

        [Fact]
        public void Can_detect_overlapping_key_by_convention_when_fk_covers_pk()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order_181909>().HasKey(
                o => new
                         {
                             o.OrderId,
                             o.CustomerId
                         });
            modelBuilder.Entity<OrderLine_181909>().HasKey(
                l => new
                         {
                             l.OrderId
                         });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<OrderLine_181909>().HasForeignKey(new[] { "OrderId", "CustomerId" }, "Order_181909");
        }

        [Fact]
        public void Identifying_overlapping_key_is_not_discovered_by_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order_181909>().HasKey(
                o => new
                         {
                             o.OrderId,
                             o.CustomerId
                         });
            modelBuilder.Entity<OrderLine_181909>().HasKey(
                l => new
                         {
                             l.CustomerId,
                             l.OrderId
                         });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<OrderLine_181909>().HasForeignKey(
                new[] { "Order_OrderId", "Order_CustomerId" },
                "Order_181909");
        }

        [Fact]
        public void Identifying_overlapping_key_is_not_discovered_by_convention_reverse_pk_ordering()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Order_181909>().HasKey(
                o => new
                         {
                             o.OrderId,
                             o.CustomerId
                         });
            modelBuilder.Entity<OrderLine_181909>().HasKey(
                l => new
                         {
                             l.OrderId,
                             l.CustomerId
                         });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<OrderLine_181909>().HasForeignKey(
                new[] { "Order_OrderId", "Order_CustomerId" },
                "Order_181909");
        }

        [Fact]
        public void Can_map_ia_to_other_table_when_entity_splitting()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Dependent_162348>()
                .Map(
                    mapping =>
                        {
                            mapping.Properties(e => e.Key1);
                            mapping.ToTable("DependentSplit");
                        });
            modelBuilder.Entity<Dependent_162348>()
                .Map(
                    mapping =>
                        {
                            mapping.Properties(e => e.PrincipalNavigationKey1);
                            mapping.ToTable("Dependent_162348");
                        });
            modelBuilder.Entity<Dependent_162348>()
                .HasOptional(e => e.PrincipalNavigation).WithMany().Map(m => m.ToTable("Dependent_162348"));

            modelBuilder.Entity<Dependent_162348>().HasKey(e => e.Key1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Configured_annotated_nullable_fk_should_be_non_nullable_when_association_end_required()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Dependent_6927>().HasRequired(e => e.PrincipalNavigation).WithMany(
                e => e.DependentNavigation);
            modelBuilder.Entity<Principal_6927>().HasKey(e => e.Key1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Dependent_6927>(d => d.DependentForeignKeyPropertyNotFromConvention1).IsFalse(f => f.Nullable);
        }

        [Fact]
        public void Configured_required_end_should_result_in_required_dependent_keys_when_configured_identifying()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PrincipalWithNullableFkIdentifying>()
                .HasOptional(p => p.DependentNavigation)
                .WithRequired(d => d.PrincipalNavigation);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Configured_required_end_should_result_in_required_dependent_keys_by_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PrincipalWithNullableFk>()
                .HasMany(p => p.DependentNavigation)
                .WithRequired(d => d.PrincipalNavigation);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Configured_required_end_should_result_in_required_dependent_keys_when_configured()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PrincipalWithNullableFk>()
                .HasMany(p => p.DependentNavigation)
                .WithRequired(d => d.PrincipalNavigation)
                .HasForeignKey(d => d.PrincipalNavigationId);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Duplicate_table_name_resolution_when_many_to_many_mapping_should_not_uniquify_distinct()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SupplierManyToManyTableNaming>()
                .HasMany(e => e.Products)
                .WithMany(e => e.Suppliers)
                .Map(m => m.ToTable("SupplierManyToManyTableNaming"));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<SupplierManyToManyTableNaming>("SupplierManyToManyTableNamings");
            databaseMapping.Assert("SupplierManyToManyTableNaming");
        }

        [Fact]
        public void Duplicate_table_name_resolution_when_many_to_many_mapping_should_uniquify_collision()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SupplierManyToManyTableNaming>()
                .HasMany(e => e.Products)
                .WithMany(e => e.Suppliers)
                .Map(m => m.ToTable("SupplierManyToManyTableNamings"));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<SupplierManyToManyTableNaming>("SupplierManyToManyTableNamings1");
            databaseMapping.Assert("SupplierManyToManyTableNamings");
        }

        [Fact]
        public void Duplicate_table_name_resolution_when_many_to_many_mapping_should_throw_when_duplicate_configured()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SupplierManyToManyTableNaming>()
                .ToTable("Foo");
            modelBuilder.Entity<SupplierManyToManyTableNaming>()
                .HasMany(e => e.Products)
                .WithMany(e => e.Suppliers)
                .Map(m => m.ToTable("Foo"));

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Throws<MetadataException>(() => databaseMapping.AssertValid());
        }

        [Fact]
        public void Configure_partial_api_plus_annotation_optional_to_required_should_have_principal()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Principal144934>().HasKey(e => e.Key1);
            modelBuilder.Entity<Dependent144934>().HasKey(e => e.PrincipalNavigationKey1);
            modelBuilder.Entity<Principal144934>().HasOptional(e => e.DependentNavigation);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Dependent144934>().HasForeignKeyColumn("PrincipalNavigationKey1");
        }

        [Fact]
        public void Should_be_able_to_mix_convention_and_configuration_when_multiple_associations()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Dependent144843>().HasRequired(d => d.Principal1).WithMany(p => p.Dependents1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            Assert.Equal(2, databaseMapping.Model.Namespaces.Single().AssociationTypes.Count);
        }

        [Fact]
        public void Configure_store_facets_for_many_to_many_self_ref()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DependentManyToManySelf>().HasKey(e => e.Key1);
            modelBuilder.Entity<DerivedDependentManyToManySelf>().Property(p => p.DerivedProperty1).HasColumnType("real");
            modelBuilder.Entity<DependentManyToManySelf>().Property(p => p.Key1).HasColumnType("numeric").HasPrecision(
                15, 5);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<DependentManyToManySelf>(d => d.Key1).DbEqual("numeric", c => c.TypeName);
        }

        [Fact]
        public void One_to_one_split_required_inverse_annotations_can_determine_principal()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DependentSelfRefInverseRequired>().HasKey(
                e => new
                         {
                             e.Key1,
                             e.Key2
                         });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Should_be_able_to_determine_column_order_when_annotations_on_abstract_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DerivedDependentKeyOrder>();
            modelBuilder.Entity<DependentAbstractKeyOrder>();
            modelBuilder.Entity<BasePrincipalAbstractKeyOrder>()
                .HasKey(
                    e => new
                             {
                                 e.Key1,
                                 e.Key2,
                             });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void One_to_one_self_ref_with_inverse_annotation()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DependentSelfRefInverse>().HasKey(e => e.Key1);
            modelBuilder.Entity<DependentSelfRefInverse>().Property(p => p.DependentSelfRefInverseKey1).HasColumnType(
                "smallint");
            modelBuilder.Entity<DerivedDependentSelfRefInverse>().Property(p => p.DerivedProperty1).HasColumnType(
                "nvarchar(max)").IsUnicode();
            modelBuilder.Entity<DependentSelfRefInverse>().Property(p => p.Key1).HasColumnType("smallint");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Foreign_key_and_inverse_on_derived_abstract_class()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PrincipalFkAbstract>().HasKey(p => p.Key1);
            modelBuilder.Entity<BaseDependentFkAbstract>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void One_to_one_self_ref_with_foreign_key_on_key_properties()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DependentSelfRef>()
                .HasKey(
                    d => new
                             {
                                 d.Key1,
                                 d.DependentForeignKeyPropertyNotFromConvention1
                             });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<DependentSelfRef>()
                .HasNoForeignKeyColumns();
        }

        [Fact]
        public void Test_fk_wierd_ordering()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PrincipalWeirdKeyOrder>()
                .HasKey(
                    d => new
                             {
                                 d.Id1,
                                 d.Id2
                             });

            modelBuilder.Entity<DependentWeirdKeyOrder>()
                .HasKey(
                    d => new
                             {
                                 d.Fk1,
                                 d.Fk2
                             })
                .HasRequired(d => d.PrincipalNavigation)
                .WithOptional();

            modelBuilder.Entity<DependentWeirdKeyOrder2>()
                .HasKey(
                    d => new
                             {
                                 d.Fk1,
                                 d.Fk2
                             })
                .HasRequired(d => d.PrincipalNavigation)
                .WithOptional();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<DependentWeirdKeyOrder>()
                .HasForeignKey(new[] { "Fk2", "Fk1" }, "PrincipalWeirdKeyOrders");
            databaseMapping.Assert<DependentWeirdKeyOrder2>()
                .HasForeignKey(new[] { "Fk1", "Fk2" }, "PrincipalWeirdKeyOrders");
        }

        [Fact]
        public void Foreign_key_annotation_on_pk_should_change_principal_end_kind_to_required_with_required_annotation_unidirectional()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DependentNoPrincipalNavRequired>().HasKey(
                d => d.DependentForeignKeyPropertyNotFromConvention1);
            modelBuilder.Entity<PrincipalNoPrincipalNav>().HasKey(d => d.Key1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var associationType = databaseMapping.Model.Namespaces.Single().AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, associationType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<DependentNoPrincipalNavRequired>()
                .HasForeignKey(new[] { "DependentForeignKeyPropertyNotFromConvention1" }, "PrincipalNoPrincipalNavs");
        }

        [Fact]
        public void Foreign_key_annotation_on_pk_should_change_principal_end_kind_required_to_optional_unidirectional()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DependentNoPrincipalNavOptional>().HasKey(
                d => d.DependentForeignKeyPropertyNotFromConvention1);
            modelBuilder.Entity<PrincipalNoPrincipalNav>().HasKey(d => d.Key1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var associationType = databaseMapping.Model.Namespaces.Single().AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, associationType.TargetEnd.RelationshipMultiplicity);

            databaseMapping.Assert<DependentNoPrincipalNavOptional>()
                .HasForeignKey(new[] { "DependentForeignKeyPropertyNotFromConvention1" }, "PrincipalNoPrincipalNavs");
        }

        [Fact]
        public void Foreign_key_annotation_on_pk_should_change_principal_end_kind_required_to_optional_bidirectional()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DependentPrincipalNavOptional>()
                .HasKey(
                    d =>
                    new
                        {
                            d.DependentForeignKeyPropertyNotFromConvention2,
                            d.DependentForeignKeyPropertyNotFromConvention1
                        });
            modelBuilder.Entity<PrincipalPrincipalNavOptional>().HasKey(
                d => new
                         {
                             d.Key2,
                             d.Key1
                         });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var associationType = databaseMapping.Model.Namespaces.Single().AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, associationType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<DependentPrincipalNavOptional>()
                .HasForeignKey(
                    new[]
                        {
                            "DependentForeignKeyPropertyNotFromConvention2",
                            "DependentForeignKeyPropertyNotFromConvention1"
                        },
                    "PrincipalPrincipalNavOptionals");
        }

        [Fact]
        public void Foreign_key_annotation_on_pk_should_change_principal_end_kind_required_to_required_bidirectional()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DependentPrincipalNavRequired>().HasKey(
                d => d.DependentForeignKeyPropertyNotFromConvention1);
            modelBuilder.Entity<PrincipalPrincipalNavRequired>().HasKey(d => d.Key1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var associationType = databaseMapping.Model.Namespaces.Single().AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, associationType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<DependentPrincipalNavRequired>()
                .HasForeignKey(
                    new[] { "DependentForeignKeyPropertyNotFromConvention1" },
                    "PrincipalPrincipalNavRequireds");
        }

        [Fact]
        public void Foreign_key_annotation_on_pk_should_change_principal_end_kind_required_to_required_bidirectional_required_dependentn()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DependentPrincipalNavRequiredDependent>().HasKey(
                d => d.DependentForeignKeyPropertyNotFromConvention1);
            modelBuilder.Entity<PrincipalPrincipalNavRequiredDependent>().HasKey(d => d.Key1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var associationType = databaseMapping.Model.Namespaces.Single().AssociationTypes.Single();

            Assert.Equal(RelationshipMultiplicity.One, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, associationType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<DependentPrincipalNavRequiredDependent>()
                .HasForeignKey(
                    new[] { "DependentForeignKeyPropertyNotFromConvention1" },
                    "PrincipalPrincipalNavRequiredDependents");
        }

        [Fact]
        public void One_to_one_byte_key_inverse_and_fk_annotations()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PrincipalByteKey>().HasKey(p => p.Key1);
            modelBuilder.Entity<DependentByteKey>().HasKey(d => d.DependentForeignKeyPropertyNotFromConvention1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Self_ref_inherited_should_not_cascade_on_delete()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SelfRefInheritedBase>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(
                OperationAction.None,
                databaseMapping.Model.Namespaces.Single().AssociationTypes.Single().SourceEnd.DeleteBehavior);

            Assert.Equal(
                OperationAction.None,
                databaseMapping.Model.Namespaces.Single().AssociationTypes.Single().TargetEnd.DeleteBehavior);
        }

        [Fact]
        public void Annotated_fk_composite_can_use_column_or_api_for_fk_ordering()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PrincipalWithCompositeAnnotatedDependent>().HasKey(
                p => new
                         {
                             p.Id2,
                             p.Id1
                         });
            modelBuilder.Entity<CompositePartiallyAnnotatedDependent>()
                .Property(c => c.TheFk1).HasColumnOrder(2);
            modelBuilder.Entity<CompositePartiallyAnnotatedDependent>()
                .Property(c => c.TheFk2).HasColumnOrder(1);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var fkConstraint
                = databaseMapping.Database.GetEntityTypes()
                    .Single(t => t.Name == "CompositeAnnotatedDependent")
                    .ForeignKeyBuilders
                    .Single();

            Assert.True(new[] { "TheFk2", "TheFk1" }.SequenceEqual(fkConstraint.DependentColumns.Select(c => c.Name)));

            fkConstraint
                = databaseMapping.Database.GetEntityTypes()
                    .Single(t => t.Name == "CompositePartiallyAnnotatedDependent")
                    .ForeignKeyBuilders
                    .Single();

            Assert.True(new[] { "TheFk2", "TheFk1" }.SequenceEqual(fkConstraint.DependentColumns.Select(c => c.Name)));
        }

        [Fact]
        public void Annotated_fk_composite_should_throw_when_no_ordering_defined()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PrincipalWithCompositeAnnotatedDependent>().HasKey(
                p => new
                         {
                             p.Id2,
                             p.Id1
                         });
            modelBuilder.Entity<CompositePartiallyAnnotatedDependent>();

            Assert.Throws<InvalidOperationException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ForeignKeyAttributeConvention_OrderRequired", typeof(CompositePartiallyAnnotatedDependent));
        }

        [Fact]
        public void Annotated_fk_one_to_one_should_use_annotation_to_determine_principal()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PrincipalWithAnnotatedDependent>().HasKey(p => p.AnId);
            modelBuilder.Entity<AnnotatedDependent>().HasKey(ad => ad.AnotherId);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<AnnotatedDependent>().HasForeignKeyColumn("AnotherId");
        }

        [Fact]
        public void Annotated_fk_should_throw_when_invalid_nav_prop_specified()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<AnnotatedDependentWrong>();

            Assert.Throws<InvalidOperationException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage(
                    "ForeignKeyAttributeConvention_InvalidNavigationProperty",
                    "Id", typeof(AnnotatedDependentWrong),
                    "Wrong");
        }

        [Fact]
        public void Generated_many_to_many_fk_columns_should_have_correct_store_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ProductA>().Property(p => p.Id).HasColumnType("bigint");
            modelBuilder.Entity<Tag>().Property(t => t.Id).HasColumnType("bigint");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var table = databaseMapping.Database.GetEntityTypes().Single(t => t.Name == "TagProductA");

            Assert.Equal("bigint", table.Properties.Single(c => c.Name == "Tag_Id").TypeName);
            Assert.Equal("bigint", table.Properties.Single(c => c.Name == "ProductA_Id").TypeName);
        }

        [Fact]
        public void Generated_ia_fk_columns_should_have_correct_store_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<ToOne>()
                .HasKey(
                    t => new
                             {
                                 t.AnotherId1,
                                 t.AnotherId2
                             })
                .HasRequired(t => t.NavOne);

            modelBuilder
                .Entity<One>()
                .HasKey(o => o.AnId)
                .HasOptional(o => o.NavToOne);
            modelBuilder
                .Entity<One>()
                .Property(o => o.AnId)
                .HasColumnType("bigint");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var table = databaseMapping.Database.GetEntityTypes().Single(t => t.Name == "ToOne");

            Assert.Equal("bigint", table.Properties.Single(c => c.Name == "NavOne_AnId").TypeName);
        }

        [Fact]
        public void Generated_fk_columns_should_have_correct_store_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<SomeItem>()
                .HasOptional(si => si.Detail)
                .WithRequired(sid => sid.Item);

            modelBuilder
                .Entity<SomeItem>()
                .Property(si => si.SomeItemId)
                .HasColumnType("bigint");

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            databaseMapping.Assert<SomeItemDetail>(sid => sid.Id).DbEqual("bigint", c => c.TypeName);
        }

        [Fact]
        public void Generated_fk_columns_should_throw_with_misconfigured_store_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<SomeItem>()
                .HasOptional(si => si.Detail)
                .WithRequired(sid => sid.Item);
            modelBuilder
                .Entity<SomeItem>()
                .Property(si => si.SomeItemId)
                .HasColumnType("bigint");

            modelBuilder
                .Entity<SomeItemDetail>()
                .Property(sid => sid.Id)
                .HasColumnType("nvarchar");

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Throws<MetadataException>(() => databaseMapping.AssertValid());
        }

        [Fact]
        public void Unconfigured_fk_one_to_one_should_throw_with_unknown_principal()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Principal>();
            modelBuilder.Entity<Dependent>();

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnableToDeterminePrincipal", typeof(Dependent).ToString(), typeof(Principal).ToString());
        }

        [Fact]
        public void Half_specified_one_to_one_relationships_should_throw_when_no_principal_specified()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<SomeItem>()
                .HasOptional(s => s.Detail);
            modelBuilder
                .Entity<SomeItemDetail>()
                .HasOptional(d => d.Item);

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnableToDeterminePrincipal", typeof(SomeItem).ToString(), typeof(SomeItemDetail).ToString());
        }

        [Fact]
        public void Half_specified_one_to_one_relationships_should_not_throw_when_principal_specified()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<SomeItem>()
                .HasOptional(s => s.Detail);
            modelBuilder
                .Entity<SomeItemDetail>()
                .HasRequired(d => d.Item);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(1, databaseMapping.Model.Namespaces.Single().AssociationTypes.Count());
        }

        [Fact]
        public void Half_specified_relationships_can_be_inversed_by_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<ProductA>()
                .HasMany(p => p.Tags);
            modelBuilder
                .Entity<Tag>()
                .HasMany(t => t.Products);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(1, databaseMapping.Model.Namespaces.Single().AssociationTypes.Count());
        }

        [Fact]
        public void Full_and_half_specified_relationships_can_be_inversed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<ProductA>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products);
            modelBuilder
                .Entity<Tag>()
                .HasMany(t => t.Products);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(1, databaseMapping.Model.Namespaces.Single().AssociationTypes.Count());
        }

        [Fact]
        public void Full_and_half_specified_relationships_can_be_uninversed()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<ProductA>()
                .HasMany(p => p.Tags)
                .WithMany();
            modelBuilder
                .Entity<Tag>()
                .HasMany(t => t.Products);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(2, databaseMapping.Model.Namespaces.Single().AssociationTypes.Count());
        }

        [Fact]
        // Regression test for Dev11 Bug 98120
        public void Half_specified_optional_relationship_overrides_fully_specified_one()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasRequired(e => e.Detail).WithOptional(e => e.Item);
            modelBuilder.Entity<SomeItem>().HasOptional(e => e.Detail);

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnableToDeterminePrincipal", typeof(SomeItem), typeof(SomeItemDetail));
        }

        [Fact]
        // Regression test for Dev11 Bug 98118
        public void Half_specified_required_relationship_overrides_fully_specified_one()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(e => e.Detail).WithRequired(e => e.Item);
            modelBuilder.Entity<SomeItem>().HasRequired(e => e.Detail);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var association = databaseMapping.Model.Namespaces.Single().AssociationTypes.Single();
            Assert.Equal("SomeItem", association.SourceEnd.GetEntityType().Name);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, association.SourceEnd.RelationshipMultiplicity);
            Assert.Equal("SomeItem", association.Constraint.DependentEnd.GetEntityType().Name);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, association.Constraint.DependentEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Half_specified_many_relationship_overrides_fully_specified_one()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<ProductSubcategory>().HasMany(s => s.Products).WithRequired(p => p.ProductSubcategory);
            modelBuilder.Entity<ProductSubcategory>().HasMany(s => s.Products);

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);
            databaseMapping.AssertValid();

            var association = databaseMapping.Model.Namespaces.Single().AssociationTypes.Single();
            Assert.Equal("ProductSubcategory", association.SourceEnd.GetEntityType().Name);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, association.SourceEnd.RelationshipMultiplicity);
            Assert.Equal("Product", association.Constraint.DependentEnd.GetEntityType().Name);
            Assert.Equal(RelationshipMultiplicity.Many, association.Constraint.DependentEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Unconfigured_one_to_one_should_throw_with_unknown_principal()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<One>().HasKey(o => o.AnId);
            modelBuilder.Entity<ToOne>().HasKey(
                o => new
                         {
                             o.AnotherId1,
                             o.AnotherId2
                         });

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnableToDeterminePrincipal", typeof(ToOne), typeof(One));
        }

        [Fact]
        public void Configured_one_to_one_should_make_ia_when_keys_incompatible()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<One>()
                .HasKey(o => o.AnId);
            modelBuilder
                .Entity<ToOne>()
                .HasKey(
                    t => new
                             {
                                 t.AnotherId1,
                                 t.AnotherId2
                             })
                .HasRequired(t => t.NavOne);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<ToOne>().HasForeignKeyColumn("NavOne_AnId");
        }

        [Fact]
        // Regression test for Dev11 Bug 98118
        public void Self_ref_many_to_optional_should_find_FK()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<TreeNode>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            Assert.Equal(1, databaseMapping.Model.Namespaces.Single().AssociationTypes.Count());
            databaseMapping.Assert<TreeNode>().HasForeignKeyColumn("ParentId");
        }

        [Fact]
        public void Unconfigured_self_ref_one_to_one_should_throw_with_unknown_principal()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SelfRefToOne>();

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("UnableToDeterminePrincipal", typeof(SelfRefToOne), typeof(SelfRefToOne));
        }

        [Fact]
        public void Self_ref_one_to_one_should_generate_ia_by_default()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SelfRefToOne>().HasRequired(s => s.SelfOne);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<SelfRefToOne>().HasForeignKeyColumn("SelfOne_Id");
        }

        [Fact]
        public void Build_model_for_ia_with_entity_splitting()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItemDetail>()
                .Map(
                    c =>
                        {
                            c.ToTable("SomeItemDetail");
                            c.Properties(s => s.Id);
                        })
                .Map(
                    c =>
                        {
                            c.ToTable("SplitTable");
                            c.Properties(s => s.Id);
                        });

            modelBuilder.Entity<SomeItem>()
                .HasOptional(s => s.Detail)
                .WithRequired(sd => sd.Item)
                .Map(c => c.ToTable("SplitTable"));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert("SplitTable").HasColumn("Item_SomeItemId");
        }

        [Fact]
        public void Build_model_for_self_referencing_required_to_required_dependent_ia_with_configuration()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SelfRef>()
                .HasRequired(s => s.Self)
                .WithRequiredDependent()
                .Map(c => c.MapKey("TheKey"));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<SelfRef>().HasColumn("TheKey");
        }

        [Fact]
        public void Build_model_for_self_referencing_required_to_required_principal_ia_with_configuration()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SelfRef>()
                .HasRequired(s => s.Self)
                .WithRequiredPrincipal()
                .Map(c => c.MapKey("TheKey"));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<SelfRef>().HasColumn("TheKey");
        }

        [Fact]
        public void Build_model_for_self_referencing_optional_to_many_ia_with_configured_key_column()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Item>()
                .HasOptional(i => i.ParentItem)
                .WithMany(i => i.ChildrenItems)
                .Map(c => c.MapKey("ParentItemId"));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Item>().HasColumn("ParentItemId");
        }

        [Fact]
        public void Build_model_for_self_referencing_optional_to_many_ia_with_configured_composite_key_column()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Item>()
                .HasKey(
                    i => new
                             {
                                 i.Id,
                                 i.Name
                             })
                .HasOptional(i => i.ParentItem)
                .WithMany(i => i.ChildrenItems)
                .Map(c => c.MapKey("TheId", "TheName"));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Item>().HasColumn("TheId");
            databaseMapping.Assert<Item>().HasColumn("TheName");
        }

        [Fact]
        public void Build_model_for_self_referencing_configured_many_to_many()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Person>()
                .HasMany(p => p.Children)
                .WithMany(p => p.Parents)
                .Map(
                    m =>
                        {
                            m.MapLeftKey("ParentId");
                            m.MapRightKey("ChildId");
                        });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert("PersonPersons").HasColumns("ParentId", "ChildId");
        }

        [Fact]
        public void Self_referencing_many_to_many_should_not_cascade_by_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Person>()
                .HasMany(p => p.Children)
                .WithMany(p => p.Parents);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var joinTable = databaseMapping.Database.GetEntityTypes().Single(t => t.Name == "PersonPerson");

            Assert.Equal(2, joinTable.ForeignKeyBuilders.Count());

            Assert.True(joinTable.ForeignKeyBuilders.All(fk => fk.DeleteAction == OperationAction.None));
        }

        [Fact]
        public void Many_to_many_mapping_configuration_correct_ends_configured()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Tag>();
            modelBuilder.Entity<ProductA>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .Map(
                    mc =>
                        {
                            mc.MapLeftKey("ProductId");
                            mc.MapRightKey("TagId");
                        });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert("ProductATags").HasColumns("ProductId", "TagId");
        }

        [Fact]
        public void Many_to_many_mapping_configuration_correct_ends_configured_several_times_last_wins()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Tag>();
            modelBuilder.Entity<ProductA>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .Map(
                    mc =>
                        {
                            mc.MapLeftKey("BadId");
                            mc.MapRightKey("BadId");
                            mc.MapLeftKey("ProductId");
                            mc.MapRightKey("TagId");
                        });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert("ProductATags").HasColumns("ProductId", "TagId");
        }

        [Fact]
        public void Many_to_many_should_generate_cascading_fks_by_convention()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Tag>();
            modelBuilder.Entity<ProductA>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .Map(
                    mc =>
                        {
                            mc.MapLeftKey("ProductId");
                            mc.MapRightKey("TagId");
                        });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var joinTable = databaseMapping.Database.GetEntityTypes().Single(t => t.Name == "ProductATag");

            Assert.Equal(2, joinTable.ForeignKeyBuilders.Count());

            Assert.True(joinTable.ForeignKeyBuilders.All(fk => fk.DeleteAction == OperationAction.Cascade));
        }

        [Fact]
        public void Many_to_many_mapping_configuration_correct_ends_configured_reverse()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ProductA>();
            modelBuilder.Entity<Tag>()
                .HasMany(t => t.Products)
                .WithMany(p => p.Tags)
                .Map(
                    mc =>
                        {
                            mc.MapLeftKey("TagId");
                            mc.MapRightKey("ProductId");
                        });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert("TagProductAs").HasColumns("TagId", "ProductId");
        }

        // TODO: METADATA [Fact]
        public void Many_to_many_mapping_configuration_repeated_key_throws()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Tag>();
            modelBuilder.Entity<ProductA>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .Map(
                    mc =>
                        {
                            mc.MapLeftKey("ProductId");
                            mc.MapRightKey("ProductId");
                        });

            var databaseMapping = BuildMapping(modelBuilder);

            Assert.Throws<MetadataException>(() => databaseMapping.AssertValid());
        }

        [Fact]
        public void Build_model_for_many_to_many_association_with_conflicting_table_name_in_different_schema()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<SalesOrderHeader>()
                .HasMany(s => s.SalesReasons)
                .WithMany(r => r.SalesOrderHeaders)
                .Map(m => m.ToTable("Products", "schema"));
            modelBuilder.Entity<SalesReason>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping
                .Assert("Products", "schema")
                .HasColumns("SalesOrderHeader_SalesOrderID", "SalesReason_SalesReasonID");
        }

        [Fact]
        public void Build_model_for_circular_associations_with_fk_discovery()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<Album>()
                .HasMany(a => a.Photos)
                .WithRequired(p => p.Album);
            modelBuilder.Entity<Photo>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            Assert.Equal(2, databaseMapping.Model.Containers.Single().AssociationSets.Count());
            Assert.Equal(2, databaseMapping.Database.GetEntityTypes().ElementAt(0).Properties.Count());
            Assert.Equal(2, databaseMapping.Database.GetEntityTypes().ElementAt(1).Properties.Count());
        }

        [Fact]
        public void Invalid_number_of_foreign_keys_specified_should_throw()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ProductCategory>();
            modelBuilder.Entity<ProductSubcategory>()
                .HasRequired(s => s.ProductCategory)
                .WithMany(c => c.ProductSubcategories)
                .HasForeignKey(
                    s => new
                             {
                                 s.ProductCategoryID,
                                 s.Name
                             });

            Assert.Throws<ModelValidationException>(
                () => modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void Configuring_nonnavigation_property_should_throw()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.ComplexType<SomeItemDetail>().Ignore(d => d.Item);
            modelBuilder.Entity<SomeItem>().HasRequired(e => e.Detail).WithMany();

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("NavigationPropertyNotFound", "Detail", "SomeItem");
        }

        [Fact]
        public void Configuring_nonmapped_property_should_throw()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().Ignore(e => e.Detail);
            modelBuilder.Entity<SomeItem>().HasRequired(e => e.Detail).WithMany();

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("NavigationPropertyNotFound", "Detail", "SomeItem");
        }

        [Fact]
        public void Build_model_for_a_simple_one_to_many_association()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ProductCategory>();
            modelBuilder.Entity<ProductSubcategory>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Discover_inverse_navigation_properties_using_annotation()
        {
            var modelBuilder = new AdventureWorksModelBuilder();
            modelBuilder.Conventions.Remove<AssociationInverseDiscoveryConvention>();

            modelBuilder.Entity<ShoppingCartItem>();
            modelBuilder.Entity<Product>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.Model.Namespaces.Single().AssociationTypes.Count);
        }

        [Fact]
        public void Discover_foreign_key_properties_using_annotation()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Employee>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.NotNull(databaseMapping.Model.Namespaces.Single().AssociationTypes.Single().Constraint);
        }

        [Fact]
        public void Build_model_for_a_simple_one_to_many_association_cascading_delete()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<ProductCategory>();
            modelBuilder.Entity<ProductSubcategory>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_a_simple_self_referencing_association()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Employee>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(1, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_simple_optional_to_many_independent_association()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<BillOfMaterials>();
            modelBuilder.Entity<Product>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_simple_required_to_many_independent_association()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<BillOfMaterials>().HasRequired(b => b.Product).WithMany(p => p.BillOfMaterials);
            modelBuilder.Entity<Product>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_simple_many_to_many_association()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<SalesOrderHeader>();
            modelBuilder.Entity<SalesReason>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_many_to_many_association_with_mapping_configuration()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<SalesOrderHeader>()
                .HasMany(s => s.SalesReasons)
                .WithMany(r => r.SalesOrderHeaders)
                .Map(
                    m => m.ToTable("MappingTable")
                             .MapLeftKey("TheOrder")
                             .MapRightKey("TheReason"));
            modelBuilder.Entity<SalesReason>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert("MappingTable").HasColumns("TheOrder", "TheReason");
            databaseMapping.Assert("MappingTable").HasForeignKey(new[] { "TheOrder" }, "SalesOrderHeaders");
            databaseMapping.Assert("MappingTable").HasForeignKey(new[] { "TheReason" }, "SalesReasons");
        }

        [Fact]
        public void Build_model_for_many_to_many_association_with_conflicting_table_name()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Product>();
            modelBuilder.Entity<SalesOrderHeader>()
                .HasMany(s => s.SalesReasons)
                .WithMany(r => r.SalesOrderHeaders)
                .Map(m => m.ToTable("Products"));
            modelBuilder.Entity<SalesReason>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert("Products").HasColumns("SalesOrderHeader_SalesOrderID", "SalesReason_SalesReasonID");
        }

        [Fact]
        public void Build_model_for_simple_optional_to_required_bidirectional_association()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDiscount>()
                .HasKey(cd => cd.CustomerID)
                .HasRequired(cd => cd.Customer);

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_simple_optional_to_required_unidirectional_association()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasOptional(c => c.CustomerDiscount)
                .WithRequired();

            modelBuilder.Entity<CustomerDiscount>()
                .HasKey(cd => cd.CustomerID)
                .Ignore(cd => cd.Customer);

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
        }

        [Fact]
        public void Build_model_for_self_referencing_many_to_many_association()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<User>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.Database.GetEntityTypes().Count());
        }

        [Fact]
        public void Inverse_navigation_property_from_base_should_throw()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<DerivedDependent>()
                .HasRequired(d => d.DerivedPrincipalNavigation)
                .WithRequiredDependent(p => p.DerivedDependentNavigation);

            Assert.Throws<MetadataException>(
                () =>
                modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void Configure_unidirectional_association_with_navigation_property_from_unmapped_base()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<DerivedDependent>().HasRequired(d => d.PrincipalNavigation).WithRequiredDependent();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void FK_from_base_should_throw()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Ignore<PrincipalBase>();
            modelBuilder.Entity<DependentBase>().Ignore(d => d.PrincipalNavigation);
            modelBuilder.Entity<DerivedDependent>()
                .HasRequired(d => d.DerivedPrincipalNavigation)
                .WithMany(p => p.DerivedDependentNavigations)
                .HasForeignKey(d => d.PrincipalNavigationId);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.BuildAndValidate(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ForeignKeyPropertyNotFound", "PrincipalNavigationId", "DerivedDependent");
        }

        [Fact]
        public void PK_as_FK_with_many_to_one_should_throw()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Ignore<PrincipalBase>();
            modelBuilder.Entity<DerivedDependent>()
                .HasRequired(d => d.DerivedPrincipalNavigation)
                .WithMany(p => p.DerivedDependentNavigations)
                .HasForeignKey(d => d.Id);

            Assert.Throws<ModelValidationException>(
                () =>
                modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void Map_FK_From_Principal_Side_And_Table_Split_FK_Into_First_Table()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<SpecialOfferProduct>()
                .HasKey(p => p.ProductID);

            modelBuilder.Entity<SpecialOfferProduct>()
                .Ignore(p => p.SpecialOffer);

            modelBuilder.Entity<SpecialOffer>()
                .HasMany(o => o.SpecialOfferProducts)
                .WithRequired()
                .HasForeignKey(p => p.SpecialOfferID);

            modelBuilder.Entity<SpecialOfferProduct>()
                .Map(
                    m =>
                        {
                            m.Properties(
                                p =>
                                new
                                    {
                                        p.ProductID,
                                        p.rowguid,
                                        p.SpecialOfferID // FK in table 1
                                    });
                            m.ToTable("ProductOne");
                        })
                .Map(
                    m =>
                        {
                            m.Properties(
                                p =>
                                new
                                    {
                                        p.ProductID,
                                        p.ModifiedDate,
                                    });
                            m.ToTable("ProductTwo");
                        });

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(3, databaseMapping.Database.GetEntityTypes().Count());

            databaseMapping.Assert<SpecialOfferProduct>("ProductOne")
                .HasColumns("ProductID", "SpecialOfferID", "rowguid")
                .HasForeignKeyColumn("SpecialOfferID");

            databaseMapping.Assert<SpecialOfferProduct>("ProductTwo")
                .HasColumns("ProductID", "ModifiedDate")
                .HasNoForeignKeyColumn("SpecialOfferID");
        }

        [Fact]
        public void Map_FK_From_Principal_Side_And_Table_Split_FK_Into_Second_Table()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<SpecialOfferProduct>()
                .HasKey(p => p.ProductID);

            modelBuilder.Entity<SpecialOfferProduct>()
                .Ignore(p => p.SpecialOffer);

            modelBuilder.Entity<SpecialOffer>()
                .HasMany(o => o.SpecialOfferProducts)
                .WithRequired()
                .HasForeignKey(p => p.SpecialOfferID);

            modelBuilder.Entity<SpecialOfferProduct>()
                .Map(
                    m =>
                        {
                            m.Properties(
                                p =>
                                new
                                    {
                                        p.ProductID,
                                        p.rowguid
                                    });
                            m.ToTable("ProductOne");
                        })
                .Map(
                    m =>
                        {
                            m.Properties(
                                p =>
                                new
                                    {
                                        p.ProductID,
                                        p.ModifiedDate,
                                        p.SpecialOfferID // FK in table 2
                                    });
                            m.ToTable("ProductTwo");
                        });

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(3, databaseMapping.Database.GetEntityTypes().Count());

            databaseMapping.Assert<SpecialOfferProduct>("ProductOne")
                .HasColumns("ProductID", "rowguid")
                .HasNoForeignKeyColumn("SpecialOfferID");

            databaseMapping.Assert<SpecialOfferProduct>("ProductTwo")
                .HasColumns("ProductID", "SpecialOfferID", "ModifiedDate")
                .HasForeignKeyColumn("SpecialOfferID");
        }

        [Fact]
        // DevDiv2 165131
        public void Map_IA_to_Pluralized_Table()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>().HasKey(e => e.CustomerID);
            modelBuilder.Entity<CustomerDiscount>()
                .HasKey(e => e.CustomerID)
                .HasRequired(cd => cd.Customer)
                .WithRequiredPrincipal(c => c.CustomerDiscount)
                .Map(m => m.ToTable("Customers"));

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.EntityContainerMappings.Single().EntitySetMappings.Count);
            databaseMapping.Assert<Customer>("Customers");
            databaseMapping.Assert<Customer>().HasForeignKeyColumn("CustomerDiscount_CustomerID");
        }

        [Fact]
        public void Map_IA_To_First_Split_Table()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<SpecialOfferProduct>()
                .HasKey(p => p.ProductID)
                .Map(
                    m =>
                        {
                            m.Properties(
                                p =>
                                new
                                    {
                                        p.ProductID,
                                        p.rowguid
                                    });
                            m.ToTable("ProductOne");
                        })
                .Map(
                    m =>
                        {
                            m.Properties(
                                p =>
                                new
                                    {
                                        p.ProductID,
                                        p.ModifiedDate,
                                        p.SpecialOfferID
                                    });
                            m.ToTable("ProductTwo");
                        });

            modelBuilder.Entity<SpecialOffer>()
                .HasKey(o => o.SpecialOfferID)
                .HasMany(o => o.SpecialOfferProducts)
                .WithRequired(p => p.SpecialOffer)
                .Map(mc => mc.MapKey("TheFK"));

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(3, databaseMapping.Database.GetEntityTypes().Count());

            databaseMapping.Assert<SpecialOfferProduct>("ProductOne")
                .HasColumns("ProductID", "rowguid", "TheFK")
                .HasForeignKeyColumn("TheFK");
            databaseMapping.Assert<SpecialOfferProduct>("ProductTwo")
                .HasColumns("ProductID", "SpecialOfferID", "ModifiedDate")
                .HasNoForeignKeyColumn("TheFK");
        }

        [Fact]
        public void Map_IA_Column_Names()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<SpecialOfferProduct>()
                .HasKey(p => p.ProductID);

            modelBuilder.Entity<SpecialOffer>()
                .HasKey(o => o.SpecialOfferID)
                .HasMany(o => o.SpecialOfferProducts)
                .WithRequired(p => p.SpecialOffer)
                .Map(mc => mc.MapKey("TheFK"));

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.Database.GetEntityTypes().Count());

            databaseMapping.Assert<SpecialOfferProduct>()
                .HasColumns("ProductID", "SpecialOfferID", "rowguid", "ModifiedDate", "TheFK")
                .HasForeignKeyColumn("TheFK");
        }

        [Fact]
        public void Map_IA_column_names_several_times_last_wins()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<SpecialOfferProduct>()
                .HasKey(p => p.ProductID);

            modelBuilder.Entity<SpecialOffer>()
                .HasKey(o => o.SpecialOfferID)
                .HasMany(o => o.SpecialOfferProducts)
                .WithRequired(p => p.SpecialOffer)
                .Map(
                    mc =>
                        {
                            mc.MapKey("BadFK");
                            mc.ToTable("BadTable");
                            mc.MapKey("TheFK");
                            mc.ToTable("SpecialOfferProducts");
                        });

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Equal(2, databaseMapping.Database.GetEntityTypes().Count());

            databaseMapping.Assert<SpecialOfferProduct>()
                .HasColumns("ProductID", "SpecialOfferID", "rowguid", "ModifiedDate", "TheFK")
                .HasForeignKeyColumn("TheFK");
        }

        // TODO: METADATA [Fact]
        public void Mapping_IA_column_name_to_existing_one_throws()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<SpecialOfferProduct>()
                .HasKey(p => p.ProductID);

            modelBuilder.Entity<SpecialOffer>()
                .HasKey(o => o.SpecialOfferID)
                .HasMany(o => o.SpecialOfferProducts)
                .WithRequired(p => p.SpecialOffer)
                .Map(mc => mc.MapKey("SpecialOfferID"));

            Assert.Throws<MetadataException>(
                () => modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo));
        }

        [Fact]
        public void Can_turn_cascade_delete_off_for_IA_association()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CDDep>().Ignore(d => d.CDPrinId);
            modelBuilder.Entity<CDPrin>().HasMany(p => p.CDDeps).WithRequired(d => d.CDPrin).WillCascadeOnDelete(false);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.Equal(OperationAction.None, aset.ElementType.SourceEnd.DeleteBehavior);
            Assert.Equal(OperationAction.None, aset.ElementType.TargetEnd.DeleteBehavior);
        }

        [Fact]
        public void Can_turn_cascade_delete_on_for_IA_association()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CDDep>();
            modelBuilder.Entity<CDDep>().Ignore(d => d.CDPrinId);
            modelBuilder.Entity<CDPrin>().HasMany(p => p.CDDeps).WithRequired(d => d.CDPrin).WillCascadeOnDelete(true);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.Equal(OperationAction.Cascade, aset.ElementType.SourceEnd.DeleteBehavior);
            Assert.Equal(OperationAction.None, aset.ElementType.TargetEnd.DeleteBehavior);
        }

        [Fact]
        public void Can_turn_cascade_delete_off_for_non_nullable_FK_association()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CDPrin>().HasMany(p => p.CDDeps).WithRequired(d => d.CDPrin).HasForeignKey(
                d => d.CDPrinId).WillCascadeOnDelete(false);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.Equal(OperationAction.None, aset.ElementType.SourceEnd.DeleteBehavior);
            Assert.Equal(OperationAction.None, aset.ElementType.TargetEnd.DeleteBehavior);
        }

        [Fact]
        public void Can_turn_cascade_delete_on_for_non_nullable_FK_association()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CDDep>();
            modelBuilder.Entity<CDPrin>().HasMany(p => p.CDDeps).WithRequired(d => d.CDPrin).HasForeignKey(
                d => d.CDPrinId).WillCascadeOnDelete(true);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.Equal(OperationAction.Cascade, aset.ElementType.SourceEnd.DeleteBehavior);
            Assert.Equal(OperationAction.None, aset.ElementType.TargetEnd.DeleteBehavior);
        }

        [Fact]
        public void Can_set_optional_to_optional_dependent_relationships_and_you_get_an_IA()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItemDetail>().HasOptional(p => p.Item).WithOptionalDependent(p => p.Detail);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.Null(aset.ElementType.Constraint);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, aset.ElementType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, aset.ElementType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<SomeItemDetail>().HasForeignKeyColumn("Item_SomeItemId"); // IA FK
            Assert.Equal(1, databaseMapping.EntityContainerMappings[0].AssociationSetMappings.Count);
        }

        [Fact]
        public void Default_ia_fk_column_names_should_incorporate_nav_prop_on_dependent()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<SomeItem>()
                .HasOptional(s => s.Detail)
                .WithRequired(d => d.Item)
                .Map(_ => { });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<SomeItemDetail>().HasForeignKeyColumn("Item_SomeItemId");
        }

        [Fact]
        public void Can_set_optional_to_optional_principal_relationships_and_you_get_an_IA()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithOptionalPrincipal(p => p.Item);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.Null(aset.ElementType.Constraint);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, aset.ElementType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, aset.ElementType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<SomeItemDetail>().HasForeignKeyColumn("Item_SomeItemId"); // IA FK
            Assert.Equal(1, databaseMapping.EntityContainerMappings[0].AssociationSetMappings.Count);
        }

        [Fact]
        public void Can_set_required_to_required_dependent_relationships_and_you_get_an_FK()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItemDetail>().HasRequired(p => p.Item).WithRequiredDependent(p => p.Detail);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.NotNull(aset.ElementType.Constraint);
            Assert.Equal(RelationshipMultiplicity.One, aset.ElementType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, aset.ElementType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<SomeItemDetail>().HasForeignKeyColumn("Id"); // FK
            Assert.Equal(0, databaseMapping.EntityContainerMappings[0].AssociationSetMappings.Count);
        }

        [Fact]
        public void Can_set_required_to_required_principal_relationships_and_you_get_an_FK()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasRequired(d => d.Detail).WithRequiredPrincipal(p => p.Item);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.NotNull(aset.ElementType.Constraint);
            Assert.Equal(RelationshipMultiplicity.One, aset.ElementType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, aset.ElementType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<SomeItemDetail>().HasForeignKeyColumn("Id"); // FK
            Assert.Equal(0, databaseMapping.EntityContainerMappings[0].AssociationSetMappings.Count);
        }

        [Fact]
        public void Can_set_required_to_optional_relationship_and_you_get_an_FK_with_correct_dependent_end()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItemDetail>().HasRequired(d => d.Item).WithOptional(p => p.Detail);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.NotNull(aset.ElementType.Constraint);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, aset.ElementType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, aset.ElementType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<SomeItemDetail>().HasForeignKeyColumn("Id"); // FK
            Assert.Equal(0, databaseMapping.EntityContainerMappings[0].AssociationSetMappings.Count);
        }

        [Fact]
        public void Can_set_optional_to_required_relationship_and_you_get_an_FK_with_correct_dependent_end()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithRequired(p => p.Item);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.NotNull(aset.ElementType.Constraint);
            Assert.Equal(RelationshipMultiplicity.One, aset.ElementType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, aset.ElementType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<SomeItemDetail>().HasForeignKeyColumn("Id"); // FK
            Assert.Equal(0, databaseMapping.EntityContainerMappings[0].AssociationSetMappings.Count);
        }

        [Fact]
        public void Ignore_FK_property_and_you_get_an_IA()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<CDPrin>().HasMany(p => p.CDDeps).WithRequired(d => d.CDPrin);
            modelBuilder.Entity<CDDep>().Ignore(d => d.CDPrinId);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.Null(aset.ElementType.Constraint);
            Assert.Equal(RelationshipMultiplicity.One, aset.ElementType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.Many, aset.ElementType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<CDDep>().HasForeignKeyColumn("CDPrin_CDPrinId"); // AI FK
            Assert.Equal(1, databaseMapping.EntityContainerMappings[0].AssociationSetMappings.Count);
        }

        #region Configuration from both sides tests

        [Fact]
        public void Redundant_many_to_many_mapping_configuration_with_left_and_right_keys_switched_should_not_throw()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ProductA>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .Map(
                    mc =>
                        {
                            mc.MapLeftKey("ProductId");
                            mc.MapRightKey("TagId");
                            mc.ToTable("ProductTags");
                        });

            modelBuilder.Entity<Tag>()
                .HasMany(t => t.Products)
                .WithMany(p => p.Tags)
                .Map(
                    mc =>
                        {
                            mc.MapLeftKey("TagId");
                            mc.MapRightKey("ProductId");
                            mc.ToTable("ProductTags");
                        });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Redundant_many_to_many_mapping_configuration_should_not_throw()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ProductA>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .Map(
                    mc =>
                        {
                            mc.MapLeftKey("TagId");
                            mc.MapRightKey("ProductId");
                            mc.ToTable("ProductTags");
                        });

            modelBuilder.Entity<Tag>()
                .HasMany(t => t.Products)
                .WithMany(p => p.Tags)
                .Map(
                    mc =>
                        {
                            mc.MapLeftKey("TagId");
                            mc.MapRightKey("ProductId");
                            mc.ToTable("ProductTags");
                        });

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Inconsistent_redundant_many_to_many_mapping_configuration_should_throw()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ProductA>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .Map(
                    mc =>
                        {
                            mc.MapLeftKey("ProductCode");
                            mc.MapRightKey("TagId");
                            mc.ToTable("ProductTags");
                        });

            modelBuilder.Entity<Tag>()
                .HasMany(t => t.Products)
                .WithMany(p => p.Tags)
                .Map(
                    mc =>
                        {
                            mc.MapLeftKey("TagId");
                            mc.MapRightKey("ProductId");
                            mc.ToTable("ProductTags");
                        });

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingMapping", "Products", "FunctionalTests.Tag");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_optional_to_one_and_one_to_one_dependent()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithRequired(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasRequired(d => d.Item).WithRequiredDependent(p => p.Detail);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingMultiplicities", "Detail", "FunctionalTests.SomeItem");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_optional_to_one_and_one_to_one_principal()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithRequired(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasRequired(d => d.Item).WithRequiredPrincipal(p => p.Detail);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingMultiplicities", "Detail", "FunctionalTests.SomeItem");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_optional_to_one_and_optional_to_one()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithRequired(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasOptional(d => d.Item).WithRequired(p => p.Detail);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingMultiplicities", "Item", "FunctionalTests.SomeItemDetail");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_optional_to_one_and_optional_to_optional_dependent()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithRequired(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasOptional(d => d.Item).WithOptionalDependent(p => p.Detail);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingMultiplicities", "Item", "FunctionalTests.SomeItemDetail");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_optional_to_one_and_optional_to_optional_principal()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithRequired(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasOptional(d => d.Item).WithOptionalPrincipal(p => p.Detail);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingMultiplicities", "Item", "FunctionalTests.SomeItemDetail");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_optional_to_optional_principal_and_optional_to_optional_principal()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithOptionalPrincipal(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasOptional(d => d.Item).WithOptionalPrincipal(p => p.Detail);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingConstraint", "Item", "FunctionalTests.SomeItemDetail");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_optional_to_optional_dependent_and_optional_to_optional_dependent()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithOptionalDependent(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasOptional(d => d.Item).WithOptionalDependent(p => p.Detail);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingConstraint", "Item", "FunctionalTests.SomeItemDetail");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_optional_to_optional_dependent_and_one_to_one_dependent()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithOptionalDependent(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasRequired(d => d.Item).WithRequiredDependent(p => p.Detail);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingMultiplicities", "Item", "FunctionalTests.SomeItemDetail");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_optional_to_optional_dependent_and_one_to_one_principal()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithOptionalDependent(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasRequired(d => d.Item).WithRequiredPrincipal(p => p.Detail);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingMultiplicities", "Item", "FunctionalTests.SomeItemDetail");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_one_to_one_principal_and_one_to_one_principal()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasRequired(d => d.Detail).WithRequiredPrincipal(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasRequired(d => d.Item).WithRequiredPrincipal(p => p.Detail);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingConstraint", "Item", "FunctionalTests.SomeItemDetail");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_one_to_one_dependent_and_one_to_one_dependent()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasRequired(d => d.Detail).WithRequiredDependent(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasRequired(d => d.Item).WithRequiredDependent(p => p.Detail);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingConstraint", "Item", "FunctionalTests.SomeItemDetail");
        }

        [Fact]
        public void Conflicting_multiplicity_throws_one_to_many_and_many_to_optional()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ProductSubcategory>().HasRequired(s => s.ProductCategory).WithMany(
                c => c.ProductSubcategories);
            modelBuilder.Entity<ProductCategory>().HasMany(c => c.ProductSubcategories).WithOptional(
                s => s.ProductCategory);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo)).
                ValidateMessage("ConflictingMultiplicities", "ProductCategory", "FunctionalTests.Model.ProductSubcategory");
        }

        // Dev11 330745
        [Fact]
        public void Using_invalid_mapping_from_two_ends_without_nav_prop_when_nav_prop_exists_should_throw_but_not_NullReferenceException()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<SomeItem>()
                .HasOptional(d => d.Detail)
                .WithRequired(p => p.Item);

            modelBuilder
                .Entity<SomeItemDetail>()
                .HasRequired(d => d.Item)
                .WithRequiredPrincipal(); // <- There is an inverse nav-prop, but pretending that there isn't.

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingMultiplicities", "Item", "FunctionalTests.SomeItemDetail");
        }

        [Fact]
        public void Redundant_optional_to_optional_configuration_should_not_throw()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithOptionalPrincipal(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasOptional(d => d.Item).WithOptionalDependent(p => p.Detail);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.Null(aset.ElementType.Constraint);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, aset.ElementType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, aset.ElementType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<SomeItemDetail>().HasForeignKeyColumn("Item_SomeItemId"); // IA FK
            Assert.Equal(1, databaseMapping.EntityContainerMappings[0].AssociationSetMappings.Count);
        }

        [Fact]
        public void Redundant_one_to_one_configuration_should_not_throw()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasRequired(d => d.Detail).WithRequiredDependent(p => p.Item);
            modelBuilder.Entity<SomeItemDetail>().HasRequired(d => d.Item).WithRequiredPrincipal(p => p.Detail);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var aset = databaseMapping.Model.Containers[0].AssociationSets[0];
            Assert.NotNull(aset.ElementType.Constraint);
            Assert.Equal(RelationshipMultiplicity.One, aset.ElementType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, aset.ElementType.TargetEnd.RelationshipMultiplicity);
            databaseMapping.Assert<SomeItem>().HasForeignKeyColumn("SomeItemId"); // FK
            Assert.Equal(0, databaseMapping.EntityContainerMappings[0].AssociationSetMappings.Count);
        }

        [Fact]
        public void Conflicting_cascade_delete_throws()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(d => d.Detail).WithRequired(p => p.Item).WillCascadeOnDelete(
                true);
            modelBuilder.Entity<SomeItemDetail>().HasRequired(d => d.Item).WithOptional(p => p.Detail).
                WillCascadeOnDelete(false);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingCascadeDeleteOperation", "Item", "FunctionalTests.SomeItemDetail");
        }

        [Fact]
        public void Conflicting_FK_throws()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ProductSubcategory>().HasRequired(s => s.ProductCategory).WithMany(
                c => c.ProductSubcategories).HasForeignKey(s => s.ProductCategoryID);
            modelBuilder.Entity<ProductCategory>().HasMany(c => c.ProductSubcategories).WithRequired(
                s => s.ProductCategory).HasForeignKey(s => s.ProductSubcategoryID);

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo)).
                ValidateMessage("ConflictingConstraint", "ProductSubcategories", "FunctionalTests.Model.ProductCategory");
        }

        [Fact]
        public void Conflicting_FK_vs_IA_configuration_throws()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ProductSubcategory>().HasRequired(s => s.ProductCategory).WithMany(
                c => c.ProductSubcategories).HasForeignKey(c => c.ProductSubcategoryID);
            modelBuilder.Entity<ProductCategory>().HasMany(c => c.ProductSubcategories).WithRequired(
                s => s.ProductCategory).Map(_ => { });

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo)).
                ValidateMessage("ConflictingConstraint", "ProductSubcategories", "FunctionalTests.Model.ProductCategory");
        }

        [Fact]
        public void Conflicting_IA_column_name_configuration_throws()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ProductSubcategory>().HasRequired(s => s.ProductCategory).WithMany(
                c => c.ProductSubcategories).Map(c => c.MapKey("Key1"));
            modelBuilder.Entity<ProductCategory>().HasMany(c => c.ProductSubcategories).WithRequired(
                s => s.ProductCategory).Map(c => c.MapKey("Key2"));

            Assert.Throws<InvalidOperationException>(
                () =>
                modelBuilder.Build(
                    ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("ConflictingMapping", "ProductSubcategories", "FunctionalTests.Model.ProductCategory");
        }

        #endregion

        [Fact]
        // Regression test for Dev11 Bug 8383 "Identity convention should not be applied when PK is an FK."
        public void Setting_required_to_optional_relationship_gives_you_a_PK_FK_that_is_not_identity()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItemDetail>().HasRequired(d => d.Item).WithOptional(p => p.Detail);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<SomeItemDetail>().HasForeignKeyColumn("Id");
            databaseMapping.Assert<SomeItemDetail>(d => d.Id)
                .DbEqual(true, c => c.IsPrimaryKeyColumn)
                .DbEqual(StoreGeneratedPattern.None, c => c.StoreGeneratedPattern);
        }

        [Fact]
        // Regression test for Dev11 Bug 8383 "Identity convention should not be applied when PK is an FK."
        public void Setting_optional_to_required_relationship_gives_you_a_PK_FK_that_is_not_identity()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SomeItem>().HasOptional(p => p.Detail).WithRequired(d => d.Item);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<SomeItemDetail>().HasForeignKeyColumn("Id");
            databaseMapping.Assert<SomeItemDetail>(d => d.Id)
                .DbEqual(true, c => c.IsPrimaryKeyColumn)
                .DbEqual(StoreGeneratedPattern.None, c => c.StoreGeneratedPattern);
        }

        // Dev11 345384
        [Fact]
        public void Identity_key_is_created_by_convention_when_table_splitting_is_specified_with_fluent_API()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableSharing1>().ToTable("SharedTable");
            modelBuilder.Entity<TableSharing2>().ToTable("SharedTable");
            modelBuilder.Entity<TableSharing2>().HasRequired(d => d.BackRef).WithRequiredPrincipal();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<TableSharing2>(d => d.Id)
                .DbEqual(true, c => c.IsPrimaryKeyColumn)
                .DbEqual(StoreGeneratedPattern.Identity, c => c.StoreGeneratedPattern);
            databaseMapping.Assert<TableSharing1>(d => d.Id)
                .DbEqual(true, c => c.IsPrimaryKeyColumn)
                .DbEqual(StoreGeneratedPattern.Identity, c => c.StoreGeneratedPattern);
        }

        // Dev11 345384
        [Fact]
        public void Identity_key_is_created_by_convention_when_table_splitting_is_specified_with_attributes()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<TableSharing2A>().HasRequired(d => d.BackRef).WithRequiredPrincipal();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<TableSharing2A>(d => d.Id)
                .DbEqual(true, c => c.IsPrimaryKeyColumn)
                .DbEqual(StoreGeneratedPattern.Identity, c => c.StoreGeneratedPattern);
            databaseMapping.Assert<TableSharing1A>(d => d.Id)
                .DbEqual(true, c => c.IsPrimaryKeyColumn)
                .DbEqual(StoreGeneratedPattern.Identity, c => c.StoreGeneratedPattern);
        }

        [Fact]
        public void Setting_IA_FK_name_also_changes_condition_column_name()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Repro150565_BaseDependent>().HasKey(e => e.Key1);
            modelBuilder.Entity<Repro150565_Dependent>().Map(mapping => mapping.ToTable("Dependent"));
            modelBuilder.Entity<Repro150565_Dependent>().HasOptional(e => e.PrincipalNavigation).WithMany(
                e => e.DependentNavigation)
                .Map(m => m.MapKey("IndependentColumn1"));
            modelBuilder.Entity<Repro150565_BaseDependent>().Map(mapping => mapping.ToTable("BaseDependent"));

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert<Repro150565_Dependent>()
                .HasColumn("IndependentColumn1");
            Assert.Equal(
                "IndependentColumn1",
                databaseMapping.EntityContainerMappings[0].AssociationSetMappings[0].ColumnConditions[0].Column
                    .Name);
        }

        // Dev11 287430
        [Fact]
        public void Data_annotations_should_not_be_applied_to_many_to_many_mapping_when_association_is_fully_configured_with_fluent_API()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<Person287430>();
            modelBuilder.Entity<Role287430>();

            modelBuilder
                .Entity<Role287430>()
                .HasMany(m => m.Persons)
                .WithMany(p => p.Roles)
                .Map(
                    m => m.ToTable("person_role", "domain")
                             .MapLeftKey("role_identifier")
                             .MapRightKey("person_identifier"));

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);
            var databaseMapping = model.DatabaseMapping;

            databaseMapping.AssertValid();
            databaseMapping.Assert("person_role", "domain").HasColumns("person_identifier", "role_identifier");
            databaseMapping.Assert("person_role", "domain").HasForeignKey(new[] { "role_identifier" }, "role");
            databaseMapping.Assert("person_role", "domain").HasForeignKey(new[] { "person_identifier" }, "person");
        }

        [Fact]
        public void Bug_46199_Sequence_contains_more_than_one_element_exception_thrown_at_navigation_property_configuration()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<APerson>().HasOptional(p => p.Mother).WithMany().Map(t => t.MapKey("MotherId"));
            modelBuilder.Entity<APerson>().HasOptional(p => p.Father).WithMany().Map(t => t.MapKey("FatherId"));
            modelBuilder.Entity<APerson>().HasOptional(p => p.Birth).WithOptionalDependent().Map(t => t.MapKey("BirthdayId"));
            modelBuilder.Entity<APerson>().HasOptional(p => p.Death).WithOptionalDependent().Map(t => t.MapKey("DeathdayId"));
            modelBuilder.Entity<Marriage>().HasRequired(e => e.WeddingDay).WithOptional().Map(t => t.MapKey("WeddingDayId"));
            modelBuilder.Entity<Marriage>().HasRequired(e => e.Wife).WithMany().Map(t => t.MapKey("WifeId"));
            modelBuilder.Entity<Marriage>().HasRequired(e => e.Husband).WithMany().Map(t => t.MapKey("HusbandId"));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void ForeignKey_annotation_is_allowed_for_one_to_one_PK_to_PK_mapping_Dev11_437725()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OneToOneResult>()
                .HasRequired(r => r.Detail)
                .WithRequiredPrincipal();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<OneToOneResultDetail>().HasForeignKey(
                new[] { "OneToOneResultId" }, "OneToOneResults");
        }
    }

    #region Fixtures

    public class CDPrin
    {
        public int CDPrinId { get; set; }
        public List<CDDep> CDDeps { get; set; }
    }

    public class CDDep
    {
        public int Id { get; set; }
        public int CDPrinId { get; set; }
        public CDPrin CDPrin { get; set; }
    }

    public class SomeItem
    {
        public int SomeItemId { get; set; }
        public SomeItemDetail Detail { get; set; }
    }

    public class SomeItemDetail
    {
        public int Id { get; set; }
        public SomeItem Item { get; set; }
    }

    public class Album
    {
        public int Id { get; set; }
        public virtual Photo Thumbnail { get; set; }
        public int? ThumbnailId { get; set; }
        public virtual ICollection<Photo> Photos { get; set; }
    }

    public class Photo
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public virtual Album Album { get; set; }
    }

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

    public class Item
    {
        public int Id { get; set; }
        public int Name { get; set; }
        public virtual Item ParentItem { get; set; }
        public virtual ICollection<Item> ChildrenItems { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public ICollection<Person> Children { get; set; }
        public ICollection<Person> Parents { get; set; }
    }

    public class SelfRef
    {
        public int Id { get; set; }
        public SelfRef Self { get; set; }
    }

    public class TreeNode
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public TreeNode Parent { get; set; }
        public ICollection<TreeNode> Children { get; set; }
    }

    public class One
    {
        public int AnId { get; set; }
        public ToOne NavToOne { get; set; }
    }

    public class ToOne
    {
        public string AnotherId1 { get; set; }
        public string AnotherId2 { get; set; }
        public One NavOne { get; set; }
    }

    public class SelfRefToOne
    {
        public int Id { get; set; }
        public SelfRefToOne SelfOne { get; set; }
        public SelfRefToOne SelfTwo { get; set; }
    }

    public class Principal
    {
        public int Id { get; set; }
        public Dependent DependentNavigation { get; set; }
    }

    public class Dependent
    {
        public int Id { get; set; }
        public Principal PrincipalNavigation { get; set; }
    }

    public class PrincipalWithAnnotatedDependent
    {
        public string AnId { get; set; }
        public AnnotatedDependent Dependent { get; set; }
    }

    public class AnnotatedDependent
    {
        [ForeignKey("Principal")]
        public string AnotherId { get; set; }

        public PrincipalWithAnnotatedDependent Principal { get; set; }
    }

    public class AnnotatedDependentWrong
    {
        [ForeignKey("Wrong")]
        public string Id { get; set; }

        public PrincipalWithAnnotatedDependent Principal { get; set; }
    }

    public class PrincipalWithCompositeAnnotatedDependent
    {
        public int Id1 { get; set; }
        public string Id2 { get; set; }
        public ICollection<CompositeAnnotatedDependent> Dependents { get; set; }
        public ICollection<CompositePartiallyAnnotatedDependent> Dependents2 { get; set; }
    }

    public class CompositeAnnotatedDependent
    {
        public int Id { get; set; }

        [ForeignKey("Principal")]
        [Column(Order = 2)]
        public int TheFk1 { get; set; }

        [ForeignKey("Principal")]
        [Column(Order = 1)]
        public string TheFk2 { get; set; }

        public PrincipalWithCompositeAnnotatedDependent Principal { get; set; }
    }

    public class CompositePartiallyAnnotatedDependent
    {
        public int Id { get; set; }

        [ForeignKey("Principal")]
        public int TheFk1 { get; set; }

        [ForeignKey("Principal")]
        public string TheFk2 { get; set; }

        public PrincipalWithCompositeAnnotatedDependent Principal { get; set; }
    }

    public class PrincipalBase
    {
        public int Id { get; set; }
        public DerivedDependent DerivedDependentNavigation { get; set; }
    }

    public class DerivedPrincipal : PrincipalBase
    {
        public ICollection<DerivedDependent> DerivedDependentNavigations { get; set; }
    }

    public class DependentBase
    {
        public int Id { get; set; }
        public int PrincipalNavigationId { get; set; }
        public PrincipalBase PrincipalNavigation { get; set; }
    }

    public class DerivedDependent : DependentBase
    {
        public DerivedPrincipal DerivedPrincipalNavigation { get; set; }
    }

    public class SelfRefInheritedBase
    {
        public string Id { get; set; }

        [Required]
        public SelfRefInheritedDerived Derived { get; set; }
    }

    public class SelfRefInheritedDerived : SelfRefInheritedBase
    {
        public ICollection<SelfRefInheritedBase> Bases { get; set; }
    }

    public class DependentNoPrincipalNavRequired
    {
        [ForeignKey("PrincipalNavigation")]
        public Guid DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        [Required]
        public PrincipalNoPrincipalNav PrincipalNavigation { get; set; }
    }

    public class DependentNoPrincipalNavOptional
    {
        [ForeignKey("PrincipalNavigation")]
        public Guid DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        public PrincipalNoPrincipalNav PrincipalNavigation { get; set; }
    }

    public class PrincipalNoPrincipalNav
    {
        public Guid? Key1 { get; set; }
    }

    public class DependentPrincipalNavOptional
    {
        [ForeignKey("PrincipalNavigation")]
        public int DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        [ForeignKey("PrincipalNavigation")]
        public Guid DependentForeignKeyPropertyNotFromConvention2 { get; set; }

        public PrincipalPrincipalNavOptional PrincipalNavigation { get; set; }
    }

    public class PrincipalPrincipalNavOptional
    {
        public int? Key1 { get; set; }
        public Guid? Key2 { get; set; }
        public DependentPrincipalNavOptional DependentNavigation { get; set; }
    }

    public class DependentPrincipalNavRequired
    {
        [ForeignKey("PrincipalNavigation")]
        public Guid DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        [Required]
        public PrincipalPrincipalNavRequired PrincipalNavigation { get; set; }
    }

    public class PrincipalPrincipalNavRequired
    {
        public Guid? Key1 { get; set; }
        public DependentPrincipalNavRequired DependentNavigation { get; set; }
    }

    public class DependentPrincipalNavRequiredDependent
    {
        [ForeignKey("PrincipalNavigation")]
        public Guid DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        public PrincipalPrincipalNavRequiredDependent PrincipalNavigation { get; set; }
    }

    public class PrincipalPrincipalNavRequiredDependent
    {
        public Guid? Key1 { get; set; }

        [Required]
        public DependentPrincipalNavRequiredDependent DependentNavigation { get; set; }
    }

    public class PrincipalByteKey
    {
        public byte[] Key1 { get; set; }

        [ForeignKey("DependentForeignKeyPropertyNotFromConvention1")]
        public DependentByteKey DependentNavigation { get; set; }
    }

    public class DependentByteKey
    {
        public byte[] DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        [InverseProperty("DependentNavigation")]
        [Required]
        public PrincipalByteKey PrincipalNavigation { get; set; }
    }

    public class DependentSelfRef
    {
        [ForeignKey("PrincipalNavigation")]
        [Column(Order = 1)]
        public DateTimeOffset Key1 { get; set; }

        [ForeignKey("PrincipalNavigation")]
        [Column(Order = 2)]
        public DateTimeOffset DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        [Required]
        public DerivedDependentSelfRef PrincipalNavigation { get; set; }
    }

    public class DerivedDependentSelfRef : DependentSelfRef
    {
        public byte[] DerivedProperty1 { get; set; }
    }

    public class DependentWeirdKeyOrder
    {
        public int Fk1 { get; set; }
        public int Fk2 { get; set; }

        [ForeignKey("Fk2,Fk1")]
        public PrincipalWeirdKeyOrder PrincipalNavigation { get; set; }
    }

    public class DependentWeirdKeyOrder2
    {
        public int Fk1 { get; set; }
        public int Fk2 { get; set; }

        [ForeignKey("Fk1,Fk2")]
        public PrincipalWeirdKeyOrder PrincipalNavigation { get; set; }
    }

    public class PrincipalWeirdKeyOrder
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }
    }

    public class BaseDependentAbstractKeyOrder
    {
        public decimal BaseProperty { get; set; }
        public int Id { get; set; }
    }

    public abstract class DependentAbstractKeyOrder : BaseDependentAbstractKeyOrder
    {
        [ForeignKey("PrincipalNavigation")]
        [Column(Order = 1)]
        public decimal? DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        [ForeignKey("PrincipalNavigation")]
        [Column(Order = 2)]
        public decimal? DependentForeignKeyPropertyNotFromConvention2 { get; set; }

        public PrincipalAbstractKeyOrder PrincipalNavigation { get; set; }
    }

    public class BasePrincipalAbstractKeyOrder
    {
        public string BaseProperty { get; set; }
        public decimal Key1 { get; set; }
        public decimal Key2 { get; set; }
    }

    public abstract class PrincipalAbstractKeyOrder : BasePrincipalAbstractKeyOrder
    {
    }

    public class DerivedPrincipalKeyOrder : PrincipalAbstractKeyOrder
    {
    }

    public class DerivedDependentKeyOrder : DependentAbstractKeyOrder
    {
        public byte DerivedProperty1 { get; set; }
    }

    public abstract class DependentSelfRefInverse
    {
        public short Key1 { get; set; }
        public short DependentSelfRefInverseKey1 { get; set; }

        public DependentSelfRefInverse DependentNavigation { get; set; }

        [InverseProperty("DependentNavigation")]
        [Required]
        public DependentSelfRefInverse PrincipalNavigation { get; set; }
    }

    public class DerivedDependentSelfRefInverse : DependentSelfRefInverse
    {
        public string DerivedProperty1 { get; set; }
    }

    public class BaseDependentFkAbstract
    {
        public DateTime? BaseProperty { get; set; }
        public int Id { get; set; }
    }

    public abstract class DependentFkAbstract : BaseDependentFkAbstract
    {
        public int? DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        [InverseProperty("DependentNavigation")]
        [ForeignKey("DependentForeignKeyPropertyNotFromConvention1")]
        public PrincipalFkAbstract PrincipalNavigation { get; set; }
    }

    public class DerivedDependentFkAbstract : DependentFkAbstract
    {
        public string DerivedProperty1 { get; set; }
    }

    public class PrincipalFkAbstract
    {
        public int? Key1 { get; set; }

        [InverseProperty("PrincipalNavigation")]
        public ICollection<DependentFkAbstract> DependentNavigation { get; set; }

        public PrincipalFkAbstract()
        {
            DependentNavigation = new List<DependentFkAbstract>();
        }
    }

    public class DerivedPrincipalFkAbstract : PrincipalFkAbstract
    {
        public byte[] DerivedProperty1 { get; set; }
    }

    public class Dependent144843
    {
        public int Id { get; set; }

        public Principal144843 Principal1 { get; set; }
        public int Principal1Id { get; set; }
    }

    public class Principal144843
    {
        public int Id { get; set; }
        public ICollection<Dependent144843> Dependents1 { get; set; }
        public Dependent144843 Dependent { get; set; }
    }

    public abstract class DependentManyToManySelf
    {
        public decimal? Key1 { get; set; }
    }

    public class DerivedDependentManyToManySelf : DependentManyToManySelf
    {
        public float DerivedProperty1 { get; set; }
    }

    public class DependentSelfRefInverseRequired
    {
        public string Key1 { get; set; }
        public string Key2 { get; set; }

        [InverseProperty("PrincipalNavigation")]
        public DependentSelfRefInverseRequired DependentNavigation { get; set; }

        [Required]
        public DependentSelfRefInverseRequired PrincipalNavigation { get; set; }
    }

    public class Principal144934
    {
        public string Key1 { get; set; }

        [InverseProperty("PrincipalNavigation")]
        public Dependent144934 DependentNavigation { get; set; }
    }

    public class Dependent144934
    {
        public string PrincipalNavigationKey1 { get; set; }

        [Required]
        public Principal144934 PrincipalNavigation { get; set; }
    }

    public class ProductManyToManyTableNaming
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<SupplierManyToManyTableNaming> Suppliers { get; set; }
    }

    public class SupplierManyToManyTableNaming
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ProductManyToManyTableNaming> Products { get; set; }
    }

    public class DependentWithNullableFk
    {
        public int Id { get; set; }
        public string PrincipalNavigationId { get; set; }
        public PrincipalWithNullableFk PrincipalNavigation { get; set; }
    }

    public class PrincipalWithNullableFk
    {
        public string Id { get; set; }
        public ICollection<DependentWithNullableFk> DependentNavigation { get; set; }
    }

    public class DependentWithNullableFkIdentifying
    {
        public int? Id { get; set; }
        public PrincipalWithNullableFkIdentifying PrincipalNavigation { get; set; }
    }

    public class PrincipalWithNullableFkIdentifying
    {
        public int? Id { get; set; }
        public DependentWithNullableFkIdentifying DependentNavigation { get; set; }
    }

    // Association from base to derived type

    public class Repro150565_Dependent : Repro150565_BaseDependent
    {
        public decimal? BaseDependentKey1 { get; set; }
        public Repro150565_BaseDependent PrincipalNavigation { get; set; }
    }

    public class Repro150565_BaseDependent
    {
        public Guid BaseProperty { get; set; }
        public decimal? Key1 { get; set; }
        public ICollection<Repro150565_Dependent> DependentNavigation { get; set; }
    }

    public class Dependent_6927
    {
        public string DependentForeignKeyPropertyNotFromConvention1 { get; set; }
        public int Id { get; set; }

        [ForeignKey("DependentForeignKeyPropertyNotFromConvention1")]
        public Principal_6927 PrincipalNavigation { get; set; }
    }

    public class Principal_6927
    {
        public string Key1 { get; set; }
        public ICollection<Dependent_6927> DependentNavigation { get; set; }
    }

    public class Dependent_162348
    {
        public short Key1 { get; set; }
        public short? PrincipalNavigationKey1 { get; set; }
        public Dependent_162348 PrincipalNavigation { get; set; }
    }

    public class Order_181909
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public virtual ICollection<OrderLine_181909> Lines { get; set; }
    }

    public class OrderLine_181909
    {
        public int OrderLineId { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public virtual Order_181909 Order { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
    }

    public class Principal_181909
    {
        public int Id { get; set; }
        public virtual Dependent_181909 Order { get; set; }
    }

    public class Dependent_181909
    {
        public string Key { get; set; }
        public int Principal_181909Id { get; set; }

        [Required]
        public virtual Principal_181909 Order { get; set; }
    }

    public class Principal_159001
    {
        public int Key1 { get; set; }
    }

    public class Dependent_159001a
    {
        [ForeignKey("PrincipalNavigation")]
        public int DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        [Required]
        public Principal_159001 PrincipalNavigation { get; set; }
    }

    public class Dependent_159001b
    {
        public int DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        [Required]
        [ForeignKey("DependentForeignKeyPropertyNotFromConvention1")]
        public Principal_159001 PrincipalNavigation { get; set; }
    }

    public class PrincipalWithNav_159001a
    {
        public int Key1 { get; set; }

        [Required]
        public DependentWithNav_159001a DependentNavigation { get; set; }
    }

    public class DependentWithNav_159001a
    {
        [ForeignKey("PrincipalNavigation")]
        public int DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        [Required]
        public PrincipalWithNav_159001a PrincipalNavigation { get; set; }
    }

    public class Dependent_172949
    {
        public int Id { get; set; }

        [ForeignKey("PrincipalNavigation")]
        public short DependentForeignKeyPropertyNotFromConvention1 { get; set; }

        public Principal_172949 PrincipalNavigation { get; set; }
    }

    public class Principal_172949
    {
        public short? Id { get; set; }

        [InverseProperty("PrincipalNavigation")]
        public ICollection<Dependent_172949> DependentNavigation { get; set; }
    }

    public class TableSharing1
    {
        public int Id { get; set; }
        public int Name { get; set; }
    }

    public class TableSharing2
    {
        public int Id { get; set; }
        public byte[] Picture { get; set; }
        public TableSharing1 BackRef { get; set; }
    }

    [Table("SharedTable")]
    public class TableSharing1A
    {
        public int Id { get; set; }
        public int Name { get; set; }
    }

    [Table("SharedTable")]
    public class TableSharing2A
    {
        public int Id { get; set; }
        public byte[] Picture { get; set; }
        public TableSharing1A BackRef { get; set; }
    }

    public class OneToOneResult
    {
        public int OneToOneResultId { get; set; }

        [ForeignKey("OneToOneResultId")]
        public virtual OneToOneResultDetail Detail { get; set; }
    }

    public class OneToOneResultDetail
    {
        [Key]
        public int OneToOneResultId { get; set; }

        public DateTime DataDate { get; set; }
    }

    #endregion

    #region Model for Dev11 287430

    [Table("person", Schema = "domain")]
    public class Person287430
    {
        [Key]
        [Column("identifier", TypeName = "nvarchar")]
        [StringLength(36, MinimumLength = 36)]
        public string Identifier { get; private set; }

        [InverseProperty("Persons")]
        public virtual ICollection<Role287430> Roles { get; set; }
    }

    [Table("role", Schema = "domain")]
    public class Role287430
    {
        [Key]
        [Column("identifier", TypeName = "nvarchar")]
        [StringLength(36, MinimumLength = 36)]
        public string Identifier { get; set; }

        [InverseProperty("Roles")]
        public virtual ICollection<Person287430> Persons { get; set; }
    }

    #endregion

    public class APerson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Biography")]
        public string Bio { get; set; }

        public Event Birth { get; set; }
        public Event Death { get; set; }

        public APerson Mother { get; set; }
        public APerson Father { get; set; }

        public GenderType Gender { get; set; }
    }

    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [DataType(DataType.Text)]
        public string Location { get; set; }
    }

    public enum GenderType
    {
        Male,
        Female
    }

    public class Marriage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Person Husband { get; set; }

        [Required]
        public Person Wife { get; set; }

        [Required]
        public Event WeddingDay { get; set; }
    }
}
