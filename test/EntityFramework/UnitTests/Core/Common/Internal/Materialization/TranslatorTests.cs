// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Spatial;
    using Moq;
    using Xunit;

    public class TranslatorTests
    {
        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(Translator.GenericTranslateColumnMap);
            Assert.NotNull(Translator.TranslatorVisitor.Translator_MultipleDiscriminatorPolymorphicColumnMapHelper);
            Assert.NotNull(Translator.TranslatorVisitor.Translator_TypedCreateInlineDelegate);
        }

        [Fact]
        public void GetGenericElementsMethod_returns_a_generic_MethodInfo()
        {
            Assert.NotNull(Translator.TranslatorVisitor.GetGenericElementsMethod(typeof(int)));
        }

        [Fact]
        public void Static_TranslateColumnMap_calls_instance_method()
        {
            var typeUsageMock = new Mock<TypeUsage>();

            var expectedShaperFactory = new ShaperFactory<object>(
                0, Objects.MockHelper.CreateCoordinatorFactory<object>(),
                new Type[0], new bool[0], MergeOption.AppendOnly);

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

        [Fact]
        public void TranslateColumnMap_returns_cached_result_for_streaming_queries()
        {
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(m => m.GetQueryCacheManager()).Returns(QueryCacheManager.Create());

            var collectionMap = BuildSimpleEntitySetColumnMap(metadataWorkspaceMock);

            var factory =
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: true, valueLayer: false);
            Assert.NotNull(factory);
            Assert.Null(factory.NullableColumns);
            Assert.Null(factory.ColumnTypes);
            Assert.Same(factory, 
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: true, valueLayer: false));
        }

        [Fact]
        public void TranslateColumnMap_returns_cached_result_for_buffering_queries()
        {
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(m => m.GetQueryCacheManager()).Returns(QueryCacheManager.Create());

            var collectionMap = BuildSimpleEntitySetColumnMap(metadataWorkspaceMock);

            var factory =
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: false, valueLayer: false);
            Assert.NotNull(factory);
            Assert.Equal(new[] { typeof(int), typeof(int) }, factory.ColumnTypes);
            Assert.Equal(new[] { true, false }, factory.NullableColumns);
            Assert.Same(factory,
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: false, valueLayer: false));
        }

        [Fact]
        public void TranslateColumnMap_does_not_return_buffering_cached_result_for_streaming_queries()
        {
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(m => m.GetQueryCacheManager()).Returns(QueryCacheManager.Create());

            var collectionMap = BuildSimpleEntitySetColumnMap(metadataWorkspaceMock);

            var factory =
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: true, valueLayer: false);
            Assert.NotNull(factory);
            Assert.NotSame(factory,
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: false, valueLayer: false));
        }

        public class SimpleEntity
        {
            public int Id { get; set; }
            public int Count { get; set; }
        }

        private SimpleCollectionColumnMap BuildSimpleEntitySetColumnMap(Mock<MetadataWorkspace> metadataWorkspaceMock, CodeFirstOSpaceTypeFactory codeFirstOSpaceTypeFactory = null)
        {
            var cSpaceEntityType = new EntityType(typeof(SimpleEntity).Name, "N", DataSpace.CSpace);

            var intTypeUsage = TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), new FacetValues{Nullable = false});
            cSpaceEntityType.AddMember(new EdmProperty("Id", intTypeUsage));
            cSpaceEntityType.AddMember(new EdmProperty("Count", intTypeUsage));

            var entityTypeUsage = TypeUsage.Create(cSpaceEntityType);
            var idScalarMap = new ScalarColumnMap(intTypeUsage, "Id", 0, 0);
            var entityMap = new EntityColumnMap(
                entityTypeUsage, "E", new[] { idScalarMap,
                new ScalarColumnMap(intTypeUsage, "Count", 0, 1)},
                new SimpleEntityIdentity(null, new SimpleColumnMap[] { idScalarMap }));
            var collectionMap = new SimpleCollectionColumnMap(
                entityTypeUsage, "MockCollectionType", entityMap, null, null);

            codeFirstOSpaceTypeFactory = codeFirstOSpaceTypeFactory ?? new CodeFirstOSpaceTypeFactory();
            var oSpaceEntityType = codeFirstOSpaceTypeFactory.TryCreateType(typeof(SimpleEntity), cSpaceEntityType);
            codeFirstOSpaceTypeFactory.CspaceToOspace.Add(cSpaceEntityType, oSpaceEntityType);

            metadataWorkspaceMock.Setup(m => m.GetItem<EdmType>(It.IsAny<string>(), DataSpace.OSpace))
                .Returns(oSpaceEntityType);
            
            return collectionMap;
        }

        public class ManyTypesEntity
        {
            public bool P1Bool { get; set; }
            public bool? P2NullableBool { get; set; }
            public byte[] P3ByteArray { get; set; }
            public TimeSpan P4Timespan { get; set; }
            public TimeSpan? P5NullableTimespan { get; set; }
            public DayOfWeek P6Enum { get; set; }
            public DayOfWeek? P7NullableEnum { get; set; }
            public DbGeography P8Geography { get; set; }
        }

        [Fact]
        public void TranslateColumnMap_returns_correct_columntypes_and_nullablecolumns_for_entity_types()
        {
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(m => m.GetQueryCacheManager()).Returns(QueryCacheManager.Create());

            var cSpaceEntityType = new EntityType(typeof(ManyTypesEntity).Name, "N", DataSpace.CSpace);
            cSpaceEntityType.AddMember(
                new EdmProperty("P1Bool",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Boolean), new FacetValues { Nullable = false })));
            cSpaceEntityType.AddMember(
                new EdmProperty("P2NullableBool",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Boolean), new FacetValues { Nullable = true })));
            cSpaceEntityType.AddMember(
                new EdmProperty("P3ByteArray",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary))));
            cSpaceEntityType.AddMember(
                new EdmProperty("P4Timespan",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Time), new FacetValues { Nullable = false })));
            cSpaceEntityType.AddMember(
                new EdmProperty("P5NullableTimespan",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Time), new FacetValues { Nullable = true })));
            var enumType = new EnumType(
                "DayOfWeek", "N", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), false, DataSpace.CSpace);
            cSpaceEntityType.AddMember(
                new EdmProperty("P6Enum", TypeUsage.Create(enumType, new FacetValues { Nullable = false })));
            cSpaceEntityType.AddMember(
                new EdmProperty("P7NullableEnum", TypeUsage.Create(enumType, new FacetValues { Nullable = true })));
            var entityTypeUsage = TypeUsage.Create(cSpaceEntityType);
            cSpaceEntityType.AddMember(
                new EdmProperty("P8Geography",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Geography))));

            var oSpaceTypeFactory = new CodeFirstOSpaceTypeFactory();
            var oSpaceEntityType = oSpaceTypeFactory.TryCreateType(typeof(ManyTypesEntity), cSpaceEntityType);
            oSpaceTypeFactory.CspaceToOspace.Add(enumType, oSpaceTypeFactory.TryCreateType(typeof(DayOfWeek), enumType));
            foreach (var resolve in oSpaceTypeFactory.ReferenceResolutions)
            {
                resolve();
            }

            var scalarMaps = new List<ScalarColumnMap>();
            foreach (var edmProperty in cSpaceEntityType.Properties)
            {
                scalarMaps.Add(new ScalarColumnMap(edmProperty.TypeUsage, edmProperty.Name, 0, scalarMaps.Count));
            }
            var entityMap = new EntityColumnMap(
                entityTypeUsage, "E", scalarMaps.ToArray(),
                new SimpleEntityIdentity(null, new SimpleColumnMap[] { scalarMaps[0] }));
            var collectionMap = new SimpleCollectionColumnMap(
                entityTypeUsage, "MockCollectionType", entityMap, null, null);

            metadataWorkspaceMock.Setup(m => m.GetItem<EdmType>(It.IsAny<string>(), DataSpace.OSpace))
                .Returns(oSpaceEntityType);

            var factory =
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: false, valueLayer: false);
            Assert.NotNull(factory);

            Assert.Equal(
                new[] { typeof(bool), typeof(bool), typeof(byte[]), typeof(TimeSpan), typeof(TimeSpan), typeof(int), typeof(int), typeof(DbGeography) },
                factory.ColumnTypes);
            // The first column is nullable as it's part of the key
            Assert.Equal(new[] { true, true, true, false, true, false, true, true }, factory.NullableColumns);
        }

        [Fact]
        public void TranslateColumnMap_returns_correct_columntypes_and_nullablecolumns_for_anonymous_types()
        {
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(m => m.GetQueryCacheManager()).Returns(QueryCacheManager.Create());
            var edmProperties = new []
                {
                    new EdmProperty("P1Int", TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32),
                        new FacetValues { Nullable = false })),
                    new EdmProperty("P2Bool", TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Boolean)))
                };

            var cSpaceEntityType = new RowType(edmProperties);
            var entityTypeUsage = TypeUsage.Create(cSpaceEntityType);
            var recordMap = new RecordColumnMap(
                entityTypeUsage, "E",
                new[] { new ScalarColumnMap(cSpaceEntityType.Properties[0].TypeUsage, cSpaceEntityType.Properties[0].Name, 0, 0) },
                new ScalarColumnMap(cSpaceEntityType.Properties[1].TypeUsage, cSpaceEntityType.Properties[1].Name, 0, 1));
            var collectionMap = new SimpleCollectionColumnMap(
                entityTypeUsage, "MockCollectionType", recordMap, null, null);

            var factory =
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: false, valueLayer: false);
            Assert.NotNull(factory);

            Assert.Equal(new[] { typeof(int), null }, factory.ColumnTypes);
            Assert.Equal(new[] { true, true }, factory.NullableColumns);
        }

        [Fact]
        public void TranslateColumnMap_returns_correct_columntypes_and_nullablecolumns_for_discriminated_types()
        {
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(m => m.GetQueryCacheManager()).Returns(QueryCacheManager.Create());
            
            var typeChoices = new Dictionary<object, TypedColumnMap>();
            var entityColumnMap = (EntityColumnMap)BuildSimpleEntitySetColumnMap(metadataWorkspaceMock).Element;

            typeChoices.Add(true, entityColumnMap);
            var recordMap = new SimplePolymorphicColumnMap(
                entityColumnMap.Type, "E", new ColumnMap[0],
                new ScalarColumnMap(TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Boolean)), "discriminator", 0, 2),
                typeChoices);
            var collectionMap = new SimpleCollectionColumnMap(
                entityColumnMap.Type, "MockCollectionType", recordMap, null, null);

            var factory =
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: false, valueLayer: false);
            Assert.NotNull(factory);

            Assert.Equal(new[] { typeof(int), typeof(int), typeof(bool) }, factory.ColumnTypes);
            Assert.Equal(new[] { true, true, true }, factory.NullableColumns);
        }

        [Fact]
        public void TranslateColumnMap_returns_correct_columntypes_and_nullablecolumns_for_discriminated_collections()
        {
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(m => m.GetQueryCacheManager()).Returns(QueryCacheManager.Create());

            var entityColumnMap = (EntityColumnMap)BuildSimpleEntitySetColumnMap(metadataWorkspaceMock).Element;

            var collectionMap = new DiscriminatedCollectionColumnMap(
                entityColumnMap.Type, "MockCollectionType", entityColumnMap, null, null,
                new ScalarColumnMap(TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Boolean)), "discriminator", 0, 2),
                true);

            var factory =
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: false, valueLayer: false);
            Assert.NotNull(factory);

            Assert.Equal(new[] { typeof(int), typeof(int), typeof(bool) }, factory.ColumnTypes);
            Assert.Equal(new[] { true, true, true }, factory.NullableColumns);
        }

        [Fact]
        public void TranslateColumnMap_returns_correct_columntypes_and_nullablecolumns_for_complex_types()
        {
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(m => m.GetQueryCacheManager()).Returns(QueryCacheManager.Create());

            var cSpaceComplexType = new ComplexType("C");
            var intTypeUsage = TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32), new FacetValues { Nullable = false });
            cSpaceComplexType.AddMember(new EdmProperty("Id", intTypeUsage));
            cSpaceComplexType.AddMember(new EdmProperty("Count", intTypeUsage));

            var complexTypeUsage = TypeUsage.Create(cSpaceComplexType);
            var recordMap = new ComplexTypeColumnMap(
                complexTypeUsage, "E",
                new[] { new ScalarColumnMap(cSpaceComplexType.Properties[0].TypeUsage, cSpaceComplexType.Properties[0].Name, 0, 0) },
                new ScalarColumnMap(cSpaceComplexType.Properties[1].TypeUsage, cSpaceComplexType.Properties[1].Name, 0, 1));
            var collectionMap = new SimpleCollectionColumnMap(
                complexTypeUsage, "MockCollectionType", recordMap, null, null);
            
            metadataWorkspaceMock.Setup(m => m.GetItem<EdmType>(It.IsAny<string>(), DataSpace.OSpace))
                .Returns(new CodeFirstOSpaceTypeFactory().TryCreateType(typeof(SimpleEntity), cSpaceComplexType));

            var factory =
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: false, valueLayer: false);
            Assert.NotNull(factory);

            Assert.Equal(new[] { typeof(int), null }, factory.ColumnTypes);
            Assert.Equal(new[] { true, true }, factory.NullableColumns);
        }

        public class RefEntity
        {
            public int Id { get; set; }
            public SimpleEntity SimpleEntity { get; set; }
        }

        [Fact]
        public void TranslateColumnMap_returns_correct_columntypes_and_nullablecolumns_for_associations()
        {
            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(m => m.GetQueryCacheManager()).Returns(QueryCacheManager.Create());

            var codeFirstOSpaceTypeFactory = new CodeFirstOSpaceTypeFactory();
            var refEntityColumnMap = (EntityColumnMap)BuildSimpleEntitySetColumnMap(metadataWorkspaceMock, codeFirstOSpaceTypeFactory).Element;
            
            var cSpaceEntityType = new EntityType(typeof(RefEntity).Name, "N", DataSpace.CSpace);
            cSpaceEntityType.AddMember(new EdmProperty("Id", TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32))));

            var navigationProperty = new NavigationProperty("SimpleEntity", TypeUsage.Create(refEntityColumnMap.Type.EdmType));
            var associationType = new AssociationType("A", "N", false, DataSpace.CSpace);
            associationType.AddMember(
                new AssociationEndMember("From", new RefType(cSpaceEntityType), RelationshipMultiplicity.One));
            associationType.AddMember(
                new AssociationEndMember("To", new RefType((EntityType)navigationProperty.TypeUsage.EdmType), RelationshipMultiplicity.One));
            associationType.SetReadOnly();

            navigationProperty.RelationshipType = associationType;
            navigationProperty.FromEndMember = associationType.RelationshipEndMembers[0];
            navigationProperty.ToEndMember = associationType.RelationshipEndMembers[1];
            cSpaceEntityType.AddMember(navigationProperty);
            var entityTypeUsage = TypeUsage.Create(cSpaceEntityType);

            var oSpaceEntityType = codeFirstOSpaceTypeFactory.TryCreateType(typeof(RefEntity), cSpaceEntityType);
            codeFirstOSpaceTypeFactory.CspaceToOspace.Add(cSpaceEntityType, oSpaceEntityType);

            var associations = new EdmItemCollection();
            associations.AddInternal(associationType);

            codeFirstOSpaceTypeFactory.CreateRelationships(associations);
            foreach (var resolve in codeFirstOSpaceTypeFactory.ReferenceResolutions)
            {
                resolve();
            }

            var idScalarMap = new ScalarColumnMap(TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)), "Id", 0, 1);
            var refColumnMap = new RefColumnMap(
                associationType.RelationshipEndMembers[1].TypeUsage, "E",
                new SimpleEntityIdentity(null, new SimpleColumnMap[] { idScalarMap }));
            var collectionMap = new SimpleCollectionColumnMap(
                entityTypeUsage, "MockCollectionType", refColumnMap, null, null);

            metadataWorkspaceMock.Setup(m => m.GetItem<EdmType>(It.IsAny<string>(), DataSpace.OSpace))
                .Returns(oSpaceEntityType);

            var factory =
                new Translator().TranslateColumnMap<object>(
                    collectionMap, metadataWorkspaceMock.Object, new SpanIndex(), MergeOption.NoTracking, streaming: false, valueLayer: false);
            Assert.NotNull(factory);

            Assert.Equal(new[] { null, typeof(object) }, factory.ColumnTypes);
            Assert.Equal(new[] { false, true }, factory.NullableColumns);
        }
    }
}
