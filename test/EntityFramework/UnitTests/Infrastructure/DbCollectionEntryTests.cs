// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class DbCollectionEntryTests
    {
        [Fact]
        public void NonGeneric_DbCollectionEntry_delegates_to_InternalReferenceEntry_correctly()
        {
            var v = new DbCollectionEntryVerifier();
            v.VerifyGetter(e => e.CurrentValue, m => m.CurrentValue);
            var currentValue = new object[0];
            v.VerifySetter(e => e.CurrentValue = currentValue, m => m.CurrentValue = currentValue);
            v.VerifyGetter(e => e.EntityEntry, m => m.InternalEntityEntry);
            v.VerifyGetter(e => e.IsLoaded, m => m.IsLoaded);
            v.VerifyGetter(e => e.Name, m => m.Name);
            v.VerifyMethod(e => e.GetValidationErrors(), m => m.GetValidationErrors());
            v.VerifyMethod(e => e.Load(), m => m.Load());
            v.VerifyMethod(e => e.Query(), m => m.Query());
        }

        [Fact]
        public void Generic_DbCollectionEntry_delegates_to_InternalReferenceEntry_correctly()
        {
            var v = new DbCollectionEntryVerifier<object, object>();
            v.VerifyGetter(e => e.CurrentValue, m => m.CurrentValue);
            var currentValue = new object[0];
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
            public void EntityEntity_can_be_obtained_from_generic_DbCollectionEntry_back_reference()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                var backEntry = entityEntry.Collection(e => e.Collection).EntityEntry;

                Assert.Same(entityEntry.Entity, backEntry.Entity);
            }

            [Fact]
            public void EntityEntity_can_be_obtained_from_non_generic_DbCollectionEntry_back_reference()
            {
                var entityEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object);

                var backEntry = entityEntry.Collection("Collection").EntityEntry;

                Assert.Same(entityEntry.Entity, backEntry.Entity);
            }
        }

        public class Cast
        {
            [Fact]
            public void Non_generic_DbMemberEntry_for_collection_can_be_converted_to_generic_version()
            {
                var memberEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member(
                    "Collection");

                var generic = memberEntry.Cast<FakeWithProps, ICollection<FakeEntity>>();

                Assert.IsType<DbCollectionEntry<FakeWithProps, FakeEntity>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbCollectionEntry_for_collection_can_be_converted_to_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Collection("Collection");

                var generic = memberEntry.Cast<FakeWithProps, FakeEntity>();

                Assert.IsType<DbCollectionEntry<FakeWithProps, FakeEntity>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_collection_can_be_converted_to_generic_version_of_base_entity_type()
            {
                var memberEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member(
                    "Collection");

                var generic = memberEntry.Cast<object, ICollection<FakeEntity>>();

                Assert.IsType<DbCollectionEntry<object, FakeEntity>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbCollectionEntry_for_collection_can_be_converted_to_generic_version_of_base_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Collection("Collection");

                var generic = memberEntry.Cast<object, FakeEntity>();

                Assert.IsType<DbCollectionEntry<object, FakeEntity>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_collection_cannot_be_converted_to_generic_version_of_base_property_type()
            {
                var memberEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member(
                    "Collection");

                // This cast fails because an ICollection<FakeEntity> is not an IColletion<object>.
                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbMemberEntry).Name, typeof(FakeWithProps).Name, typeof(ICollection<object>).Name, typeof(FakeWithProps).Name,
                        typeof(ICollection<FakeEntity>).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, ICollection<object>>()).Message);
            }

            [Fact]
            public void Non_generic_DbCollectionEntry_for_collection_can_be_converted_to_generic_version_of_base_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Collection("Collection");

                var generic = memberEntry.Cast<FakeWithProps, object>();

                Assert.IsType<DbCollectionEntry<FakeWithProps, object>>(generic);
                Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_collection_cannot_be_converted_to_generic_version_of_derived_entity_type()
            {
                var memberEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member(
                    "Collection");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbMemberEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(ICollection<FakeEntity>).Name,
                        typeof(FakeWithProps).Name, typeof(ICollection<FakeEntity>).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, ICollection<FakeEntity>>()).Message);
            }

            [Fact]
            public void Non_generic_DbCollectionEntry_for_collection_cannot_be_converted_to_generic_version_of_derived_entity_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Collection("Collection");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbCollectionEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeEntity).Name,
                        typeof(FakeWithProps).Name, typeof(FakeEntity).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeEntity>()).Message);
            }

            [Fact]
            public void Non_generic_DbMemberEntry_for_collection_cannot_be_converted_to_generic_version_of_derived_property_type()
            {
                var memberEntry = new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member(
                    "Collection");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbMemberEntry).Name, typeof(FakeWithProps).Name, typeof(ICollection<FakeDerivedEntity>).Name,
                        typeof(FakeWithProps).Name, typeof(ICollection<FakeEntity>).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, ICollection<FakeDerivedEntity>>()).Message);
            }

            [Fact]
            public void Non_generic_DbCollectionEntry_for_collection_cannot_be_converted_to_generic_version_of_derived_property_type()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Collection("Collection");

                Assert.Equal(
                    Strings.DbMember_BadTypeForCast(
                        typeof(DbCollectionEntry).Name, typeof(FakeWithProps).Name, typeof(FakeDerivedEntity).Name,
                        typeof(FakeWithProps).Name, typeof(FakeEntity).Name),
                    Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, FakeDerivedEntity>()).Message);
            }
        }

        public class ImplicitDbCollectionEntry
        {
            [Fact]
            public void Generic_DbCollectionEntry_can_be_implicitly_converted_to_non_generic_version()
            {
                var propEntry =
                    new DbCollectionEntry<FakeWithProps, FakeEntity>(
                        new InternalCollectionEntry(
                            new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, FakeWithProps.CollectionMetadata));

                NonGenericTestMethod(propEntry, "Collection");
            }

            private void NonGenericTestMethod(DbCollectionEntry nonGenericEntry, string name)
            {
                Assert.Same(name, nonGenericEntry.Name);
            }

            [Fact]
            public void Generic_DbCollectionEntry_typed_as_DbMemberEntry_can_be_implicitly_converted_to_non_generic_version()
            {
                DbMemberEntry<FakeWithProps, ICollection<FakeEntity>> propEntry =
                    new DbCollectionEntry<FakeWithProps, FakeEntity>(
                        new InternalCollectionEntry(
                            new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, FakeWithProps.CollectionMetadata));

                NonGenericTestMethodCollectionAsMember(propEntry, "Collection");
            }

            private void NonGenericTestMethodCollectionAsMember(DbMemberEntry nonGenericEntry, string name)
            {
                Assert.Same(name, nonGenericEntry.Name);
                Assert.IsType<DbCollectionEntry>(nonGenericEntry);
            }

            [Fact]
            public void Generic_DbCollectionEntry_typed_as_DbMemberEntry_can_be_implicitly_converted_to_non_generic_DbCollectionEntry()
            {
                DbMemberEntry<FakeWithProps, ICollection<FakeEntity>> propEntry =
                    new DbCollectionEntry<FakeWithProps, FakeEntity>(
                        new InternalCollectionEntry(
                            new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, FakeWithProps.CollectionMetadata));

                NonGenericTestMethod((DbCollectionEntry)propEntry, "Collection");
            }

            [Fact]
            public void Generic_DbMemberEntry_for_collection_can_be_converted_to_non_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Member<ICollection<FakeEntity>>(
                        "Collection");

                var nonGeneric = ImplicitConvert(memberEntry);

                Assert.IsType<DbCollectionEntry>(nonGeneric);
                Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
            }

            private static DbMemberEntry ImplicitConvert(DbMemberEntry nonGeneric)
            {
                return nonGeneric;
            }

            [Fact]
            public void Generic_DbCollectionEntry_for_collection_can_be_converted_to_non_generic_version()
            {
                var memberEntry =
                    new DbEntityEntry<FakeWithProps>(FakeWithProps.CreateMockInternalEntityEntry().Object).Collection(e => e.Collection);

                var nonGeneric = ImplicitConvert(memberEntry);

                Assert.IsType<DbCollectionEntry>(nonGeneric);
                Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
            }

            private static DbCollectionEntry ImplicitConvert(DbCollectionEntry nonGeneric)
            {
                return nonGeneric;
            }
        }

        #region Helpers

        internal class DbCollectionEntryVerifier : DbMemberEntryVerifier<DbCollectionEntry, InternalCollectionEntry>
        {
            protected override DbCollectionEntry CreateEntry(InternalCollectionEntry internalEntry)
            {
                return new DbCollectionEntry(internalEntry);
            }

            protected override Mock<InternalCollectionEntry> CreateInternalEntryMock()
            {
                return new Mock<InternalCollectionEntry>(
                    new Mock<InternalEntityEntryForMock<object>>().Object,
                    new NavigationEntryMetadata(typeof(object), typeof(object), "fake collection", isCollection: true));
            }
        }

        internal class DbCollectionEntryVerifier<TEntity, TElement> :
            DbMemberEntryVerifier<DbCollectionEntry<TEntity, TElement>, InternalCollectionEntry>
            where TEntity : class
        {
            protected override DbCollectionEntry<TEntity, TElement> CreateEntry(InternalCollectionEntry internalEntry)
            {
                return new DbCollectionEntry<TEntity, TElement>(internalEntry);
            }

            protected override Mock<InternalCollectionEntry> CreateInternalEntryMock()
            {
                return new Mock<InternalCollectionEntry>(
                    new Mock<InternalEntityEntryForMock<object>>().Object,
                    new NavigationEntryMetadata(typeof(object), typeof(object), "fake collection", isCollection: true));
            }
        }

        #endregion
    }
}
