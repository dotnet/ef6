// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Typed ShaperFactory
    /// </summary>
    internal class ShaperFactory<T> : ShaperFactory
    {
        private readonly int _stateCount;
        private readonly CoordinatorFactory<T> _rootCoordinatorFactory;

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification = "Used in the debug build")]
        private readonly MergeOption _mergeOption;

        internal ShaperFactory(
            int stateCount, CoordinatorFactory<T> rootCoordinatorFactory,  Type[] columnTypes, bool[] nullableColumns, MergeOption mergeOption)
        {
            _stateCount = stateCount;
            _rootCoordinatorFactory = rootCoordinatorFactory;
            ColumnTypes = columnTypes;
            NullableColumns = nullableColumns;
            _mergeOption = mergeOption;
        }

        public Type[] ColumnTypes { get; private set; }
        public bool[] NullableColumns { get; private set; }

        /// <summary>
        /// Factory method to create the Shaper for Object Layer queries.
        /// </summary>
        internal Shaper<T> Create(
            DbDataReader reader, ObjectContext context, MetadataWorkspace workspace, MergeOption mergeOption,
            bool readerOwned, bool streaming)
        {
            Debug.Assert(
                mergeOption == _mergeOption, "executing a query with a different mergeOption than was used to compile the delegate");
            return new Shaper<T>(
                reader, context, workspace, mergeOption, _stateCount, _rootCoordinatorFactory, readerOwned, streaming);
        }
    }
}
