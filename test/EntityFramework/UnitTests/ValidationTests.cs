// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiUnitTests.Validation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Validation;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Validation;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using Moq;
    using Moq.Protected;
    using Xunit;
    using Xunit.Sdk;

    #region Types for validators tests

    [CustomValidation(typeof(AirportDetails), "ValidateCountry")]
    public class AirportDetails : IValidatableObject
    {
        [Required]
        [RegularExpression("^[A-Z]{3}$")]
        public string AirportCode { get; set; }

        public string CityCode { get; set; }

        public string CountryCode { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }

        public static ValidationResult ValidateCountry(AirportDetails airportDetails, ValidationContext validationContex)
        {
            return airportDetails.CountryCode == "ZZ" && airportDetails.CityCode != "XXX"
                       ? new ValidationResult(string.Format("City '{0}' is not located in country 'ZZ'.", airportDetails.CityCode))
                       : ValidationResult.Success;
        }
    }

    public class EntityWithOptionalNestedComplexType
    {
        public int ID { get; set; }

        public AirportDetails AirportDetails { get; set; }
    }

    public class DepartureArrivalInfoWithNestedComplexType : IValidatableObject
    {
        public DepartureArrivalInfoWithNestedComplexType()
        {
            Time = DateTime.MaxValue;
        }

        [Required]
        public AirportDetails Airport { get; set; }

        public DateTime Time { get; set; }

        internal IEnumerable<ValidationResult> ValidationResults { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            return ValidationResults ?? (Time <= DateTime.Now
                                             ? new[] { new ValidationResult("Date cannot be in the past.") }
                                             : Enumerable.Empty<ValidationResult>());
        }
    }

    public class AircraftInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class FlightSegmentWithNestedComplexTypes
    {
        public int FlightSegmentId { get; set; }

        [RegularExpression(@"^[A-Z]{2}\d{4}$")]
        public string FlightNumber { get; set; }

        [Required]
        public DepartureArrivalInfoWithNestedComplexType Departure { get; set; }

        [Required]
        public DepartureArrivalInfoWithNestedComplexType Arrival { get; set; }

        public AircraftInfo Aircraft { get; set; }
    }

    [CustomValidation(typeof(FlightSegmentWithNestedComplexTypesWithTypeLevelValidation), "FailOnRequest")]
    public class FlightSegmentWithNestedComplexTypesWithTypeLevelValidation : FlightSegmentWithNestedComplexTypes, IValidatableObject
    {
        public static bool ShouldFail { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Aircraft != null && Aircraft.Code == "A380" && FlightNumber == "QF0006"
                       ? new[]
                             {
                                 new ValidationResult("Your trip may end in Singapore.", new[] { "Aircraft.Code", "FlightNumber" })
                             }
                       : Enumerable.Empty<ValidationResult>();
        }

        public static ValidationResult FailOnRequest(object entity, ValidationContext validationContex)
        {
            return ShouldFail ? new ValidationResult("Validation failed.") : ValidationResult.Success;
        }
    }

    public class MostDerivedFlightSegmentWithNestedComplexTypes : FlightSegmentWithNestedComplexTypesWithTypeLevelValidation,
                                                                  IValidatableObject
    {
        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            return null;
        }
    }

    #endregion

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

    /// <summary>
    ///     Tests for validation.
    /// </summary>
    public class ValidationTests : TestBase
    {
        #region Infrastructure/setup

        static ValidationTests()
        {
            Database.SetInitializer((IDatabaseInitializer<SelfPopulatingContext>)null);
            Database.SetInitializer((IDatabaseInitializer<ConfigurationOverridesContext>)null);
        }

        #endregion

        #region Helper methods

        #region Mock helpers

        internal class InternalNestedPropertyEntryForMock : InternalNestedPropertyEntry
        {
            private static InternalEntityPropertyEntry CreateFakePropertyEntry()
            {
                var propertyEntry = new Mock<PropertyApiTests.InternalEntityPropertyEntryForMock>();
                propertyEntry.SetupGet(p => p.InternalEntityEntry).Returns(new PropertyApiTests.InternalEntityEntryForMock<object>());
                return propertyEntry.Object;
            }

            public InternalNestedPropertyEntryForMock()
                :
                    base(CreateFakePropertyEntry(),
                        new PropertyEntryMetadata(typeof(object), typeof(object), "fake Name, mock Name property", true, true))
            {
            }
        }

        private static Mock<PropertyApiTests.InternalEntityEntryForMock<object>> CreateMockInternalEntityEntry(
            Dictionary<string, object> values)
        {
            var mockInternalEntityEntry = new Mock<PropertyApiTests.InternalEntityEntryForMock<object>>();
            foreach (var propertyName in values.Keys)
            {
                var mockEntityProperty = new Mock<PropertyApiTests.InternalEntityPropertyEntryForMock>();
                mockEntityProperty.CallBase = true;
                mockEntityProperty.SetupGet(p => p.Name).Returns(propertyName);
                mockEntityProperty.SetupGet(p => p.CurrentValue).Returns(values[propertyName]);
                mockEntityProperty.SetupGet(p => p.InternalEntityEntry).Returns(mockInternalEntityEntry.Object);

                mockInternalEntityEntry.Setup(e => e.Property(propertyName, It.IsAny<Type>(), It.IsAny<bool>()))
                    .Returns(mockEntityProperty.Object);
                mockInternalEntityEntry.Setup(e => e.Member(propertyName, It.IsAny<Type>()))
                    .Returns(mockEntityProperty.Object);
            }

            mockInternalEntityEntry.Setup(e => e.Entity).Returns(new object());

            return mockInternalEntityEntry;
        }

        private static Mock<PropertyApiTests.InternalEntityEntryForMock<T>> CreateMockInternalEntityEntry<T>(T entity)
            where T : class, new()
        {
            var mockInternalEntityEntry = new Mock<PropertyApiTests.InternalEntityEntryForMock<T>>();
            mockInternalEntityEntry.SetupGet(e => e.Entity).Returns(entity);
            mockInternalEntityEntry.Setup(e => e.EntityType).Returns(entity.GetType());

            var mockChildProperties = GetMockPropertiesForEntityOrComplexType(mockInternalEntityEntry.Object, null, entity);
            mockInternalEntityEntry.Setup(e => e.Property(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<bool>()))
                .Returns((string propertyName, Type requestedType, bool requiresComplex) => mockChildProperties[propertyName]);
            mockInternalEntityEntry.Setup(e => e.Member(It.IsAny<string>(), It.IsAny<Type>()))
                .Returns((string propertyName, Type requestedType) => mockChildProperties[propertyName]);

            return mockInternalEntityEntry;
        }

        private static Dictionary<string, InternalPropertyEntry> GetMockPropertiesForEntityOrComplexType(
            InternalEntityEntry owner, InternalPropertyEntry parentPropertyEntry, object parent)
        {
            var mockChildProperties = new Dictionary<string, InternalPropertyEntry>();

            // do not create mocks for nulls
            if (parent != null)
            {
                foreach (var childPropInfo in parent.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (childPropInfo.GetGetMethod() != null
                        && childPropInfo.GetSetMethod() != null)
                    {
                        var mockInternalPropertyEntry = CreateMockInternalPropertyEntry(owner, parentPropertyEntry, childPropInfo, parent);
                        mockChildProperties.Add(childPropInfo.Name, mockInternalPropertyEntry);
                    }
                }
            }

            return mockChildProperties;
        }

        private static InternalPropertyEntry CreateMockInternalPropertyEntry(
            InternalEntityEntry owner, InternalPropertyEntry parentPropertyEntry, PropertyInfo propInfo, object parent)
        {
            var propertyValue = propInfo.GetGetMethod().Invoke(parent, new object[0]);

            InternalPropertyEntry childPropertyEntry;
            if (parentPropertyEntry == null)
            {
                var mockEntityProperty = new Mock<PropertyApiTests.InternalEntityPropertyEntryForMock>();
                mockEntityProperty.CallBase = true;
                mockEntityProperty.SetupGet(p => p.Name).Returns(propInfo.Name);
                mockEntityProperty.SetupGet(p => p.CurrentValue).Returns(propertyValue);
                mockEntityProperty.SetupGet(p => p.InternalEntityEntry).Returns(owner);

                var mockChildProperties = GetMockPropertiesForEntityOrComplexType(owner, mockEntityProperty.Object, propertyValue);

                mockEntityProperty.Setup(p => p.Property(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<bool>()))
                    .Returns(
                        (string propertyName, Type requestedType, bool requiresComplex) =>
                        mockChildProperties.ContainsKey(propertyName)
                            ? mockChildProperties[propertyName]
                            : CreateInternalPropertyEntryForNullParent(propertyName));

                childPropertyEntry = mockEntityProperty.Object;
            }
            else
            {
                var mockComplexProperty = new Mock<InternalNestedPropertyEntryForMock>();
                mockComplexProperty.CallBase = true;
                mockComplexProperty.SetupGet(p => p.Name).Returns(propInfo.Name);
                mockComplexProperty.SetupGet(p => p.CurrentValue).Returns(propertyValue);
                mockComplexProperty.SetupGet(p => p.InternalEntityEntry).Returns(owner);
                mockComplexProperty.SetupGet(p => p.ParentPropertyEntry).Returns(parentPropertyEntry);

                var mockChildProperties = GetMockPropertiesForEntityOrComplexType(owner, mockComplexProperty.Object, propertyValue);

                mockComplexProperty.Setup(p => p.Property(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<bool>()))
                    .Returns(
                        (string propertyName, Type requestedType, bool requiresComplex) =>
                        mockChildProperties.ContainsKey(propertyName)
                            ? mockChildProperties[propertyName]
                            : CreateInternalPropertyEntryForNullParent(propertyName));

                childPropertyEntry = mockComplexProperty.Object;
            }

            return childPropertyEntry;
        }

        private static ValidatableObjectValidator CreateValidatableObjectValidator(string propertyName, string errorMessage)
        {
            var validetableObjectValidator = new Mock<ValidatableObjectValidator>(null);
            validetableObjectValidator
                .Setup(
                    v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns<EntityValidationContext, InternalMemberEntry>(
                    (c, e) =>
                    new[] { new DbValidationError(propertyName, errorMessage) });
            return validetableObjectValidator.Object;
        }

        private static ValidationAttribute CreateValidationAttribute(string errorMessage)
        {
            var validationAttribute = new Mock<ValidationAttribute>();
            validationAttribute.CallBase = true;
            validationAttribute.Setup(a => a.IsValid(It.IsAny<object>()))
                .Returns<object>(o => false);
            validationAttribute.Setup(a => a.FormatErrorMessage(It.IsAny<string>()))
                .Returns<string>(n => errorMessage);
            return validationAttribute.Object;
        }

        private static Mock<ValidationProviderForMock> CreateMockValidationProvider(EntityValidatorBuilderForMock builder = null)
        {
            builder = builder ?? CreateMockEntityValidatorBuilder().Object;

            var validationProvider = new Mock<ValidationProviderForMock>(builder);

            return validationProvider;
        }

        internal class ValidationProviderForMock : ValidationProvider
        {
            public ValidationProviderForMock(EntityValidatorBuilder builder)
                : base(builder)
            {
            }

            public EntityValidator GetEntityValidatorBase(InternalEntityEntry entityEntry)
            {
                return base.GetEntityValidator(entityEntry);
            }

            public PropertyValidator GetPropertyValidatorBase(InternalEntityEntry owningEntity, InternalMemberEntry property)
            {
                return base.GetPropertyValidator(owningEntity, property);
            }

            public PropertyValidator GetValidatorForPropertyBase(EntityValidator entityValidator, InternalMemberEntry memberEntry)
            {
                return base.GetValidatorForProperty(entityValidator, memberEntry);
            }

            public EntityValidationContext GetEntityValidationContextBase(
                InternalEntityEntry entityEntry, IDictionary<object, object> items)
            {
                return base.GetEntityValidationContext(entityEntry, items);
            }
        }

        private static Mock<EntityValidatorBuilderForMock> CreateMockEntityValidatorBuilder(
            Mock<AttributeProvider> attributeProvider = null)
        {
            attributeProvider = attributeProvider ?? new Mock<AttributeProvider>();
            var builder = new Mock<EntityValidatorBuilderForMock>(attributeProvider.Object);
            builder.Protected()
                .Setup<IList<PropertyValidator>>(
                    "BuildValidatorsForProperties", ItExpr.IsAny<IEnumerable<PropertyInfo>>(),
                    ItExpr.IsAny<IEnumerable<EdmProperty>>(), ItExpr.IsAny<IEnumerable<NavigationProperty>>())
                .Returns<IEnumerable<PropertyInfo>, IEnumerable<EdmProperty>, IEnumerable<NavigationProperty>>(
                    (pi, e, n) => new List<PropertyValidator>());

            builder.Protected()
                .Setup<IList<IValidator>>("BuildValidationAttributeValidators", ItExpr.IsAny<IEnumerable<Attribute>>())
                .Returns<IEnumerable<Attribute>>(a => new List<IValidator>());

            builder.Protected()
                .Setup<IEnumerable<IValidator>>(
                    "BuildFacetValidators", ItExpr.IsAny<PropertyInfo>(), ItExpr.IsAny<EdmMember>(), ItExpr.IsAny<IEnumerable<Attribute>>())
                .Returns<PropertyInfo, EdmMember, IEnumerable<Attribute>>((pi, e, a) => Enumerable.Empty<IValidator>());

            return builder;
        }

        internal class EntityValidatorBuilderForMock : EntityValidatorBuilder
        {
            public EntityValidatorBuilderForMock(AttributeProvider attributeProvider)
                : base(attributeProvider)
            {
            }

            public EntityValidator BuildEntityValidatorBase(InternalEntityEntry entityEntry)
            {
                return base.BuildEntityValidator(entityEntry);
            }

            public IList<PropertyValidator> BuildValidatorsForPropertiesBase(
                IEnumerable<PropertyInfo> clrProperties, IEnumerable<EdmProperty> edmProperties,
                IEnumerable<NavigationProperty> navigationProperties)
            {
                return base.BuildValidatorsForProperties(clrProperties, edmProperties, navigationProperties);
            }

            public PropertyValidator BuildPropertyValidatorBase(
                PropertyInfo clrProperty, EdmProperty edmProperty, bool buildFacetValidators)
            {
                return base.BuildPropertyValidator(clrProperty, edmProperty, buildFacetValidators);
            }

            public PropertyValidator BuildPropertyValidatorBase(PropertyInfo clrProperty)
            {
                return base.BuildPropertyValidator(clrProperty);
            }

            public ComplexTypeValidator BuildComplexTypeValidatorBase(Type clrType, ComplexType complexType)
            {
                return base.BuildComplexTypeValidator(clrType, complexType);
            }

            public IList<IValidator> BuildValidationAttributeValidatorsBase(IEnumerable<Attribute> attributes)
            {
                return base.BuildValidationAttributeValidators(attributes);
            }

            public IEnumerable<PropertyInfo> GetPublicInstancePropertiesBase(Type type)
            {
                return base.GetPublicInstanceProperties(type);
            }

            public IEnumerable<IValidator> BuildFacetValidatorsBase(
                PropertyInfo clrProperty, EdmMember edmProperty, IEnumerable<Attribute> existingAttributes)
            {
                return base.BuildFacetValidators(clrProperty, edmProperty, existingAttributes);
            }
        }

        #endregion

        #region String resource helpers

        private readonly string DbEntityValidationException_ValidationFailed = LookupString
            (EntityFrameworkAssembly, "System.Data.Entity.Properties.Resources", "DbEntityValidationException_ValidationFailed");

        private readonly string RangeAttribute_ValidationError = LookupString
            (
                SystemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources",
                "RangeAttribute_ValidationError");

        private readonly string RegexAttribute_ValidationError = LookupString
            (
                SystemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources",
                "RegexAttribute_ValidationError");

        private readonly string RequiredAttribute_ValidationError = LookupString
            (
                SystemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources",
                "RequiredAttribute_ValidationError");

        private readonly string StringLengthAttribute_ValidationError = LookupString
            (
                SystemComponentModelDataAnnotationsAssembly, "System.ComponentModel.DataAnnotations.Resources.DataAnnotationsResources",
                "StringLengthAttribute_ValidationError");

        #endregion

        #region Validator helpers

        private static EntityValidationContext CreateEntityValidationContext(InternalEntityEntry entityEntry)
        {
            return new EntityValidationContext(entityEntry, new ValidationContext(entityEntry.Entity, null, null));
        }

        private static InternalPropertyEntry CreateInternalPropertyEntryForNullParent(string propertyName)
        {
            var parentNullPropertyEntry = new Mock<PropertyApiTests.InternalEntityPropertyEntryForMock>();
            parentNullPropertyEntry.SetupGet(p => p.Name).Returns(propertyName);
            parentNullPropertyEntry.SetupGet(p => p.ParentPropertyEntry);
            parentNullPropertyEntry.SetupGet(p => p.CurrentValue).Throws(new NullReferenceException());

            return parentNullPropertyEntry.Object;
        }

        private void VerifyResults(Tuple<string, string>[] expectedResults, IEnumerable<DbValidationError> actualResults)
        {
            Assert.Equal(expectedResults.Count(), actualResults.Count());

            foreach (var validationError in actualResults)
            {
                Assert.True(
                    expectedResults.SingleOrDefault(
                        r => r.Item1 == validationError.PropertyName && r.Item2 == validationError.ErrorMessage) != null,
                    string.Format(
                        "Unexpected error message '{0}' for property '{1}' not found", validationError.ErrorMessage,
                        validationError.PropertyName));
            }
        }

        public static ValidationResult FailMiserably(object entity, ValidationContext validationContex)
        {
            return new ValidationResult("The entity is not valid");
        }

        #endregion

        #endregion

        #region Validator tests

        #region ValidationAttributeValidator

        [Fact]
        public void ValidationAttributeValidator_does_not_return_errors_if_property_value_is_valid()
        {
            var mockInternalEntityEntry = CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "abc" }
                    });

            var ValidationAttributeValidator = new ValidationAttributeValidator(new StringLengthAttribute(10), null);

            var results = ValidationAttributeValidator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Name"));

            Assert.False(results.Any());
        }

        [Fact]
        public void ValidationAttributeValidator_returns_validation_errors_if_property_value_is_not_valid()
        {
            var mockInternalEntityEntry = CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "abcdefghijklmnopq" }
                    });

            var ValidationAttributeValidator = new ValidationAttributeValidator(new StringLengthAttribute(10), null);

            var results = ValidationAttributeValidator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Name"));

            Assert.Equal(1, results.Count());
            var error = results.Single();
            Assert.Equal("Name", error.PropertyName);
            Assert.Equal(string.Format(StringLengthAttribute_ValidationError, "Name", 10), error.ErrorMessage);
        }

        [Fact]
        public void ValidationAttributeValidator_returns_errors_for_invalid_complex_property_child_property_values()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
                             {
                                 Departure = new DepartureArrivalInfoWithNestedComplexType
                                                 {
                                                     Airport = new AirportDetails
                                                                   {
                                                                       AirportCode = "???",
                                                                   }
                                                 }
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);

            var validator = new ValidationAttributeValidator(new RegularExpressionAttribute("^[A-Z]{3}$"), null);

            var results = validator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure").Property("Airport").Property("AirportCode"));

            VerifyResults(
                new[]
                    {
                        new Tuple<string, string>(
                            "Departure.Airport.AirportCode",
                            string.Format(RegexAttribute_ValidationError, "Departure.Airport.AirportCode", "^[A-Z]{3}$"))
                    }, results);
        }

        [Fact]
        public void ValidationAttributeValidator_returns_errors_if_complex_property_type_level_validation_fails()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
                             {
                                 Departure = new DepartureArrivalInfoWithNestedComplexType
                                                 {
                                                     Airport = new AirportDetails
                                                                   {
                                                                       AirportCode = "YVR",
                                                                       CityCode = "YVR",
                                                                       CountryCode = "ZZ"
                                                                   }
                                                 }
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);

            var validator = new ValidationAttributeValidator(new CustomValidationAttribute(typeof(AirportDetails), "ValidateCountry"), null);

            var results = validator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure").Property("Airport"));

            VerifyResults(
                new[]
                    {
                        new Tuple<string, string>("Departure.Airport", "City 'YVR' is not located in country 'ZZ'.")
                    }, results);
        }

        [Fact]
        public void ValidationAttributeValidator_returns_validation_errors_if_entity_validation_with_type_level_annotation_attributes_fails(
            )
        {
            var mockInternalEntityEntry = CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "abcdefghijklmnopq" }
                    });
            mockInternalEntityEntry.Setup(e => e.Entity).Returns(new object());

            var ValidationAttributeValidator = new ValidationAttributeValidator(
                new CustomValidationAttribute(GetType(), "FailMiserably"), null);

            var results = ValidationAttributeValidator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object), null);

            Assert.Equal(1, results.Count());
            var error = results.Single();
            Assert.Equal(null, error.PropertyName);
            Assert.Equal("The entity is not valid", error.ErrorMessage);
        }

        [Fact]
        public void ValidationAttributeValidator_wraps_exceptions()
        {
            var mockInternalEntityEntry = CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "ID", 1 }
                    });

            var ValidationAttributeValidator = new ValidationAttributeValidator(new StringLengthAttribute(10), null);

            Assert.Equal(
                new DbUnexpectedValidationException(
                    Strings.DbUnexpectedValidationException_ValidationAttribute(
                        "ID", "System.ComponentModel.DataAnnotations.StringLengthAttribute")).Message,
                Assert.Throws<DbUnexpectedValidationException>(
                    () => ValidationAttributeValidator.Validate(
                        CreateEntityValidationContext(mockInternalEntityEntry.Object),
                        mockInternalEntityEntry.Object.Property("ID"))).Message);
        }

        #endregion

        #region ValidatableObjectValidator

        [Fact]
        public void ValidatableObjectValidator_returns_errors_if_entity_IValidatableObject_validation_fails()
        {
            var entity = new FlightSegmentWithNestedComplexTypesWithTypeLevelValidation
                             {
                                 Aircraft = new AircraftInfo
                                                {
                                                    Code = "A380"
                                                },
                                 FlightNumber = "QF0006"
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);

            var validator = new ValidatableObjectValidator(null);

            var results = validator.Validate(CreateEntityValidationContext(mockInternalEntityEntry.Object), null);

            VerifyResults(
                new[]
                    {
                        new Tuple<string, string>("Aircraft.Code", "Your trip may end in Singapore."),
                        new Tuple<string, string>("FlightNumber", "Your trip may end in Singapore.")
                    }, results);
        }

        [Fact]
        public void ValidatableObjectValidator_returns_errors_if_complex_property_IValidatableObject_validation_fails()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
                             {
                                 Departure = new DepartureArrivalInfoWithNestedComplexType
                                                 {
                                                     Time = DateTime.MinValue
                                                 }
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);

            var validator = new ValidatableObjectValidator(null);

            var results = validator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure"));

            VerifyResults(new[] { new Tuple<string, string>("Departure", "Date cannot be in the past.") }, results);
        }

        [Fact]
        // Regression test for Dev 11 #165071
        public void ValidatableObjectValidator_returns_empty_enumerator_if_complex_property_IValidatableObject_validation_returns_null()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
                             {
                                 Departure = new DepartureArrivalInfoWithNestedComplexType
                                                 {
                                                     ValidationResults = new[] { ValidationResult.Success, null }
                                                 }
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);

            var validator = new ValidatableObjectValidator(null);

            var results = validator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure"));

            VerifyResults(new Tuple<string, string>[0], results);
        }

        [Fact]
        public void ValidatableObjectValidator_does_not_return_errors_for_null_complex_property_with_IValidatableObject_validation()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
                             {
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);

            var validator = new ValidatableObjectValidator(null);

            var results = validator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure"));

            Assert.False(results.Any());
        }

        [Fact]
        public void ValidatableObjectValidator_wraps_exceptions()
        {
            var entity = new DepartureArrivalInfoWithNestedComplexType
                             {
                                 Airport = new AirportDetails(),
                             };
            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);

            var validator = new ValidatableObjectValidator(
                new DisplayAttribute
                    {
                        Name = "Airport information"
                    });

            Assert.Equal(
                new DbUnexpectedValidationException(
                    Strings.DbUnexpectedValidationException_IValidatableObject(
                        "Airport information", "ProductivityApiUnitTests.Validation.AirportDetails")).Message,
                Assert.Throws<DbUnexpectedValidationException>(
                    () => validator.Validate(
                        CreateEntityValidationContext(mockInternalEntityEntry.Object),
                        mockInternalEntityEntry.Object.Property("Airport"))).Message);
        }

        #endregion

        #region PropertyValidator

        [Fact]
        public void PropertyValidator_does_not_return_errors_if_primitive_property_value_is_valid()
        {
            var mockValidator = new Mock<IValidator>();
            mockValidator
                .Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns(() => Enumerable.Empty<DbValidationError>());
            var propertyValidator = new PropertyValidator(
                "Name",
                new[] { mockValidator.Object });

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "abc" }
                    });

            var results = propertyValidator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Member("Name"));

            Assert.False(results.Any());
        }

        [Fact]
        public void PropertyValidator_returns_validation_errors_if_primitive_property_value_is_not_valid()
        {
            var mockValidator = new Mock<IValidator>();
            mockValidator
                .Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns(() => new[] { new DbValidationError("Name", "error") });

            var propertyValidator = new PropertyValidator(
                "Name",
                new[] { mockValidator.Object });

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "" }
                    });

            var results = propertyValidator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Member("Name"));

            VerifyResults(
                new[] { new Tuple<string, string>("Name", "error") },
                results);
        }

        #endregion

        #region ComplexPropertyValidator

        [Fact]
        public void ComplexPropertyValidator_does_not_return_errors_if_complex_property_value_is_valid()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
                             {
                                 Departure = new DepartureArrivalInfoWithNestedComplexType
                                                 {
                                                     Airport = new AirportDetails
                                                                   {
                                                                   },
                                                 }
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);
            var propertyValidator = new ComplexPropertyValidator(
                "Departure", new ValidationAttributeValidator[0],
                new ComplexTypeValidator(new PropertyValidator[0], new ValidationAttributeValidator[0]));

            var results = propertyValidator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure"));

            Assert.False(results.Any());
        }

        [Fact]
        public void ComplexPropertyValidator_does_not_return_errors_if_complex_type_validator_is_null()
        {
            var entity = new FlightSegmentWithNestedComplexTypesWithTypeLevelValidation
                             {
                                 Departure = new DepartureArrivalInfoWithNestedComplexType
                                                 {
                                                     Airport = new AirportDetails
                                                                   {
                                                                   },
                                                 },
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);
            var propertyValidator = new ComplexPropertyValidator(
                "Departure", new ValidationAttributeValidator[0],
                null);

            var results = propertyValidator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure"));

            Assert.False(results.Any());
        }

        [Fact]
        public void ComplexPropertyValidator_does_not_run_complex_type_validation_if_property_validation_failed()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
                             {
                                 Departure = new DepartureArrivalInfoWithNestedComplexType
                                                 {
                                                     Airport = new AirportDetails
                                                                   {
                                                                       AirportCode = null,
                                                                   },
                                                 }
                             };

            var mockValidator = new Mock<IValidator>();
            mockValidator
                .Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns(() => new[] { new DbValidationError("Airport", "error") });

            var mockComplexValidator = new Mock<IValidator>(MockBehavior.Strict);

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);
            var propertyValidator = new ComplexPropertyValidator(
                "Airport",
                new[]
                    {
                        mockValidator.Object
                    },
                new ComplexTypeValidator(
                    new[]
                        {
                            new PropertyValidator(
                                "AirportCode",
                                new[] { mockComplexValidator.Object })
                        }, new ValidationAttributeValidator[0]));

            var results = propertyValidator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure").Property("Airport"));

            Assert.Equal(1, results.Count());

            VerifyResults(
                new[]
                    {
                        new Tuple<string, string>("Airport", "error")
                    }, results);
        }

        [Fact]
        public void ComplexPropertyValidator_does_not_run_complex_type_validation_if_property_is_null()
        {
            var entity = new EntityWithOptionalNestedComplexType
                             {
                                 ID = 1
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);
            var propertyValidator = new ComplexPropertyValidator(
                "AirportDetails", new ValidationAttributeValidator[0],
                new ComplexTypeValidator(
                    new[]
                        {
                            new PropertyValidator(
                                "AirportCode",
                                new[] { new ValidationAttributeValidator(new RequiredAttribute(), null) })
                        }, new ValidationAttributeValidator[0]));

            var results = propertyValidator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("AirportDetails"));

            Assert.False(results.Any());
        }

        #endregion

        #region EntityValidator

        [Fact]
        public void EntityValidator_does_not_return_error_if_entity_is_valid()
        {
            var mockValidator = new Mock<IValidator>();
            mockValidator
                .Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns(() => Enumerable.Empty<DbValidationError>());

            var entityValidator = new EntityValidator(
                new[]
                    {
                        new PropertyValidator(
                            "Name",
                            new[] { mockValidator.Object })
                    }, new ValidationAttributeValidator[0]);

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "Name", "abc" }
                    });

            var validationResult = entityValidator.Validate(CreateEntityValidationContext(mockInternalEntityEntry.Object));
            Assert.NotNull(validationResult);
            Assert.True(validationResult.IsValid);
        }

        [Fact]
        public void EntityValidator_does_not_run_other_validation_if_property_validation_failed()
        {
            var mockValidator = new Mock<IValidator>();
            mockValidator
                .Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalMemberEntry>()))
                .Returns(() => new[] { new DbValidationError("ID", "error") });

            var mockUncalledValidator = new Mock<IValidator>(MockBehavior.Strict);

            var entityValidator = new EntityValidator(
                new[]
                    {
                        new PropertyValidator(
                            "ID",
                            new[] { mockValidator.Object })
                    },
                new[] { mockUncalledValidator.Object });

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(
                new Dictionary<string, object>
                    {
                        { "ID", -1 }
                    });

            var entityValidationResult = entityValidator.Validate(CreateEntityValidationContext(mockInternalEntityEntry.Object));

            Assert.NotNull(entityValidationResult);

            VerifyResults(
                new[] { new Tuple<string, string>("ID", "error") }, entityValidationResult.ValidationErrors);
        }

        [Fact]
        public void EntityValidator_returns_an_error_if_IValidatableObject_validation_failed()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
                             {
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);

            var entityValidator = new EntityValidator(
                new PropertyValidator[0],
                new[] { CreateValidatableObjectValidator("object", "IValidatableObject is invalid") });

            var entityValidationResult = entityValidator.Validate(CreateEntityValidationContext(mockInternalEntityEntry.Object));

            Assert.NotNull(entityValidationResult);

            VerifyResults(
                new[] { new Tuple<string, string>("object", "IValidatableObject is invalid") }, entityValidationResult.ValidationErrors);
        }

        #endregion

        #region ComplexTypeValidator

        [Fact]
        public void ComplexTypeValidator_returns_an_error_if_IValidatableObject_validation_failed()
        {
            var entity = new FlightSegmentWithNestedComplexTypes
                             {
                                 Departure = new DepartureArrivalInfoWithNestedComplexType
                                                 {
                                                     Airport = new AirportDetails
                                                                   {
                                                                       AirportCode = "???",
                                                                   }
                                                 }
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);

            var entityValidator = new ComplexTypeValidator(
                new PropertyValidator[0],
                new[] { CreateValidatableObjectValidator("object", "IValidatableObject is invalid") });

            var entityValidationResult = entityValidator.Validate(
                CreateEntityValidationContext(mockInternalEntityEntry.Object),
                mockInternalEntityEntry.Object.Property("Departure").Property("Airport").Property("AirportCode"));

            Assert.NotNull(entityValidationResult);

            VerifyResults(
                new[] { new Tuple<string, string>("object", "IValidatableObject is invalid") }, entityValidationResult);
        }

        #endregion

        #endregion

        #region EntityValidatorBuilder tests

        [Fact]
        public void BuildEntityValidator_returns_null_for_an_entity_with_no_validation()
        {
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                Assert.Null(builder.Object.BuildEntityValidatorBase(ctx.Entry(entity).InternalEntry));
            }
        }

        [Fact]
        public void BuildEntityValidator_does_not_return_null_for_an_IValidatableObject()
        {
            var builder = CreateMockEntityValidatorBuilder();

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
            var builder = CreateMockEntityValidatorBuilder();
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
            var builder = CreateMockEntityValidatorBuilder();

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
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validators = builder.Object.BuildValidatorsForPropertiesBase(
                    entity.GetType().GetProperties(),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties,
                    ctx.Entry(entity).InternalEntry.EdmEntityType.NavigationProperties);

                Assert.Equal(0, validators.Count);
            }
        }

        [Fact]
        public void BuildComplexTypeValidator_returns_null_for_a_complex_type_with_no_validation()
        {
            var builder = CreateMockEntityValidatorBuilder();

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
            var builder = CreateMockEntityValidatorBuilder();

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
            var builder = CreateMockEntityValidatorBuilder();
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
            var builder = CreateMockEntityValidatorBuilder();

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
            var builder = CreateMockEntityValidatorBuilder();
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
                .Throws<AssertException>();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validators = builder.Object.BuildValidatorsForPropertiesBase(
                    new[] { entity.GetType().GetProperty("ID") },
                    new EdmProperty[0],
                    new NavigationProperty[0]);

                Assert.Equal(1, validators.Count);
            }
        }

        [Fact]
        public void BuildValidatorsForProperties_calls_BuildPropertyValidator_for_a_scalar_property()
        {
            var builder = CreateMockEntityValidatorBuilder();
            builder.Protected()
                .Setup<PropertyValidator>("BuildPropertyValidator", ItExpr.IsAny<PropertyInfo>())
                .Throws<AssertException>();

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
                    new[] { entity.GetType().GetProperty("ID") },
                    new[] { ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ID").Single() },
                    new NavigationProperty[0]);

                Assert.Equal(1, validators.Count);
            }
        }

        [Fact]
        public void BuildPropertyValidator_returns_null_for_a_scalar_property_with_no_validation()
        {
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                Assert.Null(
                    builder.Object.BuildPropertyValidatorBase(
                        entity.GetType().GetProperty("ID"),
                        ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ID").Single(),
                        true));
            }
        }

        [Fact]
        public void BuildPropertyValidator_does_not_return_null_for_a_scalar_property_with_facet_validation()
        {
            var builder = CreateMockEntityValidatorBuilder();
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
                    entity.GetType().GetProperty("ID"),
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
            var builder = CreateMockEntityValidatorBuilder();
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
                    entity.GetType().GetProperty("ID"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ID").Single(),
                    false);

                Assert.Null(validator);
            }
        }

        [Fact]
        public void BuildPropertyValidator_does_not_return_null_for_a_scalar_property_with_attribute_validation()
        {
            var builder = CreateMockEntityValidatorBuilder();

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
                    entity.GetType().GetProperty("ID"),
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
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                Assert.Null(
                    builder.Object.BuildPropertyValidatorBase(
                        entity.GetType().GetProperty("ComplexProperty"),
                        ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ComplexProperty").Single(),
                        true));
            }
        }

        [Fact]
        public void BuildPropertyValidator_returns_null_for_a_complex_property_with_facet_validation()
        {
            var builder = CreateMockEntityValidatorBuilder();
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
                        entity.GetType().GetProperty("ComplexProperty"),
                        ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ComplexProperty").Single(),
                        true));
            }
        }

        [Fact]
        public void BuildPropertyValidator_does_not_return_null_for_a_complex_property_with_attribute_validation()
        {
            var builder = CreateMockEntityValidatorBuilder();

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
                    entity.GetType().GetProperty("ComplexProperty"),
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
            var builder = CreateMockEntityValidatorBuilder();

            builder.Protected()
                .Setup<ComplexTypeValidator>("BuildComplexTypeValidator", ItExpr.IsAny<Type>(), ItExpr.IsAny<ComplexType>())
                .Returns<Type, ComplexType>(
                    (t, c) => new ComplexTypeValidator(new PropertyValidator[0], new ValidationAttributeValidator[0]));

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validator = builder.Object.BuildPropertyValidatorBase(
                    entity.GetType().GetProperty("ComplexProperty"),
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
            var builder = CreateMockEntityValidatorBuilder();
            builder.Protected()
                .Setup<IEnumerable<IValidator>>(
                    "BuildFacetValidators", ItExpr.IsAny<PropertyInfo>(), ItExpr.IsAny<EdmMember>(), ItExpr.IsAny<IEnumerable<Attribute>>())
                .Throws<AssertException>();

            Assert.Null(builder.Object.BuildPropertyValidatorBase(typeof(EntityWithComplexType).GetProperty("ID")));
        }

        [Fact]
        public void GetInstanceProperties_returns_all_instance_properties()
        {
            var builder = CreateMockEntityValidatorBuilder();

            var properties = builder.Object.GetPublicInstancePropertiesBase(typeof(EntityWithComplexType));

            Assert.True(
                properties.Select(pi => pi.Name).OrderBy(s => s).SequenceEqual(
                    new[] { "ComplexProperty", "GetterProperty", "ID", "NonNullableProperty", "Self" }));
        }

        [Fact]
        public void BuildPropertyValidator_does_not_return_null_for_a_transient_property_with_attribute_validation()
        {
            var builder = CreateMockEntityValidatorBuilder();

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
                .Throws<AssertException>();

            var validator = builder.Object.BuildPropertyValidatorBase(typeof(EntityWithComplexType).GetProperty("ID"));

            Assert.NotNull(validator);
            Assert.IsNotType<ComplexPropertyValidator>(validator);
            Assert.Equal("ID", validator.PropertyName);
            Assert.Equal(1, validator.PropertyAttributeValidators.Count());
        }

        [Fact]
        public void BuildValidationAttributeValidators_returns_validators_for_ValidationAttributes()
        {
            var builder = CreateMockEntityValidatorBuilder();

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
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new SelfPopulatingContext(entity))
            {
                var validators = builder.Object.BuildFacetValidatorsBase(
                    entity.GetType().GetProperty("Self"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.NavigationProperties.Where(p => p.Name == "Self").Single(),
                    new Attribute[0]);

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_a_validator_for_required_property()
        {
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var validators = builder.Object.BuildFacetValidatorsBase(
                    entity.GetType().GetProperty("Self"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.NavigationProperties.Where(p => p.Name == "Self").Single(),
                    new Attribute[0]);

                Assert.Equal(1, validators.Count());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_required_value_type_property()
        {
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var validators = builder.Object.BuildFacetValidatorsBase(
                    entity.GetType().GetProperty("NonNullableProperty"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "NonNullableProperty").Single(),
                    new Attribute[0]);

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_required_storeGenerated_property()
        {
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var validators = builder.Object.BuildFacetValidatorsBase(
                    entity.GetType().GetProperty("ID"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.Properties.Where(p => p.Name == "ID").Single(),
                    new Attribute[0]);

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_required_property_with_ValidationAttribute()
        {
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var validators = builder.Object.BuildFacetValidatorsBase(
                    entity.GetType().GetProperty("Self"),
                    ctx.Entry(entity).InternalEntry.EdmEntityType.NavigationProperties.Where(p => p.Name == "Self").Single(),
                    new[] { new RequiredAttribute() });

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_a_validator_for_a_property_with_MaxLength_facet()
        {
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                var validators = builder.Object.BuildFacetValidatorsBase(
                    typeof(ComplexTypeWithNoValidation).GetProperty("AnotherStringProperty"),
                    complexType.Properties.Where(p => p.Name == "AnotherStringProperty").Single(),
                    new Attribute[0]);

                Assert.Equal(1, validators.Count());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_a_property_with_MaxLength_facet_and_MaxLengthAttribute()
        {
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                var validators = builder.Object.BuildFacetValidatorsBase(
                    typeof(ComplexTypeWithNoValidation).GetProperty("AnotherStringProperty"),
                    complexType.Properties.Where(p => p.Name == "AnotherStringProperty").Single(),
                    new[] { new MaxLengthAttribute() });

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_a_property_with_MaxLength_facet_and_StringLengthAttribute()
        {
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                var validators = builder.Object.BuildFacetValidatorsBase(
                    typeof(ComplexTypeWithNoValidation).GetProperty("AnotherStringProperty"),
                    complexType.Properties.Where(p => p.Name == "AnotherStringProperty").Single(),
                    new[] { new StringLengthAttribute(1) });

                Assert.False(validators.Any());
            }
        }

        [Fact]
        public void BuildFacetValidators_returns_empty_for_a_property_with_IsMaxLength_facet()
        {
            var builder = CreateMockEntityValidatorBuilder();

            object entity = new EntityWithComplexType();
            using (var ctx = new ConfigurationOverridesContext(entity))
            {
                var complexType = (ComplexType)ctx.Entry(entity).InternalEntry.EdmEntityType.Properties
                                                   .Where(p => p.Name == "ComplexProperty").Single().TypeUsage.EdmType;
                var validators = builder.Object.BuildFacetValidatorsBase(
                    typeof(ComplexTypeWithNoValidation).GetProperty("StringProperty"),
                    complexType.Properties.Where(p => p.Name == "StringProperty").Single(),
                    new Attribute[0]);

                Assert.False(validators.Any());
            }
        }

        #endregion

        #region DbEntityValidationResult tests

        [Fact]
        public void DbEntityValidationResult_IsValid_true_if_no_validation_errors_occurred()
        {
            Assert.True(
                new DbEntityValidationResult(
                    new DbEntityEntry(new PropertyApiTests.InternalEntityEntryForMock<object>()),
                    new List<DbValidationError>())
                    .IsValid);
        }

        [Fact]
        public void DbEntityValidationResult_IsValid_false_if_there_were_validation_errors()
        {
            Assert.False(
                new DbEntityValidationResult(
                    new DbEntityEntry(new PropertyApiTests.InternalEntityEntryForMock<object>()),
                    new List<DbValidationError>(
                        new[] { new DbValidationError("property", "errormessage") }))
                    .IsValid);
        }

        #endregion

        #region DbEntityValidationException tests

        [Fact]
        public void DbEntityValidationException_parameterless_constructor()
        {
            var exception = new DbEntityValidationException();

            Assert.Equal(string.Format(DbEntityValidationException_ValidationFailed, "EntityValidationErrors"), exception.Message);
            Assert.False(exception.EntityValidationErrors.Any());
        }

        [Fact]
        public void DbEntityValidationException_exceptionmessage_validationresult_constructor()
        {
            var validationExceptionCtorParamTypes = new[]
                                                        {
                                                            new[] { typeof(string) },
                                                            new[] { typeof(string), typeof(IEnumerable<DbEntityValidationResult>) },
                                                            new[] { typeof(string), typeof(Exception) },
                                                            new[]
                                                                {
                                                                    typeof(string), typeof(IEnumerable<DbEntityValidationResult>),
                                                                    typeof(Exception)
                                                                },
                                                        };

            foreach (var ctorParamTypes in validationExceptionCtorParamTypes)
            {
                TestDbValidationExceptionCtor(ctorParamTypes);
            }
        }

        private void TestDbValidationExceptionCtor(Type[] types)
        {
            Debug.Assert(types.Length > 0 && types.Length <= 3);

            var ctor = typeof(DbEntityValidationException).GetConstructor(types);
            Debug.Assert(ctor != null);

            var maxCtorParams = new object[]
                                    {
                                        "error",
                                        new[]
                                            {
                                                new DbEntityValidationResult(
                                                    new Mock<PropertyApiTests.InternalEntityEntryForMock<object>>().Object, new[]
                                                                                                                                {
                                                                                                                                    new DbValidationError
                                                                                                                                        (
                                                                                                                                        "propA",
                                                                                                                                        "propA is Invalid")
                                                                                                                                    ,
                                                                                                                                    new DbValidationError
                                                                                                                                        (
                                                                                                                                        "propB",
                                                                                                                                        "propB is Invalid")
                                                                                                                                    ,
                                                                                                                                }),
                                                new DbEntityValidationResult(
                                                    new Mock<PropertyApiTests.InternalEntityEntryForMock<object>>().Object, new[]
                                                                                                                                {
                                                                                                                                    new DbValidationError
                                                                                                                                        (
                                                                                                                                        null,
                                                                                                                                        "The entity is invalid")
                                                                                                                                })
                                            },
                                        new Exception("dummy exception")
                                    };

            var ctorParams = maxCtorParams.Take(types.Length).ToArray();
            // 1st param is always a string, 3rd param is always an Exception, 
            // 2nd param can be either exception or IEnumerable<DbValidationResult> 
            // so it may need to be fixed up.
            if (types.Length == 2
                && types[1] == typeof(Exception))
            {
                ctorParams[1] = maxCtorParams[2];
            }

            var validationException = (DbEntityValidationException)ctor.Invoke(ctorParams);

            foreach (var param in ctorParams)
            {
                if (param is string)
                {
                    Assert.Equal(param, validationException.Message);
                }
                else if (param is Exception)
                {
                    Assert.Equal(param, validationException.InnerException);
                }
                else
                {
                    Debug.Assert(param is DbEntityValidationResult[]);
                    var expected = (DbEntityValidationResult[])maxCtorParams[1];
                    var actual = (DbEntityValidationResult[])param;

                    Assert.Equal(expected.Count(), actual.Count());
                    foreach (var expectedValidationResult in expected)
                    {
                        var actualValidationResult = actual.Single(r => r == expectedValidationResult);
                        Assert.Equal(expectedValidationResult.ValidationErrors.Count, actualValidationResult.ValidationErrors.Count);
                        Assert.Equal(expectedValidationResult.Entry.Entity, actualValidationResult.Entry.Entity);

                        foreach (var expectedValidationError in expectedValidationResult.ValidationErrors)
                        {
                            var actualValidationError = actualValidationResult.ValidationErrors.Single(e => e == expectedValidationError);

                            Assert.Equal(expectedValidationError.PropertyName, actualValidationError.PropertyName);
                            Assert.Equal(expectedValidationError.ErrorMessage, actualValidationError.ErrorMessage);
                        }
                    }
                }
            }
        }

        [Fact]
        public void DbEntityValidationException_serialization_and_deserialization()
        {
            var validationException = new DbEntityValidationException(
                "error",
                new[]
                    {
                        new DbEntityValidationResult(
                            new Mock<PropertyApiTests.InternalEntityEntryForMock<object>>().Object, new[]
                                                                                                        {
                                                                                                            new DbValidationError(
                                                                                                                "propA", "propA is Invalid")
                                                                                                            ,
                                                                                                            new DbValidationError(
                                                                                                                "propB", "propB is Invalid")
                                                                                                            ,
                                                                                                        }),
                        new DbEntityValidationResult(
                            new Mock<PropertyApiTests.InternalEntityEntryForMock<object>>().Object, new[]
                                                                                                        {
                                                                                                            new DbValidationError(
                                                                                                                null,
                                                                                                                "The entity is invalid")
                                                                                                        })
                    },
                new Exception("dummy exception")
                );

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, validationException);
                ms.Position = 0;

                var deserializedException = (DbEntityValidationException)formatter.Deserialize(ms);

                Assert.Equal("error", deserializedException.Message);
                Assert.Equal("dummy exception", deserializedException.InnerException.Message);

                var expected = validationException.EntityValidationErrors.ToArray();
                var actual = deserializedException.EntityValidationErrors.ToArray();

                Assert.Equal(expected.Count(), actual.Count());

                for (var idx = 0; idx < expected.Length; idx++)
                {
                    var expectedValidationResult = expected[idx];
                    var actualValidationResult = actual[idx];

                    // entities are not serialized
                    Assert.Null(actualValidationResult.Entry);
                    Assert.Equal(expectedValidationResult.ValidationErrors.Count, actualValidationResult.ValidationErrors.Count);
                    Assert.False(
                        expectedValidationResult.ValidationErrors.Zip(
                            actualValidationResult.ValidationErrors,
                            (actualValidationError, expectedValidationError) =>
                            actualValidationError.ErrorMessage == expectedValidationError.ErrorMessage &&
                            actualValidationError.PropertyName == expectedValidationError.PropertyName).Any(r => !r));
                }
            }
        }

        #endregion

        #region DbUnexpectedValidationException tests

        [Fact]
        public void DbUnexpectedValidationException_parameterless_constructor()
        {
            var exception = new DbUnexpectedValidationException();

            Assert.Equal("Data Exception.", exception.Message);
        }

        [Fact]
        public void DbUnexpectedValidationException_string_constructor()
        {
            var exception = new DbUnexpectedValidationException("Exception");

            Assert.Equal("Exception", exception.Message);
        }

        [Fact]
        public void DbUnexpectedValidationException_string_exception_constructor()
        {
            var innerException = new Exception();
            var exception = new DbUnexpectedValidationException("Exception", innerException);

            Assert.Equal("Exception", exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        #endregion

        #region ValidationContext tests

        [Fact]
        public void Verify_custom_validation_items_dictionary_gets_to_validator()
        {
            var mockIValidator = new Mock<IValidator>();
            mockIValidator.Setup(v => v.Validate(It.IsAny<EntityValidationContext>(), It.IsAny<InternalPropertyEntry>()))
                .Returns(
                    (EntityValidationContext ctx, InternalPropertyEntry property) =>
                        {
                            var validationContext = ctx.ExternalValidationContext;
                            Assert.NotNull(validationContext);
                            Assert.NotNull(validationContext.Items);
                            Assert.Equal(1, validationContext.Items.Count);
                            Assert.Equal(1, validationContext.Items["test"]);

                            return Enumerable.Empty<DbValidationError>();
                        });

            var mockValidationProvider = CreateMockValidationProvider();
            mockValidationProvider.Setup(
                p => p.GetEntityValidator(It.IsAny<InternalEntityEntry>()))
                .Returns(new EntityValidator(Enumerable.Empty<PropertyValidator>(), new[] { mockIValidator.Object }));
            mockValidationProvider.CallBase = true;

            var mockInternalEntity = new Mock<PropertyApiTests.InternalEntityEntryForMock<object>>();
            var mockInternalContext = Mock.Get((InternalContextForMock)mockInternalEntity.Object.InternalContext);
            mockInternalContext.SetupGet(c => c.ValidationProvider).Returns(mockValidationProvider.Object);

            var items = new Dictionary<object, object>
                            {
                                { "test", 1 }
                            };

            // GetValidationResult on entity
            mockInternalEntity.Object.GetValidationResult(items);
        }

        [Fact]
        public void Verify_custom_validation_items_dictionary_is_not_null_by_default()
        {
            var mockInternalEntity = new Mock<PropertyApiTests.InternalEntityEntryForMock<object>>();
            mockInternalEntity.Setup(e => e.GetValidationResult(It.IsAny<Dictionary<object, object>>()))
                .Returns(new DbEntityValidationResult(mockInternalEntity.Object, Enumerable.Empty<DbValidationError>()));
            mockInternalEntity.CallBase = true;
            Mock.Get((InternalContextForMock)mockInternalEntity.Object.InternalContext).CallBase = true;
            Mock.Get(mockInternalEntity.Object.InternalContext.Owner).CallBase = true;

            new DbEntityEntry(mockInternalEntity.Object).GetValidationResult();

            mockInternalEntity.Verify(e => e.GetValidationResult(null), Times.Never());
            mockInternalEntity.Verify(e => e.GetValidationResult(It.IsAny<Dictionary<object, object>>()), Times.Once());
        }

        #endregion

        #region ValidationProvider tests

        [Fact]
        public void GetEntityValidator_returns_cached_validator()
        {
            var mockInternalEntity = CreateMockInternalEntityEntry(new object());
            var mockBuilder = CreateMockEntityValidatorBuilder();
            var provider = CreateMockValidationProvider(mockBuilder.Object);

            var expectedValidator = new EntityValidator(new PropertyValidator[0], new IValidator[0]);
            mockBuilder.Setup(b => b.BuildEntityValidator(It.IsAny<InternalEntityEntry>()))
                .Returns<InternalEntityEntry>(e => expectedValidator);

            var entityValidator = provider.Object.GetEntityValidatorBase(mockInternalEntity.Object);
            Assert.Same(expectedValidator, entityValidator);
            mockBuilder.Verify(b => b.BuildEntityValidator(It.IsAny<InternalEntityEntry>()), Times.Once());

            // Now it should get the cached one
            entityValidator = provider.Object.GetEntityValidatorBase(mockInternalEntity.Object);
            Assert.Same(expectedValidator, entityValidator);
            mockBuilder.Verify(b => b.BuildEntityValidator(It.IsAny<InternalEntityEntry>()), Times.Once());
        }

        [Fact]
        public void GetPropertyValidator_returns_correct_validator()
        {
            var entity = new FlightSegmentWithNestedComplexTypes();
            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);
            var propertyEntry = mockInternalEntityEntry.Object.Property("Departure");

            var propertyValidator = new PropertyValidator("Departure", new IValidator[0]);
            var entityValidator = new EntityValidator(new[] { propertyValidator }, new IValidator[0]);

            var mockValidationProvider = CreateMockValidationProvider();
            mockValidationProvider.Setup(p => p.GetEntityValidator(It.IsAny<InternalEntityEntry>()))
                .Returns<InternalEntityEntry>(e => entityValidator);
            mockValidationProvider.Protected()
                .Setup<PropertyValidator>("GetValidatorForProperty", ItExpr.IsAny<EntityValidator>(), ItExpr.IsAny<InternalMemberEntry>())
                .Returns<EntityValidator, InternalMemberEntry>((ev, e) => propertyValidator);

            var actualPropertyValidator = mockValidationProvider.Object.GetPropertyValidatorBase(
                mockInternalEntityEntry.Object, propertyEntry);

            Assert.Same(propertyValidator, actualPropertyValidator);
        }

        [Fact]
        public void GetPropertyValidator_returns_null_if_entity_validator_does_not_exist()
        {
            var entity = new FlightSegmentWithNestedComplexTypes();
            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);
            var propertyEntry = mockInternalEntityEntry.Object.Property("Departure");

            var propertyValidator = CreateMockValidationProvider().Object.GetPropertyValidatorBase(
                mockInternalEntityEntry.Object, propertyEntry);

            Assert.Null(propertyValidator);
        }

        [Fact]
        public void GetEntityValidationContext_returns_correct_context()
        {
            var mockInternalEntity = CreateMockInternalEntityEntry(new object());
            var items = new Dictionary<object, object>
                            {
                                { "test", 1 }
                            };

            var entityValidationContext = CreateMockValidationProvider().Object.GetEntityValidationContextBase(
                mockInternalEntity.Object, items);

            Assert.Equal(entityValidationContext.InternalEntity, mockInternalEntity.Object);

            var validationContext = entityValidationContext.ExternalValidationContext;
            Assert.NotNull(validationContext);
            Assert.Same(mockInternalEntity.Object.Entity, validationContext.ObjectInstance);
            Assert.Equal(1, validationContext.Items.Count);
            Assert.Equal(1, validationContext.Items["test"]);
        }

        [Fact]
        public void GetValidatorForProperty_returns_correct_validator_for_child_complex_property()
        {
            var entity = new DepartureArrivalInfoWithNestedComplexType
                             {
                                 Airport = new AirportDetails(),
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);
            var childPropertyEntry = mockInternalEntityEntry.Object.Property("Airport").Property("AirportCode");

            var childPropertyValidator = new PropertyValidator("AirportCode", new IValidator[0]);
            var complexPropertyValidator = new ComplexPropertyValidator(
                "Airport", new IValidator[0],
                new ComplexTypeValidator(new[] { childPropertyValidator }, new IValidator[0]));
            var entityValidator = new EntityValidator(new[] { complexPropertyValidator }, new IValidator[0]);

            var mockValidationProvider = CreateMockValidationProvider();

            mockValidationProvider.Protected()
                .Setup<PropertyValidator>("GetValidatorForProperty", ItExpr.IsAny<EntityValidator>(), ItExpr.IsAny<InternalMemberEntry>())
                .Returns<EntityValidator, InternalMemberEntry>((ev, e) => complexPropertyValidator);

            var actualPropertyValidator = mockValidationProvider.Object.GetValidatorForPropertyBase(entityValidator, childPropertyEntry);

            Assert.Same(childPropertyValidator, actualPropertyValidator);
        }

        [Fact]
        public void GetValidatorForProperty_returns_null_for_child_complex_property_if_no_child_property_validation()
        {
            var entity = new DepartureArrivalInfoWithNestedComplexType
                             {
                                 Airport = new AirportDetails(),
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);
            var childPropertyEntry = mockInternalEntityEntry.Object.Property("Airport").Property("AirportCode");

            var complexPropertyValidator = new ComplexPropertyValidator(
                "Airport", new IValidator[0],
                null);
            var entityValidator = new EntityValidator(new[] { complexPropertyValidator }, new IValidator[0]);

            var mockValidationProvider = CreateMockValidationProvider();

            mockValidationProvider.Protected()
                .Setup<PropertyValidator>("GetValidatorForProperty", ItExpr.IsAny<EntityValidator>(), ItExpr.IsAny<InternalMemberEntry>())
                .Returns<EntityValidator, InternalMemberEntry>((ev, e) => complexPropertyValidator);

            var actualPropertyValidator = mockValidationProvider.Object.GetValidatorForPropertyBase(entityValidator, childPropertyEntry);

            Assert.Null(actualPropertyValidator);
        }

        [Fact]
        public void GetValidatorForProperty_returns_null_for_child_complex_property_if_no_property_validation()
        {
            var entity = new DepartureArrivalInfoWithNestedComplexType
                             {
                                 Airport = new AirportDetails(),
                             };

            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);
            var childPropertyEntry = mockInternalEntityEntry.Object.Property("Airport").Property("AirportCode");

            var entityValidator = new EntityValidator(new PropertyValidator[0], new IValidator[0]);

            var actualPropertyValidator = CreateMockValidationProvider().Object.GetValidatorForPropertyBase(
                entityValidator, childPropertyEntry);

            Assert.Null(actualPropertyValidator);
        }

        [Fact]
        public void GetValidatorForProperty_returns_correct_validator_for_scalar_property()
        {
            var entity = new FlightSegmentWithNestedComplexTypes();
            var mockInternalEntityEntry = CreateMockInternalEntityEntry(entity);
            var propertyEntry = mockInternalEntityEntry.Object.Property("FlightNumber");

            var propertyValidator = new PropertyValidator("FlightNumber", new IValidator[0]);
            var entityValidator = new EntityValidator(new[] { propertyValidator }, new IValidator[0]);

            var actualPropertyValidator = CreateMockValidationProvider().Object.GetValidatorForPropertyBase(entityValidator, propertyEntry);

            Assert.Same(propertyValidator, actualPropertyValidator);
        }

        #endregion
    }
}
