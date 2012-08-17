// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Threading;
    using Xunit;

    public class InternalReferenceEntryTests
    {
        public class CurrentValue
        {
            [Fact]
            public void InternalReferenceEntry_gets_current_value_from_entity_if_property_exists()
            {
                InternalReferenceEntry_gets_current_value_from_entity_if_property_exists_implementation(isDetached: false);
            }

            [Fact]
            public void InternalReferenceEntry_gets_current_value_from_entity_even_when_detached_if_property_exists()
            {
                InternalReferenceEntry_gets_current_value_from_entity_if_property_exists_implementation(isDetached: true);
            }

            private void InternalReferenceEntry_gets_current_value_from_entity_if_property_exists_implementation(bool isDetached)
            {
                var relatedEntity = new FakeEntity();
                var entity = new FakeWithProps
                                 {
                                     Reference = relatedEntity
                                 };
                var internalEntry = new InternalReferenceEntry(
                    MockHelper.CreateMockInternalEntityEntry(
                        entity, isDetached).Object, FakeWithProps.ReferenceMetadata);

                var propValue = internalEntry.CurrentValue;

                Assert.Same(relatedEntity, propValue);
            }

            [Fact]
            public void InternalReferenceEntry_sets_current_value_onto_entity_if_property_exists()
            {
                InternalReferenceEntry_sets_current_value_onto_entity_if_property_exists_implementation(isDetached: false);
            }

            [Fact]
            public void InternalReferenceEntry_sets_current_value_onto_entity_even_when_detached_if_property_exists()
            {
                InternalReferenceEntry_sets_current_value_onto_entity_if_property_exists_implementation(isDetached: true);
            }

            private void InternalReferenceEntry_sets_current_value_onto_entity_if_property_exists_implementation(bool isDetached)
            {
                var entity = new FakeWithProps
                                 {
                                     Reference = new FakeEntity()
                                 };
                var internalEntry = new InternalReferenceEntry(
                    MockHelper.CreateMockInternalEntityEntry(entity, isDetached).Object, FakeWithProps.ReferenceMetadata);

                var relatedEntity = new FakeEntity();
                internalEntry.CurrentValue = relatedEntity;

                Assert.Same(relatedEntity, entity.Reference);
            }

            [Fact]
            public void InternalReferenceEntry_gets_current_value_from_RelatedEnd_if_navigation_property_has_been_removed_from_entity()
            {
                InternalReferenceEntry_gets_current_value_from_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation
                    (new FakeEntity());
            }

            [Fact]
            public void
                InternalReferenceEntry_gets_null_from_RelatedEnd_if_navigation_property_has_been_removed_from_entity_and_no_value_is_set()
            {
                InternalReferenceEntry_gets_current_value_from_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation
                    (null);
            }

            private void
                InternalReferenceEntry_gets_current_value_from_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation
                (FakeEntity relatedEntity)
            {
                var mockRelatedEnd = Core.Objects.DataClasses.MockHelper.CreateMockEntityReference(relatedEntity);

                var internalEntry =
                    new InternalReferenceEntry(
                        MockHelper.CreateMockInternalEntityEntry(new FakeEntity(), mockRelatedEnd.Object).Object,
                        FakeWithProps.ReferenceMetadata);

                var propValue = internalEntry.CurrentValue;

                Assert.Same(relatedEntity, propValue);
            }

            [Fact]
            public void InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity()
            {
                InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation
                    (new FakeEntity(), new FakeEntity());
            }

            [Fact]
            public void InternalReferenceEntry_sets_null_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity()
            {
                InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation
                    (new FakeEntity(), null);
            }

            [Fact]
            public void
                InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_when_current_value_is_null
                ()
            {
                InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation
                    (null, new FakeEntity());
            }

            [Fact]
            public void
                InternalReferenceEntry_sets_null_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_when_current_value_and_new_value_are_both_null
                ()
            {
                InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation
                    (null, null);
            }

            [Fact]
            public void
                InternalReferenceEntry_does_not_set_anything_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_when_current_value_and_new_value_are_same
                ()
            {
                var relatedEntity = new FakeEntity();
                InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation
                    (relatedEntity, relatedEntity);
            }

            /// <summary>
            ///   Validates that the call to set a value on an <see cref="EntityReference{TEntity}" /> is made as
            ///   expected without mocking EntityReference (since it is sealed). This could be dones with Moq
            ///   but it turned out easier and clearer this way.
            /// </summary>
            internal class FakeInternalReferenceEntry : InternalReferenceEntry
            {
                public FakeInternalReferenceEntry(InternalEntityEntry internalEntityEntry, NavigationEntryMetadata navigationMetadata)
                    : base(internalEntityEntry, navigationMetadata)
                {
                }

                protected override void SetNavigationPropertyOnRelatedEnd(object value)
                {
                    SetCount++;
                    ValueSet = value;
                }

                public object ValueSet { get; set; }
                public int SetCount { get; set; }
            }

            private void
                InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation
                (FakeEntity currentRelatedEntity, FakeEntity newRelatedEntity)
            {
                var mockRelatedEnd = Core.Objects.DataClasses.MockHelper.CreateMockEntityReference(currentRelatedEntity);

                var internalEntry =
                    new FakeInternalReferenceEntry(
                        MockHelper.CreateMockInternalEntityEntry(new FakeEntity(), mockRelatedEnd.Object).Object,
                        FakeWithProps.ReferenceMetadata);

                internalEntry.CurrentValue = newRelatedEntity;

                Assert.Equal(1, internalEntry.SetCount);
                Assert.Same(newRelatedEntity, internalEntry.ValueSet);
            }

            [Fact]
            public void InternalReferenceEntry_throws_getting_current_value_from_detached_entity_if_navigation_property_has_been_removed()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, FakeWithProps.ReferenceMetadata);

                Assert.Equal(
                    Strings.DbPropertyEntry_NotSupportedForDetached("CurrentValue", "Reference", "FakeEntity"),
                    Assert.Throws<InvalidOperationException>(() => { var _ = internalEntry.CurrentValue; }).Message);
            }

            [Fact]
            public void InternalReferenceEntry_throws_setting_current_value_from_detached_entity_if_navigation_property_has_been_removed()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, FakeWithProps.ReferenceMetadata);

                Assert.Equal(
                    Strings.DbPropertyEntry_SettingEntityRefNotSupported("Reference", "FakeEntity", "Detached"),
                    Assert.Throws<NotSupportedException>(() => { internalEntry.CurrentValue = null; }).Message);
            }
        }

        public class Load
        {
            [Fact]
            public void InternalReferenceEntry_Load_throws_if_used_with_Detached_entity()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, FakeWithProps.ReferenceMetadata);

                Assert.Equal(
                    Strings.DbPropertyEntry_NotSupportedForDetached("Load", "Reference", "FakeEntity"),
                    Assert.Throws<InvalidOperationException>(() => internalEntry.Load()).Message);
            }
        }

#if !NET40

        public class LoadAsync
        {
            [Fact]
            public void InternalReferenceEntry_LoadAsync_throws_if_used_with_Detached_entity()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, FakeWithProps.ReferenceMetadata);

                Assert.Equal(
                    Strings.DbPropertyEntry_NotSupportedForDetached("LoadAsync", "Reference", "FakeEntity"),
                    Assert.Throws<InvalidOperationException>(() => 
                        ExceptionHelpers.UnwrapAggregateExceptions<object>(() =>
                                                                       {
                                                                           internalEntry.LoadAsync(CancellationToken.None).Wait();
                                                                           return null;
                                                                       })).Message);
            }
        }

#endif

        public class IsLoaded
        {
            [Fact]
            public void InternalReferenceEntry_IsLoaded_throws_if_used_with_Detached_entity()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, FakeWithProps.ReferenceMetadata);

                Assert.Equal(
                    Strings.DbPropertyEntry_NotSupportedForDetached("IsLoaded", "Reference", "FakeEntity"),
                    Assert.Throws<InvalidOperationException>(() => { var _ = internalEntry.IsLoaded; }).Message);
            }
        }

        public class Name
        {
            [Fact]
            public void InternalReferenceEntry_Name_works_even_when_used_with_Detached_entity()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, FakeWithProps.ReferenceMetadata);

                Assert.Equal("Reference", internalEntry.Name);
            }
        }

        public class Query
        {
            [Fact]
            public void InternalReferenceEntry_Query_throws_if_used_with_Detached_entity()
            {
                var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                    new FakeEntity(), isDetached: true);
                var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, FakeWithProps.ReferenceMetadata);

                Assert.Equal(
                    Strings.DbPropertyEntry_NotSupportedForDetached("Query", "Reference", "FakeEntity"),
                    Assert.Throws<InvalidOperationException>(() => internalEntry.Query()).Message);
            }
        }
    }
}
