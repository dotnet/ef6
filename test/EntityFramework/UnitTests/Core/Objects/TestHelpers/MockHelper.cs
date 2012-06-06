namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Core.Common.Internal.Materialization;
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
    }
}
