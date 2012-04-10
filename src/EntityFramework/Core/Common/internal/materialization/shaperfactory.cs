using System.Data.Entity.Core.Common.QueryCache;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// An immutable type used to generate Shaper instances.
    /// </summary>
    internal abstract class ShaperFactory
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static ShaperFactory Create(Type elementType, QueryCacheManager cacheManager, ColumnMap columnMap, MetadataWorkspace metadata, SpanIndex spanInfo, MergeOption mergeOption, bool valueLayer)
        {
            ShaperFactoryCreator creator = (ShaperFactoryCreator)Activator.CreateInstance(typeof(TypedShaperFactoryCreator<>).MakeGenericType(elementType));
            return creator.TypedCreate(cacheManager, columnMap, metadata, spanInfo, mergeOption, valueLayer);
        }

        private abstract class ShaperFactoryCreator
        {
            internal abstract ShaperFactory TypedCreate(QueryCacheManager cacheManager, ColumnMap columnMap, MetadataWorkspace metadata, SpanIndex spanInfo, MergeOption mergeOption, bool valueLayer);
        }

        private sealed class TypedShaperFactoryCreator<T> : ShaperFactoryCreator
        {
            public TypedShaperFactoryCreator() {}
            internal override ShaperFactory TypedCreate(QueryCacheManager cacheManager, ColumnMap columnMap, MetadataWorkspace metadata, SpanIndex spanInfo, MergeOption mergeOption, bool valueLayer)
            {
                return Translator.TranslateColumnMap<T>(cacheManager, columnMap, metadata, spanInfo, mergeOption, valueLayer);
            }
        }
    }

    /// <summary>
    /// Typed ShaperFactory
    /// </summary>
    internal class ShaperFactory<T> : ShaperFactory
    {
        private readonly int _stateCount;
        private readonly CoordinatorFactory<T> _rootCoordinatorFactory;
        private readonly Action _checkPermissions;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private readonly MergeOption _mergeOption;

        internal ShaperFactory(int stateCount, CoordinatorFactory<T> rootCoordinatorFactory, Action checkPermissions, MergeOption mergeOption)
        {
            _stateCount = stateCount;
            _rootCoordinatorFactory = rootCoordinatorFactory;
            _checkPermissions = checkPermissions;
            _mergeOption = mergeOption;
        }

        /// <summary>
        /// Factory method to create the Shaper for Object Layer queries.
        /// </summary>
        internal Shaper<T> Create(DbDataReader reader, ObjectContext context, MetadataWorkspace workspace, MergeOption mergeOption, bool readerOwned)
        {
            Debug.Assert(mergeOption == _mergeOption, "executing a query with a different mergeOption than was used to compile the delegate");
            return new Shaper<T>(reader, context, workspace, mergeOption, _stateCount, _rootCoordinatorFactory, _checkPermissions, readerOwned);
        }
    }
}