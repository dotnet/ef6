namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Core.Query.InternalTrees;
    using Moq;

    internal static class MockHelper
    {
        public static Translator CreateRecordStateTranslator()
        {
            var shaperFactory = new ShaperFactory<RecordState>(2,
                Objects.MockHelper.CreateCoordinatorFactory<object, RecordState>(0, 0, 0, new CoordinatorFactory[0], new List<RecordState>()),
                null,
                MergeOption.NoTracking);

            var translatorMock = new Mock<Translator>();
            translatorMock.Setup(m => m.TranslateColumnMap<RecordState>(
                    It.IsAny<QueryCacheManager>(), It.IsAny<ColumnMap>(), It.IsAny<MetadataWorkspace>(),
                    It.IsAny<SpanIndex>(), It.IsAny<MergeOption>(), It.IsAny<bool>())).Returns(shaperFactory);

            return translatorMock.Object;
        }

        public static Translator CreateTranslator<T>() where T : class
        {
            var shaperFactory = new ShaperFactory<T>(1,
                Objects.MockHelper.CreateCoordinatorFactory<object, T>(0, 0, 0, new CoordinatorFactory[0], new List<T>()),
                null,
                MergeOption.NoTracking);

            var translatorMock = new Mock<Translator>();
            translatorMock.Setup(m => m.TranslateColumnMap<T>(
                    It.IsAny<QueryCacheManager>(), It.IsAny<ColumnMap>(), It.IsAny<MetadataWorkspace>(),
                    It.IsAny<SpanIndex>(), It.IsAny<MergeOption>(), It.IsAny<bool>())).Returns(shaperFactory);

            return translatorMock.Object;
        }
    }
}