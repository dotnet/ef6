namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Core.Query.InternalTrees;
    using Moq;
    using Xunit;

    public class TranslatorTests
    {
        [Fact]
        public void Static_TranslateColumnMap_calls_instance_method()
        {
            var typeUsageMock = new Mock<TypeUsage>();

            var expectedShaperFactory = new ShaperFactory<object>(0, Objects.MockHelper.CreateCoordinatorFactory<object>(), null,
                MergeOption.AppendOnly);

            var translatorMock = new Mock<Translator>();
            translatorMock.Setup(m => m.TranslateColumnMap<object>(
                    It.IsAny<QueryCacheManager>(), It.IsAny<ColumnMap>(), It.IsAny<MetadataWorkspace>(),
                    It.IsAny<SpanIndex>(), It.IsAny<MergeOption>(), It.IsAny<bool>())).Returns(expectedShaperFactory);

            var actualShaperFactory = Translator.TranslateColumnMap(
                translatorMock.Object, typeof(object), QueryCacheManager.Create(),
                new ScalarColumnMap(typeUsageMock.Object, null, 0, 0), new MetadataWorkspace(),
                new SpanIndex(), MergeOption.AppendOnly, valueLayer: true);

            translatorMock.Verify(m => m.TranslateColumnMap<object>(It.IsAny<QueryCacheManager>(), It.IsAny<ColumnMap>(), It.IsAny<MetadataWorkspace>(),
                    It.IsAny<SpanIndex>(), It.IsAny<MergeOption>(), It.IsAny<bool>()), Times.Once());
            Assert.Same(expectedShaperFactory, actualShaperFactory);
        }
    }
}