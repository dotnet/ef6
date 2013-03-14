// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Linq.Expressions;
    using System.Threading;
    using Moq;
    using Xunit;

    public class DbEntityEntryTests
    {
        [Fact]
        public void NonGeneric_DbEntityEntry_delegates_to_InternalReferenceEntry()
        {
            var v = new DbEntityEntryVerifier();
            var name = "foo";
            v.VerifyGetter(e => e.CurrentValues, m => m.CurrentValues);
            v.VerifyGetter(e => e.OriginalValues, m => m.OriginalValues);
            v.VerifyMethod(e => e.Collection(name), m => m.Collection(name, null));
            v.VerifyMethod(e => e.ComplexProperty(name), m => m.Property(name, null, true));
            v.VerifyMethod(e => e.GetDatabaseValues(), m => m.GetDatabaseValues());
            v.VerifyMethod(e => e.GetValidationResult(), m => m.GetValidationResult(It.IsAny<IDictionary<object, object>>()));
            v.VerifyMethod(e => e.Member(name), m => m.Member(name, null));
            v.VerifyMethod(e => e.Property(name), m => m.Property(name, null, false));
            v.VerifyMethod(e => e.Reference(name), m => m.Reference(name, null));
            v.VerifyMethod(e => e.Reload(), m => m.Reload());

#if !NET40
            v.VerifyMethod(e => e.GetDatabaseValuesAsync(), m => m.GetDatabaseValuesAsync(CancellationToken.None));
            v.VerifyMethod(e => e.GetDatabaseValuesAsync(CancellationToken.None), m => m.GetDatabaseValuesAsync(CancellationToken.None));
            v.VerifyMethod(e => e.ReloadAsync(), m => m.ReloadAsync(CancellationToken.None));
            v.VerifyMethod(e => e.ReloadAsync(CancellationToken.None), m => m.ReloadAsync(CancellationToken.None));
#endif
        }

        [Fact]
        public void Generic_DbEntityEntry_delegates_to_InternalReferenceEntry()
        {
            var v = new DbEntityEntryVerifier<object>();
            var name = "foo";
            v.VerifyGetter(e => e.CurrentValues, m => m.CurrentValues);
            v.VerifyGetter(e => e.OriginalValues, m => m.OriginalValues);
            v.VerifyMethod(e => e.Collection(name), m => m.Collection(name, null));
            v.VerifyMethod(e => e.Collection<object>(name), m => m.Collection(name, typeof(object)));
            v.VerifyMethod(e => e.ComplexProperty(name), m => m.Property(name, null, true));
            v.VerifyMethod(e => e.ComplexProperty<object>(name), m => m.Property(name, typeof(object), true));
            v.VerifyMethod(e => e.GetDatabaseValues(), m => m.GetDatabaseValues());
            v.VerifyMethod(e => e.GetValidationResult(), m => m.GetValidationResult(It.IsAny<IDictionary<object, object>>()));
            v.VerifyMethod(e => e.Member(name), m => m.Member(name, null));
            v.VerifyMethod(e => e.Member<object>(name), m => m.Member(name, typeof(object)));
            v.VerifyMethod(e => e.Property(name), m => m.Property(name, null, false));
            v.VerifyMethod(e => e.Property<object>(name), m => m.Property(name, typeof(object), false));
            v.VerifyMethod(e => e.Reference(name), m => m.Reference(name, null));
            v.VerifyMethod(e => e.Reference<object>(name), m => m.Reference(name, typeof(object)));
            v.VerifyMethod(e => e.Reload(), m => m.Reload());

#if !NET40
            v.VerifyMethod(e => e.GetDatabaseValuesAsync(), m => m.GetDatabaseValuesAsync(CancellationToken.None));
            v.VerifyMethod(e => e.GetDatabaseValuesAsync(CancellationToken.None), m => m.GetDatabaseValuesAsync(CancellationToken.None));
            v.VerifyMethod(e => e.ReloadAsync(), m => m.ReloadAsync(CancellationToken.None));
            v.VerifyMethod(e => e.ReloadAsync(CancellationToken.None), m => m.ReloadAsync(CancellationToken.None));
#endif
        }

        public class Reference
        {
            [Fact]
            public void Can_get_reference_entry_using_expression_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Reference(e => e.Reference);
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
            }

            [Fact]
            public void Can_get_reference_entry_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Reference("Reference");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
            }

            [Fact]
            public void Can_get_reference_entry_using_generic_string_method_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Reference<FakeEntity>("Reference");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
            }

            [Fact]
            public void Can_get_reference_entry_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var propEntry = entityEntry.Reference("Reference");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
            }

            [Fact]
            public void Passing_null_expression_to_generic_DbEntityEntry_Reference_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    "navigationProperty",
                    Assert.Throws<ArgumentNullException>(() => entityEntry.Reference((Expression<Func<FakeWithProps, string>>)null)).
                        ParamName);
            }

            [Fact]
            public void Passing_null_string_to_generic_DbEntityEntry_Reference_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbEntityEntry_Reference_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbEntityEntry_Reference_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_generic_DbEntityEntry_Reference_generic_string_method_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference<object>((string)null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbEntityEntry_Reference_generic_string_method_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference<string>("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbEntityEntry_Reference_generic_string_method_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference<Random>(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_non_generic_DbEntityEntry_Reference_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_non_generic_DbEntityEntry_Reference_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_non_generic_DbEntityEntry_Reference_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference(" ")).Message);
            }

            [Fact]
            public void Passing_bad_expression_to_generic_DbEntityEntry_Reference_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Reference", "FakeWithProps"), "navigationProperty").
                        Message, Assert.Throws<ArgumentException>(() => entityEntry.Reference(e => new FakeEntity())).Message);
            }

            [Fact]
            public void Using_Reference_with_dotted_lamda_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.RefTypeProp"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference(e => e.ComplexProp.RefTypeProp)).Message);
            }

            [Fact]
            public void Using_Reference_with_dotted_string_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.RefTypeProp"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference("ComplexProp.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_Reference_with_dotted_generic_string_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.RefTypeProp"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference<string>("ComplexProp.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_non_generic_Reference_with_dotted_string_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.RefTypeProp"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Reference("ComplexProp.RefTypeProp")).Message);
            }
        }

        public class Collection
        {
            [Fact]
            public void Can_get_collection_entry_using_expression_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Collection(e => e.Collection);
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
            }

            [Fact]
            public void Can_get_collection_entry_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Collection("Collection");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
            }

            [Fact]
            public void Can_get_collection_entry_using_generic_string_method_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Collection<FakeEntity>("Collection");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
            }

            [Fact]
            public void Can_get_collection_entry_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var propEntry = entityEntry.Collection("Collection");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
            }

            [Fact]
            public void Passing_null_expression_to_generic_DbEntityEntry_Collection_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    "navigationProperty",
                    Assert.Throws<ArgumentNullException>(
                        () => entityEntry.Collection((Expression<Func<FakeWithProps, ICollection<string>>>)null)).ParamName);
            }

            [Fact]
            public void Passing_null_string_to_generic_DbEntityEntry_Collection_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbEntityEntry_Collection_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbEntityEntry_Collection_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_generic_DbEntityEntry_Collection_generic_string_method_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection<string>((string)null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbEntityEntry_Collection_generic_string_method_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection<object>("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbEntityEntry_Collection_generic_string_method_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection<Random>(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_non_generic_DbEntityEntry_Collection_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_non_generic_DbEntityEntry_Collection_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_non_generic_DbEntityEntry_Collection_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("navigationProperty"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection(" ")).Message);
            }

            [Fact]
            public void Passing_bad_expression_to_generic_DbEntityEntry_Collection_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Collection", "FakeWithProps"), "navigationProperty").
                        Message, Assert.Throws<ArgumentException>(() => entityEntry.Collection(e => new List<FakeEntity>())).Message);
            }

            [Fact]
            public void Using_Collection_with_dotted_lamda_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.Collection"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection(e => e.ComplexProp.Collection)).Message);
            }

            [Fact]
            public void Using_Collection_with_dotted_string_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.Collection"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection("ComplexProp.Collection")).Message);
            }

            [Fact]
            public void Using_Collection_with_dotted_generic_string_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.Collection"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection<FakeWithProps>("ComplexProp.Collection")).Message);
            }

            [Fact]
            public void Using_non_generic_Collection_with_dotted_string_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.Collection"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Collection("ComplexProp.Collection")).Message);
            }
        }

        public class Property
        {
            [Fact]
            public void Can_get_value_type_property_entry_using_expression_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Property(e => e.ValueTypeProp);
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
            }

            [Fact]
            public void Can_get_value_type_property_entry_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Property("ValueTypeProp");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_value_type_property_entry_using_generic_string_method_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Property<int>("ValueTypeProp");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
            }

            [Fact]
            public void Can_get_value_type_property_entry_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var propEntry = entityEntry.Property("ValueTypeProp");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_reference_type_property_entry_using_expression_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Property(e => e.RefTypeProp);
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(string)));
            }

            [Fact]
            public void Can_get_reference_type_property_entry_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Property("RefTypeProp");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_reference_type_property_entry_using_generic_string_method_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Property<string>("RefTypeProp");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(string)));
            }

            [Fact]
            public void Can_get_reference_type_property_entry_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var propEntry = entityEntry.Property("RefTypeProp");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_complex_type_property_entry_using_expression_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Property(e => e.ComplexProp);
                Assert.NotNull(propEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void Can_get_complex_type_property_entry_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Property("ComplexProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbComplexPropertyEntry>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_complex_type_property_entry_using_generic_string_method_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Property<FakeWithProps>("ComplexProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void Can_get_complex_type_property_entry_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var propEntry = entityEntry.Property("ComplexProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbComplexPropertyEntry>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_nested_property_entry_using_dotted_lambda_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property(e => e.ComplexProp.ValueTypeProp);

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
            }

            [Fact]
            public void Can_get_nested_property_entry_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property("ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            }

            [Fact]
            public void Can_get_generic_nested_property_entry_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property<int>("ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
            }

            [Fact]
            public void Can_get_nested_property_entry_using_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property("ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_dotted_lambda_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property(e => e.ComplexProp.ComplexProp);

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            }

            [Fact]
            public void Can_get_generic_nested_complex_property_entry_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property<FakeWithProps>("ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            }

            [Fact]
            public void Can_get_double_nested_property_entry_using_dotted_lambda_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property(e => e.ComplexProp.ComplexProp.ValueTypeProp);

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
            }

            [Fact]
            public void Can_get_double_nested_property_entry_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            }

            [Fact]
            public void Can_get_generic_double_nested_property_entry_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property<int>("ComplexProp.ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
            }

            [Fact]
            public void Can_get_double_nested_property_entry_using_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            }

            [Fact]
            public void Can_get_double_nested_complex_property_entry_using_dotted_lambda_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property(e => e.ComplexProp.ComplexProp.ComplexProp);

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
            }

            [Fact]
            public void Can_get_double_nested_complex_property_entry_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
            }

            [Fact]
            public void Can_get_generic_double_nested_complex_property_entry_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property<FakeWithProps>("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
            }

            [Fact]
            public void Can_get_double_nested_complex_property_entry_using_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
            }

            [Fact]
            public void Passing_null_expression_to_generic_DbEntityEntry_Property_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    "property",
                    Assert.Throws<ArgumentNullException>(() => entityEntry.Property((Expression<Func<FakeWithProps, string>>)null)).
                        ParamName);
            }

            [Fact]
            public void Passing_null_string_to_generic_DbEntityEntry_Property_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbEntityEntry_Property_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbEntityEntry_Property_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_generic_DbEntityEntry_generic_string_Property_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property<int>((string)null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbEntityEntry_generic_string_Property_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbEntityEntry_generic_string_Property_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property<Random>(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_non_generic_DbEntityEntry_Property_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_non_generic_DbEntityEntry_Property_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_non_generic_DbEntityEntry_Property_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property(" ")).Message);
            }

            [Fact]
            public void Passing_bad_expression_to_generic_DbEntityEntry_Property_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Property", "FakeWithProps"), "property").Message,
                    Assert.Throws<ArgumentException>(() => entityEntry.Property(e => new FakeEntity())).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_lamda_containing_scalar_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property(e => e.RefTypeProp.Length)).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_string_containing_scalar_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_generic_string_containing_scalar_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_non_generic_Property_with_dotted_string_containing_scalar_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_lamda_containing_reference_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property(e => e.Reference.Id)).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_string_containing_reference_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("Reference.Id")).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_generic_string_containing_reference_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("Reference.Id")).Message);
            }

            [Fact]
            public void Using_non_generic_Property_with_dotted_string_containing_reference_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("Reference.Id")).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_lamda_containing_collection_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property(e => e.Collection.Count)).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_string_containing_collection_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("Collection.Count")).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_generic_string_containing_collection_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("Collection.Count")).Message);
            }

            [Fact]
            public void Using_non_generic_Property_with_dotted_string_containing_collection_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("Collection.Count")).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_string_containing_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_generic_string_containing_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_non_generic_Property_with_dotted_string_containing_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_string_ending_in_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_Property_with_dotted_generic_string_ending_in_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_non_generic_Property_with_dotted_string_ending_in_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Property("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_lamda_containing_scalar_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property(e => e.RefTypeProp.Length)).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_string_containing_scalar_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_generic_string_containing_scalar_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property<string>("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_non_generic_Property_on_DbComplexProperty_with_dotted_string_containing_scalar_name_throws()
            {
                var propEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_lamda_containing_reference_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property(e => e.Reference.Id)).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("Reference.Id")).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_generic_string_containing_reference_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property<string>("Reference.Id")).Message);
            }

            [Fact]
            public void Using_non_generic_Property_on_DbComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws()
            {
                var propEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("Reference.Id")).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_lamda_containing_collection_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property(e => e.Collection.Count)).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("Collection.Count")).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_generic_string_containing_collection_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property<string>("Collection.Count")).Message);
            }

            [Fact]
            public void Using_non_generic_Property_on_DbComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws()
            {
                var propEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("Collection.Count")).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_generic_string_containing_missing_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property<string>("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_non_generic_Property_on_DbComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
            {
                var propEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_Property_on_DbComplexProperty_with_dotted_generic_string_ending_in_missing_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property<string>("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_non_generic_Property_on_DbComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
            {
                var propEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("ComplexProp.Missing")).Message);
            }
        }

        public class ComplexProperty
        {
            [Fact]
            public void Can_get_complex_type_property_entry_using_ComplexProperty_with_expression_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.ComplexProperty(e => e.ComplexProp);
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void Can_get_complex_type_property_entry_using_ComplexProperty_with_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.ComplexProperty("ComplexProp");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_complex_type_property_entry_using_generic_ComplexProperty_with_string_method_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.ComplexProperty<FakeWithProps>("ComplexProp");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void Can_get_complex_type_property_entry_using_ComplexProperty_with_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var propEntry = entityEntry.ComplexProperty("ComplexProp");
                Assert.NotNull(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_dotted_lambda_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.ComplexProperty(e => e.ComplexProp.ComplexProp);

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.ComplexProperty("ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            }

            [Fact]
            public void Can_get_generic_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.ComplexProperty<FakeWithProps>("ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var nestedEntry = entityEntry.ComplexProperty("ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            }

            [Fact]
            public void Can_get_double_nested_complex_property_entry_using_ComplexProperty_with_dotted_lambda_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.ComplexProperty(e => e.ComplexProp.ComplexProp.ComplexProp);

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
            }

            [Fact]
            public void Can_get_double_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.ComplexProperty("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
            }

            [Fact]
            public void Can_get_generic_double_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.ComplexProperty<FakeWithProps>("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
            }

            [Fact]
            public void Can_get_double_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var nestedEntry = entityEntry.ComplexProperty("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
            }

            [Fact]
            public void Passing_null_string_to_generic_DbEntityEntry_ComplexProperty_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbEntityEntry_ComplexProperty_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbEntityEntry_ComplexProperty_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_generic_DbEntityEntry_generic_string_ComplexProperty_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<int>((string)null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbEntityEntry_generic_string_ComplexProperty_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbEntityEntry_generic_string_ComplexProperty_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<Random>(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_non_generic_DbEntityEntry_ComplexProperty_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_non_generic_DbEntityEntry_ComplexProperty_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_non_generic_DbEntityEntry_ComplexProperty_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(" ")).Message);
            }

            [Fact]
            public void Passing_bad_expression_to_generic_DbEntityEntry_ComplexProperty_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Property", "FakeWithProps"), "property").Message,
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(e => new FakeEntity())).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_lamda_containing_scalar_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(e => e.RefTypeProp.Length)).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_string_containing_scalar_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_generic_string_containing_scalar_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_with_dotted_string_containing_scalar_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_lamda_containing_reference_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(e => e.Reference.Id)).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Reference.Id")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_generic_string_containing_reference_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("Reference.Id")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Reference.Id")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_lamda_containing_collection_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(e => e.Collection.Count)).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Collection.Count")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_generic_string_containing_collection_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("Collection.Count")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Collection.Count")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_generic_string_containing_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_generic_string_ending_in_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_lambda_ending_in_scalar_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(e => e.ComplexProp.RefTypeProp)).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_string_ending_in_scalar_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("ComplexProp.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_with_dotted_generic_string_ending_in_scalar_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("ComplexProp.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_with_dotted_string_ending_in_scalar_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("ComplexProp.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_lamda_containing_scalar_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(e => e.RefTypeProp.Length)).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_scalar_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_containing_scalar_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_scalar_name_throws()
            {
                var propEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_lamda_containing_reference_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(e => e.Reference.Id)).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Reference.Id")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_containing_reference_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("Reference.Id")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws(
                
                )
            {
                var propEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Reference.Id")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_lamda_containing_collection_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(e => e.Collection.Count)).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Collection.Count")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_containing_collection_nav_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("Collection.Count")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws
                ()
            {
                var propEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Collection.Count")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_containing_missing_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
            {
                var propEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_ending_in_missing_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
            {
                var propEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_lambda_ending_in_scalar_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(e => e.ComplexProp.RefTypeProp)).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_ending_in_scalar_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("ComplexProp.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_ending_in_scalar_prop_name_throws()
            {
                var propEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("ComplexProp.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_ending_in_scalar_prop_name_throws()
            {
                var propEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("ComplexProp.RefTypeProp")).Message);
            }
        }

        public class Member
        {
            [Fact]
            public void Can_get_reference_entry_with_Member_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Member("Reference");
                Assert.NotNull(propEntry);
                Assert.IsType<DbReferenceEntry>(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
            }

            [Fact]
            public void Can_get_reference_entry_with_Member_using_generic_string_method_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Member<FakeEntity>("Reference");
                Assert.NotNull(propEntry);
                Assert.IsType<DbReferenceEntry<FakeWithProps, FakeEntity>>(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
            }

            [Fact]
            public void Can_get_reference_entry_with_Member_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var propEntry = entityEntry.Member("Reference");
                Assert.NotNull(propEntry);
                Assert.IsType<DbReferenceEntry>(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
            }

            [Fact]
            public void Can_get_collection_entry_with_Member_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Member("Collection");
                Assert.NotNull(propEntry);
                Assert.IsType<DbCollectionEntry>(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
            }

            [Fact]
            public void Can_get_collection_entry_with_Member_using_generic_string_method_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Member<ICollection<FakeEntity>>("Collection");
                Assert.NotNull(propEntry);
                Assert.IsType<DbCollectionEntry<FakeWithProps, FakeEntity>>(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
            }

            [Fact]
            public void Can_get_collection_entry_with_Member_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var propEntry = entityEntry.Member("Collection");
                Assert.NotNull(propEntry);
                Assert.IsType<DbCollectionEntry>(propEntry);

                mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
            }

            [Fact]
            public void Can_get_value_type_property_entry_with_Member_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Member("ValueTypeProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbPropertyEntry>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_value_type_property_entry_with_Member_using_generic_string_method_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Member<int>("ValueTypeProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbPropertyEntry<FakeWithProps, int>>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
            }

            [Fact]
            public void Can_get_value_type_property_entry_with_Member_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var propEntry = entityEntry.Member("ValueTypeProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbPropertyEntry>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_reference_type_property_entry_with_Member_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Member("RefTypeProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbPropertyEntry>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_reference_type_property_entry_with_Member_using_generic_string_method_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Member<string>("RefTypeProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbPropertyEntry<FakeWithProps, string>>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(string)));
            }

            [Fact]
            public void Can_get_reference_type_property_entry_with_Member_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var propEntry = entityEntry.Member("RefTypeProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbPropertyEntry>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_complex_type_property_entry_with_Member_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Member("ComplexProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbComplexPropertyEntry>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_complex_type_property_entry_with_Member_using_generic_string_method_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var propEntry = entityEntry.Member<FakeWithProps>("ComplexProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void Can_get_complex_type_property_entry_with_Member_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var propEntry = entityEntry.Member("ComplexProp");
                Assert.NotNull(propEntry);
                Assert.IsType<DbComplexPropertyEntry>(propEntry);

                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_nested_property_entry_using_Member_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member("ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            }

            [Fact]
            public void Can_get_generic_nested_property_entry_using_Member_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member<int>("ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbPropertyEntry<FakeWithProps, int>>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
            }

            [Fact]
            public void Can_get_nested_property_entry_using_Member_with_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member("ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_Member_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            }

            [Fact]
            public void Can_get_generic_nested_complex_property_entry_using_Member_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member<FakeWithProps>("ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_Member_with_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            }

            [Fact]
            public void Can_get_double_nested_property_entry_using_Member_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            }

            [Fact]
            public void Can_get_generic_double_nested_property_entry_using_Member_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member<int>("ComplexProp.ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbPropertyEntry<FakeWithProps, int>>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
            }

            [Fact]
            public void Can_get_double_nested_property_entry_using_Member_with_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            }

            [Fact]
            public void Can_get_double_nested_complex_property_entry_using_Member_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
            }

            [Fact]
            public void Can_get_generic_double_nested_complex_property_entry_using_Member_with_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member<FakeWithProps>("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
            }

            [Fact]
            public void Can_get_double_nested_complex_property_entry_using_Member_with_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

                var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
            }

            [Fact]
            public void Passing_null_string_to_generic_DbEntityEntry_Member_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbEntityEntry_Member_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbEntityEntry_Member_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_generic_DbEntityEntry_generic_string_Member_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member<int>(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbEntityEntry_generic_string_Member_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbEntityEntry_generic_string_Member_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member<Random>(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_non_generic_DbEntityEntry_Member_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_non_generic_DbEntityEntry_Member_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_non_generic_DbEntityEntry_Member_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member(" ")).Message);
            }

            [Fact]
            public void Using_Member_with_dotted_string_containing_scalar_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_Member_with_dotted_generic_string_containing_scalar_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_non_generic_Member_with_dotted_string_containing_scalar_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("RefTypeProp.Length")).Message);
            }

            [Fact]
            public void Using_Member_with_dotted_string_containing_reference_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("Reference.Id")).Message);
            }

            [Fact]
            public void Using_Member_with_dotted_generic_string_containing_reference_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("Reference.Id")).Message);
            }

            [Fact]
            public void Using_non_generic_Member_with_dotted_string_containing_reference_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("Reference.Id")).Message);
            }

            [Fact]
            public void Using_Member_with_dotted_string_containing_collection_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("Collection.Count")).Message);
            }

            [Fact]
            public void Using_Member_with_dotted_generic_string_containing_collection_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("Collection.Count")).Message);
            }

            [Fact]
            public void Using_non_generic_Member_with_dotted_string_containing_collection_nav_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("Collection.Count")).Message);
            }

            [Fact]
            public void Using_Member_with_dotted_string_containing_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_Member_with_dotted_generic_string_containing_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_non_generic_Member_with_dotted_string_containing_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("Missing.RefTypeProp")).Message);
            }

            [Fact]
            public void Using_Member_with_dotted_string_ending_in_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_Member_with_dotted_generic_string_ending_in_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("ComplexProp.Missing")).Message);
            }

            [Fact]
            public void Using_non_generic_Member_with_dotted_string_ending_in_missing_prop_name_throws()
            {
                var entityEntry = new DbEntityEntry(FakeWithProps.CreateMockInternalEntityEntry().Object);

                Assert.Equal(
                    Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"),
                    Assert.Throws<ArgumentException>(() => entityEntry.Member("ComplexProp.Missing")).Message);
            }
        }

        #region Helpers

        internal class DbEntityEntryVerifier : DbMemberEntryVerifier<DbEntityEntry, InternalEntityEntry>
        {
            protected override DbEntityEntry CreateEntry(InternalEntityEntry internalEntry)
            {
                return new DbEntityEntry(internalEntry);
            }

            protected override Mock<InternalEntityEntry> CreateInternalEntryMock()
            {
                return new Mock<InternalEntityEntry>(
                    new Mock<InternalContextForMock>
                        {
                            CallBase = true
                        }.Object,
                    MockHelper.CreateMockStateEntry<object>().Object);
            }
        }

        internal class DbEntityEntryVerifier<TEntity> :
            DbMemberEntryVerifier<DbEntityEntry<TEntity>, InternalEntityEntry>
            where TEntity : class, new()
        {
            protected override DbEntityEntry<TEntity> CreateEntry(InternalEntityEntry internalEntry)
            {
                return new DbEntityEntry<TEntity>(internalEntry);
            }

            protected override Mock<InternalEntityEntry> CreateInternalEntryMock()
            {
                return new Mock<InternalEntityEntry>(
                    new Mock<InternalContextForMock>
                        {
                            CallBase = true
                        }.Object,
                    MockHelper.CreateMockStateEntry<TEntity>().Object);
            }
        }

        #endregion
    }
}
