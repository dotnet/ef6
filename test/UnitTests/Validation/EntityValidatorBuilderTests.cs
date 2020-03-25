// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal.Validation;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using Moq.Protected;
    using Xunit;
    using Xunit.Sdk;

    #region Context and types for EntityValidatorBuilder tests

    public class SelfPopulatingContext : DbContext
    {
        public SelfPopulatingContext()
            : this(new object[0])
        {
        }

        public SelfPopulatingContext(params object[] entities)
        {
            foreach (var entity in entities ?? new object[0])
            {
                Set(entity.GetType()).Add(entity);
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityWithComplexType>();
            modelBuilder.Entity<ValidatableEntity>();

            modelBuilder.ComplexType<ComplexTypeWithNoValidation>();
            modelBuilder.ComplexType<ValidatableComplexType>();
        }

        public Func<DbEntityEntry, DbEntityValidationResult> ValidateEntityFunc;
        public Func<DbEntityEntry, bool> ShouldValidateEntityFunc;

        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry dbEntityEntry, IDictionary<object, object> items)
        {
            return ValidateEntityFunc != null
                       ? ValidateEntityFunc(dbEntityEntry)
                       : base.ValidateEntity(dbEntityEntry, items);
        }

        protected override bool ShouldValidateEntity(DbEntityEntry dbEntityEntry)
        {
            return ShouldValidateEntityFunc != null
                       ? ShouldValidateEntityFunc(dbEntityEntry)
                       : base.ShouldValidateEntity(dbEntityEntry);
        }

        public Action<DbModelBuilder> CustomOnModelCreating { private get; set; }
    }

    public class ConfigurationOverridesContext : DbContext
    {
        public ConfigurationOverridesContext()
            : this(new object[0])
        {
        }

        public ConfigurationOverridesContext(params object[] entities)
        {
            foreach (var entity in entities ?? new object[0])
            {
                Set(entity.GetType()).Add(entity);
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityWithComplexType>();
            modelBuilder.ComplexType<ComplexTypeWithNoValidation>();

            modelBuilder.Entity<EntityWithComplexType>().Property(p => p.ID).IsRequired();
            modelBuilder.Entity<EntityWithComplexType>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<EntityWithComplexType>().HasRequired(p => p.Self);
            modelBuilder.Entity<EntityWithComplexType>().Property(p => p.NonNullableProperty).IsRequired();

            modelBuilder.ComplexType<ComplexTypeWithNoValidation>().Property(p => p.StringProperty).IsMaxLength();
            modelBuilder.ComplexType<ComplexTypeWithNoValidation>().Property(p => p.AnotherStringProperty).HasMaxLength(10);
        }
    }

    public class ComplexTypeWithNoValidation
    {
        public string StringProperty { get; set; }
        public string AnotherStringProperty { get; set; }
    }

    public class EntityWithComplexType
    {
        public int? ID { get; set; }

        public ComplexTypeWithNoValidation ComplexProperty { get; set; }

        public EntityWithComplexType Self { get; set; }

        public int NonNullableProperty { get; set; }

        private int PrivateProperty { get; set; }

        public int SetterProperty
        {
            set { }
        }

        public int GetterProperty
        {
            get { return 0; }
        }

        private static int StaticProperty { get; set; }

        public string this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }

    public class ValidatableEntity : IValidatableObject
    {
        public int ID { get; set; }

        public ValidatableComplexType ComplexProperty { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }

    public class ValidatableComplexType : IValidatableObject
    {
        public string StringProperty { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    public class EntityValidatorBuilderTests
    {
        public EntityValidatorBuilderTests()
        {
            Database.SetInitializer((IDatabaseInitializer<SelfPopulatingContext>)null);
            Database.SetInitializer((IDatabaseInitializer<ConfigurationOverridesContext>)null);
        }

        [Fact]
        public void BuildEntityValidator_returns_null_for_an_entity_with_no_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                Assert.Null(builder.Object.BuildEntityValidatorBase(ctx.Entry(entity).InternalEntry));
            }
        }

        [Fact]
        public void BuildEntityValidator_does_not_return_null_for_an_IValidatableObject()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new ValidatableEntity();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validator = builder.Object.BuildEntityValidatorBase(ctx.Entry(entity).InternalEntry);

                Assert.NotNull(validator);
                Assert.False(validator.PropertyValidators.Any());
                Assert.True(validator.TypeLevelValidators.Any());
            }
        }

        [Fact]
        public void BuildEntityValidator_does_not_return_null_for_an_entity_with_property_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();
            builder.Protected()
                .Setup<IList<PropertyValidator>>(
                    "BuildValidatorsForProperties", ItExpr.IsAny<IEnumerable<PropertyInfo>>(), ItExpr.IsAny<IEnumerable<EdmProperty>>(),
                    ItExpr.IsAny<IEnumerable<NavigationProperty>>())
                .Returns<IEnumerable<PropertyInfo>, IEnumerable<EdmProperty>, IEnumerable<NavigationProperty>>(
                    (pi, e, n) => new List<PropertyValidator>
                                      {
                                          new PropertyValidator("foo", Enumerable.Empty<ValidationAttributeValidator>())
                                      });

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validator = builder.Object.BuildEntityValidatorBase(ctx.Entry(entity).InternalEntry);

                Assert.NotNull(validator);
                Assert.Equal(1, validator.PropertyValidators.Count());
                Assert.Equal(0, validator.TypeLevelValidators.Count());
            }
        }

        [Fact]
        public void BuildEntityValidator_does_not_return_null_for_an_entity_with_entity_level_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            builder.Protected()
                .Setup<IList<IValidator>>("BuildValidationAttributeValidators", ItExpr.IsAny<IEnumerable<Attribute>>())
                .Returns<IEnumerable<Attribute>>(
                    a => new List<IValidator>
                             {
                                 new ValidationAttributeValidator(new RequiredAttribute(), null)
                             });

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validator = builder.Object.BuildEntityValidatorBase(ctx.Entry(entity).InternalEntry);

                Assert.NotNull(validator);
                Assert.Equal(0, validator.PropertyValidators.Count());
                Assert.Equal(1, validator.TypeLevelValidators.Count());
            }
        }

        [Fact]
        public void BuildValidatorsForProperties_returns_empty_if_no_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validators = builder.Object.BuildValidatorsForPropertiesBase(
                    entity.GetType().GetRuntimeProperties().Where(p => p.IsPublic()),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties,
                    ctx.Entry(entity).InternalEntry.EdmEntityType.NavigationProperties);

                Assert.Equal(0, validators.Count);
            }
        }

        [Fact]
        public void BuildComplexTypeValidator_returns_null_for_a_complex_type_with_no_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                Assert.Null(builder.Object.BuildComplexTypeValidatorBase(typeof(ComplexTypeWithNoValidation), complexType));
            }
        }

        [Fact]
        public void BuildComplexTypeValidator_does_not_return_null_for_an_IValidatableObject()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new ValidatableEntity();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                var validator = builder.Object.BuildComplexTypeValidatorBase(typeof(ValidatableComplexType), complexType);

                Assert.NotNull(validator);
                Assert.False(validator.PropertyValidators.Any());
                Assert.True(validator.TypeLevelValidators.Any());
            }
        }

        [Fact]
        public void BuildComplexTypeValidator_does_not_return_null_for_a_complex_type_with_property_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();
            builder.Protected()
                .Setup<IList<PropertyValidator>>(
                    "BuildValidatorsForProperties", ItExpr.IsAny<IEnumerable<PropertyInfo>>(), ItExpr.IsAny<IEnumerable<EdmProperty>>(),
                    ItExpr.IsAny<IEnumerable<NavigationProperty>>())
                .Returns<IEnumerable<PropertyInfo>, IEnumerable<EdmProperty>, IEnumerable<NavigationProperty>>(
                    (pi, e, n) => new List<PropertyValidator>
                                      {
                                          new PropertyValidator("foo", Enumerable.Empty<IValidator>())
                                      });

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                var validator = builder.Object.BuildComplexTypeValidatorBase(typeof(ComplexTypeWithNoValidation), complexType);

                Assert.NotNull(validator);
                Assert.Equal(1, validator.PropertyValidators.Count());
                Assert.Equal(0, validator.TypeLevelValidators.Count());
            }
        }

        [Fact]
        public void BuildComplexTypeValidator_does_not_return_null_for_a_complex_type_with_entity_level_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            builder.Protected()
                .Setup<IList<IValidator>>("BuildValidationAttributeValidators", ItExpr.IsAny<IEnumerable<Attribute>>())
                .Returns<IEnumerable<Attribute>>(
                    a => new List<IValidator>
                             {
                                 new ValidationAttributeValidator(new RequiredAttribute(), null)
                             });

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                var validator = builder.Object.BuildComplexTypeValidatorBase(typeof(ComplexTypeWithNoValidation), complexType);

                Assert.NotNull(validator);
                Assert.Equal(0, validator.PropertyValidators.Count());
                Assert.Equal(1, validator.TypeLevelValidators.Count());
            }
        }

        [Fact]
        public void BuildValidatorsForProperties_calls_BuildPropertyValidator_for_a_transient_property()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();
            builder.Protected()
                .Setup<PropertyValidator>("BuildPropertyValidator", ItExpr.IsAny<PropertyInfo>())
                .Returns<PropertyInfo>(
                    pi => new PropertyValidator(
                              "ID", new[]
                                        {
                                            new ValidationAttributeValidator(new RequiredAttribute(), null)
                                        }));

            builder.Protected()
                .Setup<PropertyValidator>(
                    "BuildPropertyValidator", ItExpr.IsAny<PropertyInfo>(), ItExpr.IsAny<EdmProperty>(), ItExpr.IsAny<bool>())
                .Throws<XunitException>();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validators = builder.Object.BuildValidatorsForPropertiesBase(
                    new[] { entity.GetType().GetDeclaredProperty("ID") },
                    new EdmProperty[0],
                    new NavigationProperty[0]);

                Assert.Equal(1, validators.Count);
            }
        }

        [Fact]
        public void BuildValidatorsForProperties_calls_BuildPropertyValidator_for_a_scalar_property()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();
            builder.Protected()
                .Setup<PropertyValidator>("BuildPropertyValidator", ItExpr.IsAny<PropertyInfo>())
                .Throws<XunitException>();

            builder.Protected()
                .Setup<PropertyValidator>(
                    "BuildPropertyValidator", ItExpr.IsAny<PropertyInfo>(), ItExpr.IsAny<EdmProperty>(), ItExpr.IsAny<bool>())
                .Returns<PropertyInfo, EdmProperty, bool>(
                    (pi, e, f) => new PropertyValidator(
                                      "ID", new[]
                                                {
                                                    new ValidationAttributeValidator(new RequiredAttribute(), null)
                                                }));

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validators = builder.Object.BuildValidatorsForPropertiesBase(
                    new[] { entity.GetType().GetDeclaredProperty("ID") },
                    new[] { ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ID").Single() },
                    new NavigationProperty[0]);

                Assert.Equal(1, validators.Count);
            }
        }

        [Fact]
        public void BuildPropertyValidator_returns_null_for_a_scalar_property_with_no_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                Assert.Null(
                    builder.Object.BuildPropertyValidatorBase(
                        entity.GetType().GetDeclaredProperty("ID"),
                        ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ID").Single(),
                        true));
            }
        }

        [Fact]
        public void BuildPropertyValidator_does_not_return_null_for_a_scalar_property_with_facet_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();
            builder.Protected()
                .Setup<IEnumerable<IValidator>>(
                    "BuildFacetValidators", ItExpr.IsAny<PropertyInfo>(), ItExpr.IsAny<EdmMember>(), ItExpr.IsAny<IEnumerable<Attribute>>())
                .Returns<PropertyInfo, EdmMember, IEnumerable<Attribute>>(
                    (pi, e, a) => new[]
                                      {
                                          new ValidationAttributeValidator(new RequiredAttribute(), null)
                                      });

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validator = builder.Object.BuildPropertyValidatorBase(
                    entity.GetType().GetDeclaredProperty("ID"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ID").Single(),
                    true);

                Assert.NotNull(validator);
                Assert.IsNotType<ComplexPropertyValidator>(validator);
                Assert.Equal("ID", validator.PropertyName);
                Assert.Equal(1, validator.PropertyAttributeValidators.Count());
            }
        }

        [Fact]
        public void BuildPropertyValidator_with_buildFacetValidators_set_to_false_returns_null_for_a_scalar_property_with_facet_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();
            builder.Protected()
                .Setup<IEnumerable<IValidator>>(
                    "BuildFacetValidators", ItExpr.IsAny<PropertyInfo>(),
                    ItExpr.IsAny<EdmMember>(), ItExpr.IsAny<IEnumerable<Attribute>>())
                .Returns<PropertyInfo, EdmMember, IEnumerable<Attribute>>(
                    (pi, e, a) => new[]
                                      {
                                          new ValidationAttributeValidator(new RequiredAttribute(), null)
                                      });

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validator = builder.Object.BuildPropertyValidatorBase(
                    entity.GetType().GetDeclaredProperty("ID"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ID").Single(),
                    false);

                Assert.Null(validator);
            }
        }

        [Fact]
        public void BuildPropertyValidator_does_not_return_null_for_a_scalar_property_with_attribute_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            builder.Protected()
                .Setup<IList<IValidator>>("BuildValidationAttributeValidators", ItExpr.IsAny<IEnumerable<Attribute>>())
                .Returns<IEnumerable<Attribute>>(
                    a => new List<IValidator>
                             {
                                 new ValidationAttributeValidator(new RequiredAttribute(), null)
                             });

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validator = builder.Object.BuildPropertyValidatorBase(
                    entity.GetType().GetDeclaredProperty("ID"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ID").Single(),
                    false);

                Assert.NotNull(validator);
                Assert.IsNotType<ComplexPropertyValidator>(validator);
                Assert.Equal("ID", validator.PropertyName);
                Assert.Equal(1, validator.PropertyAttributeValidators.Count());
            }
        }

        [Fact]
        public void BuildPropertyValidator_returns_null_for_a_complex_property_with_no_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                Assert.Null(
                    builder.Object.BuildPropertyValidatorBase(
                        entity.GetType().GetDeclaredProperty("ComplexProperty"),
                        ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ComplexProperty").Single(),
                        true));
            }
        }

        [Fact]
        public void BuildPropertyValidator_returns_null_for_a_complex_property_with_facet_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();
            builder.Protected()
                .Setup<IEnumerable<IValidator>>(
                    "BuildFacetValidators", ItExpr.IsAny<PropertyInfo>(), ItExpr.IsAny<EdmMember>(), ItExpr.IsAny<IEnumerable<Attribute>>())
                .Returns<PropertyInfo, EdmMember, IEnumerable<Attribute>>(
                    (pi, e, a) => new[]
                                      {
                                          new ValidationAttributeValidator(new RequiredAttribute(), null)
                                      });

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                Assert.Null(
                    builder.Object.BuildPropertyValidatorBase(
                        entity.GetType().GetDeclaredProperty("ComplexProperty"),
                        ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ComplexProperty").Single(),
                        true));
            }
        }

        [Fact]
        public void BuildPropertyValidator_does_not_return_null_for_a_complex_property_with_attribute_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            builder.Protected()
                .Setup<IList<IValidator>>("BuildValidationAttributeValidators", ItExpr.IsAny<IEnumerable<Attribute>>())
                .Returns<IEnumerable<Attribute>>(
                    a => new List<IValidator>
                             {
                                 new ValidationAttributeValidator(new RequiredAttribute(), null)
                             });

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validator = builder.Object.BuildPropertyValidatorBase(
                    entity.GetType().GetDeclaredProperty("ComplexProperty"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ComplexProperty").Single(),
                    true);

                Assert.NotNull(validator);
                Assert.IsType<ComplexPropertyValidator>(validator);
                Assert.Equal("ComplexProperty", validator.PropertyName);
                Assert.Equal(1, validator.PropertyAttributeValidators.Count());
                Assert.Null(((ComplexPropertyValidator)validator).ComplexTypeValidator);
            }
        }

        [Fact]
        public void BuildPropertyValidator_does_not_return_null_for_a_complex_property_with_type_level_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            builder.Protected()
                .Setup<ComplexTypeValidator>("BuildComplexTypeValidator", ItExpr.IsAny<Type>(), ItExpr.IsAny<ComplexType>())
                .Returns<Type, ComplexType>(
                    (t, c) => new ComplexTypeValidator(new PropertyValidator[0], new ValidationAttributeValidator[0]));

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validator = builder.Object.BuildPropertyValidatorBase(
                    entity.GetType().GetDeclaredProperty("ComplexProperty"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ComplexProperty").Single(),
                    false);

                Assert.NotNull(validator);
                Assert.IsType<ComplexPropertyValidator>(validator);
                Assert.Equal("ComplexProperty", validator.PropertyName);
                Assert.Equal(0, validator.PropertyAttributeValidators.Count());
                Assert.NotNull(((ComplexPropertyValidator)validator).ComplexTypeValidator);
            }
        }

        [Fact]
        public void BuildPropertyValidator_returns_null_for_transient_property_with_no_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();
            builder.Protected()
                .Setup<IEnumerable<IValidator>>(
                    "BuildFacetValidators", ItExpr.IsAny<PropertyInfo>(), ItExpr.IsAny<EdmMember>(), ItExpr.IsAny<IEnumerable<Attribute>>())
                .Throws<XunitException>();

            Assert.Null(builder.Object.BuildPropertyValidatorBase(typeof(EntityWithComplexType).GetDeclaredProperty("ID")));
        }

        [Fact]
        public void GetInstanceProperties_returns_all_instance_properties()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            var properties = builder.Object.GetPublicInstancePropertiesBase(typeof(EntityWithComplexType));

            Assert.True(
                properties.Select(pi => pi.Name).OrderBy(s => s).SequenceEqual(
                    new[] { "ComplexProperty", "GetterProperty", "ID", "NonNullableProperty", "Self" }));
        }

        [Fact]
        public void BuildPropertyValidator_does_not_return_null_for_a_transient_property_with_attribute_validation()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            builder.Protected()
                .Setup<IList<IValidator>>("BuildValidationAttributeValidators", ItExpr.IsAny<IEnumerable<Attribute>>())
                .Returns<IEnumerable<Attribute>>(
                    a => new List<IValidator>
                             {
                                 new ValidationAttributeValidator(new RequiredAttribute(), null)
                             });

            builder.Protected()
                .Setup<IEnumerable<IValidator>>(
                    "BuildFacetValidators", ItExpr.IsAny<PropertyInfo>(), ItExpr.IsAny<EdmMember>(), ItExpr.IsAny<IEnumerable<Attribute>>())
                .Throws<XunitException>();

            var validator = builder.Object.BuildPropertyValidatorBase(typeof(EntityWithComplexType).GetDeclaredProperty("ID"));

            Assert.NotNull(validator);
            Assert.IsNotType<ComplexPropertyValidator>(validator);
            Assert.Equal("ID", validator.PropertyName);
            Assert.Equal(1, validator.PropertyAttributeValidators.Count());
        }

        [Fact]
        public void BuildValidationAttributeValidators_returns_validators_for_ValidationAttributes()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            var validators = builder.Object.BuildValidationAttributeValidatorsBase(
                new Attribute[]
                    {
                        new RequiredAttribute(), new CLSCompliantAttribute(true)
                    });

            Assert.IsType<ValidationAttributeValidator>(validators.Single());
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_optional_property()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validators = builder.Object.BuildFacetValidatorsBase(
                    entity.GetType().GetDeclaredProperty("Self"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.NavigationProperties.Where(p => p.Name == "Self").Single(),
                    new Attribute[0]);

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_a_validator_for_required_property()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var validators = builder.Object.BuildFacetValidatorsBase(
                    entity.GetType().GetDeclaredProperty("Self"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.NavigationProperties.Where(p => p.Name == "Self").Single(),
                    new Attribute[0]);

                Assert.Equal(1, validators.Count());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_required_value_type_property()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var validators = builder.Object.BuildFacetValidatorsBase(
                    entity.GetType().GetDeclaredProperty("NonNullableProperty"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "NonNullableProperty").Single(),
                    new Attribute[0]);

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_required_storeGenerated_property()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var validators = builder.Object.BuildFacetValidatorsBase(
                    entity.GetType().GetDeclaredProperty("ID"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ID").Single(),
                    new Attribute[0]);

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_required_property_with_ValidationAttribute()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var validators = builder.Object.BuildFacetValidatorsBase(
                    entity.GetType().GetDeclaredProperty("Self"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.NavigationProperties.Where(p => p.Name == "Self").Single(),
                    new[] { new RequiredAttribute() });

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_a_validator_for_a_property_with_MaxLength_facet()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                var validators = builder.Object.BuildFacetValidatorsBase(
                    typeof(ComplexTypeWithNoValidation).GetDeclaredProperty("AnotherStringProperty"),
                    complexType.Properties.Where(p => p.Name == "AnotherStringProperty").Single(),
                    new Attribute[0]);

                Assert.Equal(1, validators.Count());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_a_property_with_MaxLength_facet_and_MaxLengthAttribute()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                var validators = builder.Object.BuildFacetValidatorsBase(
                    typeof(ComplexTypeWithNoValidation).GetDeclaredProperty("AnotherStringProperty"),
                    complexType.Properties.Where(p => p.Name == "AnotherStringProperty").Single(),
                    new[] { new MaxLengthAttribute() });

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_a_property_with_MaxLength_facet_and_StringLengthAttribute()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                var validators = builder.Object.BuildFacetValidatorsBase(
                    typeof(ComplexTypeWithNoValidation).GetDeclaredProperty("AnotherStringProperty"),
                    complexType.Properties.Where(p => p.Name == "AnotherStringProperty").Single(),
                    new[] { new StringLengthAttribute(1) });

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_a_property_with_IsMaxLength_facet()
        {
            var builder = MockHelper.CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                var validators = builder.Object.BuildFacetValidatorsBase(
                    typeof(ComplexTypeWithNoValidation).GetDeclaredProperty("StringProperty"),
                    complexType.Properties.Where(p => p.Name == "StringProperty").Single(),
                    new Attribute[0]);

                Assert.False(validators.Any());
            }
        }
    }
}
