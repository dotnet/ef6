namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// An immutable type used to generate Shaper instances.
    /// </summary>
    internal abstract class ShaperFactory
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static ShaperFactory Create(
            Type elementType, QueryCacheManager cacheManager, ColumnMap columnMap, MetadataWorkspace metadata, SpanIndex spanInfo,
            MergeOption mergeOption, bool valueLayer)
        {
            var creator = (ShaperFactoryCreator)Activator.CreateInstance(typeof(TypedShaperFactoryCreator<>).MakeGenericType(elementType));
            return creator.TypedCreate(cacheManager, columnMap, metadata, spanInfo, mergeOption, valueLayer);
        }

        private abstract class ShaperFactoryCreator
        {
            internal abstract ShaperFactory TypedCreate(
                QueryCacheManager cacheManager, ColumnMap columnMap, MetadataWorkspace metadata, SpanIndex spanInfo, MergeOption mergeOption,
                bool valueLayer);
        }

        private sealed class TypedShaperFactoryCreator<T> : ShaperFactoryCreator
        {
            internal override ShaperFactory TypedCreate(
                QueryCacheManager cacheManager, ColumnMap columnMap, MetadataWorkspace metadata, SpanIndex spanInfo, MergeOption mergeOption,
                bool valueLayer)
            {
                return Translator.TranslateColumnMap<T>(cacheManager, columnMap, metadata, spanInfo, mergeOption, valueLayer);
            }
        }
    }
}
