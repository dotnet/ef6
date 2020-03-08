// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.SqlServer;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq.Protected;
#if !NET40
    using System.Threading;
    using System.Threading.Tasks;
#endif
    using Moq;

    public static class MockHelper
    {
        internal static Mock<Shaper<T>> CreateShaperMock<T>()
        {
            return new Mock<Shaper<T>>(
                /*reader*/ null, /*context*/ null, /*workspace*/ null,
                MergeOption.AppendOnly, /*stateCount*/ 1, /*rootCoordinatorFactory*/ CreateCoordinatorFactory<T>(),
                /*readerOwned*/ false, /*streaming*/ false);
        }

        internal static CoordinatorFactory<T> CreateCoordinatorFactory<T>(Expression<Func<Shaper, T>> element = null, Expression<Func<Shaper, bool>> hasData = null)
        {
            if (element == null)
            {
                element = (shaper) => default(T);
            }
            return new CoordinatorFactory<T>(
                depth: 0,
                stateSlot: 0,
                hasData: hasData,
                setKeys: null,
                checkKeys: null,
                nestedCoordinators: new CoordinatorFactory[0],
                element: element,
                wrappedElement: null,
                elementWithErrorHandling: element,
                initializeCollection: null,
                recordStateFactories: new RecordStateFactory[0]);
        }

        internal static CoordinatorFactory<TResult> CreateCoordinatorFactory<TKey, TResult>(
            int depth, int stateSlot, int ordinal, CoordinatorFactory[] nestedCoordinators, List<TResult> producedValues)
            where TResult : class
        {
            var recordStateFactories = new RecordStateFactory[0];
            Expression<Func<Shaper, TResult>> element;
            if (typeof(TResult)
                == typeof(RecordState))
            {
                element = shaper => (shaper.Reader.IsDBNull(ordinal)
                                         ? ((RecordState)shaper.State[stateSlot + 1]).SetNullRecord()
                                         : ((RecordState)shaper.State[stateSlot + 1]).GatherData(shaper)) as TResult;

                var edmTypeMock = new Mock<EdmType>();
                edmTypeMock.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.SimpleType);

                recordStateFactories = new[]
                                           {
                                               new RecordStateFactory(
                                                   stateSlotNumber: stateSlot + 1,
                                                   columnCount: 1,
                                                   nestedRecordStateFactories: new RecordStateFactory[0],
                                                   dataRecordInfo: null,
                                                   gatherData:
                                                   shaper => shaper.SetColumnValue(stateSlot + 1, 0, shaper.Reader.GetValue(ordinal)),
                                                   propertyNames: new string[0],
                                                   typeUsages: new[] { TypeUsage.Create(edmTypeMock.Object) },
                                                   isColumnNested: new[] { false })
                                           };
            }
            else
            {
                element = shaper => producedValues.AddAndReturn((TResult)shaper.Reader.GetValue(ordinal));
            }

            var keysStore = new HashSet<TKey>();
            return new CoordinatorFactory<TResult>(
                depth: depth,
                stateSlot: stateSlot,
                hasData: shaper => shaper.Reader.GetValue(ordinal) != null,
                setKeys: shaper => keysStore.Add((TKey)shaper.Reader.GetValue(ordinal)),
                checkKeys: shaper => keysStore.Contains((TKey)shaper.Reader.GetValue(ordinal)),
                nestedCoordinators: nestedCoordinators,
                element: element,
                wrappedElement: null,
                elementWithErrorHandling: element,
                initializeCollection: null,
                recordStateFactories: recordStateFactories);
        }

        internal static ObjectContext CreateMockObjectContext<TEntity>(EntityConnection entityConnection = null, IEntityAdapter entityAdapter = null)
        {
            if (entityConnection == null)
            {
                var dbConnectionMock = new Mock<DbConnection>();
                dbConnectionMock.Setup(m => m.DataSource).Returns("fakeDb");
                dbConnectionMock.Setup(m => m.ConnectionString).Returns("Data Source=fakeDb");
                dbConnectionMock.Protected().SetupGet<DbProviderFactory>("DbProviderFactory")
                    .Returns(GenericProviderFactory<DbProviderFactory>.Instance);
                var entityConnectionMock = new Mock<EntityConnection>();
                entityConnectionMock.SetupGet(m => m.StoreConnection).Returns(dbConnectionMock.Object);
                entityConnection = entityConnectionMock.Object;

                var state = ConnectionState.Closed;
                entityConnectionMock.Setup(m => m.Close()).Callback(() => { state = ConnectionState.Closed; });
                entityConnectionMock.Setup(m => m.Open()).Callback(
                    () =>
                    {
                        state = ConnectionState.Open;
                    });
                
#if !NET40
                entityConnectionMock.Setup(m => m.OpenAsync(It.IsAny<CancellationToken>())).Returns(
                    () =>
                        {
                            state = ConnectionState.Open;
                            return Task.FromResult(true);
                        });
#endif
                entityConnectionMock.Setup(m => m.State).Returns(() => state);
            }

            var objectContextMock = new Mock<ObjectContext>(null, null, null, entityAdapter)
                                        {
                                            CallBase = true
                                        };
            objectContextMock.Setup(m => m.Connection).Returns(entityConnection);

            var model = EdmModel.CreateStoreModel(
                new DbProviderInfo(GenericProviderFactory<DbProviderFactory>.Instance.InvariantProviderName, "2008"),
                new SqlProviderManifest("2008"));
            
            var storeItemCollectionMock = new Mock<StoreItemCollection>(model) { CallBase = true };
            
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(m => m.GetItemCollection(DataSpace.SSpace)).Returns(storeItemCollectionMock.Object);
            objectContextMock.Setup(m => m.MetadataWorkspace).Returns(metadataWorkspaceMock.Object);

            var objectStateManagerMock = new Mock<ObjectStateManager>(metadataWorkspaceMock.Object);
            objectContextMock.Setup(m => m.ObjectStateManager).Returns(objectStateManagerMock.Object);

            var mockObjectQuery = CreateMockObjectQuery(default(TEntity), objectContext: objectContextMock.Object);
            var mockObjectQueryProvider = new Mock<ObjectQueryProvider>(mockObjectQuery.Object);
            mockObjectQueryProvider.Setup(m => m.CreateQuery<TEntity>(It.IsAny<Expression>()))
                                   .Returns(mockObjectQuery.Object);
            mockObjectQueryProvider.Setup(m => m.CreateQuery(It.IsAny<Expression>(), typeof(TEntity)))
                                   .Returns(mockObjectQuery.Object);

            var fakeQueryable = new TEntity[0].AsQueryable();
            mockObjectQuery.Setup(m => m.GetEnumeratorInternal()).Returns(fakeQueryable.GetEnumerator);
            mockObjectQuery.Setup(m => m.GetExpression()).Returns(() => fakeQueryable.Expression);
            mockObjectQuery.Setup(m => m.ObjectQueryProvider).Returns(() => mockObjectQueryProvider.Object);

            objectContextMock.Setup(m => m.CreateQuery<TEntity>(It.IsAny<string>(), It.IsAny<ObjectParameter[]>())).Returns(
                () => mockObjectQuery.Object);

            return objectContextMock.Object;
        }

        internal static Mock<ObjectQuery<TEntity>> CreateMockObjectQuery<TEntity>(
            TEntity refreshedValue, Shaper<TEntity> shaper = null, ObjectContext objectContext = null)
        {
            return CreateMockObjectQuery(new[] { refreshedValue }, shaper, objectContext);
        }

        internal static Mock<ObjectQuery<TEntity>> CreateMockObjectQuery<TEntity>(
            IEnumerable<TEntity> refreshedValues, Shaper<TEntity> shaper = null, ObjectContext objectContext = null)
        {
            var shaperMock = CreateShaperMock<TEntity>();
            shaperMock.Setup(m => m.GetEnumerator()).Returns(
                () => new DbEnumeratorShim<TEntity>(refreshedValues.GetEnumerator()));
            shaper = shaper ?? shaperMock.Object;

            var objectResultMock = new Mock<ObjectResult<TEntity>>(shaper, null, null)
                                       {
                                           CallBase = true
                                       };
            
            objectContext = objectContext ?? CreateMockObjectContext<object>();
            var objectQueryStateMock = new Mock<ObjectQueryState>(typeof(TEntity), objectContext, /*parameters:*/ null, /*span:*/ null)
                                           {
                                               CallBase = true
                                           };

            var objectQueryExecutionPlanMock = new Mock<ObjectQueryExecutionPlan>(
                MockBehavior.Loose, null, null, null, MergeOption.NoTracking, false, null, null);
            objectQueryExecutionPlanMock.Setup(m => m.Execute<TEntity>(It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>()))
                                        .Returns(() => objectResultMock.Object);

#if !NET40
            objectQueryExecutionPlanMock.Setup(
                m => m.ExecuteAsync<TEntity>(
                    It.IsAny<ObjectContext>(), It.IsAny<ObjectParameterCollection>(),
                    It.IsAny<CancellationToken>()))
                                        .Returns(() => Task.FromResult(objectResultMock.Object));
#endif

            objectQueryStateMock.Setup(m => m.GetExecutionPlan(It.IsAny<MergeOption?>())).Returns(objectQueryExecutionPlanMock.Object);

            return new Mock<ObjectQuery<TEntity>>(objectQueryStateMock.Object)
                                      {
                                          CallBase = true
                                      };
        }
    }
}
