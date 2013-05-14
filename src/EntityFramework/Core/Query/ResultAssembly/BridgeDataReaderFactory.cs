// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.ResultAssembly
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.Internal.Materialization;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    internal class BridgeDataReaderFactory
    {
        private readonly Translator _translator;

        public BridgeDataReaderFactory(Translator translator = null)
        {
            _translator = translator ?? new Translator();
        }

        /// <summary>
        ///     The primary factory method to produce the BridgeDataReader; given a store data
        ///     reader and a column map, create the BridgeDataReader, hooking up the IteratorSources
        ///     and ResultColumn Hierarchy.  All construction of top level data readers go through
        ///     this method.
        /// </summary>
        /// <param name="storeDataReader"> </param>
        /// <param name="columnMap"> column map of the first result set </param>
        /// <param name="workspace"> </param>
        /// <param name="nextResultColumnMaps"> enumerable of the column maps for NextResult() calls. </param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        public virtual DbDataReader Create(
            DbDataReader storeDataReader, ColumnMap columnMap, MetadataWorkspace workspace, IEnumerable<ColumnMap> nextResultColumnMaps)
        {
            DebugCheck.NotNull(storeDataReader);
            DebugCheck.NotNull(columnMap);
            DebugCheck.NotNull(workspace);
            DebugCheck.NotNull(nextResultColumnMaps);

            var shaperInfo = CreateShaperInfo(storeDataReader, columnMap, workspace);
            DbDataReader result = new BridgeDataReader(
                shaperInfo.Key, shaperInfo.Value, /*depth:*/ 0,
                GetNextResultShaperInfo(storeDataReader, workspace, nextResultColumnMaps).GetEnumerator());
            return result;
        }

        private KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>> CreateShaperInfo(
            DbDataReader storeDataReader, ColumnMap columnMap, MetadataWorkspace workspace)
        {
            DebugCheck.NotNull(storeDataReader);
            DebugCheck.NotNull(columnMap);
            DebugCheck.NotNull(workspace);

            var shaperFactory = _translator.TranslateColumnMap<RecordState>(columnMap, workspace, null, MergeOption.NoTracking, valueLayer: true);
            var recordShaper = shaperFactory.Create(
                storeDataReader, null, workspace, MergeOption.NoTracking, readerOwned: true, useSpatialReader: true,
                shouldReleaseConnection: true);

            return new KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>(
                recordShaper, recordShaper.RootCoordinator.TypedCoordinatorFactory);
        }

        private IEnumerable<KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>> GetNextResultShaperInfo(
            DbDataReader storeDataReader, MetadataWorkspace workspace, IEnumerable<ColumnMap> nextResultColumnMaps)
        {
            // It is important to do this lazily as the storeDataReader will have advanced to the next result set
            // by the time this IEnumerable is advanced
            return nextResultColumnMaps.Select(nextResultColumnMap => CreateShaperInfo(storeDataReader, nextResultColumnMap, workspace));
        }
    }
}
