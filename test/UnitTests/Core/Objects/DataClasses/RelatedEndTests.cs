// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Linq;
    using Moq;
    using Xunit;

    public class RelatedEndTests
    {
        public class Merge
        {
            [Fact]
            public void IEnumerable_overload_calls_List_of_IEntityWrapper_overload_when_collection_is_not_a_List_of_IEntityWrapper()
            {
                var entity = new object();
                var collection = new[] { entity };
                foreach (var isLoadedValue in new[] { true, false })
                {
                    foreach (MergeOption mergeOption in Enum.GetValues(typeof(MergeOption)))
                    {
                        var relatedEndMock = CreateMockRelatedEnd();
                        relatedEndMock.Setup(m => m.ObjectContext).Returns(() => null);

                        var timesMergeCalled = 0;
                        relatedEndMock.Setup(
                            m => m.Merge<object>(It.IsAny<List<IEntityWrapper>>(), It.IsAny<MergeOption>(), It.IsAny<bool>()))
                            .Callback(
                                (List<IEntityWrapper> actualCollection, MergeOption actualMergeOption, bool setIsLoaded) =>
                                    {
                                        timesMergeCalled++;
                                        Assert.Equal(collection, actualCollection.Select(w => w.Entity));
                                        Assert.Equal(mergeOption, actualMergeOption);
                                        Assert.Equal(isLoadedValue, setIsLoaded);
                                    });

                        var mockEntityWrapperFactory = Mock.Get(relatedEndMock.Object.EntityWrapperFactory);
                        var timesUpdateNoTrackingWrapperCalled = 0;
                        mockEntityWrapperFactory
                            .Setup(
                                m => m.UpdateNoTrackingWrapper(It.IsAny<IEntityWrapper>(), It.IsAny<ObjectContext>(), It.IsAny<EntitySet>()))
                            .Callback(
                                (IEntityWrapper wrapper, ObjectContext context, EntitySet entitySet) =>
                                    {
                                        timesUpdateNoTrackingWrapperCalled++;
                                        Assert.Same(entity, wrapper.Entity);
                                    });

                        relatedEndMock.Object.Merge(collection, mergeOption, isLoadedValue);

                        Assert.True(1 == timesMergeCalled, "Expected Merge to be called once for MergeOption." + mergeOption);
                        if (mergeOption == MergeOption.NoTracking)
                        {
                            Assert.True(
                                1 == timesUpdateNoTrackingWrapperCalled,
                                "Expected UpdateNoTrackingWrapper to be called once for MergeOption." + mergeOption);
                        }
                        else
                        {
                            Assert.True(
                                0 == timesUpdateNoTrackingWrapperCalled,
                                "Expected UpdateNoTrackingWrapper not to be called for MergeOption." + mergeOption);
                        }
                    }
                }
            }

            [Fact]
            public void IEnumerable_overload_calls_List_of_IEntityWrapper_overload_when_collection_is_a_List_of_IEntityWrapper()
            {
                var collection = new List<IEntityWrapper>();
                foreach (var isLoadedValue in new[] { true, false })
                {
                    foreach (MergeOption mergeOption in Enum.GetValues(typeof(MergeOption)))
                    {
                        var relatedEndMock = CreateMockRelatedEnd();

                        var timesMergeCalled = 0;
                        relatedEndMock.Setup(
                            m => m.Merge<object>(It.IsAny<List<IEntityWrapper>>(), It.IsAny<MergeOption>(), It.IsAny<bool>()))
                            .Callback(
                                (List<IEntityWrapper> actualCollection, MergeOption actualMergeOption, bool setIsLoaded) =>
                                    {
                                        timesMergeCalled++;
                                        Assert.Same(collection, actualCollection);
                                        Assert.Equal(mergeOption, actualMergeOption);
                                        Assert.Equal(isLoadedValue, setIsLoaded);
                                    });

                        relatedEndMock.Object.Merge((IEnumerable<object>)collection, mergeOption, isLoadedValue);

                        Assert.True(1 == timesMergeCalled, "Expected Merge to be called once for MergeOption." + mergeOption);
                    }
                }
            }

            [Fact]
            public void List_of_IEntityWrapper_overload_calls_UpdateRelationships()
            {
                var collection = new List<IEntityWrapper>();
                foreach (var isLoadedValue in new[] { true, false })
                {
                    foreach (MergeOption mergeOption in Enum.GetValues(typeof(MergeOption)))
                    {
                        var relatedEndMock = CreateMockRelatedEnd();

                        var objectContext = Objects.MockHelper.CreateMockObjectContext<DbDataRecord>();
                        relatedEndMock.Setup(m => m.ObjectContext).Returns(objectContext);
                        relatedEndMock.Setup(m => m.RelationshipSet).Returns(() => null);
                        relatedEndMock.Setup(m => m.FromEndMember).Returns(() => null);
                        relatedEndMock.Setup(m => m.ToEndMember).Returns(() => null);

                        var entityWrapper = CreateMockEntityWrapper(null).Object;
                        relatedEndMock.Setup(m => m.WrappedOwner).Returns(entityWrapper);

                        relatedEndMock.Setup(m => m.CheckOwnerNull());

                        var objectStateManagerMock = Mock.Get(objectContext.ObjectStateManager);

                        var timesUpdateRelationshipsCalled = 0;
                        objectStateManagerMock.Setup(
                            m => m.UpdateRelationships(
                                It.IsAny<ObjectContext>(),
                                It.IsAny<MergeOption>(),
                                It.IsAny<AssociationSet>(),
                                It.IsAny<AssociationEndMember>(),
                                It.IsAny<IEntityWrapper>(),
                                It.IsAny<AssociationEndMember>(),
                                It.IsAny<IList>(),
                                It.IsAny<bool>()))
                            .Returns(
                                (ObjectContext actualContext,
                                    MergeOption actualMergeOption,
                                    AssociationSet actualAssociationSet,
                                    AssociationEndMember actualSourceMember,
                                    IEntityWrapper actualWrappedSource,
                                    AssociationEndMember actualTargetMember,
                                    IList actualTargets,
                                    bool actualSetIsLoaded) =>
                                    {
                                        timesUpdateRelationshipsCalled++;
                                        Assert.Same(objectContext, actualContext);
                                        Assert.Equal(mergeOption, actualMergeOption);
                                        Assert.Null(actualAssociationSet);
                                        Assert.Null(actualSourceMember);
                                        Assert.Same(entityWrapper, actualWrappedSource);
                                        Assert.Null(actualTargetMember);
                                        Assert.Same(collection, actualTargets);
                                        Assert.Equal(isLoadedValue, actualSetIsLoaded);

                                        return 0;
                                    });

                        var relatedEnd = relatedEndMock.Object;

                        Assert.False(relatedEnd.IsLoaded);
                        relatedEndMock.Object.Merge<object>(collection, mergeOption, isLoadedValue);
                        Assert.Equal(isLoadedValue, relatedEnd.IsLoaded);

                        Assert.True(
                            1 == timesUpdateRelationshipsCalled,
                            "Expected UpdateRelationships to be called once for MergeOption." + mergeOption);
                    }
                }
            }

            private static Mock<IEntityWrapper> CreateMockEntityWrapper(object entity)
            {
                var mockEntityWrapper = new Mock<IEntityWrapper>(MockBehavior.Strict);
                mockEntityWrapper.Setup(m => m.Entity).Returns(entity);

                var entityKey = new EntityKey(qualifiedEntitySetName: "entityContainerName.entitySetName");
                mockEntityWrapper.Setup(m => m.EntityKey).Returns(entityKey);

                return mockEntityWrapper;
            }

            private static Mock<EntityWrapperFactory> CreateMockEntityWrapperFactory()
            {
                var mockEntityWrapperFactory = new Mock<EntityWrapperFactory>(MockBehavior.Strict);
                EntityEntry existingEntry = null;
                mockEntityWrapperFactory
                    .Setup(
                        m =>
                        m.WrapEntityUsingStateManagerGettingEntry(It.IsAny<object>(), It.IsAny<ObjectStateManager>(), out existingEntry))
                    .Returns(
                        (object entity, ObjectStateManager stateManager, EntityEntry actualExistingEntry) => { return CreateMockEntityWrapper(entity).Object; });

                return mockEntityWrapperFactory;
            }

            private static Mock<RelatedEnd> CreateMockRelatedEnd()
            {
                var relatedEndMock = new Mock<RelatedEnd>
                                         {
                                             CallBase = true
                                         };

                var associationType = new AssociationType(
                    name: "associationName",
                    namespaceName: "associationNamespace",
                    foreignKey: true,
                    dataSpace: DataSpace.CSpace);
                var associationSet = new AssociationSet(name: "associationSetName", associationType: associationType);

                var refType =
                    new RefType(new EntityType(name: "entityTypeName", namespaceName: "entityTypeNamespace", dataSpace: DataSpace.CSpace));
                var toEndMember = new AssociationEndMember(
                    name: "toEndMember",
                    endRefType: refType,
                    multiplicity: RelationshipMultiplicity.Many);

                associationSet.AddAssociationSetEnd(new AssociationSetEnd(new EntitySet(), associationSet, toEndMember));
                relatedEndMock.Setup(m => m.RelationshipSet).Returns(associationSet);
                relatedEndMock.Setup(m => m.TargetRoleName).Returns("toEndMember");

                relatedEndMock.Setup(m => m.EntityWrapperFactory).Returns(CreateMockEntityWrapperFactory().Object);

                return relatedEndMock;
            }
        }
    }
}
