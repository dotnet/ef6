namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    public class EntityReferenceTests
    {
        public class GetEnumerator
        {
            [Fact]
            public void Throws_when_not_initialized()
            {
                var entityReference = new EntityReference<object>();

                Assert.Equal(
                    Strings.RelatedEnd_OwnerIsNull,
                    Assert.Throws(typeof(InvalidOperationException), () => entityReference.GetEnumerator()).Message);
            }
        }

        public class Load
        {
            [Fact]
            public void Throws_when_not_initialized()
            {
                var entityReference = new EntityReference<object>();

                Assert.Equal(
                    Strings.RelatedEnd_OwnerIsNull,
                    Assert.Throws(typeof(InvalidOperationException), () => entityReference.Load()).Message);
            }

            [Fact]
            public void FK_association_with_nothing_to_load_and_AppendOnly_option_does_nothing()
            {
                var entityReference = MockHelper.CreateMockEntityReference<object>(refreshedValue: null).Object;

                Assert.False(entityReference.IsLoaded);
                entityReference.Load(MergeOption.AppendOnly);
                Assert.True(entityReference.IsLoaded);
            }

            [Fact]
            public void FK_association_with_nothing_to_load_and_NoTracking_option_does_nothing()
            {
                var entityReference = MockHelper.CreateMockEntityReference<object>(refreshedValue: null).Object;

                Assert.False(entityReference.IsLoaded);
                entityReference.Load(MergeOption.NoTracking);
                Assert.True(entityReference.IsLoaded);
            }

            [Fact]
            public void FK_association_with_nothing_to_load_and_OverwriteChanges_option_calls_RemoveRelationships()
            {
                var entityReference = MockHelper.CreateMockEntityReference<object>(refreshedValue: null).Object;

                Assert.False(entityReference.IsLoaded);
                entityReference.Load(MergeOption.OverwriteChanges);
                Assert.True(entityReference.IsLoaded);

                var objectContext = entityReference.ObjectContext;
                var objectStateManagerMock = Mock.Get(objectContext.ObjectStateManager);

                objectStateManagerMock.Verify(m =>
                    m.RemoveRelationships(MergeOption.OverwriteChanges,
                    (AssociationSet)entityReference.RelationshipSet,
                    entityReference.WrappedOwner.EntityKey,
                    (AssociationEndMember)entityReference.FromEndMember));
            }

            [Fact]
            public void FK_association_with_nothing_to_load_and_PreserveChanges_option_calls_RemoveRelationships()
            {
                var entityReference = MockHelper.CreateMockEntityReference<object>(refreshedValue: null).Object;

                Assert.False(entityReference.IsLoaded);
                entityReference.Load(MergeOption.PreserveChanges);
                Assert.True(entityReference.IsLoaded);

                var objectContext = entityReference.ObjectContext;
                var objectStateManagerMock = Mock.Get(objectContext.ObjectStateManager);

                objectStateManagerMock.Verify(m =>
                    m.RemoveRelationships(MergeOption.PreserveChanges,
                    (AssociationSet)entityReference.RelationshipSet,
                    entityReference.WrappedOwner.EntityKey,
                    (AssociationEndMember)entityReference.FromEndMember));
            }

            [Fact]
            public void Calls_merge_if_an_entity_is_available_for_all_merge_options()
            {
                foreach (MergeOption option in Enum.GetValues(typeof(MergeOption)))
                {
                    Calls_merge_if_an_entity_is_available(option);
                }
            }

            private void Calls_merge_if_an_entity_is_available(MergeOption mergeOption)
            {
                var entityReferenceMock = MockHelper.CreateMockEntityReference<object>(refreshedValue: new object());
                int timesMergeCalled = 0;
                entityReferenceMock.Setup(m => m.Merge(It.IsAny<IEnumerable<object>>(), It.IsAny<MergeOption>(), true))
                    .Callback((IEnumerable<object> collection, MergeOption actualMergeOption, bool setIsLoaded) =>
                    {
                        timesMergeCalled++;
                        Assert.Equal(mergeOption, actualMergeOption);
                    });

                var entityReference = entityReferenceMock.Object;
                entityReference.Load(mergeOption);
                Assert.True(1 == timesMergeCalled, "Expected Merge to be called once for MergeOption." + mergeOption);
            }
        }
    }
}
