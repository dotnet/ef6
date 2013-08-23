// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.CodeFirst.FunctionalTests.ProductivityApi.Validation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Validation;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Transactions;
    using ProductivityApiTests;
    using SimpleModel;
    using Xunit;

    #region Context and entity types used by validation tests

    public class SelfPopulatingContext : DbContext
    {
        public SelfPopulatingContext()
            : this(new object[0])
        {
        }

        public SelfPopulatingContext(params object[] entities)
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<SelfPopulatingContext>());

            foreach (var entity in entities ?? new object[0])
            {
                Set(entity.GetType()).Add(entity);
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityWithNoValidation>();
            modelBuilder.Entity<EntityWithSomeValidation>();
            modelBuilder.Entity<EntityWithBuiltInValidationAttributes>();
            modelBuilder.Entity<EntityWithTransientPropertyValidation>();
            modelBuilder.Entity<EntityWithPropertyLevelCustomValidationAttributes>();
            modelBuilder.Entity<EntityWithEntityLevelCustomValidationAttributes>();
            modelBuilder.Entity<EntityWithComplexType>();
            modelBuilder.Entity<EntityWithComplexTypeLevelCustomValidationAttributes>();
            modelBuilder.Entity<EntityWithCollectionNavigationProperty>();
            modelBuilder.Entity<EntityWithReferenceNavigationProperty>();
            modelBuilder.Entity<EntityWithFKReferenceNavigationPropertyDependant>();
            modelBuilder.Entity<EntityWithFKReferenceNavigationPropertyPrincipal>();
            modelBuilder.Entity<EntityWithFKReferenceNavigationPropertyWorkaround>();
            modelBuilder.Entity<ValidatableEntity>();
            modelBuilder.Entity<EntityWithAllKindsOfValidation>();
            modelBuilder.Entity<EntityWithValidationAttributesOnMetadataType>();

            modelBuilder.ComplexType<ComplexType>();
            modelBuilder.ComplexType<ComplexTypeWithTypeLevelCustomValidationAttributes>();

            modelBuilder.Entity<EntityWithTransientPropertyValidation>().Ignore<string>(p => p.Name);
            modelBuilder.Entity<EntityWithTransientPropertyValidation>().Ignore<string>(p => p.OtherName);

            modelBuilder.Entity<EntityWithFKReferenceNavigationPropertyDependant>()
                .HasRequired(e => e.RelatedEntity)
                .WithRequiredDependent();
            modelBuilder.Entity<EntityWithFKReferenceNavigationPropertyPrincipal>()
                .HasRequired(e => e.RelatedEntity)
                .WithRequiredPrincipal();
        }

        public Func<DbEntityEntry, DbEntityValidationResult> ValidateEntityFunc;
        public Func<DbEntityEntry, bool> ShouldValidateEntityFunc;

        public DbEntityValidationResult ValidateEntity(DbEntityEntry dbEntityEntry)
        {
            return ValidateEntity(dbEntityEntry, null);
        }

        protected override DbEntityValidationResult ValidateEntity(
            DbEntityEntry dbEntityEntry,
            IDictionary<object, object> items)
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

    public class EntityWithNoValidation
    {
        public int ID { get; set; }
    }

    public class EntityWithSomeValidation
    {
        public int ID { get; set; }

        [MaxLength(5)]
        [RegularExpression(@"[\w]+")]
        public string Name { get; set; }

        [Required]
        public int? Required { get; set; }
    }

    public class EntityWithBuiltInValidationAttributes
    {
        [Range(0, 100)]
        public int ID { get; set; }

        [StringLength(5)]
        [RegularExpression(@"[\w]+")]
        public virtual string Name { get; set; }
    }

    public class EntityWithTransientPropertyValidation
    {
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Other Name")]
        public string OtherName
        {
            get { return Name; }
        }

        [Required]
        private string PrivateName { get; set; }
    }

    public class EntityWithPropertyLevelCustomValidationAttributes
    {
        [CustomValidation(typeof(EntityWithPropertyLevelCustomValidationAttributes), "ValidateWithoutContext")]
        [CustomValidation(typeof(EntityWithPropertyLevelCustomValidationAttributes), "ValidateWithContext")]
        public int ID { get; set; }

        public static Func<int, ValidationResult> ValidateWithoutContextFunc;

        public static ValidationResult ValidateWithoutContext(int value)
        {
            return ValidateWithoutContextFunc != null ? ValidateWithoutContextFunc(value) : null;
        }

        public static Func<int, ValidationContext, ValidationResult> ValidateWithContextFunc;

        public static ValidationResult ValidateWithContext(int value, ValidationContext validationContext)
        {
            return ValidateWithContextFunc != null ? ValidateWithContextFunc(value, validationContext) : null;
        }
    }

    [CustomValidation(typeof(EntityWithEntityLevelCustomValidationAttributes), "ValidateWithoutContext")]
    [CustomValidation(typeof(EntityWithEntityLevelCustomValidationAttributes), "ValidateWithContext")]
    public class EntityWithEntityLevelCustomValidationAttributes
    {
        public string ID { get; set; }

        public static Func<object, ValidationResult> ValidateWithoutContextFunc;

        public static ValidationResult ValidateWithoutContext(object value)
        {
            return ValidateWithoutContextFunc != null ? ValidateWithoutContextFunc(value) : ValidationResult.Success;
        }

        public static Func<object, ValidationContext, ValidationResult> ValidateWithContextFunc;

        public static ValidationResult ValidateWithContext(object value, ValidationContext validationContext)
        {
            return ValidateWithContextFunc != null
                       ? ValidateWithContextFunc(value, validationContext)
                       : ValidationResult.Success;
        }
    }

    public class ComplexType
    {
        [Required]
        public string RequiredProperty { get; set; }

        [MaxLength(10)]
        public byte[] ByteArray { get; set; }

        [StringLength(10, MinimumLength = 8)]
        public string StringWithStringLengthAttribute { get; set; }

        [MaxLength(10)]
        [StringLength(5)]
        public string StringWithMaxLengthAndStringLengthAttributes { get; set; }
    }

    public class EntityWithComplexType
    {
        public int ID { get; set; }

        public ComplexType ComplexProperty { get; set; }
    }

    [CustomValidation(typeof(ComplexTypeWithTypeLevelCustomValidationAttributes), "ValidateWithoutContext")]
    [CustomValidation(typeof(ComplexTypeWithTypeLevelCustomValidationAttributes), "ValidateWithContext")]
    public class ComplexTypeWithTypeLevelCustomValidationAttributes
    {
        public ComplexTypeWithTypeLevelCustomValidationAttributes()
        {
            IsValid = true;
        }

        public bool IsValid { get; set; }

        public static Func<ComplexTypeWithTypeLevelCustomValidationAttributes, ValidationResult>
            ValidateWithoutContextFunc;

        public static ValidationResult ValidateWithoutContext(ComplexTypeWithTypeLevelCustomValidationAttributes value)
        {
            return ValidateWithoutContextFunc != null ? ValidateWithoutContextFunc(value) : ValidationResult.Success;
        }

        public static Func<ComplexTypeWithTypeLevelCustomValidationAttributes, ValidationContext, ValidationResult>
            ValidateWithContextFunc;

        public static ValidationResult ValidateWithContext(
            ComplexTypeWithTypeLevelCustomValidationAttributes value,
            ValidationContext validationContext)
        {
            return ValidateWithContextFunc != null
                       ? ValidateWithContextFunc(value, validationContext)
                       : ValidationResult.Success;
        }
    }

    public class EntityWithComplexTypeLevelCustomValidationAttributes
    {
        public int ID { get; set; }

        [CustomValidation(typeof(EntityWithComplexTypeLevelCustomValidationAttributes), "ValidateWithoutContext")]
        public ComplexTypeWithTypeLevelCustomValidationAttributes ComplexProperty { get; set; }

        public static Func<object, ValidationResult> ValidateWithoutContextFunc;

        public static ValidationResult ValidateWithoutContext(object value)
        {
            return ValidateWithoutContextFunc != null ? ValidateWithoutContextFunc(value) : null;
        }
    }

    public class EntityWithReferenceNavigationProperty
    {
        public int ID { get; set; }

        [StringLength(5)]
        [RegularExpression(@"[\w]+")]
        public string Name { get; set; }

        [Required]
        public virtual EntityWithBuiltInValidationAttributes RelatedEntity { get; set; }
    }

    public class EntityWithFKReferenceNavigationPropertyDependant
    {
        public string ID { get; set; }

        public EntityWithEntityLevelCustomValidationAttributes RelatedEntity { get; set; }
    }

    public class EntityWithFKReferenceNavigationPropertyPrincipal
    {
        public string ID { get; set; }

        public EntityWithEntityLevelCustomValidationAttributes RelatedEntity { get; set; }
    }

    public class EntityWithFKReferenceNavigationPropertyWorkaround
    {
        public EntityWithFKReferenceNavigationPropertyWorkaround()
        {
            // Workaround for navigation property FK validation
            ID = string.Empty;
        }

        [Required(AllowEmptyStrings = true)]
        public string ID { get; set; }

        public EntityWithEntityLevelCustomValidationAttributes RelatedEntity { get; set; }
    }

    public class EntityWithCollectionNavigationProperty
    {
        public int ID { get; set; }

        [StringLength(5)]
        [RegularExpression(@"[\w]+")]
        public string Name { get; set; }

        [Required]
        [CustomValidation(typeof(EntityWithCollectionNavigationProperty), "ValidateRelatedEntities")]
        public ICollection<EntityWithBuiltInValidationAttributes> RelatedEntities { get; set; }

        public static ValidationResult ValidateRelatedEntities(
            ICollection<EntityWithBuiltInValidationAttributes> value,
            ValidationContext validationContext)
        {
            // Required attribute should check for null.
            if (value != null)
            {
                if (!value.Any())
                {
                    return new ValidationResult("Collection cannot be empty.", new[] { "RelatedEntities" });
                }
            }

            return ValidationResult.Success;
        }
    }

    public class ValidatableEntity : IValidatableObject
    {
        public int ID { get; set; }

        public IEnumerable<ValidationResult> ValidationResults { private get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return ValidationResults;
        }
    }

    [CustomValidation(typeof(EntityWithAllKindsOfValidation), "CustomValidate")]
    public class EntityWithAllKindsOfValidation : EntityWithBuiltInValidationAttributes, IValidatableObject
    {
        [Required]
        public override string Name { get; set; }

        public string Phone { get; set; }

        public Func<ValidationContext, IEnumerable<ValidationResult>> ValidateFunc;

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            return ValidateFunc != null
                       ? ValidateFunc(validationContext)
                       : null;
        }

        public static Func<object, ValidationContext, ValidationResult> CustomValidateFunc;

        public static ValidationResult CustomValidate(object value, ValidationContext validationContext)
        {
            return CustomValidateFunc != null ? CustomValidateFunc(value, validationContext) : null;
        }
    }

    [MetadataType(typeof(EntityWithValidationAttributesOnMetadataTypeMetadata))]
    public class EntityWithValidationAttributesOnMetadataType
    {
        public int ID { get; set; }

        public string Name { get; set; }
    }

    public class EntityWithValidationAttributesOnMetadataTypeMetadata
    {
        [StringLength(5)]
        public string Name { get; set; }
    }

    #endregion

    public class ValidationTests : FunctionalTestBase
    {
        #region DbContext.ShouldValidateEntity tests

        [Fact]
        public void Entity_validated_if_ShouldValidateEntity_returns_true()
        {
            ShouldValidateEntityTestRunner(shouldValidateEntityReturnValue: true);
        }

        [Fact]
        public void Entity_not_validated_if_ShouldValidateEntity_returns_false()
        {
            ShouldValidateEntityTestRunner(shouldValidateEntityReturnValue: false);
        }

        public void ShouldValidateEntityTestRunner(bool shouldValidateEntityReturnValue)
        {
            // the type of the entity does not matter here
            var entity = new EntityWithNoValidation();

            using (var ctx = new SelfPopulatingContext(new object[] { entity }))
            {
                var callCount = 0;
                ctx.ShouldValidateEntityFunc = (dbEntityEntry) => { return shouldValidateEntityReturnValue; };
                ctx.ValidateEntityFunc = (dbEntityEntry) =>
                                             {
                                                 callCount++;
                                                 return new DbEntityValidationResult(
                                                     dbEntityEntry,
                                                     Enumerable.Empty<DbValidationError>());
                                             };

                var entityEntry = ctx.Entry(entity);
                foreach (EntityState state in Enum.GetValues(typeof(EntityState)))
                {
                    entityEntry.State = state;
                    // setting state to Detached will make the entity invisible for validation
                    ctx.GetValidationErrors();
                }

                Assert.Equal(
                    shouldValidateEntityReturnValue
                        ? 4
                        : // we don't see entities in Detached state so validation is not called at all
                    0,
                    callCount);
            }
        }

        [Fact]
        // Regression test for Dev 11 #151015 
        public void ShouldValidateEntity_should_not_run_for_metadata_entities()
        {
            using (var ctx = new SelfPopulatingContext())
            {
                ctx.ShouldValidateEntityFunc =
                    (dbEntityEntry) => { throw new NotImplementedException(); };

                ctx.Database.Initialize(false);
            }
        }

        #endregion

        #region DbContext.GetValidationErrors tests

        [Fact]
        public void GetValidationErrors_wont_crash_if_no_entities_tracked()
        {
            var results = Invoke_DbContext_GetValidationErrors(null);
            Assert.NotNull(results);
            Assert.False(results.Any());
        }

        [Fact]
        public void GetValidationErrors_does_not_return_errors_for_valid_entities()
        {
            var results = Invoke_DbContext_GetValidationErrors(
                new object[]
                    {
                        new EntityWithNoValidation(),
                        new EntityWithAllKindsOfValidation
                            {
                                ID = 5,
                                Name = "abc"
                            }
                    });
            Assert.NotNull(results);
            Assert.False(results.Any());
        }

        [Fact]
        public void GetValidationErrors_returns_errors_for_invalid_entities_builtin_validation_attributes()
        {
            var results =
                Invoke_DbContext_GetValidationErrors(
                    new object[]
                        {
                            new EntityWithAllKindsOfValidation
                                {
                                    ID = -1,
                                    Name = "!@#$%XXX"
                                }
                        });
            Assert.NotNull(results);
            var entityValidationResult = results.Single();
            Assert.False(entityValidationResult.IsValid);

            Assert.Equal(3, entityValidationResult.ValidationErrors.Count);
            Assert.True(
                entityValidationResult.ValidationErrors.SingleOrDefault(
                    e => e.PropertyName == "ID" &&
                         e.ErrorMessage == string.Format(RangeAttribute_ValidationError, "ID", 0, 100)) != null);
            Assert.True(
                entityValidationResult.ValidationErrors.SingleOrDefault(
                    e => e.PropertyName == "Name" &&
                         e.ErrorMessage == string.Format(StringLengthAttribute_ValidationError, "Name", 5)) != null);
            Assert.True(
                entityValidationResult.ValidationErrors.SingleOrDefault(
                    e => e.PropertyName == "Name" &&
                         e.ErrorMessage == string.Format(RegexAttribute_ValidationError, "Name", @"[\w]+")) != null);
        }

        [Fact]
        public void GetValidationErrors_returns_errors_for_invalid_entities_custom_validation_attributes()
        {
            try
            {
                var validationFailure = "Validation with custom attributes failed.";
                EntityWithAllKindsOfValidation.CustomValidateFunc =
                    (value, validationContext) => { return new ValidationResult(validationFailure, new[] { "ID" }); };

                var results =
                    Invoke_DbContext_GetValidationErrors(
                        new object[]
                            {
                                new EntityWithAllKindsOfValidation
                                    {
                                        ID = 5,
                                        Name = "abc"
                                    }
                            });
                Assert.NotNull(results);
                var validationResult = results.Single();

                Assert.False(validationResult.IsValid);
                Assert.Equal(1, validationResult.ValidationErrors.Count);
                Assert.Equal(validationFailure, validationResult.ValidationErrors.Single().ErrorMessage);
                Assert.Equal("ID", validationResult.ValidationErrors.Single().PropertyName);
            }
            finally
            {
                EntityWithAllKindsOfValidation.CustomValidateFunc = null;
            }
        }

        [Fact]
        public void GetValidationErrors_returns_errors_for_invalid_entities_IValidatableObject()
        {
            var validationFailure = "Invalid entity.";
            var entity = new EntityWithAllKindsOfValidation
                             {
                                 ID = 5,
                                 Name = "abc"
                             };
            entity.ValidateFunc =
                (validationContext) => { return new[] { new ValidationResult(validationFailure) }; };

            var results = Invoke_DbContext_GetValidationErrors(new object[] { entity });

            Assert.NotNull(results);
            var validationResult = results.Single();

            Assert.False(validationResult.IsValid);
            Assert.Equal(1, validationResult.ValidationErrors.Count);
            Assert.Equal(validationFailure, validationResult.ValidationErrors.Single().ErrorMessage);
            Assert.Equal(null, validationResult.ValidationErrors.Single().PropertyName);
        }

        [Fact]
        public void GetValidationErrors_returns_errors_for_invalid_entities_MetadataType()
        {
            var entity = new EntityWithValidationAttributesOnMetadataType
                             {
                                 Name = "AAAAAA"
                             };

            var results = Invoke_DbContext_GetValidationErrors(new[] { entity });

            Assert.NotNull(results);

            var validationResult = results.Single();
            Assert.False(validationResult.IsValid);

            var validationError = validationResult.ValidationErrors.Single();
            Assert.Equal(string.Format(StringLengthAttribute_ValidationError, "Name", 5), validationError.ErrorMessage);
            Assert.Equal("Name", validationError.PropertyName);
        }

        [Fact]
        public void GetValidationErrors_validates_only_added_and_modified_entities_by_default()
        {
            // the type of entity does not matter here
            var entity = new EntityWithNoValidation();

            using (var ctx = new SelfPopulatingContext(new object[] { entity }))
            {
                var callCount = 0;

                ctx.ValidateEntityFunc = (dbEntityEntry) =>
                                             {
                                                 Assert.True(
                                                     (dbEntityEntry.State &
                                                      (EntityState.Added | EntityState.Modified)) != 0);
                                                 callCount++;
                                                 return new DbEntityValidationResult(
                                                     dbEntityEntry,
                                                     Enumerable.Empty<DbValidationError>());
                                             };

                var entityEntry = ctx.Entry(entity);

                foreach (EntityState state in Enum.GetValues(typeof(EntityState)))
                {
                    entityEntry.State = state;
                    ctx.GetValidationErrors();
                }

                Assert.Equal(2, callCount);
            }
        }

        [Fact]
        public void GetValidationErrors_calls_DetectChanges_once()
        {
            using (var context = new DbContextTests.ValidationTestContext())
            {
                context.Database.Initialize(force: false);

                using (new TransactionScope())
                {
                    context.Configuration.ValidateOnSaveEnabled = true;
                    var food = context.Entry(new Category("FOOD"));
                    context.Categories.Add(food.Entity);

                    var pets = context.Entry(new Category("PETS"));
                    context.Categories.Add(pets.Entity);
                    context.SaveChanges();

                    Assert.Equal(EntityState.Unchanged, food.State);
                    food.Entity.DetailedDescription = "foo";
                    Assert.Equal(EntityState.Unchanged, food.State);

                    context.ValidateEntityFunc = (entry) =>
                                                     {
                                                         Assert.Same(food.Entity, entry.Entity);
                                                         // DetectChanges should have been called before this
                                                         Assert.Equal(EntityState.Modified, entry.State);
                                                         entry.State = EntityState.Unchanged;

                                                         Assert.Equal(EntityState.Unchanged, pets.State);
                                                         pets.Entity.DetailedDescription = "bar";
                                                         Assert.Equal(EntityState.Unchanged, pets.State);

                                                         context.ValidateEntityFunc = (e) =>
                                                                                          {
                                                                                              Assert.Same(pets.Entity, e.Entity);
                                                                                              Assert.Equal(
                                                                                                  EntityState.Unchanged, e.State);
                                                                                              Assert.Equal(
                                                                                                  EntityState.Unchanged,
                                                                                                  food.State);

                                                                                              Assert.Equal(
                                                                                                  "bar",
                                                                                                  pets.Entity.
                                                                                                      DetailedDescription);
                                                                                              pets.Entity.DetailedDescription =
                                                                                                  "foo";
                                                                                              Assert.Equal(
                                                                                                  EntityState.Unchanged,
                                                                                                  pets.State);

                                                                                              return
                                                                                                  new DbEntityValidationResult(
                                                                                                      entry,
                                                                                                      Enumerable.Empty
                                                                                                          <DbValidationError>());
                                                                                          };

                                                         return new DbEntityValidationResult(
                                                             entry,
                                                             Enumerable.Empty
                                                                 <DbValidationError>());
                                                     };

                    context.GetValidationErrors();

                    Assert.Equal(EntityState.Unchanged, food.State);
                    Assert.Equal(EntityState.Unchanged, pets.State);

                    // Ensure that ValidateEntityFunc ran twice
                    Assert.Equal("foo", pets.Entity.DetailedDescription);
                }
            }
        }

        #endregion

        #region DbEntityEntry and DbEntityEntry<T> GetValidationResult tests

        [Fact]
        public void DbEntityEntry_GetValidationResult_returns_no_errors_for_valid_entities()
        {
            var entity = new EntityWithBuiltInValidationAttributes
                             {
                                 ID = 3,
                                 Name = "abc"
                             };

            foreach (var useGenericDbEntityEntry in new[] { false, true })
            {
                var validationResult = Invoke_DbEntityEntry_GetValidationResult(entity, useGenericDbEntityEntry);

                Assert.NotNull(validationResult);
                Assert.True(validationResult.IsValid);
            }
        }

        [Fact]
        public void DbEntityEntry_GetValidationResult_returns_error_for_invalid_entities()
        {
            var entity = new EntityWithBuiltInValidationAttributes
                             {
                                 ID = -3,
                                 Name = "????"
                             };

            foreach (var useGenericDbEntityEntry in new[] { false, true })
            {
                var validationResult = Invoke_DbEntityEntry_GetValidationResult(entity, useGenericDbEntityEntry);

                Assert.NotNull(validationResult);
                Assert.True(!validationResult.IsValid);
                Assert.Same(entity, validationResult.Entry.Entity);
                Assert.Equal(2, validationResult.ValidationErrors.Count);
            }
        }

        [Fact]
        public void DbEntityEntry_GetValidationResult_returns_errors_for_invalid_entities_MetadataType()
        {
            var entity = new EntityWithValidationAttributesOnMetadataType
                             {
                                 Name = "AAAAAA"
                             };

            foreach (var asGeneric in new[] { true, false })
            {
                var validationResult = Invoke_DbEntityEntry_GetValidationResult(entity, asGeneric);

                Assert.False(validationResult.IsValid);

                var validationError = validationResult.ValidationErrors.Single();
                Assert.Equal(
                    string.Format(StringLengthAttribute_ValidationError, "Name", 5),
                    validationError.ErrorMessage);
                Assert.Equal("Name", validationError.PropertyName);
            }
        }

        [Fact]
        public void DbContext_ValidateEntity_is_called_when_validating_using_DbEntityEntry_GetValidationResult()
        {
            var entity = new EntityWithBuiltInValidationAttributes
                             {
                                 ID = 3,
                                 Name = "abc"
                             };

            using (var ctx = new SelfPopulatingContext(new object[] { entity }))
            {
                // this is to validate that we call it even if the entity is not in added/modified state
                ctx.Entry(entity).State = EntityState.Unchanged;

                var callCount = 0;

                ctx.ValidateEntityFunc = (dbEntityEntry) =>
                                             {
                                                 Assert.Equal(EntityState.Unchanged, dbEntityEntry.State);
                                                 callCount++;
                                                 return new DbEntityValidationResult(
                                                     dbEntityEntry,
                                                     Enumerable.Empty<DbValidationError>());
                                             };

                foreach (var useGenericDbEntityEntry in new[] { false, true })
                {
                    var validationResult = useGenericDbEntityEntry
                                               ? ctx.Entry(entity).GetValidationResult()
                                               : ctx.Entry((object)entity).GetValidationResult();

                    Assert.True(validationResult.IsValid);
                }

                // DbContext.ValidateEntity should be called once for each .GetValidationResult() call
                Assert.Equal(2, callCount);
            }
        }

        [Fact]
        public void DbEntityEntry_GetValidationResult_does_not_call_DetectChanges()
        {
            using (var context = new DbContextTests.ValidationTestContext())
            {
                context.Database.Initialize(force: false);

                using (new TransactionScope())
                {
                    context.Configuration.ValidateOnSaveEnabled = true;
                    var food = context.Entry(new Category("FOOD"));
                    context.Categories.Add(food.Entity);
                    context.SaveChanges();

                    Assert.Equal(EntityState.Unchanged, food.State);
                    food.Entity.DetailedDescription = "foo";
                    Assert.Equal(EntityState.Unchanged, food.State);

                    context.ValidateEntityFunc = (entry) =>
                                                     {
                                                         Assert.Same(food.Entity, entry.Entity);
                                                         Assert.Equal(EntityState.Unchanged, entry.State);

                                                         return new DbEntityValidationResult(
                                                             entry,
                                                             Enumerable.Empty
                                                                 <DbValidationError>());
                                                     };

                    var validationResult = food.GetValidationResult();

                    Assert.True(validationResult.IsValid);

                    Assert.Equal(EntityState.Unchanged, food.State);
                }
            }
        }

        [Fact]
        public void Nongeneric_DbEntityEntry_GetValidationResult_does_not_call_DetectChanges()
        {
            using (var context = new DbContextTests.ValidationTestContext())
            {
                context.Database.Initialize(force: false);

                using (new TransactionScope())
                {
                    context.Configuration.ValidateOnSaveEnabled = true;
                    var food = context.Entry((object)new Category("FOOD"));
                    context.Categories.Add((Category)food.Entity);
                    context.SaveChanges();

                    Assert.Equal(EntityState.Unchanged, food.State);
                    ((Category)food.Entity).DetailedDescription = "foo";
                    Assert.Equal(EntityState.Unchanged, food.State);

                    context.ValidateEntityFunc = (entry) =>
                                                     {
                                                         Assert.Same(food.Entity, entry.Entity);
                                                         Assert.Equal(EntityState.Unchanged, entry.State);

                                                         return new DbEntityValidationResult(
                                                             entry,
                                                             Enumerable.Empty
                                                                 <DbValidationError>());
                                                     };

                    var validationResult = food.GetValidationResult();

                    Assert.True(validationResult.IsValid);

                    Assert.Equal(EntityState.Unchanged, food.State);
                }
            }
        }

        #endregion

        #region DbMemberEntry and DbMemberEntry<T> GetValidationErrors tests

        [Fact]
        public void DbMemberEntry_GetValidationErrors_returns_no_errors_when_validating_property_of_entity_without_any_validation()
        {
            var entity = new EntityWithNoValidation
                             {
                                 ID = 3
                             };
            foreach (var useGenericDbPropertyEntry in new[] { false, true })
            {
                var errors = Invoke_DbMemberEntry_GetValidationErrors<EntityWithNoValidation, int>(
                    entity, "ID",
                    useGenericDbPropertyEntry);
                Assert.NotNull(errors);
                Assert.False(errors.Any());
            }
        }

        [Fact]
        public void DbMemberEntry_GetValidationErrors_returns_no_errors_when_validating_property_not_having_validators()
        {
            // the validated member does not have any validators but the entity does
            var entity = new EntityWithSomeValidation
                             {
                                 ID = 3,
                                 Name = "abc"
                             };
            foreach (var useGenericDbMemberEntry in new[] { false, true })
            {
                var errors = Invoke_DbMemberEntry_GetValidationErrors<EntityWithSomeValidation, int>(
                    entity, "ID",
                    useGenericDbMemberEntry);
                Assert.NotNull(errors);
                Assert.False(errors.Any());
            }
        }

        [Fact]
        public void DbMemberEntry_GetValidationErrors_returns_no_errors_for_valid_members()
        {
            var entity = new EntityWithBuiltInValidationAttributes
                             {
                                 ID = 3,
                                 Name = "abc"
                             };
            foreach (var useGenericDbMemberEntry in new[] { false, true })
            {
                var errors =
                    Invoke_DbMemberEntry_GetValidationErrors<EntityWithBuiltInValidationAttributes, string>(
                        entity,
                        "Name",
                        useGenericDbMemberEntry);
                Assert.NotNull(errors);
                Assert.False(errors.Any());
            }
        }

        [Fact]
        public void DbPropertyEntry_GetValidationErrors_returns_errors_for_invalid_members()
        {
            var entity = new EntityWithBuiltInValidationAttributes
                             {
                                 ID = 3,
                                 Name = "???????"
                             };
            foreach (var useGenericDbMemberEntry in new[] { false, true })
            {
                var errors =
                    Invoke_DbMemberEntry_GetValidationErrors<EntityWithBuiltInValidationAttributes, string>(
                        entity,
                        "Name",
                        useGenericDbMemberEntry);
                Assert.NotNull(errors);
                Assert.Equal(2, errors.Count());
            }
        }

        [Fact]
        public void DbPropertyEntry_GetValidationErrors_returns_errors_for_invalid_members_MetadataType()
        {
            var entity = new EntityWithValidationAttributesOnMetadataType
                             {
                                 Name = "AAAAAA"
                             };

            foreach (var asGeneric in new[] { true, false })
            {
                var validationErrors =
                    Invoke_DbMemberEntry_GetValidationErrors<EntityWithValidationAttributesOnMetadataType, string>(
                        entity, "Name", asGeneric);
                Assert.NotNull(validationErrors);

                var validationError = validationErrors.Single();
                Assert.Equal(
                    string.Format(StringLengthAttribute_ValidationError, "Name", 5),
                    validationError.ErrorMessage);
                Assert.Equal("Name", validationError.PropertyName);
            }
        }

        #endregion

        #region DbPropertyEntry and DbPropertyEntry<T> GetValidationErrors tests

        [Fact]
        public void DbPropertyEntry_GetValidationErrors_returns_no_errors_for_valid_primitive_properties()
        {
            var entity = new EntityWithBuiltInValidationAttributes
                             {
                                 ID = 3,
                                 Name = "abc"
                             };
            foreach (var useGenericDbPropertyEntry in new[] { false, true })
            {
                var errors = Invoke_DbPropertyEntry_GetValidationErrors(entity, e => e.Name, useGenericDbPropertyEntry);
                Assert.NotNull(errors);
                Assert.False(errors.Any());
            }
        }

        [Fact]
        public void DbPropertyEntry_GetValidationErrors_returns_errors_for_invalid_primitive_properties()
        {
            var entity = new EntityWithBuiltInValidationAttributes
                             {
                                 ID = 3,
                                 Name = "???????"
                             };
            foreach (var useGenericDbPropertyEntry in new[] { false, true })
            {
                var errors = Invoke_DbPropertyEntry_GetValidationErrors(entity, e => e.Name, useGenericDbPropertyEntry);
                Assert.NotNull(errors);
                Assert.Equal(2, errors.Count());
            }
        }

        #endregion

        #region DbComplexPropertyEntry and DbComplexPropertyEntry<T> GetValidationErrors tests

        [Fact]
        public void DbComplexPropertyEntry_GetValidationErrors_returns_no_errors_for_valid_complex_property()
        {
            var entity = new EntityWithComplexType
                             {
                                 ID = 3,
                                 ComplexProperty = new ComplexType
                                                       {
                                                           RequiredProperty = "abc"
                                                       }
                             };
            foreach (var useGenericDbPropertyEntry in new[] { false, true })
            {
                var errors = Invoke_DbComplexPropertyEntry_GetValidationErrors(
                    entity, e => e.ComplexProperty,
                    useGenericDbPropertyEntry);
                Assert.NotNull(errors);
                Assert.False(errors.Any());
            }
        }

        [Fact]
        public void DbComplexPropertyEntry_GetValidationErrors_returns_errors_for_invalid_complex_property()
        {
            var entity = new EntityWithComplexType
                             {
                                 ID = 3,
                                 ComplexProperty = new ComplexType
                                                       {
                                                           RequiredProperty = null
                                                       }
                             };
            foreach (var useGenericDbPropertyEntry in new[] { false, true })
            {
                var errors = Invoke_DbPropertyEntry_GetValidationErrors(
                    entity, e => e.ComplexProperty,
                    useGenericDbPropertyEntry);
                Assert.NotNull(errors);
                Assert.Equal(1, errors.Count());
            }
        }

        #endregion

        #region DbReferenceEntry and DbReferenceEntry<T> GetValidationErrors tests

        [Fact]
        public void DbPropertyEntry_GetValidationErrors_returns_errors_for_invalid_navigation_properties()
        {
            var entity = new EntityWithReferenceNavigationProperty
                             {
                                 ID = 1,
                                 Name = "Test",
                                 RelatedEntity = null
                             };

            foreach (var useGenericDbReferenceEntry in new[] { false, true })
            {
                var errors = Invoke_DbReferenceEntry_GetValidationErrors(
                    entity, e => e.RelatedEntity,
                    useGenericDbReferenceEntry);
                Assert.NotNull(errors);
                Assert.Equal(1, errors.Count());
                var validationError = errors.Single();
                Assert.Equal("RelatedEntity", validationError.PropertyName);
                Assert.Equal(
                    string.Format(RequiredAttribute_ValidationError, "RelatedEntity"),
                    validationError.ErrorMessage);
            }
        }

        [Fact]
        public void DbPropertyEntry_GetValidationErrors_does_not_drill_into_navigation_properties()
        {
            var entity = new EntityWithReferenceNavigationProperty
                             {
                                 ID = 1,
                                 Name = "Test",
                                 RelatedEntity = new EntityWithBuiltInValidationAttributes
                                                     {
                                                         ID = 3,
                                                         Name = "<>?<>?"
                                                     }
                             };

            foreach (var useGenericDbReferenceEntry in new[] { false, true })
            {
                var errors = Invoke_DbReferenceEntry_GetValidationErrors(
                    entity, e => e.RelatedEntity,
                    useGenericDbReferenceEntry);
                Assert.NotNull(errors);
                Assert.Equal(0, errors.Count());
            }
        }

        #endregion

        #region DbCollectionEntry and DbCollectionEntry<T> GetValidationErrors tests

        [Fact]
        public void DbCollectionEntry_GetValidationErrors_returns_errors_for_invalid_navigation_properties()
        {
            var entity = new EntityWithCollectionNavigationProperty
                             {
                                 ID = 1,
                                 Name = "Test",
                                 RelatedEntities = null
                             };

            foreach (var useGenericDbCollectionEntry in new[] { false, true })
            {
                var errors = Invoke_DbCollectionEntry_GetValidationErrors(
                    entity, e => e.RelatedEntities,
                    useGenericDbCollectionEntry);
                Assert.NotNull(errors);
                Assert.Equal(1, errors.Count());
                var validationError = errors.Single();
                Assert.Equal("RelatedEntities", validationError.PropertyName);
                Assert.Equal(
                    string.Format(RequiredAttribute_ValidationError, "RelatedEntities"),
                    validationError.ErrorMessage);
            }
        }

        [Fact]
        public void DbCollectionEntry_GetValidationErrors_does_not_drill_into_navigation_properties()
        {
            var entity = new EntityWithCollectionNavigationProperty
                             {
                                 ID = 1,
                                 Name = "Test",
                                 RelatedEntities = new List<EntityWithBuiltInValidationAttributes>(
                                     new[]
                                         {
                                             new EntityWithBuiltInValidationAttributes
                                                 {
                                                     ID = 3,
                                                     Name = "<>?<>?"
                                                 }
                                         })
                             };

            foreach (var useGenericDbCollectionEntry in new[] { false, true })
            {
                var errors = Invoke_DbCollectionEntry_GetValidationErrors(
                    entity, e => e.RelatedEntities,
                    useGenericDbCollectionEntry);
                Assert.NotNull(errors);
                Assert.Equal(0, errors.Count());
            }
        }

        #endregion

        #region ValidateEntity Tests

        #region Primitive and Complex properties

        [Fact]
        public void ValidateEntity_does_not_return_errors_for_valid_entities()
        {
            var entityWithEntityLevelCustomValidationAttributes =
                new EntityWithEntityLevelCustomValidationAttributes
                    {
                        ID = "5"
                    };
            var entities = new object[]
                               {
                                   new EntityWithNoValidation
                                       {
                                           ID = 1
                                       },
                                   new EntityWithSomeValidation
                                       {
                                           ID = 5,
                                           Name = "123",
                                           Required = 42
                                       },
                                   new EntityWithBuiltInValidationAttributes
                                       {
                                           ID = 3,
                                           Name = "abc"
                                       },
                                   new EntityWithComplexType
                                       {
                                           ID = 42,
                                           ComplexProperty = new ComplexType
                                                                 {
                                                                     RequiredProperty = "abc"
                                                                 }
                                       },
                                   new EntityWithComplexTypeLevelCustomValidationAttributes
                                       {
                                           ID = 43,
                                           ComplexProperty = new ComplexTypeWithTypeLevelCustomValidationAttributes()
                                       },
                                   new EntityWithPropertyLevelCustomValidationAttributes
                                       {
                                           ID = 2
                                       },
                                   entityWithEntityLevelCustomValidationAttributes,
                                   new EntityWithFKReferenceNavigationPropertyDependant
                                       {
                                           RelatedEntity = entityWithEntityLevelCustomValidationAttributes
                                       },
                                   new EntityWithFKReferenceNavigationPropertyWorkaround
                                       {
                                           RelatedEntity = entityWithEntityLevelCustomValidationAttributes
                                       },
                                   new ValidatableEntity()
                               };

            foreach (var entity in entities)
            {
                var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
                Assert.NotNull(entityValidationResult);
                Assert.True(entityValidationResult.IsValid);
            }
        }

        [Fact]
        public void ValidateEntity_returns_errors_for_invalid_entities_with_built_in_attributes_defined_on_properties()
        {
            var entity = new EntityWithBuiltInValidationAttributes
                             {
                                 ID = -1,
                                 Name = "1+2=??"
                             };
            var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);

            Assert.NotNull(entityValidationResult);
            Assert.False(entityValidationResult.IsValid);
            Assert.Same(entity, entityValidationResult.Entry.Entity);

            Assert.Equal(3, entityValidationResult.ValidationErrors.Count);
            Assert.True(
                entityValidationResult.ValidationErrors.SingleOrDefault(
                    e => e.PropertyName == "ID" &&
                         e.ErrorMessage == string.Format(RangeAttribute_ValidationError, "ID", 0, 100)) != null);
            Assert.True(
                entityValidationResult.ValidationErrors.SingleOrDefault(
                    e => e.PropertyName == "Name" &&
                         e.ErrorMessage == string.Format(StringLengthAttribute_ValidationError, "Name", 5)) != null);
            Assert.True(
                entityValidationResult.ValidationErrors.SingleOrDefault(
                    e => e.PropertyName == "Name" &&
                         e.ErrorMessage == string.Format(RegexAttribute_ValidationError, "Name", @"[\w]+")) != null);
        }

        [Fact]
        public void ValidateEntity_returns_errors_for_entities_with_invalid_complex_properties()
        {
            var entity = new EntityWithComplexType
                             {
                                 ID = -1,
                                 ComplexProperty = new ComplexType
                                                       {
                                                           RequiredProperty = null
                                                       }
                             };
            var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);

            Assert.NotNull(entityValidationResult);
            Assert.False(entityValidationResult.IsValid);
            Assert.Same(entity, entityValidationResult.Entry.Entity);

            Assert.Equal(1, entityValidationResult.ValidationErrors.Count);
            Assert.True(
                entityValidationResult.ValidationErrors.SingleOrDefault(
                    e => e.PropertyName == "ComplexProperty.RequiredProperty" &&
                         e.ErrorMessage ==
                         string.Format(RequiredAttribute_ValidationError, "ComplexProperty.RequiredProperty")) != null);
        }

        #endregion

        #region Reference and Collection kinds of properties

        [Fact]
        public void ValidateEntity_returns_errors_for_entities_with_invalid_reference_properties()
        {
            var entity = new EntityWithReferenceNavigationProperty
                             {
                                 ID = 1,
                                 Name = "Test",
                                 RelatedEntity = null
                             };

            var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
            Assert.NotNull(entityValidationResult);
            Assert.False(entityValidationResult.IsValid);
            Assert.Equal(1, entityValidationResult.ValidationErrors.Count());
            var validationError = entityValidationResult.ValidationErrors.Single();

            Assert.Equal("RelatedEntity", validationError.PropertyName);
            Assert.Equal(string.Format(RequiredAttribute_ValidationError, "RelatedEntity"), validationError.ErrorMessage);
        }

        [Fact]
        public void ValidateEntity_returns_errors_for_principal_entities_in_one_to_one_relationships_with_null_keys()
        {
            var entity = new EntityWithFKReferenceNavigationPropertyPrincipal
                             {
                                 RelatedEntity = new EntityWithEntityLevelCustomValidationAttributes
                                                     {
                                                         ID = "foo"
                                                     }
                             };

            var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
            Assert.NotNull(entityValidationResult);
            Assert.False(entityValidationResult.IsValid);
            Assert.Equal(1, entityValidationResult.ValidationErrors.Count());
            var validationError = entityValidationResult.ValidationErrors.Single();

            Assert.Equal("ID", validationError.PropertyName);
            Assert.Equal(string.Format(RequiredAttribute_ValidationError, "ID"), validationError.ErrorMessage);
        }

        [Fact]
        public void ValidateEntity_does_not_drill_into_navigation_properties()
        {
            var entity = new EntityWithReferenceNavigationProperty
                             {
                                 ID = 1,
                                 Name = "Test",
                                 RelatedEntity = new EntityWithBuiltInValidationAttributes
                                                     {
                                                         ID = 3,
                                                         Name = "<>?<>?"
                                                     }
                             };

            var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
            Assert.NotNull(entityValidationResult);
            Assert.True(entityValidationResult.IsValid);
        }

        [Fact]
        public void ValidateEntity_returns_errors_for_entities_with_invalid_collection_properties()
        {
            var entity = new EntityWithCollectionNavigationProperty
                             {
                                 ID = 1,
                                 Name = "Test",
                                 RelatedEntities = new List<EntityWithBuiltInValidationAttributes>()
                             };

            var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
            Assert.NotNull(entityValidationResult);
            Assert.False(entityValidationResult.IsValid);
            Assert.Equal(1, entityValidationResult.ValidationErrors.Count());
            var validationError = entityValidationResult.ValidationErrors.Single();

            Assert.Equal("RelatedEntities", validationError.PropertyName);
            Assert.Equal("Collection cannot be empty.", validationError.ErrorMessage);
        }

        [Fact]
        public void ValidateEntity_does_not_drill_into_collection_properties()
        {
            var entity = new EntityWithCollectionNavigationProperty
                             {
                                 ID = 1,
                                 Name = "Test",
                                 RelatedEntities = new List<EntityWithBuiltInValidationAttributes>(
                                     new[]
                                         {
                                             new EntityWithBuiltInValidationAttributes
                                                 {
                                                     ID = 1,
                                                     Name = "?"
                                                 }
                                         })
                             };

            var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
            Assert.NotNull(entityValidationResult);
            Assert.True(entityValidationResult.IsValid);
        }

        #endregion

        #region Navigation properties

        [Fact]
        public void ValidateEntity_does_not_load_entities_if_LazyLoading_is_enabled()
        {
            Test_ValidateEntity_LazyLoading(true);
        }

        [Fact]
        public void ValidateEntity_does_not_load_entities_if_LazyLoading_is_disabled()
        {
            Test_ValidateEntity_LazyLoading(false);
        }

        private void Test_ValidateEntity_LazyLoading(bool lazyLoadingEnabled)
        {
            var entity = new EntityWithReferenceNavigationProperty
                             {
                                 ID = 1,
                                 Name = "Test",
                                 RelatedEntity = new EntityWithBuiltInValidationAttributes
                                                     {
                                                         ID = 3,
                                                         Name = "abc"
                                                     }
                             };

            using (var context = new SelfPopulatingContext(entity))
            {
                using (new TransactionScope())
                {
                    context.SaveChanges();

                    foreach (var entry in context.ChangeTracker.Entries().ToList())
                    {
                        entry.State = EntityState.Detached;
                    }

                    context.Configuration.LazyLoadingEnabled = lazyLoadingEnabled;

                    entity = context.Set<EntityWithReferenceNavigationProperty>().Single();

                    var errors = context.ValidateEntity(context.Entry(entity)).ValidationErrors;

                    Assert.NotNull(errors);
                    Assert.Equal(1, errors.Count());
                    var validationError = errors.Single();
                    Assert.Equal("RelatedEntity", validationError.PropertyName);
                    Assert.Equal(
                        string.Format(RequiredAttribute_ValidationError, "RelatedEntity"),
                        validationError.ErrorMessage);

                    if (lazyLoadingEnabled)
                    {
                        Assert.NotNull(entity.RelatedEntity);
                    }
                    else
                    {
                        Assert.Null(entity.RelatedEntity);
                    }

                    Assert.Equal(lazyLoadingEnabled, context.Configuration.LazyLoadingEnabled);
                    Assert.Equal(lazyLoadingEnabled, GetObjectContext(context).ContextOptions.LazyLoadingEnabled);
                }
            }
        }

        #endregion

        #region Transient properties

        [Fact]
        public void ValidateEntity_returns_errors_for_entities_with_invalid_transient_properties()
        {
            var entity = new EntityWithTransientPropertyValidation();

            var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
            Assert.NotNull(entityValidationResult);
            Assert.False(entityValidationResult.IsValid);
            Assert.Equal(2, entityValidationResult.ValidationErrors.Count());
            var validationError = entityValidationResult.ValidationErrors.First();

            Assert.Equal("Name", validationError.PropertyName);
            Assert.Equal(string.Format(RequiredAttribute_ValidationError, "Name"), validationError.ErrorMessage);

            validationError = entityValidationResult.ValidationErrors.ElementAt(1);
            Assert.Equal("OtherName", validationError.PropertyName);
            Assert.Equal(string.Format(RequiredAttribute_ValidationError, "Other Name"), validationError.ErrorMessage);
        }

        [Fact]
        public void ValidateEntity_does_not_return_errors_for_entities_with_valid_transient_properties()
        {
            var entity = new EntityWithTransientPropertyValidation
                             {
                                 Name = "abc"
                             };

            var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
            Assert.NotNull(entityValidationResult);
            Assert.True(entityValidationResult.IsValid);
            Assert.Equal(0, entityValidationResult.ValidationErrors.Count());
        }

        #endregion

        #region Custom validation attributes, IValidatableObject

        [Fact]
        public void ValidateEntity_returns_errors_for_invalid_entities_with_custom_attributes_defined_on_properties()
        {
            try
            {
                var results = new[]
                                  {
                                      null,
                                      // this will actually prevent from returning any ValidationResults - equivalent to returning ValidationResult.Success

                                      new ValidationResult("Error", null),
                                      new ValidationResult("Error msg", new string[] { }),
                                      new ValidationResult("Error msg", new string[] { null }),
                                      new ValidationResult("Error msg", new[] { "property" }),
                                      new ValidationResult(
                                          "Error msg",
                                          new[] { "Property1", "Property2" }),
                                      new ValidationResult(
                                          "Error msg",
                                          new[] { "Property1", null, "Property2" }),
                                      new ValidationResult(null, null),
                                      new ValidationResult(null, new string[] { }),
                                      new ValidationResult(null, new string[] { null }),
                                      new ValidationResult(null, new[] { "property" }),
                                      new ValidationResult(null, new[] { "Property1", "Property2" }),
                                      new ValidationResult(
                                          null,
                                          new[] { "Property1", null, "Property2" }),
                                  };

                var entity = new EntityWithPropertyLevelCustomValidationAttributes
                                 {
                                     ID = -3
                                 };

                foreach (var resultForValidationWithContext in results)
                {
                    foreach (var resultForValidationWithoutContext in results)
                    {
                        EntityWithPropertyLevelCustomValidationAttributes.ValidateWithoutContextFunc =
                            (propertyValue) =>
                                {
                                    Assert.Equal(entity.ID, propertyValue);
                                    return resultForValidationWithoutContext;
                                };
                        EntityWithPropertyLevelCustomValidationAttributes.ValidateWithContextFunc =
                            (propertyValue, ctx) =>
                                {
                                    Assert.Equal(entity.ID, propertyValue);
                                    Assert.Equal("ID", ctx.DisplayName);
                                    return resultForValidationWithContext;
                                };

                        var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
                        VerifyValidationResults(
                            entity,
                            "ID",
                            new[] { resultForValidationWithoutContext, resultForValidationWithContext },
                            entityValidationResult);
                    }
                }
            }
            finally
            {
                EntityWithPropertyLevelCustomValidationAttributes.ValidateWithContextFunc = null;
                EntityWithPropertyLevelCustomValidationAttributes.ValidateWithoutContextFunc = null;
            }
        }

        [Fact]
        public void ValidateEntity_returns_errors_for_invalid_entities_with_custom_attributes_defined_on_entity_type()
        {
            try
            {
                var results = new[]
                                  {
                                      null,
                                      new ValidationResult("Error", null),
                                      new ValidationResult("Error msg", new string[] { }),
                                      new ValidationResult("Error msg", new string[] { null }),
                                      new ValidationResult("Error msg", new[] { "property" }),
                                      new ValidationResult(
                                          "Error msg",
                                          new[] { "Property1", "Property2" }),
                                      new ValidationResult(
                                          "Error msg",
                                          new[] { "Property1", null, "Property2" }),
                                      new ValidationResult(null, null),
                                      new ValidationResult(null, new string[] { }),
                                      new ValidationResult(null, new string[] { null }),
                                      new ValidationResult(null, new[] { "property" }),
                                      new ValidationResult(null, new[] { "Property1", "Property2" }),
                                      new ValidationResult(
                                          null,
                                          new[] { "Property1", null, "Property2" }),
                                  };

                var entity = new EntityWithEntityLevelCustomValidationAttributes
                                 {
                                     ID = "-3"
                                 };

                foreach (var resultForValidationWithContext in results)
                {
                    foreach (var resultForValidationWithoutContext in results)
                    {
                        EntityWithEntityLevelCustomValidationAttributes.ValidateWithoutContextFunc =
                            (validatedEntity) =>
                                {
                                    Assert.NotNull(validatedEntity);
                                    Assert.Same(entity, validatedEntity);
                                    return resultForValidationWithoutContext;
                                };
                        EntityWithEntityLevelCustomValidationAttributes.ValidateWithContextFunc =
                            (validatedEntity, ctx) =>
                                {
                                    Assert.NotNull(validatedEntity);
                                    Assert.Same(entity, validatedEntity);
                                    Assert.Equal(validatedEntity.GetType().Name, ctx.DisplayName);
                                    return resultForValidationWithContext;
                                };

                        var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
                        VerifyValidationResults(
                            entity,
                            null,
                            new[] { resultForValidationWithoutContext, resultForValidationWithContext },
                            entityValidationResult);
                    }
                }
            }
            finally
            {
                EntityWithEntityLevelCustomValidationAttributes.ValidateWithContextFunc = null;
                EntityWithEntityLevelCustomValidationAttributes.ValidateWithoutContextFunc = null;
            }
        }

        [Fact]
        public void ValidateEntity_does_not_return_errors_for_valid_validatable_entity()
        {
            var entity = new ValidatableEntity
                             {
                                 ID = 3
                             };

            var validationResultCombinations = new[]
                                                   {
                                                       null,
                                                       new ValidationResult[0]
                                                   };

            foreach (var validationResults in validationResultCombinations)
            {
                entity.ValidationResults = validationResults;
                var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);

                Assert.True(entityValidationResult.IsValid);
                Assert.True(!entityValidationResult.ValidationErrors.Any());
            }
        }

        [Fact]
        public void ValidateEntity_returns_error_for_invalid_validatable_entity()
        {
            var entity = new ValidatableEntity
                             {
                                 ID = 3
                             };

            var validationResultCombinations = new[]
                                                   {
                                                       new[] { new ValidationResult(null, null) },
                                                       new[]
                                                           { new ValidationResult("The entity is not valid.") },
                                                       new[]
                                                           { new ValidationResult("The entity is not valid.", null) },
                                                       new[]
                                                           { new ValidationResult("The entity is not valid.", new string[0]) },
                                                       new[]
                                                           {
                                                               new ValidationResult("1st invalid entity"),
                                                               new ValidationResult("2nd invalid entity", new[] { "ID" })
                                                               ,
                                                           },
                                                       new[]
                                                           {
                                                               new ValidationResult(
                                                                   "Invalid",
                                                                   new[] { "ID", null, "Test" })
                                                           },
                                                       new[]
                                                           {
                                                               new ValidationResult(
                                                                   "Some properties are invalid",
                                                                   new[] { "Name", "xyz" }),
                                                               new ValidationResult(
                                                                   "Invalid",
                                                                   new[] { "ID", null, "Test" })
                                                           },
                                                   };

            foreach (var validationResults in validationResultCombinations)
            {
                entity.ValidationResults = validationResults;
                var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
                VerifyValidationResults(entity, null, validationResults, entityValidationResult);
            }
        }

        private void VerifyValidationResults(
            object entity, string validatedPropertyName,
            IEnumerable<ValidationResult> sourceValidationResults,
            DbEntityValidationResult entityValidationResult)
        {
            Debug.Assert(sourceValidationResults != null);
            Debug.Assert(entityValidationResult != null);

            Assert.True(
                sourceValidationResults.Any(r => r != null) || entityValidationResult.IsValid,
                "Errors returned for valid entities");
            Assert.True(
                !sourceValidationResults.Any(r => r != null) || !entityValidationResult.IsValid,
                "Not all errors returned for an invalid entity");

            if (entityValidationResult != null)
            {
                Assert.Same(entity, entityValidationResult.Entry.Entity);

                var validationErrors =
                    new List<DbValidationError>(entityValidationResult.ValidationErrors);

                foreach (var validationResult in sourceValidationResults.Where(r => r != null))
                {
                    // if the ValidationResult returned by the user does not contain any names in MemberNames property 
                    // Validator should set the property name to the name of the property that is being validated or to null
                    // if this is entity level validation. 
                    var sourceMemberNames = (validationResult.MemberNames == null || !validationResult.MemberNames.Any())
                                                ? new string[1] { validatedPropertyName }
                                                : validationResult.MemberNames;

                    // if ErrorMessage property of the ValidationResult instance returned by the user from CusomtValidationAttribute 
                    // is null, DataAnnotations validator comes up with its own non-null error message. In addition if display name 
                    // is not provided type name is being used. Note that this is not true in case of results returned IValidatableObject.Validate().
                    var expectedErrorMessage = validationResult.ErrorMessage ??
                                               (entity is IValidatableObject
                                                    ? null
                                                    : string.Format(
                                                        "{0} is not valid.",
                                                        validatedPropertyName ?? entity.GetType().Name));

                    foreach (var memberName in sourceMemberNames)
                    {
                        var validationError = validationErrors.FirstOrDefault(
                            e =>
                            e.PropertyName == (memberName ?? validatedPropertyName) &&
                            e.ErrorMessage == expectedErrorMessage);

                        Assert.NotNull(validationError);
                        validationErrors.Remove(validationError);
                    }
                }

                Assert.False(
                    validationErrors.Any(),
                    "Entity validation error contains errors that were not reported by validation.");
            }
        }

        #endregion

        #region Custom validation attributes on Complex properties

        [Fact]
        public void ValidateEntity_does_not_run_type_level_validation_for_null_complex_properties()
        {
            var entity = new EntityWithComplexTypeLevelCustomValidationAttributes
                             {
                                 ID = 3
                             };

            try
            {
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithoutContextFunc =
                    (propertyValue) =>
                        {
                            Assert.True(false, "This shouldn't run.");
                            return ValidationResult.Success;
                        };
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithContextFunc =
                    (propertyValue, ctx) =>
                        {
                            Assert.True(false, "This shouldn't run.");
                            return ValidationResult.Success;
                        };

                var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
                Assert.NotNull(entityValidationResult);
                Assert.True(entityValidationResult.IsValid);
                Assert.Same(entity, entityValidationResult.Entry.Entity);
            }
            finally
            {
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithoutContextFunc = null;
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithContextFunc = null;
            }
        }

        [Fact]
        public void ValidateEntity_returns_errors_for_invalid_entities_with_custom_type_level_attributes_defined_on_complex_properties()
        {
            var entity = new EntityWithComplexTypeLevelCustomValidationAttributes
                             {
                                 ID = 4,
                                 ComplexProperty =
                                     new ComplexTypeWithTypeLevelCustomValidationAttributes
                                         {
                                             IsValid = false
                                         }
                             };

            var result = new ValidationResult("Property is not valid.");

            try
            {
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithoutContextFunc =
                    (value) =>
                        {
                            if (!value.IsValid)
                            {
                                return result;
                            }
                            return ValidationResult.Success;
                        };
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithContextFunc =
                    (value, ctx) =>
                        {
                            if (!value.IsValid)
                            {
                                return result;
                            }

                            Assert.Equal("ID", ctx.DisplayName);
                            return ValidationResult.Success;
                        };

                var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
                VerifyValidationResults(
                    entity,
                    "ComplexProperty",
                    new[] { result, result },
                    entityValidationResult);
            }
            finally
            {
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithoutContextFunc = null;
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithContextFunc = null;
            }
        }

        [Fact]
        public void ValidateEntity_does_not_run_type_level_validation_for_invalid_complex_properties()
        {
            var entity = new EntityWithComplexTypeLevelCustomValidationAttributes
                             {
                                 ID = 4,
                                 ComplexProperty =
                                     new ComplexTypeWithTypeLevelCustomValidationAttributes
                                         {
                                             IsValid = false
                                         }
                             };

            var errorMessage = "Error";

            try
            {
                EntityWithComplexTypeLevelCustomValidationAttributes.ValidateWithoutContextFunc =
                    (propertyValue) => { return new ValidationResult(errorMessage); };
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithoutContextFunc =
                    (value) =>
                        {
                            Assert.True(false, "This shouldn't run.");
                            return ValidationResult.Success;
                        };
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithContextFunc =
                    (value, ctx) =>
                        {
                            Assert.True(false, "This shouldn't run.");
                            return ValidationResult.Success;
                        };

                var entityValidationResult = Invoke_DbContext_ValidateEntity(entity);
                Assert.NotNull(entityValidationResult);
                Assert.False(entityValidationResult.IsValid);
                Assert.Same(entity, entityValidationResult.Entry.Entity);

                Assert.Equal(1, entityValidationResult.ValidationErrors.Count);
                Assert.True(
                    entityValidationResult.ValidationErrors.SingleOrDefault(
                        e => e.PropertyName == "ComplexProperty" && e.ErrorMessage == errorMessage) != null);
            }
            finally
            {
                EntityWithComplexTypeLevelCustomValidationAttributes.ValidateWithoutContextFunc = null;
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithoutContextFunc = null;
                ComplexTypeWithTypeLevelCustomValidationAttributes.ValidateWithContextFunc = null;
            }
        }

        #endregion

        #endregion

        #region Helper invokers used by tests

        public static IEnumerable<DbEntityValidationResult> Invoke_DbContext_GetValidationErrors(object[] entities)
        {
            using (var ctx = new SelfPopulatingContext(entities))
            {
                return ctx.GetValidationErrors();
            }
        }

        public static DbEntityValidationResult Invoke_DbContext_ValidateEntity(object entity)
        {
            Database.SetInitializer((IDatabaseInitializer<SelfPopulatingContext>)null);

            using (var ctx = new SelfPopulatingContext(new object[0]))
            {
                return ctx.ValidateEntity(ctx.Entry(entity));
            }
        }

        public static DbEntityValidationResult Invoke_DbEntityEntry_GetValidationResult<TEntity>(
            TEntity entity,
            bool asGeneric)
            where TEntity : class, new()
        {
            using (var ctx = new SelfPopulatingContext(new object[] { entity }))
            {
                var entityEntry = ctx.Entry(entity);
                return asGeneric
                           ? entityEntry.GetValidationResult()
                           : ((DbEntityEntry)entityEntry).GetValidationResult();
            }
        }

        public static ICollection<DbValidationError> Invoke_DbMemberEntry_GetValidationErrors<TEntity, TProperty>(
            TEntity entity, string property, bool asGeneric)
            where TEntity : class, new()
        {
            using (var ctx = new SelfPopulatingContext(new object[] { entity }))
            {
                var memberEntry = ctx.Entry(entity).Member<TProperty>(property);

                return asGeneric
                           ? memberEntry.GetValidationErrors()
                           : ((DbMemberEntry)memberEntry).GetValidationErrors();
            }
        }

        public static ICollection<DbValidationError> Invoke_DbPropertyEntry_GetValidationErrors<TEntity, TProperty>(
            TEntity entity, Expression<Func<TEntity, TProperty>> property, bool asGeneric)
            where TEntity : class, new()
        {
            using (var ctx = new SelfPopulatingContext(new object[] { entity }))
            {
                var propertyEntry = ctx.Entry(entity).Property(property);

                return asGeneric
                           ? propertyEntry.GetValidationErrors()
                           : ((DbPropertyEntry)propertyEntry).GetValidationErrors();
            }
        }

        public static ICollection<DbValidationError> Invoke_DbComplexPropertyEntry_GetValidationErrors
            <TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> property, bool asGeneric)
            where TEntity : class, new()
        {
            using (var ctx = new SelfPopulatingContext(new object[] { entity }))
            {
                var propertyEntry = ctx.Entry(entity).ComplexProperty(property);

                return asGeneric
                           ? propertyEntry.GetValidationErrors()
                           : ((DbComplexPropertyEntry)propertyEntry).GetValidationErrors();
            }
        }

        public static ICollection<DbValidationError> Invoke_DbReferenceEntry_GetValidationErrors<TEntity, TProperty>(
            TEntity entity, Expression<Func<TEntity, TProperty>> property, bool asGeneric)
            where TEntity : class, new()
            where TProperty : class
        {
            using (var ctx = new SelfPopulatingContext(new object[] { entity }))
            {
                var referenceEntry = ctx.Entry(entity).Reference(property);

                return asGeneric
                           ? referenceEntry.GetValidationErrors()
                           : ((DbReferenceEntry)referenceEntry).GetValidationErrors();
            }
        }

        public static ICollection<DbValidationError> Invoke_DbCollectionEntry_GetValidationErrors<TEntity, TElement>(
            TEntity entity, Expression<Func<TEntity, ICollection<TElement>>> property, bool asGeneric)
            where TEntity : class, new()
            where TElement : class
        {
            using (var ctx = new SelfPopulatingContext(new object[] { entity }))
            {
                var collectionEntry = ctx.Entry(entity).Collection(property);

                return asGeneric
                           ? collectionEntry.GetValidationErrors()
                           : ((DbCollectionEntry)collectionEntry).GetValidationErrors();
            }
        }

        #endregion

        #region String resource helpers

        private readonly string MaxLengthAttribute_ValidationError = LookupString
            (EntityFrameworkAssembly, "System.Data.Entity.Properties.Resources", "MaxLengthAttribute_ValidationError");

        private readonly string RangeAttribute_ValidationError = LookupString
            (
                SystemComponentModelDataAnnotationsAssembly,
                "System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources",
                "RangeAttribute_ValidationError");

        private readonly string RegexAttribute_ValidationError = LookupString
            (
                SystemComponentModelDataAnnotationsAssembly,
                "System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources",
                "RegexAttribute_ValidationError");

        private readonly string RequiredAttribute_ValidationError = LookupString
            (
                SystemComponentModelDataAnnotationsAssembly,
                "System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources",
                "RequiredAttribute_ValidationError");

        private readonly string StringLengthAttribute_ValidationError = LookupString
            (
                SystemComponentModelDataAnnotationsAssembly,
                "System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources",
                "StringLengthAttribute_ValidationError");

        private readonly string StringLengthAttribute_ValidationErrorIncludingMinimum = LookupString
            (
                SystemComponentModelDataAnnotationsAssembly,
                "System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources",
                "StringLengthAttribute_ValidationErrorIncludingMinimum");

        #endregion

        #region DataAnnotationConfigurationOverridesConvention tests

        public class DataAnnotationsConfigurationOverridesCtx : DbContext
        {
            public DataAnnotationsConfigurationOverridesCtx()
            {
                Database.SetInitializer((IDatabaseInitializer<DataAnnotationsConfigurationOverridesCtx>)null);
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<EntityWithSomeValidation>();
                modelBuilder.Entity<EntityWithComplexType>();
                modelBuilder.ComplexType<ComplexType>();

                modelBuilder.Entity<EntityWithSomeValidation>().Property(p => p.Required).IsOptional();
                modelBuilder.Entity<EntityWithSomeValidation>().Property(p => p.Name).HasMaxLength(20);

                modelBuilder.ComplexType<ComplexType>().Property(p => p.RequiredProperty).IsOptional();
                modelBuilder.ComplexType<ComplexType>().Property(p => p.ByteArray).HasMaxLength(5);
                modelBuilder.ComplexType<ComplexType>().Property(p => p.StringWithStringLengthAttribute).HasMaxLength(20);
                modelBuilder.ComplexType<ComplexType>().Property(p => p.StringWithMaxLengthAndStringLengthAttributes).
                    IsMaxLength();

                modelBuilder.ComplexType<ComplexType>().Property(p => p.StringWithStringLengthAttribute).IsRequired();
            }
        }

        [Fact]
        public void Setting_entity_property_as_optional_does_not_override_Required_attribute()
        {
            using (var ctx = new DataAnnotationsConfigurationOverridesCtx())
            {
                Assert.False(
                    ctx.Entry(
                        new EntityWithSomeValidation
                            {
                                ID = 0,
                                Name = "abc",
                                Required = null
                            })
                        .GetValidationResult().IsValid);
            }
        }

        [Fact]
        public void Changing_entity_property_MaxLength_value_does_not_override_attribute_setting()
        {
            var entity = new EntityWithSomeValidation
                             {
                                 ID = 0,
                                 Name = new string('a', 5),
                                 Required = 0
                             };

            using (var ctx = new DataAnnotationsConfigurationOverridesCtx())
            {
                Assert.Equal(
                    0,
                    ctx.Entry(entity).GetValidationResult().ValidationErrors.Count());
                entity.Name = new string('a', 10);
                Assert.Equal(
                    1,
                    ctx.Entry(entity).GetValidationResult().ValidationErrors.Count());
                entity.Name = new string('a', 21);
                Assert.Equal(
                    1,
                    ctx.Entry(entity).GetValidationResult().ValidationErrors.Count());
            }
        }

        [Fact]
        public void Configuring_complex_type_property_MaxLength_setting_does_not_override_attribute_setting()
        {
            var entity = new EntityWithComplexType
                             {
                                 ComplexProperty =
                                     new ComplexType
                                         {
                                             ByteArray = new byte[3],
                                             RequiredProperty = "a",
                                             StringWithStringLengthAttribute = new string('a', 8)
                                         }
                             };

            using (var ctx = new DataAnnotationsConfigurationOverridesCtx())
            {
                Assert.Equal(0, ctx.Entry(entity).GetValidationResult().ValidationErrors.Count());
                entity.ComplexProperty.ByteArray = new byte[11];
                Assert.Equal(1, ctx.Entry(entity).GetValidationResult().ValidationErrors.Count());
            }
        }

        [Fact]
        public void Configuring_property_to_be_MaxLength_does_not_override_StringLength_and_MaxLength_attributes()
        {
            var entity = new EntityWithComplexType
                             {
                                 ComplexProperty =
                                     new ComplexType
                                         {
                                             StringWithMaxLengthAndStringLengthAttributes = new string('z', 4096),
                                             RequiredProperty = "a",
                                             StringWithStringLengthAttribute = new string('a', 8)
                                         }
                             };

            using (var ctx = new DataAnnotationsConfigurationOverridesCtx())
            {
                Assert.Equal(2, ctx.Entry(entity).GetValidationResult().ValidationErrors.Count());
            }
        }

        #endregion
    }
}
