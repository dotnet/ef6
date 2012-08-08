// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Linq.Expressions;
    using Moq;
    using Xunit;

    public class DbComplexPropertyEntryTests
    {
        [Fact]
        public void NonGeneric_DbComplexPropertyEntry_delegates_to_InternalReferenceEntry()
        {
            var v = new DbComplexPropertyEntryVerifier();
            v.VerifyGetter(e => e.CurrentValue, m => m.CurrentValue);
            var value = new object();
            v.VerifySetter(e => e.CurrentValue = value, m => m.CurrentValue = value);
            v.VerifyGetter(e => e.EntityEntry, m => m.InternalEntityEntry);
            v.VerifyGetter(e => e.IsModified, m => m.IsModified);
            v.VerifySetter(e => e.IsModified = true, m => m.IsModified = true);
            v.VerifyGetter(e => e.Name, m => m.Name);
            v.VerifyGetter(e => e.OriginalValue, m => m.OriginalValue);
            v.VerifySetter(e => e.OriginalValue = value, m => m.OriginalValue = value);
            v.VerifyGetter(e => e.ParentProperty, m => m.ParentPropertyEntry);
            v.VerifyMethod(e => e.GetValidationErrors(), m => m.GetValidationErrors());
        }

        [Fact]
        public void Generic_DbComplexPropertyEntry_delegates_to_InternalReferenceEntry()
        {
            var v = new DbComplexPropertyEntryVerifier<object, object>();
            v.VerifyGetter(e => e.CurrentValue, m => m.CurrentValue);
            var value = new object();
            v.VerifySetter(e => e.CurrentValue = value, m => m.CurrentValue = value);
            v.VerifyGetter(e => e.EntityEntry, m => m.InternalEntityEntry);
            v.VerifyGetter(e => e.IsModified, m => m.IsModified);
            v.VerifySetter(e => e.IsModified = true, m => m.IsModified = true);
            v.VerifyGetter(e => e.Name, m => m.Name);
            v.VerifyGetter(e => e.OriginalValue, m => m.OriginalValue);
            v.VerifySetter(e => e.OriginalValue = value, m => m.OriginalValue = value);
            v.VerifyGetter(e => e.ParentProperty, m => m.ParentPropertyEntry);
            v.VerifyMethod(e => e.GetValidationErrors(), m => m.GetValidationErrors());
        }

        public class Cast
        {
            [Fact]
            public void Non_generic_DbMemberEntry_for_complex_property_can_be_converted_to_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("ComplexProp");

                var generic = memberEntry.Cast<FakeWithProps, FakeWithProps>();

                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbPropertyEntry_for_complex_property_can_be_converted_to_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property("ComplexProp");

                var generic = memberEntry.Cast<FakeWithProps, FakeWithProps>();

                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbComplexPropertyEntry_for_complex_property_can_be_converted_to_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                var generic = memberEntry.Cast<FakeWithProps, FakeWithProps>();

                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_complex_property_can_be_converted_to_generic_version_of_base_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("ComplexProp");

                var generic = memberEntry.Cast<object, FakeWithProps>();

                Assert.IsType<DbComplexPropertyEntry<object, FakeWithProps>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbPropertyEntry_for_complex_property_can_be_converted_to_generic_version_of_base_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property("ComplexProp");

                var generic = memberEntry.Cast<object, FakeWithProps>();

                Assert.IsType<DbComplexPropertyEntry<object, FakeWithProps>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbComplexPropertyEntry_for_complex_property_can_be_converted_to_generic_version_of_base_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                var generic = memberEntry.Cast<object, FakeWithProps>();

                Assert.IsType<DbComplexPropertyEntry<object, FakeWithProps>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_complex_property_can_be_converted_to_generic_version_of_base_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("ComplexProp");

                var generic = memberEntry.Cast<FakeWithProps, object>();

                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, object>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbPropertyEntry_for_complex_property_can_be_converted_to_generic_version_of_base_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property("ComplexProp");

                var generic = memberEntry.Cast<FakeWithProps, object>();

                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, object>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbComplexPropertyEntry_for_complex_property_can_be_converted_to_generic_version_of_base_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                var generic = memberEntry.Cast<FakeWithProps, object>();

                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, object>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("ComplexProp");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbMemberEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeWithProps).Name,
                        typeof(FakeWithProps).Name, typeof(FakeWithProps).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeWithProps>()).Message);
            }

            [Fact]
            public void Non_generic_DbPropertyEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property("ComplexProp");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbPropertyEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeWithProps).Name,
                        typeof(FakeWithProps).Name, typeof(FakeWithProps).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeWithProps>()).Message);
            }

            [Fact]
            public void
                Non_generic_DbComplexPropertyEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbComplexPropertyEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeWithProps).Name,
                        typeof(FakeWithProps).Name, typeof(FakeWithProps).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeWithProps>()).Message);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("ComplexProp");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbMemberEntry).Name, typeof(FakeWithProps).Name, typeof(DerivedFakeWithProps).Name,
                        typeof(FakeWithProps).Name, typeof(FakeWithProps).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, DerivedFakeWithProps>()).Message);
            }

            [Fact]
            public void Non_generic_DbPropertyEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property("ComplexProp");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbPropertyEntry).Name, typeof(FakeWithProps).Name, typeof(DerivedFakeWithProps).Name,
                        typeof(FakeWithProps).Name, typeof(FakeWithProps).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, DerivedFakeWithProps>()).Message);
            }

            [Fact]
            public void
                Non_generic_DbComplexPropertyEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty("ComplexProp");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbComplexPropertyEntry).Name, typeof(FakeWithProps).Name, typeof(DerivedFakeWithProps).Name,
                        typeof(FakeWithProps).Name, typeof(FakeWithProps).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, DerivedFakeWithProps>()).Message);
            }
        }

        public class Property
        {
            [Fact]
            public void Can_get_nested_property_entry_using_lambda_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property(e => e.ValueTypeProp);

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
            }

            [Fact]
            public void Can_get_nested_property_entry_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property("ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_generic_nested_property_entry_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property<int>("ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
            }

            [Fact]
            public void Can_get_nested_property_entry_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property("ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_lambda_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property(e => e.ComplexProp);

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property("ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_generic_nested_complex_property_entry_using_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property<FakeWithProps>("ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property("ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_double_nested_property_entry_from_DbComplexProperty_using_dotted_lambda_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property(e => e.ComplexProp.ComplexProp.ValueTypeProp);

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
            }

            [Fact]
            public void Can_get_double_nested_property_entry_from_DbComplexProperty_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property("ComplexProp.ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_generic_double_nested_property_entry_from_DbComplexProperty_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property<int>("ComplexProp.ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
            }

            [Fact]
            public void Can_get_double_nested_property_entry_from_DbComplexProperty_using_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property("ComplexProp.ComplexProp.ValueTypeProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_dotted_lambda_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property(e => e.ComplexProp.ComplexProp.ComplexProp);

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
            }

            [Fact]
            public void Can_get_generic_double_nested_complex_property_entry_from_DbComplexProperty_using_dotted_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property<FakeWithProps>("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_dotted_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.Property("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
            }

            [Fact]
            public void Passing_null_expression_to_generic_DbPropertyEntry_Property_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata).Object);

                Assert.Equal(
                    "property",
                    Assert.Throws<ArgumentNullException>(() => propEntry.Property((Expression<Func<FakeWithProps, string>>)null)).ParamName);
            }

            [Fact]
            public void Passing_null_string_to_generic_DbPropertyEntry_Property_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbPropertyEntry_Property_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbPropertyEntry_Property_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_generic_method_on_generic_DbPropertyEntry_Property_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property<int>((string)null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_method_on_generic_DbPropertyEntry_Property_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property<int>("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_method_on_generic_DbPropertyEntry_Property_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property<int>(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_non_generic_DbPropertyEntry_Property_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_non_generic_DbPropertyEntry_Property_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_non_generic_DbPropertyEntry_Property_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.Property(" ")).Message);
            }

            [Fact]
            public void Passing_bad_expression_to_generic_DbPropertyEntry_Property_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Property", "FakeWithProps"), "property").Message,
                    Assert.Throws<ArgumentException>(() => propEntry.Property(e => new FakeEntity())).Message);
            }
        }

        public class ComplexProperty
        {
            [Fact]
            public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_lambda_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.ComplexProperty(e => e.ComplexProp);

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.ComplexProperty("ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void Can_get_generic_nested_complex_property_entry_using_ComplexProperty_with_string_on_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.ComplexProperty<FakeWithProps>("ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_string_on_non_generic_API()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.ComplexProperty("ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
            }

            [Fact]
            public void
                Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_ComplexProperty_with_dotted_lambda_on_generic_API(
                )
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.ComplexProperty(e => e.ComplexProp.ComplexProp.ComplexProp);

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void
                Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_ComplexProperty_with_dotted_string_on_generic_API(
                )
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.ComplexProperty("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
            }

            [Fact]
            public void
                Can_get_generic_double_nested_complex_property_entry_from_DbComplexProperty_using_ComplexProperty_with_dotted_string_on_generic_API
                ()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.ComplexProperty<FakeWithProps>("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
                mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
            }

            [Fact]
            public void
                Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_ComplexProperty_with_dotted_string_on_non_generic_API
                ()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var nestedEntry = propEntry.ComplexProperty("ComplexProp.ComplexProp.ComplexProp");

                Assert.NotNull(nestedEntry);
                mockInternalEntry.Verify(
                    e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
            }

            [Fact]
            public void Passing_null_expression_to_generic_DbPropertyEntry_ComplexProperty_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata).Object);

                Assert.Equal(
                    "property",
                    Assert.Throws<ArgumentNullException>(() => propEntry.ComplexProperty((Expression<Func<FakeWithProps, string>>)null)).
                        ParamName);
            }

            [Fact]
            public void Passing_null_string_to_generic_DbPropertyEntry_ComplexProperty_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_DbPropertyEntry_ComplexProperty_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_DbPropertyEntry_ComplexProperty_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_generic_method_on_generic_DbPropertyEntry_ComplexProperty_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<int>((string)null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_generic_method_on_generic_DbPropertyEntry_ComplexProperty_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<int>("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_generic_method_on_generic_DbPropertyEntry_ComplexProperty_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<int>(" ")).Message);
            }

            [Fact]
            public void Passing_null_string_to_non_generic_DbPropertyEntry_ComplexProperty_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(null)).Message);
            }

            [Fact]
            public void Passing_empty_string_to_non_generic_DbPropertyEntry_ComplexProperty_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("")).Message);
            }

            [Fact]
            public void Passing_whitespace_string_to_non_generic_DbPropertyEntry_ComplexProperty_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    Strings.ArgumentIsNullOrWhitespace("propertyName"),
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(" ")).Message);
            }

            [Fact]
            public void Passing_bad_expression_to_generic_DbPropertyEntry_ComplexProperty_throws()
            {
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new Mock<InternalEntityPropertyEntry>(
                            FakeWithProps.CreateMockInternalEntityEntry().Object, FakeWithProps.ComplexPropertyMetadata)
                            {
                                CallBase = true
                            }.Object);

                Assert.Equal(
                    new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Property", "FakeWithProps"), "property").Message,
                    Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(e => new FakeEntity())).Message);
            }
        }

        public class EntityEntry
        {
            [Fact]
            public void EntityEntity_can_be_obtained_from_nested_generic_DbComplexPropertyEntry_back_reference()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var backEntry = propEntry.Property(e => e.ValueTypeProp).EntityEntry;

                Assert.Same(mockInternalEntry.Object.Entity, backEntry.Entity);
            }

            [Fact]
            public void EntityEntity_can_be_obtained_from_nested_non_generic_DbComplexPropertyEntry_back_reference()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var backEntry = propEntry.Property("ValueTypeProp").EntityEntry;

                Assert.Same(mockInternalEntry.Object.Entity, backEntry.Entity);
            }
        }

        public class ImplicitDbComplexProperty
        {
            [Fact]
            public void Generic_DbMemberEntry_for_complex_property_can_be_converted_to_non_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member<FakeWithProps>(
                        "ComplexProp");

                var nonGeneric = ImplicitConvert(memberEntry);

                Assert.IsType<DbComplexPropertyEntry>(nonGeneric);
                Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
            }

            private static DbMemberEntry ImplicitConvert(DbMemberEntry nonGeneric)
            {
                return nonGeneric;
            }

            [Fact]
            public void Generic_DbComplexPropertyEntry_for_complex_property_can_be_converted_to_non_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).ComplexProperty(
                        e => e.ComplexProp);

                var nonGeneric = ImplicitConvert(memberEntry);

                Assert.IsType<DbComplexPropertyEntry>(nonGeneric);
                Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
            }

            private static DbComplexPropertyEntry ImplicitConvert(DbComplexPropertyEntry nonGeneric)
            {
                return nonGeneric;
            }
        }

        #region Helpers

        internal class DbComplexPropertyEntryVerifier : DbMemberEntryVerifier<DbComplexPropertyEntry, InternalPropertyEntry>
        {
            protected override DbComplexPropertyEntry CreateEntry(InternalPropertyEntry internalEntry)
            {
                return new DbComplexPropertyEntry(internalEntry);
            }

            protected override Mock<InternalPropertyEntry> CreateInternalEntryMock()
            {
                return new Mock<InternalPropertyEntry>(
                    new Mock<InternalEntityEntryForMock<object>>().Object,
                    new PropertyEntryMetadata(typeof(object), typeof(object), "fake property", isMapped: true, isComplex: true));
            }
        }

        internal class DbComplexPropertyEntryVerifier<TEntity, TComplexProperty> :
            DbMemberEntryVerifier<DbComplexPropertyEntry<TEntity, TComplexProperty>, InternalPropertyEntry>
            where TEntity : class
        {
            protected override DbComplexPropertyEntry<TEntity, TComplexProperty> CreateEntry(InternalPropertyEntry internalEntry)
            {
                return new DbComplexPropertyEntry<TEntity, TComplexProperty>(internalEntry);
            }

            protected override Mock<InternalPropertyEntry> CreateInternalEntryMock()
            {
                return new Mock<InternalPropertyEntry>(
                    new Mock<InternalEntityEntryForMock<object>>().Object,
                    new PropertyEntryMetadata(typeof(object), typeof(object), "fake property", isMapped: true, isComplex: true));
            }
        }

        #endregion
    }
}
