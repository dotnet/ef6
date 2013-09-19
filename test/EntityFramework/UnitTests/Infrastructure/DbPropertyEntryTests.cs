// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class DbPropertyEntryTests
    {
        [Fact]
        public void NonGeneric_DbPropertyEntry_delegates_to_InternalReferenceEntry()
        {
            var v = new DbPropertyEntryVerifier();
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
        public void Generic_DbPropertyEntry_delegates_to_InternalReferenceEntry()
        {
            var v = new DbPropertyEntryVerifier<object, object>();
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

        public class EntityEntry
        {
            [Fact]
            public void EntityEntity_can_be_obtained_from_generic_DbPropertyEntry_back_reference()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                var backEntry = entityEntry.Property(e => e.ValueTypeProp).EntityEntry;

                Assert.Same(entityEntry.Entity, backEntry.Entity);
            }

            [Fact]
            public void EntityEntity_can_be_obtained_from_non_generic_DbPropertyEntry_back_reference()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                var backEntry = entityEntry.Property("ValueTypeProp").EntityEntry;

                Assert.Same(entityEntry.Entity, backEntry.Entity);
            }
        }

        public class ParentProperty
        {
            [Fact]
            public void Parent_PropertyEntity_can_be_obtained_from_nested_generic_DbComplexPropertyEntry_back_reference()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var backEntry = propEntry.Property(e => e.ValueTypeProp).ParentProperty;

                Assert.Same(propEntry.Name, backEntry.Name);
            }

            [Fact]
            public void Parent_PropertyEntity_can_be_obtained_from_nested_non_generic_DbComplexPropertyEntry_back_reference()
            {
                var mockInternalEntry = FakeWithProps.CreateMockInternalEntityEntry();
                var propEntry =
                    new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(
                        new InternalEntityPropertyEntry(mockInternalEntry.Object, FakeWithProps.ComplexPropertyMetadata));

                var backEntry = propEntry.Property("ValueTypeProp").ParentProperty;

                Assert.Same(propEntry.Name, backEntry.Name);
            }

            [Fact]
            public void Parent_PropertyEntity_returns_null_for_non_nested_generic_DbPropertyEntry_back_reference()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                var backEntry = entityEntry.Property(e => e.ValueTypeProp).ParentProperty;

                Assert.Null(backEntry);
            }

            [Fact]
            public void Parent_PropertyEntity_returns_null_for_non_nested_non_generic_DbPropertyEntry_back_reference()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                var backEntry = entityEntry.Property("ValueTypeProp").ParentProperty;

                Assert.Null(backEntry);
            }
        }

        public class Cast
        {
            [Fact]
            public void Non_generic_DbMemberEntry_for_property_can_be_converted_to_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("ValueTypeProp");

                var generic = memberEntry.Cast<FakeWithProps, int>();

                Assert.IsType<DbPropertyEntry<FakeWithProps, int>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbPropertyEntry_for_property_can_be_converted_to_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property("ValueTypeProp");

                var generic = memberEntry.Cast<FakeWithProps, int>();

                Assert.IsType<DbPropertyEntry<FakeWithProps, int>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_property_can_be_converted_to_generic_version_of_base_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("ValueTypeProp");

                var generic = memberEntry.Cast<object, int>();

                Assert.IsType<DbPropertyEntry<object, int>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbPropertyEntry_for_property_can_be_converted_to_generic_version_of_base_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property("ValueTypeProp");

                var generic = memberEntry.Cast<object, int>();

                Assert.IsType<DbPropertyEntry<object, int>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_property_can_be_converted_to_generic_version_of_base_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("ValueTypeProp");

                var generic = memberEntry.Cast<FakeWithProps, object>();

                Assert.IsType<DbPropertyEntry<FakeWithProps, object>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbPropertyEntry_for_property_can_be_converted_to_generic_version_of_base_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property("ValueTypeProp");

                var generic = memberEntry.Cast<FakeWithProps, object>();

                Assert.IsType<DbPropertyEntry<FakeWithProps, object>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_property_cannot_be_converted_to_generic_version_of_derived_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("ValueTypeProp");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbMemberEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(int).Name, typeof(FakeWithProps).Name,
                        typeof(int).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, int>()).Message);
            }

            [Fact]
            public void Non_generic_DbPropertyEntry_for_property_cannot_be_converted_to_generic_version_of_derived_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property("ValueTypeProp");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbPropertyEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(int).Name, typeof(FakeWithProps).Name,
                        typeof(int).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, int>()).Message);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_property_cannot_be_converted_to_generic_version_of_bad_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("ValueTypeProp");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbMemberEntry).Name, typeof(FakeWithProps).Name, typeof(string).Name, typeof(FakeWithProps).Name,
                        typeof(int).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, string>()).Message);
            }

            [Fact]
            public void Non_generic_DbPropertyEntry_for_property_cannot_be_converted_to_generic_version_of_bad_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property("ValueTypeProp");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbPropertyEntry).Name, typeof(FakeWithProps).Name, typeof(short).Name, typeof(FakeWithProps).Name,
                        typeof(int).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, short>()).Message);
            }
        }

        public class ImplicitDbPropertyEntry
        {
            [Fact]
            public void Generic_DbPropertyEntry_can_be_implicitly_converted_to_non_generic_version()
            {
                var propEntry =
                    new DbPropertyEntry<FakeWithProps, FakeEntity>(
                        new InternalEntityPropertyEntry(
                            new Mock<InternalEntityEntryForMock<FakeEntity>>().Object,
                            FakeEntity.FakeNamedFooPropertyMetadata));

                NonGenericTestMethod(propEntry, "Foo");
            }

            private void NonGenericTestMethod(DbPropertyEntry nonGenericEntry, string name)
            {
                Assert.Same(name, nonGenericEntry.Name);
            }

            [Fact]
            public void Generic_DbPropertyEntry_typed_as_DbMemberEntry_can_be_implicitly_converted_to_non_generic_version()
            {
                DbMemberEntry<FakeWithProps, FakeEntity> propEntry =
                    new DbPropertyEntry<FakeWithProps, FakeEntity>(
                        new InternalEntityPropertyEntry(
                            new Mock<InternalEntityEntryForMock<FakeEntity>>().Object,
                            FakeEntity.FakeNamedFooPropertyMetadata));

                NonGenericTestMethodPropAsMember(propEntry, "Foo");
            }

            private void NonGenericTestMethodPropAsMember(DbMemberEntry nonGenericEntry, string name)
            {
                Assert.Same(name, nonGenericEntry.Name);
                Assert.IsType<DbPropertyEntry>(nonGenericEntry);
            }

            [Fact]
            public void Generic_DbPropertyEntry_typed_as_DbMemberEntry_can_be_explicitly_converted_to_non_generic_DbPropertyEntry()
            {
                DbMemberEntry<FakeWithProps, FakeEntity> propEntry =
                    new DbPropertyEntry<FakeWithProps, FakeEntity>(
                        new InternalEntityPropertyEntry(
                            new Mock<InternalEntityEntryForMock<FakeEntity>>().Object,
                            FakeEntity.FakeNamedFooPropertyMetadata));

                NonGenericTestMethod((DbPropertyEntry)propEntry, "Foo");
            }

            [Fact]
            public void Generic_DbMemberEntry_for_property_can_be_converted_to_non_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member<int>("ValueTypeProp");

                var nonGeneric = ImplicitConvert(memberEntry);

                Assert.IsType<DbPropertyEntry>(nonGeneric);
                Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
            }

            private static DbMemberEntry ImplicitConvert(DbMemberEntry nonGeneric)
            {
                return nonGeneric;
            }

            [Fact]
            public void Generic_DbPropertyEntry_for_property_can_be_converted_to_non_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property(e => e.ValueTypeProp);

                var nonGeneric = ImplicitConvert(memberEntry);

                Assert.IsType<DbPropertyEntry>(nonGeneric);
                Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
            }

            [Fact]
            public void Generic_DbPropertyEntry_for_complex_property_can_be_converted_to_non_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Property(e => e.ComplexProp);

                var nonGeneric = ImplicitConvert(memberEntry);

                Assert.IsType<DbComplexPropertyEntry>(nonGeneric);
                Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
            }

            private static DbPropertyEntry ImplicitConvert(DbPropertyEntry nonGeneric)
            {
                return nonGeneric;
            }
        }

        #region Helpers

        internal class DbPropertyEntryVerifier : DbMemberEntryVerifier<DbPropertyEntry, InternalPropertyEntry>
        {
            protected override DbPropertyEntry CreateEntry(InternalPropertyEntry internalEntry)
            {
                return new DbPropertyEntry(internalEntry);
            }

            protected override Mock<InternalPropertyEntry> CreateInternalEntryMock()
            {
                return new Mock<InternalPropertyEntry>(
                    new Mock<InternalEntityEntryForMock<object>>().Object,
                    new PropertyEntryMetadata(typeof(object), typeof(object), "fake property", isMapped: true, isComplex: true));
            }
        }

        internal class DbPropertyEntryVerifier<TEntity, TComplexProperty> :
            DbMemberEntryVerifier<DbPropertyEntry<TEntity, TComplexProperty>, InternalPropertyEntry>
            where TEntity : class
        {
            protected override DbPropertyEntry<TEntity, TComplexProperty> CreateEntry(InternalPropertyEntry internalEntry)
            {
                return new DbPropertyEntry<TEntity, TComplexProperty>(internalEntry);
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
