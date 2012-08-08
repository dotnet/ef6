// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class DbReferenceEntryTests
    {
        [Fact]
        public void NonGeneric_DbReferenceEntry_delegates_to_InternalReferenceEntry_correctly()
        {
            var v = new DbReferenceEntryVerifier();
            v.VerifyGetter(e => e.CurrentValue, m => m.CurrentValue);
            var currentValue = new object();
            v.VerifySetter(e => e.CurrentValue = currentValue, m => m.CurrentValue = currentValue);
            v.VerifyGetter(e => e.EntityEntry, m => m.InternalEntityEntry);
            v.VerifyGetter(e => e.IsLoaded, m => m.IsLoaded);
            v.VerifyGetter(e => e.Name, m => m.Name);
            v.VerifyMethod(e => e.GetValidationErrors(), m => m.GetValidationErrors());
            v.VerifyMethod(e => e.Load(), m => m.Load());
            v.VerifyMethod(e => e.Query(), m => m.Query());
        }

        [Fact]
        public void Generic_DbReferenceEntry_delegates_to_InternalReferenceEntry_correctly()
        {
            var v = new DbReferenceEntryVerifier();
            v.VerifyGetter(e => e.CurrentValue, m => m.CurrentValue);
            var currentValue = new object();
            v.VerifySetter(e => e.CurrentValue = currentValue, m => m.CurrentValue = currentValue);
            v.VerifyGetter(e => e.EntityEntry, m => m.InternalEntityEntry);
            v.VerifyGetter(e => e.IsLoaded, m => m.IsLoaded);
            v.VerifyGetter(e => e.Name, m => m.Name);
            v.VerifyMethod(e => e.GetValidationErrors(), m => m.GetValidationErrors());
            v.VerifyMethod(e => e.Load(), m => m.Load());
            v.VerifyMethod(e => e.Query(), m => m.Query());
        }

        public class EntityEntry
        {
            [Fact]
            public void EntityEntity_can_be_obtained_from_generic_DbReferenceEntry_back_reference()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                var backEntry = entityEntry.Reference(e => e.Reference).EntityEntry;

                Assert.Same(entityEntry.Entity, backEntry.Entity);
            }

            [Fact]
            public void EntityEntity_can_be_obtained_from_non_generic_DbReferenceEntry_back_reference()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                var backEntry = entityEntry.Reference("Reference").EntityEntry;

                Assert.Same(entityEntry.Entity, backEntry.Entity);
            }
        }

        public class Cast
        {
            [Fact]
            public void Non_generic_DbMemberEntry_for_reference_can_be_converted_to_generic_version()
            {
                var memberEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("Reference");

                var generic = memberEntry.Cast<FakeWithProps, FakeEntity>();

                Assert.IsType<DbReferenceEntry<FakeWithProps, FakeEntity>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbReferenceEntry_for_reference_can_be_converted_to_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Reference("Reference");

                var generic = memberEntry.Cast<FakeWithProps, FakeEntity>();

                Assert.IsType<DbReferenceEntry<FakeWithProps, FakeEntity>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_reference_can_be_converted_to_generic_version_of_base_entity_type()
            {
                var memberEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("Reference");

                var generic = memberEntry.Cast<object, FakeEntity>();

                Assert.IsType<DbReferenceEntry<object, FakeEntity>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbReferenceEntry_for_reference_can_be_converted_to_generic_version_of_base_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Reference("Reference");

                var generic = memberEntry.Cast<object, FakeEntity>();

                Assert.IsType<DbReferenceEntry<object, FakeEntity>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_reference_can_be_converted_to_generic_version_of_base_property_type()
            {
                var memberEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("Reference");

                var generic = memberEntry.Cast<FakeWithProps, object>();

                Assert.IsType<DbReferenceEntry<FakeWithProps, object>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbReferenceEntry_for_reference_can_be_converted_to_generic_version_of_base_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Reference("Reference");

                var generic = memberEntry.Cast<FakeWithProps, object>();

                Assert.IsType<DbReferenceEntry<FakeWithProps, object>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_reference_cannot_be_converted_to_generic_version_of_derived_entity_type()
            {
                var memberEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("Reference");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbMemberEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeEntity).Name, typeof(FakeWithProps).Name,
                        typeof(FakeEntity).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeEntity>()).Message);
            }

            [Fact]
            public void Non_generic_DbReferenceEntry_for_reference_cannot_be_converted_to_generic_version_of_derived_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Reference("Reference");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbReferenceEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeEntity).Name,
                        typeof(FakeWithProps).Name, typeof(FakeEntity).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeEntity>()).Message);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_reference_cannot_be_converted_to_generic_version_of_derived_property_type()
            {
                var memberEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member("Reference");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbMemberEntry).Name, typeof(FakeWithProps).Name, typeof(FakeDerivedEntity).Name, typeof(FakeWithProps).Name,
                        typeof(FakeEntity).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, FakeDerivedEntity>()).Message);
            }

            [Fact]
            public void Non_generic_DbReferenceEntry_for_reference_cannot_be_converted_to_generic_version_of_derived_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Reference("Reference");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbReferenceEntry).Name, typeof(FakeWithProps).Name, typeof(FakeDerivedEntity).Name,
                        typeof(FakeWithProps).Name, typeof(FakeEntity).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, FakeDerivedEntity>()).Message);
            }
        }

        public class ImplicitDbReferenceEntry
        {
            [Fact]
            public void Generic_DbReferenceEntry_can_be_implicitly_converted_to_non_generic_version()
            {
                var propEntry =
                    new DbReferenceEntry<FakeWithProps, FakeEntity>(
                        new InternalReferenceEntry(
                            new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, FakeWithProps.ReferenceMetadata));

                NonGenericTestMethod(propEntry, "Reference");
            }

            private void NonGenericTestMethod(DbReferenceEntry nonGenericEntry, string name)
            {
                Assert.Same(name, nonGenericEntry.Name);
            }

            [Fact]
            public void Generic_DbReferenceEntry_typed_as_DbMemberEntry_can_be_implicitly_converted_to_non_generic_version()
            {
                DbMemberEntry<FakeWithProps, FakeEntity> propEntry =
                    new DbReferenceEntry<FakeWithProps, FakeEntity>(
                        new InternalReferenceEntry(
                            new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, FakeWithProps.ReferenceMetadata));

                NonGenericTestMethodRefAsMember(propEntry, "Reference");
            }

            private void NonGenericTestMethodRefAsMember(DbMemberEntry nonGenericEntry, string name)
            {
                Assert.Same(name, nonGenericEntry.Name);
                Assert.IsType<DbReferenceEntry>(nonGenericEntry);
            }

            [Fact]
            public void Generic_DbReferenceEntry_typed_as_DbMemberEntry_can_be_implicitly_converted_to_non_generic_DbReferenceEntry()
            {
                DbMemberEntry<FakeWithProps, FakeEntity> propEntry =
                    new DbReferenceEntry<FakeWithProps, FakeEntity>(
                        new InternalReferenceEntry(
                            new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, FakeWithProps.ReferenceMetadata));

                NonGenericTestMethod((DbReferenceEntry)propEntry, "Reference");
            }

            [Fact]
            public void Generic_DbMemberEntry_for_reference_can_be_converted_to_non_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member<FakeEntity>("Reference");

                var nonGeneric = ImplicitConvert(memberEntry);

                Assert.IsType<DbReferenceEntry>(nonGeneric);
                Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
            }

            private static DbMemberEntry ImplicitConvert(DbMemberEntry nonGeneric)
            {
                return nonGeneric;
            }

            [Fact]
            public void Generic_DbReferenceEntry_for_reference_can_be_converted_to_non_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Reference(e => e.Reference);

                var nonGeneric = ImplicitConvert(memberEntry);

                Assert.IsType<DbReferenceEntry>(nonGeneric);
                Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
            }

            private static DbReferenceEntry ImplicitConvert(DbReferenceEntry nonGeneric)
            {
                return nonGeneric;
            }
        }

        #region Helpers

        internal class DbReferenceEntryVerifier : DbMemberEntryVerifier<DbReferenceEntry, InternalReferenceEntry>
        {
            protected override DbReferenceEntry CreateEntry(InternalReferenceEntry internalEntry)
            {
                return new DbReferenceEntry(internalEntry);
            }

            protected override Mock<InternalReferenceEntry> CreateInternalEntryMock()
            {
                return new Mock<InternalReferenceEntry>(
                    MockHelper.CreateMockInternalEntityEntry(new object()).Object,
                    new NavigationEntryMetadataForMock());
            }
        }

        internal class DbReferenceEntryVerifier<TEntity, Tproperty> :
            DbMemberEntryVerifier<DbReferenceEntry<TEntity, Tproperty>, InternalReferenceEntry>
            where TEntity : class
        {
            protected override DbReferenceEntry<TEntity, Tproperty> CreateEntry(InternalReferenceEntry internalEntry)
            {
                return new DbReferenceEntry<TEntity, Tproperty>(internalEntry);
            }

            protected override Mock<InternalReferenceEntry> CreateInternalEntryMock()
            {
                return new Mock<InternalReferenceEntry>(
                    MockHelper.CreateMockInternalEntityEntry(new object()).Object,
                    new NavigationEntryMetadataForMock());
            }
        }

        #endregion
    }
}
