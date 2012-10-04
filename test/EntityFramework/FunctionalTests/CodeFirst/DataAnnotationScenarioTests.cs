// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Linq.Expressions;
    using FunctionalTests.Model;
    using Xunit;

    public sealed class DataAnnotationScenarioTests : TestBase
    {
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
                () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("DuplicateConfiguredColumnOrder", "Entity_10558");
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
                .DbFacetEqual(128, f => f.MaxLength)
                .DbEqual(true, c => c.IsPrimaryKeyColumn);
        }

        [Fact]
        public void NotMapped_should_propagate_down_inheritance_hierachy()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<NotMappedDerived>();

            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                .ValidateMessage("InvalidEntityType", typeof(NotMappedDerived));
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
                    databaseMapping.Model.Namespaces.Single().EntityTypes.SelectMany(e => e.Properties)
                        .Any(p => p.Name == "BaseClassProperty"));
                Assert.False(
                    databaseMapping.Model.Namespaces.Single().EntityTypes.SelectMany(e => e.Properties)
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
                        databaseMapping.Model.Namespaces.Single().EntityTypes.SelectMany(e => e.Properties)
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

                Assert.True(databaseMapping.Model.Namespaces.Single().EntityTypes.Any(e => e.Name == "AbstractBaseEntity"));
                Assert.False(
                    databaseMapping.Model.Namespaces.Single().EntityTypes.SelectMany(e => e.Properties)
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
                    databaseMapping.Model.Namespaces.Single().EntityTypes.SelectMany(e => e.Properties)
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

                Assert.Throws<InvalidOperationException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                    .ValidateMessage(
                        "CannotIgnoreMappedBaseProperty",
                        "VirtualBaseClassProperty", "FunctionalTests.Unit",
                        "FunctionalTests.BaseEntity");
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
                    databaseMapping.Model.Namespaces.Single().EntityTypes.Single().Properties.Any(
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
                        databaseMapping.Model.Namespaces.Single().EntityTypes.Single().Properties.Any(
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
                    databaseMapping.Model.Namespaces.Single().EntityTypes.Single().Properties.Any(
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
                    databaseMapping.Model.Namespaces.Single().EntityTypes.Single().Properties.Any(
                        p => p.Name == "VirtualBaseClassProperty"));
            }
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
                    .DbFacetEqual(128, f => f.MaxLength);
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
                .DbFacetEqual(128, f => f.MaxLength)
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
                .DbFacetEqual(128, f => f.MaxLength)
                .DbEqual(true, c => c.IsPrimaryKeyColumn);

            Assert.Equal(1, databaseMapping.Model.Namespaces.Single().AssociationTypes.Count());
            databaseMapping.Assert<ReferencingClass>().ForeignKeyColumn("Person_PersonFirstName")
                .DbEqual("nvarchar", c => c.TypeName)
                .DbFacetEqual(128, f => f.MaxLength);
        }

        [Fact]
        public void Key_column_and_MaxLength_work_together()
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
                    .DbFacetEqual(64, f => f.MaxLength)
                    .DbEqual(true, c => c.IsPrimaryKeyColumn);
            }
        }

        [Fact]
        public void Key_column_and_MaxLength_work_together_in_an_IA()
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
                    .DbFacetEqual(64, f => f.MaxLength)
                    .DbEqual(true, c => c.IsPrimaryKeyColumn);

                Assert.Equal(1, databaseMapping.Model.Namespaces.Single().AssociationTypes.Count());
                databaseMapping.Assert<ReferencingClass>().ForeignKeyColumn("Person_PersonFirstName")
                    .DbEqual("nvarchar", c => c.TypeName)
                    .DbFacetEqual(64, f => f.MaxLength);
            }
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

            // should be nothing configured on derived type
            Assert.Equal(0, modelBuilder.ModelConfiguration.Entity(typeof(DODerived)).ConfiguredProperties.Count());
        }

        [Fact]
        public void Key_on_nav_prop_is_ignored()
        {
            var modelBuilder = new DbModelBuilder();

            modelBuilder.Entity<KeyOnNavProp>();

            var databaseMapping = BuildMapping(modelBuilder);
            databaseMapping.AssertValid();

            databaseMapping.Assert<KeyOnNavProp>().DbEqual("Id", t => t.KeyColumns.Single().Name);
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
                t.Columns.Single(x => x.Name == "MaxTimestamp").
                    TypeName);
            databaseMapping.Assert<TimestampAndMaxlen>().DbEqual(
                null,
                t =>
                t.Columns.Single(x => x.Name == "MaxTimestamp").Facets.
                    IsMaxLength);
            databaseMapping.Assert<TimestampAndMaxlen>().DbEqual(
                null,
                t =>
                t.Columns.Single(x => x.Name == "MaxTimestamp").Facets.
                    MaxLength);
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
                t.Columns.Single(x => x.Name == "NonMaxTimestamp").
                    TypeName);
            databaseMapping.Assert<TimestampAndMaxlen>().DbEqual(
                null,
                t =>
                t.Columns.Single(x => x.Name == "NonMaxTimestamp").
                    Facets.IsMaxLength);
            databaseMapping.Assert<TimestampAndMaxlen>().DbEqual(
                null,
                t =>
                t.Columns.Single(x => x.Name == "NonMaxTimestamp").
                    Facets.MaxLength);
        }

        [Fact]
        public void Annotation_in_derived_class_when_base_class_processed_after_derived_class()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<StyledProduct>();
            modelBuilder.Entity<Product>();

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

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
                        () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
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
                        () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
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
                        () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
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
                        () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
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
                        () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
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
                        () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
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

                    var association = databaseMapping.Model.Namespaces.Single().AssociationTypes.Single();
                    Assert.Equal("Profile", association.SourceEnd.GetEntityType().Name);
                    Assert.Equal(RelationshipMultiplicity.ZeroOrOne, association.SourceEnd.RelationshipMultiplicity);
                    Assert.Equal("Login", association.TargetEnd.GetEntityType().Name);
                    Assert.Equal(RelationshipMultiplicity.One, association.TargetEnd.RelationshipMultiplicity);
                    Assert.Equal("Profile", association.Constraint.DependentEnd.GetEntityType().Name);
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
                        () => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                        .ValidateMessage("UnableToDeterminePrincipal", typeof(Profile), typeof(Login));
                }
            }
        }

        [Fact]
        public void TableNameAttribute_affects_only_base_in_TPT()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<TNAttrBase>()
                .Map<TNAttrDerived>(mc => mc.ToTable("B"));

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

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

            var databaseMapping = modelBuilder.BuildAndValidate(ProviderRegistry.Sql2008_ProviderInfo);

            databaseMapping.Assert<TNAttrBase>("A");
            databaseMapping.AssertMapping<TNAttrBase>("A", false).HasColumnCondition("disc", "A");
            databaseMapping.Assert<TNAttrDerived>("A");
            databaseMapping.AssertMapping<TNAttrDerived>("A").HasColumnCondition("disc", "B");
        }
    }

    #region Fixtures

    public class MaxLengthAnnotationClass
    {
        public int Id { get; set; }

        [StringLength(500)]
        [MaxLength]
        public string PersonFirstName { get; set; }
    }

    public class MaxLengthWithLengthAnnotationClass
    {
        public int Id { get; set; }

        [StringLength(500)]
        [MaxLength(30)]
        public string PersonFirstName { get; set; }
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

    [NotMapped]
    public class NotMappedBase
    {
        public int Id { get; set; }
    }

    public class NotMappedDerived : NotMappedBase
    {
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

    #endregion

    #region Bug324763

    namespace Bug324763
    {
        using System.Data.Entity.ModelConfiguration.Conventions;

        public class Product
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
            public ICollection<OrderLine> OrderLines { get; set; }
        }

        public class OrderLine
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

            public Product Product { get; set; }
        }

        public class Test324763
        {
            [Fact]
            public void Repro324763_Build_Is_Not_Idempotent()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Entity<Product>();
                modelBuilder.Entity<OrderLine>();

                ValidateBuildIsIdempotent(modelBuilder);
            }

            private void ValidateBuildIsIdempotent(DbModelBuilder modelBuilder)
            {
                var mapping1 = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;
                var mapping2 = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo).DatabaseMapping;
                Assert.True(mapping1.EdmxIsEqualTo(mapping2));
            }

            [Fact]
            public void Repro324763_Build_Is_Not_Idempotent_Inverse()
            {
                var modelBuilder = new DbModelBuilder();
                modelBuilder.Conventions.Remove<AssociationInverseDiscoveryConvention>();
                modelBuilder.Entity<Product>();
                modelBuilder.Entity<OrderLine>();

                ValidateBuildIsIdempotent(modelBuilder);
            }
        }
    }

    #endregion
}
