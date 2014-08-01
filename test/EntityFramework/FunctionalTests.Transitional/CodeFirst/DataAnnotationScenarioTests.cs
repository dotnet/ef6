// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Linq;
    using System.Linq.Expressions;
    using FunctionalTests.Model;
    using SimpleModel;
    using Xunit;
    using Product = FunctionalTests.Model.Product;

    public class DataAnnotationScenarioTests : TestBase, IDisposable
    {
        public void Dispose()
        {
            DbConfiguration.DependencyResolver.GetService<AttributeProvider>().ClearCache();
        }

        public class Person
        {
            public int Id { get; set; }

            [StringLength(5)]
            public string Name { get; set; }
        }

        public class Employee : Person
        {
        }
        
        [Fact]
        public void Explicit_configuration_on_derived_type_overrides_annotation_on_unmapped_base_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<Employee>()
                .Property(p => p.Name)
                .HasMaxLength(10);

            var databaseMapping = BuildMapping(modelBuilder);
            
            databaseMapping.AssertValid();
            databaseMapping.Assert<Employee>(e => e.Name).FacetEqual(10, p => p.MaxLength);
        }

        [Fact]
        public void Explicit_configuration_on_derived_type_overrides_annotation_on_mapped_base_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<Person>();

            modelBuilder
                .Entity<Employee>()
                .Property(p => p.Name)
                .HasMaxLength(10);

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
            databaseMapping.Assert<Employee>(e => e.Name).FacetEqual(10, p => p.MaxLength);
        }

        [Fact]
        public void Explicit_configuration_on_derived_type_throws_when_conflict_with_mapped_base_type()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder
                .Entity<Person>()
                .Property(p => p.Name)
                .HasMaxLength(5);

            modelBuilder
                .Entity<Employee>()
                .Property(p => p.Name)
                .HasMaxLength(10);

            Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder));
        }
        
        [Fact]
        public void Duplicate_column_order_should_not_throw_when_v1_convention_set()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V4_1);

            modelBuilder.Entity<Entity_10558>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.AssertValid();
        }

        [Fact]
        public void Duplicate_column_order_should_throw_when_v2_convention_set()
        {
            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V5_0);

            modelBuilder.Entity<Entity_10558>();

            Assert.Throws<InvalidOperationException>(
                () => BuildMapping(modelBuilder))
                .ValidateMessage("DuplicateConfiguredColumnOrder", "Entity_10558");
        }

        public class Entity_10558
        {
            [Key]
            [Column(Order = 1)]
            public int Key1 { get; set; }

            [Key]
            [Column(Order = 1)]
            public int Key2 { get; set; }

            public string Name { get; set; }
        }

        [Fact]
        public void Non_public_annotations_are_enabled()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<PrivateMemberAnnotationClass>().Property(
                PrivateMemberAnnotationClass.PersonFirstNameExpr);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert(PrivateMemberAnnotationClass.PersonFirstNameObjectExpr)
                .DbEqual("dsdsd", c => c.Name)
                .DbEqual("nvarchar", c => c.TypeName)
                .DbEqual(128, f => f.MaxLength)
                .DbEqual(true, c => c.IsPrimaryKeyColumn);
        }

        public class PrivateMemberAnnotationClass
        {
            public static Expression<Func<PrivateMemberAnnotationClass, string>> PersonFirstNameExpr =
                p => p.PersonFirstName;

            public static Expression<Func<PrivateMemberAnnotationClass, object>> PersonFirstNameObjectExpr =
                p => p.PersonFirstName;

            [Key]
            [Column("dsdsd", Order = 1, TypeName = "nvarchar")]
            private string PersonFirstName { get; set; }
        }

        [Fact]
        public void NotMapped_should_propagate_down_inheritance_hierarchy()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<NotMappedDerived>();

            Assert.Throws<InvalidOperationException>(
                () => BuildMapping(modelBuilder))
                .ValidateMessage("InvalidEntityType", typeof(NotMappedDerived));
        }

        [NotMapped]
        public class NotMappedBase
        {
            public int Id { get; set; }
        }

        public class NotMappedDerived : NotMappedBase
        {
        }

        [Fact]
        public void NotMapped_on_base_class_property_ignores_it()
        {
            using (var baseEntityConfiguration = new DynamicTypeDescriptionConfiguration<BaseEntity>())
            {
                var modelBuilder = new DbModelBuilder();

                baseEntityConfiguration.SetPropertyAttributes(b => b.BaseClassProperty, new NotMappedAttribute());
                baseEntityConfiguration.SetPropertyAttributes(b => b.VirtualBaseClassProperty, new NotMappedAttribute());

                modelBuilder.Entity<Unit>();
                modelBuilder.Entity<BaseEntity>();

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                Assert.False(
                    databaseMapping.Model.EntityTypes.SelectMany(e => e.Properties)
                        .Any(p => p.Name == "BaseClassProperty"));
                Assert.False(
                    databaseMapping.Model.EntityTypes.SelectMany(e => e.Properties)
                        .Any(p => p.Name == "VirtualBaseClassProperty"));
            }
        }

        [Fact]
        public void NotMapped_on_base_class_property_and_overriden_property_ignores_them()
        {
            using (var baseEntityConfiguration = new DynamicTypeDescriptionConfiguration<BaseEntity>())
            {
                using (var unitConfiguration = new DynamicTypeDescriptionConfiguration<Unit>())
                {
                    var modelBuilder = new DbModelBuilder();

                    unitConfiguration.SetPropertyAttributes(b => b.VirtualBaseClassProperty, new NotMappedAttribute());
                    baseEntityConfiguration.SetPropertyAttributes(b => b.VirtualBaseClassProperty, new NotMappedAttribute());

                    modelBuilder.Entity<Unit>();
                    modelBuilder.Entity<BaseEntity>();

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.False(
                        databaseMapping.Model.EntityTypes.SelectMany(e => e.Properties)
                            .Any(p => p.Name == "VirtualBaseClassProperty"));
                }
            }
        }

        [Fact]
        public void NotMapped_on_base_class_property_discovered_through_navigation_ignores_it()
        {
            using (var abstractBaseEntityConfiguration = new DynamicTypeDescriptionConfiguration<AbstractBaseEntity>())
            {
                var modelBuilder = new DbModelBuilder();

                abstractBaseEntityConfiguration.SetPropertyAttributes(b => b.AbstractBaseClassProperty, new NotMappedAttribute());

                modelBuilder.Entity<Unit>();

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                Assert.True(databaseMapping.Model.EntityTypes.Any(e => e.Name == "AbstractBaseEntity"));
                Assert.False(
                    databaseMapping.Model.EntityTypes.SelectMany(e => e.Properties)
                        .Any(p => p.Name == "AbstractBaseClassProperty"));
            }
        }

        [Fact]
        public void NotMapped_on_abstract_base_class_property_ignores_it()
        {
            using (var abstractBaseEntityConfiguration = new DynamicTypeDescriptionConfiguration<AbstractBaseEntity>())
            {
                var modelBuilder = new DbModelBuilder();

                abstractBaseEntityConfiguration.SetPropertyAttributes(b => b.AbstractBaseClassProperty, new NotMappedAttribute());

                modelBuilder.Entity<AbstractBaseEntity>();
                modelBuilder.Entity<BaseEntity>();
                modelBuilder.Entity<Unit>();

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                Assert.False(
                    databaseMapping.Model.EntityTypes.SelectMany(e => e.Properties)
                        .Any(p => p.Name == "AbstractBaseClassProperty"));
            }
        }

        [Fact]
        public void NotMapped_on_overriden_mapped_base_class_property_throws()
        {
            using (var unitConfiguration = new DynamicTypeDescriptionConfiguration<Unit>())
            {
                var modelBuilder = new DbModelBuilder();

                unitConfiguration.SetPropertyAttributes(b => b.VirtualBaseClassProperty, new NotMappedAttribute());

                modelBuilder.Ignore<DifferentUnit>();
                modelBuilder.Entity<Unit>();
                modelBuilder.Entity<BaseEntity>();

                Assert.Throws<InvalidOperationException>(() => BuildMapping(modelBuilder))
                    .ValidateMessage(
                        "CannotIgnoreMappedBaseProperty",
                        "VirtualBaseClassProperty", "FunctionalTests.DataAnnotationScenarioTests+Unit",
                        "FunctionalTests.DataAnnotationScenarioTests+BaseEntity");
            }
        }

        [Fact]
        public void NotMapped_on_unmapped_derived_property_ignores_it()
        {
            using (var unitConfiguration = new DynamicTypeDescriptionConfiguration<Unit>())
            {
                var modelBuilder = new DbModelBuilder();

                unitConfiguration.SetPropertyAttributes(b => b.VirtualBaseClassProperty, new NotMappedAttribute());

                modelBuilder.Ignore<AbstractBaseEntity>();
                modelBuilder.Ignore<BaseEntity>();
                modelBuilder.Entity<Unit>();

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                Assert.False(
                    databaseMapping.Model.EntityTypes.Single().Properties.Any(
                        p => p.Name == "VirtualBaseClassProperty"));
            }
        }

        [Fact]
        public void NotMapped_on_unmapped_base_class_property_and_overriden_property_ignores_it()
        {
            using (var unitConfiguration = new DynamicTypeDescriptionConfiguration<Unit>())
            {
                using (var baseEntityConfiguration = new DynamicTypeDescriptionConfiguration<BaseEntity>())
                {
                    var modelBuilder = new DbModelBuilder();

                    baseEntityConfiguration.SetPropertyAttributes(b => b.VirtualBaseClassProperty, new NotMappedAttribute());
                    unitConfiguration.SetPropertyAttributes(b => b.VirtualBaseClassProperty, new NotMappedAttribute());

                    modelBuilder.Ignore<AbstractBaseEntity>();
                    modelBuilder.Ignore<BaseEntity>();
                    modelBuilder.Entity<Unit>();

                    var databaseMapping = BuildMapping(modelBuilder);

                    databaseMapping.AssertValid();

                    Assert.False(
                        databaseMapping.Model.EntityTypes.Single().Properties.Any(
                            p => p.Name == "VirtualBaseClassProperty"));
                }
            }
        }

        [Fact]
        public void NotMapped_on_unmapped_base_class_property_ignores_it()
        {
            using (var baseEntityConfiguration = new DynamicTypeDescriptionConfiguration<BaseEntity>())
            {
                var modelBuilder = new DbModelBuilder();

                baseEntityConfiguration.SetPropertyAttributes(b => b.VirtualBaseClassProperty, new NotMappedAttribute());

                modelBuilder.Ignore<AbstractBaseEntity>();
                modelBuilder.Ignore<BaseEntity>();
                modelBuilder.Entity<Unit>();

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                Assert.False(
                    databaseMapping.Model.EntityTypes.Single().Properties.Any(
                        p => p.Name == "VirtualBaseClassProperty"));
            }
        }

        [Fact]
        public void NotMapped_on_new_property_with_same_name_as_in_unmapped_base_class_ignores_it()
        {
            using (var differentUnitConfiguration = new DynamicTypeDescriptionConfiguration<DifferentUnit>())
            {
                var modelBuilder = new DbModelBuilder();

                differentUnitConfiguration.SetPropertyAttributes(b => b.VirtualBaseClassProperty, new NotMappedAttribute());

                modelBuilder.Entity<DifferentUnit>();

                var databaseMapping = BuildMapping(modelBuilder);

                databaseMapping.AssertValid();

                Assert.False(
                    databaseMapping.Model.EntityTypes.Single().Properties.Any(
                        p => p.Name == "VirtualBaseClassProperty"));
            }
        }

        public abstract class AbstractBaseEntity
        {
            public long Id { get; set; }
            public abstract string AbstractBaseClassProperty { get; set; }
        }

        public class BaseEntity : AbstractBaseEntity
        {
            public string BaseClassProperty { get; set; }
            public virtual string VirtualBaseClassProperty { get; set; }
            public override string AbstractBaseClassProperty { get; set; }
        }

        public class Unit : BaseEntity
        {
            public override string VirtualBaseClassProperty { get; set; }
            public virtual AbstractBaseEntity Related { get; set; }
        }

        public class DifferentUnit : BaseEntity
        {
            public new string VirtualBaseClassProperty { get; set; }
        }

        [Fact]
        public void MaxLength_takes_presedence_over_StringLength()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MaxLengthAnnotationClass>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<MaxLengthAnnotationClass>(x => x.PersonFirstName).FacetEqual(true, a => a.IsMaxLength);
        }

        public class MaxLengthAnnotationClass
        {
            public int Id { get; set; }

            [StringLength(500)]
            [MaxLength]
            public string PersonFirstName { get; set; }
        }

        [Fact]
        public void MaxLength_with_length_takes_precedence_over_StringLength()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<MaxLengthWithLengthAnnotationClass>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<MaxLengthWithLengthAnnotationClass>(x => x.PersonFirstName).FacetEqual(
                30,
                a =>
                a.MaxLength);
        }

        public class MaxLengthWithLengthAnnotationClass
        {
            public int Id { get; set; }

            [StringLength(500)]
            [MaxLength(30)]
            public string PersonFirstName { get; set; }
        }

        [Fact]
        public void Default_length_for_key_string_column()
        {
            using (var loginConfiguration = new DynamicTypeDescriptionConfiguration<Login>())
            {
                loginConfiguration.SetPropertyAttributes(l => l.UserName, new KeyAttribute());
                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<Login>();
                modelBuilder.Ignore<Profile>();

                var databaseMapping = BuildMapping(modelBuilder);
                databaseMapping.AssertValid();

                databaseMapping.Assert<Login>(x => x.UserName)
                    .DbEqual("nvarchar", c => c.TypeName)
                    .DbEqual(128, f => f.MaxLength);
            }
        }

        [Fact]
        public void Key_and_column_work_together()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ColumnKeyAnnotationClass>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<ColumnKeyAnnotationClass>(x => x.PersonFirstName)
                .DbEqual("dsdsd", c => c.Name)
                .DbEqual("nvarchar", c => c.TypeName)
                .DbEqual(128, f => f.MaxLength)
                .DbEqual(true, c => c.IsPrimaryKeyColumn);
        }

        [Fact]
        // Regression test for Dev11 Bug 87347
        public void Key_and_column_work_together_in_an_IA()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ColumnKeyAnnotationClass>();
            modelBuilder.Entity<ReferencingClass>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<ColumnKeyAnnotationClass>(x => x.PersonFirstName)
                .DbEqual("dsdsd", c => c.Name)
                .DbEqual("nvarchar", c => c.TypeName)
                .DbEqual(128, f => f.MaxLength)
                .DbEqual(true, c => c.IsPrimaryKeyColumn);

            Assert.Equal(1, databaseMapping.Model.AssociationTypes.Count());
            databaseMapping.Assert<ReferencingClass>().ForeignKeyColumn("Person_PersonFirstName")
                .DbEqual("nvarchar", c => c.TypeName)
                .DbEqual(128, f => f.MaxLength);
        }

        [Fact]
        public void Key_nvarchar_column_and_MaxLength_64_produce_nvarchar_64()
        {
            using (var entityClassConfiguration = new DynamicTypeDescriptionConfiguration<ColumnKeyAnnotationClass>())
            {
                entityClassConfiguration.SetPropertyAttributes(c => c.PersonFirstName, new MaxLengthAttribute(64));

                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<ColumnKeyAnnotationClass>();

                var databaseMapping = BuildMapping(modelBuilder);
                databaseMapping.AssertValid();

                databaseMapping.Assert<ColumnKeyAnnotationClass>(x => x.PersonFirstName)
                    .DbEqual("dsdsd", c => c.Name)
                    .DbEqual("nvarchar", c => c.TypeName)
                    .DbEqual(64, f => f.MaxLength)
                    .DbEqual(true, c => c.IsPrimaryKeyColumn);
            }
        }

        [Fact]
        public void Key_nvarchar_column_and_unbounded_MaxLength_produce_nvarchar_max()
        {
            using (var entityClassConfiguration = new DynamicTypeDescriptionConfiguration<ColumnKeyAnnotationClass>())
            {
                entityClassConfiguration.SetPropertyAttributes(c => c.PersonFirstName, new MaxLengthAttribute());

                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<ColumnKeyAnnotationClass>();

                var databaseMapping = BuildMapping(modelBuilder);
                databaseMapping.AssertValid();

                databaseMapping.Assert<ColumnKeyAnnotationClass>(x => x.PersonFirstName)
                    .DbEqual("dsdsd", c => c.Name)
                    .DbEqual("nvarchar", c => c.TypeName)
                    .DbEqual(true, f => f.IsMaxLength)
                    .DbEqual(null, f => f.MaxLength)
                    .DbEqual(true, c => c.IsPrimaryKeyColumn);
            }
        }

        [Fact]
        public void Key_nvarchar_column_and_no_MaxLength_produce_nvarchar_128()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ColumnKeyAnnotationClass>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<ColumnKeyAnnotationClass>(x => x.PersonFirstName)
                .DbEqual("dsdsd", c => c.Name)
                .DbEqual("nvarchar", c => c.TypeName)
                .DbEqual(128, f => f.MaxLength)
                .DbEqual(true, c => c.IsPrimaryKeyColumn);
        }

        [Fact]
        public void Key_nvarchar_column_and_MaxLength_work_together_in_an_IA()
        {
            using (var entityClassConfiguration = new DynamicTypeDescriptionConfiguration<ColumnKeyAnnotationClass>())
            {
                entityClassConfiguration.SetPropertyAttributes(c => c.PersonFirstName, new MaxLengthAttribute(64));

                var modelBuilder = new DbModelBuilder();

                modelBuilder.Entity<ColumnKeyAnnotationClass>();
                modelBuilder.Entity<ReferencingClass>();

                var databaseMapping = BuildMapping(modelBuilder);
                databaseMapping.AssertValid();

                databaseMapping.Assert<ColumnKeyAnnotationClass>(x => x.PersonFirstName)
                    .DbEqual("dsdsd", c => c.Name)
                    .DbEqual("nvarchar", c => c.TypeName)
                    .DbEqual(64, f => f.MaxLength)
                    .DbEqual(true, c => c.IsPrimaryKeyColumn);

                Assert.Equal(1, databaseMapping.Model.AssociationTypes.Count());
                databaseMapping.Assert<ReferencingClass>().ForeignKeyColumn("Person_PersonFirstName")
                    .DbEqual("nvarchar", c => c.TypeName)
                    .DbEqual(64, f => f.MaxLength);
            }
        }

        public class ColumnKeyAnnotationClass
        {
            [Key]
            [Column("dsdsd", Order = 1, TypeName = "nvarchar")]
            public string PersonFirstName { get; set; }
        }

        public class ReferencingClass
        {
            public int Id { get; set; }
            public ColumnKeyAnnotationClass Person { get; set; }
        }
        
        [Fact]
        public void Nvarchar_max_column_produces_nvarchar_max()
        {
            using (var entityClassConfiguration = new DynamicTypeDescriptionConfiguration<TNAttrBase>())
            {
                entityClassConfiguration.SetPropertyAttributes(c => c.BaseData,
                    new ColumnAttribute { TypeName = "nvarchar(max)" });

                using (var context = new NvarcharMaxContext())
                {
                    context.CustomOnModelCreating = modelBuilder =>
                    {
                        modelBuilder.Entity<TNAttrBase>();

                        var databaseMapping = BuildMapping(modelBuilder);
                        databaseMapping.AssertValid();

                        databaseMapping.Assert<TNAttrBase>(x => x.BaseData)
                            .DbEqual("nvarchar(max)", c => c.TypeName)
                            .DbEqual(false, f => f.IsMaxLength)
                            .DbEqual(int.MaxValue / 2, f => f.MaxLength);
                    };

                    context.Database.CreateIfNotExists();

                    var column = GetInfoContext(context).Columns.Single(c => c.Name == "BaseData");

                    Assert.Equal("nvarchar", column.Type);
                    Assert.Equal(-1, column.MaxLength);
                }
            }
        }

        public class NvarcharMaxContext : EmptyContext
        {
        }

        [Fact]
        public void Nvarchar_max_column_and_unbounded_MaxLength_produce_nvarchar_max()
        {
            using (var entityClassConfiguration = new DynamicTypeDescriptionConfiguration<TNAttrBase>())
            {
                entityClassConfiguration.SetPropertyAttributes(
                    c => c.BaseData,
                    new MaxLengthAttribute(), new ColumnAttribute { TypeName = "nvarchar(max)" });

                using (var context = new NvarcharMaxMaxContext())
                {
                    context.CustomOnModelCreating = modelBuilder =>
                    {
                        modelBuilder.Entity<TNAttrBase>();

                        var databaseMapping = BuildMapping(modelBuilder);
                        databaseMapping.AssertValid();

                        databaseMapping.Assert<TNAttrBase>(x => x.BaseData)
                            .DbEqual("nvarchar(max)", c => c.TypeName)
                            .DbEqual(false, f => f.IsMaxLength)
                            .DbEqual(int.MaxValue / 2, f => f.MaxLength);
                    };

                    context.Database.CreateIfNotExists();

                    var column = GetInfoContext(context).Columns.Single(c => c.Name == "BaseData");

                    Assert.Equal("nvarchar", column.Type);
                    Assert.Equal(-1, column.MaxLength);
                }
            }
        }

        public class NvarcharMaxMaxContext : EmptyContext
        {
        }

        [Fact]
        public void Nvarchar_max_column_and_MaxLength_64_produce_nvarchar_max()
        {
            using (var entityClassConfiguration = new DynamicTypeDescriptionConfiguration<TNAttrBase>())
            {
                entityClassConfiguration.SetPropertyAttributes(
                    c => c.BaseData,
                    new MaxLengthAttribute(64), new ColumnAttribute { TypeName = "nvarchar(max)" });

                using (var context = new NvarcharMax64Context())
                {
                    context.CustomOnModelCreating = modelBuilder =>
                    {
                        modelBuilder.Entity<TNAttrBase>();

                        var databaseMapping = BuildMapping(modelBuilder);
                        databaseMapping.AssertValid();

                        databaseMapping.Assert<TNAttrBase>(x => x.BaseData)
                            .DbEqual("nvarchar(max)", c => c.TypeName)
                            .DbEqual(false, f => f.IsMaxLength)
                            .DbEqual(int.MaxValue / 2, f => f.MaxLength);
                    };

                    context.Database.CreateIfNotExists();

                    var column = GetInfoContext(context).Columns.Single(c => c.Name == "BaseData");

                    Assert.Equal("nvarchar", column.Type);
                    Assert.Equal(-1, column.MaxLength);
                }
            }
        }

        public class NvarcharMax64Context : EmptyContext
        {
        }

        [Fact]
        public void Nvarchar_column_produces_nvarchar_4000()
        {
            using (var entityClassConfiguration = new DynamicTypeDescriptionConfiguration<TNAttrBase>())
            {
                entityClassConfiguration.SetPropertyAttributes(
                    c => c.BaseData,
                    new ColumnAttribute { TypeName = "nvarchar" });

                using (var context = new NvarcharContext())
                {
                    context.CustomOnModelCreating = modelBuilder =>
                    {
                        modelBuilder.Entity<TNAttrBase>();

                        var databaseMapping = BuildMapping(modelBuilder);
                        databaseMapping.AssertValid();

                        databaseMapping.Assert<TNAttrBase>(x => x.BaseData)
                            .DbEqual("nvarchar", c => c.TypeName)
                            .DbEqual(4000, f => f.MaxLength)
                            .DbEqual(false, f => f.IsMaxLength);
                    };

                    context.Database.CreateIfNotExists();

                    var column = GetInfoContext(context).Columns.Single(c => c.Name == "BaseData");

                    Assert.Equal("nvarchar", column.Type);
                    Assert.Equal(4000, column.MaxLength);
                }
            }
        }

        public class NvarcharContext : EmptyContext
        {
        }

        [Fact]
        public void Nvarchar_column_and_unbounded_MaxLength_produce_nvarchar_4000()
        {
            using (var entityClassConfiguration = new DynamicTypeDescriptionConfiguration<TNAttrBase>())
            {
                entityClassConfiguration.SetPropertyAttributes(
                    c => c.BaseData,
                    new MaxLengthAttribute(), new ColumnAttribute { TypeName = "nvarchar" });

                using (var context = new NvarcharUnboundedContext())
                {
                    context.CustomOnModelCreating = modelBuilder =>
                    {
                        modelBuilder.Entity<TNAttrBase>();

                        var databaseMapping = BuildMapping(modelBuilder);
                        databaseMapping.AssertValid();

                        databaseMapping.Assert<TNAttrBase>(x => x.BaseData)
                            .DbEqual("nvarchar", c => c.TypeName)
                            .DbEqual(null, f => f.MaxLength)
                            .DbEqual(true, f => f.IsMaxLength);
                    };

                    context.Database.CreateIfNotExists();

                    var column = GetInfoContext(context).Columns.Single(c => c.Name == "BaseData");

                    Assert.Equal("nvarchar", column.Type);
                    Assert.Equal(4000, column.MaxLength);
                }
            }
        }

        public class NvarcharUnboundedContext : EmptyContext
        {
        }

        [Fact]
        public void Nvarchar_column_and_MaxLength_64_produce_nvarchar_64()
        {
            using (var entityClassConfiguration = new DynamicTypeDescriptionConfiguration<TNAttrBase>())
            {
                entityClassConfiguration.SetPropertyAttributes(
                    c => c.BaseData,
                    new MaxLengthAttribute(64), new ColumnAttribute { TypeName = "nvarchar" });

                using (var context = new Nvarchar64Context())
                {
                    context.CustomOnModelCreating = modelBuilder =>
                    {
                        modelBuilder.Entity<TNAttrBase>();

                        var databaseMapping = BuildMapping(modelBuilder);
                        databaseMapping.AssertValid();

                        databaseMapping.Assert<TNAttrBase>(x => x.BaseData)
                            .DbEqual("nvarchar", c => c.TypeName)
                            .DbEqual(64, f => f.MaxLength)
                            .DbEqual(false, f => f.IsMaxLength);
                    };

                    context.Database.CreateIfNotExists();

                    var column = GetInfoContext(context).Columns.Single(c => c.Name == "BaseData");

                    Assert.Equal("nvarchar", column.Type);
                    Assert.Equal(64, column.MaxLength);
                }
            }
        }

        public class Nvarchar64Context : EmptyContext
        {
        }

        [Fact]
        public void Key_from_base_type_is_recognized()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<SRelated>();
            modelBuilder.Entity<OKeyBase>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            // one thing configured because of key property
            Assert.Equal(1, modelBuilder.ModelConfiguration.Entity(typeof(OKeyBase)).ConfiguredProperties.Count());
            Assert.Equal(1, modelBuilder.ModelConfiguration.Entity(typeof(OKeyBase)).KeyProperties.Count());

            // derived type should have equivalent configuration, but no key property
            Assert.Equal(1, modelBuilder.ModelConfiguration.Entity(typeof(DODerived)).ConfiguredProperties.Count());
            Assert.Equal(0, modelBuilder.ModelConfiguration.Entity(typeof(DODerived)).KeyProperties.Count());
        }

        [Fact]
        public void Key_from_base_type_is_recognized_if_base_discovered_first()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<OKeyBase>();
            modelBuilder.Entity<SRelated>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            // one thing configured because of key property
            Assert.Equal(1, modelBuilder.ModelConfiguration.Entity(typeof(OKeyBase)).ConfiguredProperties.Count());
            Assert.Equal(1, modelBuilder.ModelConfiguration.Entity(typeof(OKeyBase)).KeyProperties.Count());

            // derived type should have equivalent configuration, but no key property
            Assert.Equal(1, modelBuilder.ModelConfiguration.Entity(typeof(DODerived)).ConfiguredProperties.Count());
            Assert.Equal(0, modelBuilder.ModelConfiguration.Entity(typeof(DODerived)).KeyProperties.Count());
        }

        public class SRelated
        {
            public int SRelatedId { get; set; }
            public ICollection<DODerived> DADeriveds { get; set; }
        }

        public class OKeyBase
        {
            [Key]
            public int OrderLineNo { get; set; }

            public int Quantity { get; set; }
        }

        public class DODerived : OKeyBase
        {
            public SRelated DARelated { get; set; }
            public string Special { get; set; }
        }

        [Fact]
        public void Key_on_nav_prop_is_ignored()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<KeyOnNavProp>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<KeyOnNavProp>().DbEqual("Id", t => t.KeyProperties.Single().Name);
        }

        public class DASimple
        {
            public int Id { get; set; }
        }

        public class KeyOnNavProp
        {
            public int Id { get; set; }

            [Key]
            public ICollection<DASimple> Simples { get; set; }

            [Key]
            public DASimple SpecialSimple { get; set; }
        }

        [Fact]
        public void Timestamp_takes_precedence_over_MaxLength()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<TimestampAndMaxlen>().Ignore(x => x.NonMaxTimestamp);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<TimestampAndMaxlen>().DbEqual(
                "rowversion",
                t =>
                t.Properties.Single(x => x.Name == "MaxTimestamp").
                    TypeName);

            databaseMapping.Assert<TimestampAndMaxlen>().DbEqual(
                false,
                t =>
                t.Properties.Single(x => x.Name == "MaxTimestamp").IsMaxLength);

            databaseMapping.Assert<TimestampAndMaxlen>().DbEqual(
                true,
                t =>
                t.Properties.Single(x => x.Name == "MaxTimestamp").IsMaxLengthConstant);
        }

        [Fact]
        public void Timestamp_takes_precedence_over_MaxLength_with_value()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<TimestampAndMaxlen>().Ignore(x => x.MaxTimestamp);

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<TimestampAndMaxlen>().DbEqual(
                "rowversion",
                t =>
                t.Properties.Single(x => x.Name == "NonMaxTimestamp").
                    TypeName);

            databaseMapping.Assert<TimestampAndMaxlen>().DbEqual(
                false,
                t =>
                t.Properties.Single(x => x.Name == "NonMaxTimestamp").IsMaxLength);

            databaseMapping.Assert<TimestampAndMaxlen>().DbEqual(
                8,
                t =>
                t.Properties.Single(x => x.Name == "NonMaxTimestamp").MaxLength);
        }

        public class TimestampAndMaxlen
        {
            public int Id { get; set; }

            [MaxLength]
            [Timestamp]
            public byte[] MaxTimestamp { get; set; }

            [MaxLength(100)]
            [Timestamp]
            public byte[] NonMaxTimestamp { get; set; }
        }

        [Fact]
        public void Annotation_in_derived_class_when_base_class_processed_after_derived_class()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<StyledProduct>();
            modelBuilder.Entity<Product>();

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<StyledProduct>(s => s.Style).FacetEqual(150, f => f.MaxLength);
        }

        [Fact]
        public void Required_and_ForeignKey_to_Required()
        {
            using (var loginConfiguration = new DynamicTypeDescriptionConfiguration<Login>())
            {
                using (var profileConfiguration = new DynamicTypeDescriptionConfiguration<Profile>())
                {
                    loginConfiguration.SetPropertyAttributes(
                        l => l.Profile, new RequiredAttribute(),
                        new ForeignKeyAttribute("LoginId"));
                    profileConfiguration.SetPropertyAttributes(p => p.User, new RequiredAttribute());

                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Login>();
                    modelBuilder.Entity<Profile>();

                    Assert.Throws<InvalidOperationException>(
                        () => BuildMapping(modelBuilder))
                        .ValidateMessage("UnableToDeterminePrincipal", typeof(Login), typeof(Profile));
                }
            }
        }

        [Fact]
        // Regression test for Dev11 Bug 94993
        public void Required_to_Required_and_ForeignKey()
        {
            using (var loginConfiguration = new DynamicTypeDescriptionConfiguration<Login>())
            {
                using (var profileConfiguration = new DynamicTypeDescriptionConfiguration<Profile>())
                {
                    loginConfiguration.SetPropertyAttributes(l => l.Profile, new RequiredAttribute());
                    profileConfiguration.SetPropertyAttributes(
                        p => p.User, new RequiredAttribute(),
                        new ForeignKeyAttribute("ProfileId"));

                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Login>();
                    modelBuilder.Entity<Profile>();

                    Assert.Throws<InvalidOperationException>(
                        () => BuildMapping(modelBuilder))
                        .ValidateMessage("UnableToDeterminePrincipal", typeof(Login), typeof(Profile));
                }
            }
        }

        [Fact]
        public void Required_and_ForeignKey_to_Required_and_ForeignKey()
        {
            using (var loginConfiguration = new DynamicTypeDescriptionConfiguration<Login>())
            {
                using (var profileConfiguration = new DynamicTypeDescriptionConfiguration<Profile>())
                {
                    loginConfiguration.SetPropertyAttributes(
                        l => l.Profile, new RequiredAttribute(),
                        new ForeignKeyAttribute("LoginId"));
                    profileConfiguration.SetPropertyAttributes(
                        p => p.User, new RequiredAttribute(),
                        new ForeignKeyAttribute("ProfileId"));

                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Login>();
                    modelBuilder.Entity<Profile>();

                    Assert.Throws<InvalidOperationException>(
                        () => BuildMapping(modelBuilder))
                        .ValidateMessage("UnableToDeterminePrincipal", typeof(Login), typeof(Profile));
                }
            }
        }

        [Fact]
        public void ForeignKey_to_nothing()
        {
            using (var loginConfiguration = new DynamicTypeDescriptionConfiguration<Login>())
            {
                using (new DynamicTypeDescriptionConfiguration<Profile>())
                {
                    loginConfiguration.SetPropertyAttributes(l => l.Profile, new ForeignKeyAttribute("LoginId"));

                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Login>();
                    modelBuilder.Entity<Profile>();

                    Assert.Throws<InvalidOperationException>(
                        () => BuildMapping(modelBuilder))
                        .ValidateMessage("UnableToDeterminePrincipal", typeof(Profile), typeof(Login));
                }
            }
        }

        [Fact]
        public void Required_and_ForeignKey_to_nothing()
        {
            using (var loginConfiguration = new DynamicTypeDescriptionConfiguration<Login>())
            {
                using (new DynamicTypeDescriptionConfiguration<Profile>())
                {
                    loginConfiguration.SetPropertyAttributes(l => l.Profile, new ForeignKeyAttribute("LoginId"));

                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Login>();
                    modelBuilder.Entity<Profile>();

                    Assert.Throws<InvalidOperationException>(
                        () => BuildMapping(modelBuilder))
                        .ValidateMessage("UnableToDeterminePrincipal", typeof(Profile), typeof(Login));
                }
            }
        }

        [Fact]
        public void Nothing_to_ForeignKey()
        {
            using (var loginConfiguration = new DynamicTypeDescriptionConfiguration<Login>())
            {
                using (var profileConfiguration = new DynamicTypeDescriptionConfiguration<Profile>())
                {
                    loginConfiguration.SetPropertyAttributes(l => l.Profile);
                    profileConfiguration.SetPropertyAttributes(p => p.User, new ForeignKeyAttribute("ProfileId"));

                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Login>();
                    modelBuilder.Entity<Profile>();

                    Assert.Throws<InvalidOperationException>(
                        () => BuildMapping(modelBuilder))
                        .ValidateMessage("UnableToDeterminePrincipal", typeof(Profile), typeof(Login));
                }
            }
        }

        [Fact]
        public void Nothing_to_Required_and_ForeignKey()
        {
            using (var loginConfiguration = new DynamicTypeDescriptionConfiguration<Login>())
            {
                using (var profileConfiguration = new DynamicTypeDescriptionConfiguration<Profile>())
                {
                    loginConfiguration.SetPropertyAttributes(l => l.Profile);
                    profileConfiguration.SetPropertyAttributes(
                        p => p.User, new RequiredAttribute(),
                        new ForeignKeyAttribute("ProfileId"));

                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Login>();
                    modelBuilder.Entity<Profile>();

                    var databaseMapping = BuildMapping(modelBuilder);
                    databaseMapping.AssertValid();

                    var association = databaseMapping.Model.AssociationTypes.Single();
                    Assert.Equal("Profile", association.SourceEnd.GetEntityType().Name);
                    Assert.Equal(RelationshipMultiplicity.ZeroOrOne, association.SourceEnd.RelationshipMultiplicity);
                    Assert.Equal("Login", association.TargetEnd.GetEntityType().Name);
                    Assert.Equal(RelationshipMultiplicity.One, association.TargetEnd.RelationshipMultiplicity);
                    Assert.Equal("Profile", association.Constraint.ToRole.GetEntityType().Name);
                }
            }
        }

        [Fact]
        public void ForeignKey_to_ForeignKey()
        {
            using (var loginConfiguration = new DynamicTypeDescriptionConfiguration<Login>())
            {
                using (var profileConfiguration = new DynamicTypeDescriptionConfiguration<Profile>())
                {
                    loginConfiguration.SetPropertyAttributes(l => l.Profile, new ForeignKeyAttribute("LoginId"));
                    profileConfiguration.SetPropertyAttributes(p => p.User, new ForeignKeyAttribute("ProfileId"));

                    var modelBuilder = new DbModelBuilder();

                    modelBuilder.Entity<Login>();
                    modelBuilder.Entity<Profile>();

                    Assert.Throws<InvalidOperationException>(
                        () => BuildMapping(modelBuilder))
                        .ValidateMessage("UnableToDeterminePrincipal", typeof(Profile), typeof(Login));
                }
            }
        }

        public class Login
        {
            public int LoginId { get; set; }
            public string UserName { get; set; }
            public virtual Profile Profile { get; set; }
        }

        public class Profile
        {
            public int ProfileId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public virtual Login User { get; set; }
        }

        [Fact]
        public void InversePropertyAttribute_on_inhereted_property_from_unmapped_class_does_not_throw()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<ConcreteRecord>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            var association = databaseMapping.Model.AssociationTypes.Single();
            Assert.Equal("ConcreteRecord", association.SourceEnd.GetEntityType().Name);
            Assert.Equal(RelationshipMultiplicity.One, association.SourceEnd.RelationshipMultiplicity);
            Assert.Equal("ConcreteRecord", association.TargetEnd.GetEntityType().Name);
            Assert.Equal(RelationshipMultiplicity.Many, association.TargetEnd.RelationshipMultiplicity);
            Assert.Equal("ConcreteRecord", association.Constraint.ToRole.GetEntityType().Name);
            Assert.Equal("ConcreteRecord", association.Constraint.FromRole.GetEntityType().Name);
        }

        public abstract class AbstractRecord
        {
            [Key]
            public long Id { get; set; }

            public long MasterId { get; set; }

            [ForeignKey("MasterId")]
            public virtual ConcreteRecord Master { get; set; }

            [InverseProperty("Master")]
            public virtual ICollection<ConcreteRecord> Suggestions { get; set; }
        }

        public class ConcreteRecord : AbstractRecord
        {
            public string Foo { get; set; }
        }

        [Fact]
        public void TableNameAttribute_affects_only_base_in_TPT()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<TNAttrBase>()
                .Map<TNAttrDerived>(mc => mc.ToTable("B"));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<TNAttrBase>("A");
            databaseMapping.Assert<TNAttrDerived>("B");
        }

        [Fact]
        public void TableNameAttribute_affects_table_name_in_TPH()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<TNAttrBase>()
                .Map(mc => mc.Requires("disc").HasValue("A"))
                .Map<TNAttrDerived>(mc => mc.Requires("disc").HasValue("B"));

            var databaseMapping = BuildMapping(modelBuilder);

            databaseMapping.Assert<TNAttrBase>("A");
            databaseMapping.AssertMapping<TNAttrBase>("A", false).HasColumnCondition("disc", "A");
            databaseMapping.Assert<TNAttrDerived>("A");
            databaseMapping.AssertMapping<TNAttrDerived>("A").HasColumnCondition("disc", "B");
        }

        [Table("A")]
        public class TNAttrBase
        {
            public int Id { get; set; }
            public string BaseData { get; set; }
        }

        public class TNAttrDerived : TNAttrBase
        {
            public string DerivedData { get; set; }
        }
    }

    #region Bug324763

        public class Product324763
        {
            [Timestamp]
            public byte[] Version { get; set; }

            [Key]
            [Column(Order = 0)]
            public int ProductId { get; set; }

            [Key]
            [Column(Order = 1)]
            [MaxLength(128)]
            public string Sku { get; set; }

            [Required]
            [StringLength(15)]
            public string Name { get; set; }

            public byte[] Image { get; set; }

            [InverseProperty("Product")]
            public ICollection<OrderLine324763> OrderLines { get; set; }
        }

        public class OrderLine324763
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            public int OrderId { get; set; }

            [NotMapped]
            public short Quantity { get; set; }

            public decimal Price { get; set; }
            public decimal Total { get; set; }
            public bool? IsShipped { get; set; }

            [ForeignKey("Product")]
            [Column(Order = 0)]
            public int ProductIdFk { get; set; }

            [ForeignKey("Product")]
            [Column(Order = 1)]
            public string ProductSkuFk { get; set; }

            [MaxLength(128)]
            public string Sku { get; set; }

            [ConcurrencyCheck]
            public int EngineSupplierId { get; set; }

            public Product324763 Product { get; set; }
        }

        public class Test324763 : FunctionalTestBase
        {
            [Fact]
            public void Build_Is_Not_Idempotent()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<Product324763>();
                modelBuilder.Entity<OrderLine324763>();

                ValidateBuildIsIdempotent(modelBuilder);
            }

            private void ValidateBuildIsIdempotent(DbModelBuilder modelBuilder)
            {
                var mapping1 = BuildMapping(modelBuilder);
                var mapping2 = BuildMapping(modelBuilder);
                Assert.True(mapping1.EdmxIsEqualTo(mapping2));
            }

            [Fact]
            public void Build_Is_Not_Idempotent_Inverse()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Conventions.Remove<AssociationInverseDiscoveryConvention>();
                modelBuilder.Entity<Product324763>();
                modelBuilder.Entity<OrderLine324763>();

                ValidateBuildIsIdempotent(modelBuilder);
            }
        }

    #endregion
}
