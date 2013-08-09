// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
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

            var expectedShaperFactory = new ShaperFactory<object>(
                0, Objects.MockHelper.CreateCoordinatorFactory<object>(),
                MergeOption.AppendOnly);

            var translatorMock = new Mock<Translator>();
            translatorMock.Setup(
                m => m.TranslateColumnMap<object>(
                    It.IsAny<ColumnMap>(), It.IsAny<MetadataWorkspace>(),
                    It.IsAny<SpanIndex>(), It.IsAny<MergeOption>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(expectedShaperFactory);

            var actualShaperFactory = Translator.TranslateColumnMap(
                translatorMock.Object, typeof(object), 
                new ScalarColumnMap(typeUsageMock.Object, null, 0, 0), new MetadataWorkspace(),
                new SpanIndex(), MergeOption.AppendOnly, streaming: false, valueLayer: true);

            translatorMock.Verify(
                m => m.TranslateColumnMap<object>(
                    It.IsAny<ColumnMap>(), It.IsAny<MetadataWorkspace>(),
                    It.IsAny<SpanIndex>(), It.IsAny<MergeOption>(), false, true), Times.Once());
            Assert.Same(expectedShaperFactory, actualShaperFactory);
        }

        [Fact]
        public void TranslateColumnMap_with_MultipleDiscriminatorPolymorphicColumnMap_returns_a_ShaperFactory()
        {
            var polymorphicMap = new MultipleDiscriminatorPolymorphicColumnMap(
                new Mock<TypeUsage>().Object, "MockType", new ColumnMap[0], new SimpleColumnMap[0],
                new Dictionary<EntityType, TypedColumnMap>(),
                discriminatorValues => new EntityType("E", "N", DataSpace.CSpace));
            CollectionColumnMap collection = new SimpleCollectionColumnMap(
                new Mock<TypeUsage>().Object, "MockCollectionType", polymorphicMap, null, null);

            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(m => m.GetQueryCacheManager()).Returns(QueryCacheManager.Create());

            Assert.NotNull(
                new Translator().TranslateColumnMap<object>(
                    collection, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: true, valueLayer: false));
        }
    }
}
