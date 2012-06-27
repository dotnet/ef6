namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq.Expressions;
    using Moq;

    public static class MockHelper
    {
        internal static Mock<Shaper<T>> CreateShaperMock<T>()
        {
            return new Mock<Shaper<T>>(/*reader*/ null, /*context*/ null, /*workspace*/ null,
                    MergeOption.AppendOnly, /*stateCount*/ 1, /*rootCoordinatorFactory*/ CreateCoordinatorFactory<T>(),
                /*checkPermissions*/ null, /*readerOwned*/ false);
        }

        internal static CoordinatorFactory<T> CreateCoordinatorFactory<T>(Expression<Func<Shaper, T>> element = null)
        {
            if (element == null)
            {
                element = (shaper) => default(T);
            }
            return new CoordinatorFactory<T>(
                depth: 0,
                stateSlot: 0,
                hasData: null,
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
            if (typeof(TResult) == typeof(RecordState))
            {
                element = shaper => (shaper.Reader.IsDBNull(ordinal)
                                        ? ((RecordState)shaper.State[stateSlot + 1]).SetNullRecord()
                                        : ((RecordState)shaper.State[stateSlot + 1]).GatherData(shaper)) as TResult;

                var edmTypeMock = new Mock<EdmType>();
                edmTypeMock.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.SimpleType);

                recordStateFactories = new[] { new RecordStateFactory(
                    stateSlotNumber: stateSlot + 1,
                    columnCount: 1,
                    nestedRecordStateFactories: new RecordStateFactory[0],
                    dataRecordInfo: null,
                    gatherData: shaper => shaper.SetColumnValue(stateSlot + 1, 0, shaper.Reader.GetValue(ordinal)),
                    propertyNames: new string[0],
                    typeUsages: new []{ TypeUsage.Create(edmTypeMock.Object) },
                    isColumnNested: new[] { false })};
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
    }
}
