namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;

    /// <summary>
    /// Base class for Nest operations
    /// </summary>
    internal abstract class NestBaseOp : PhysicalOp
    {
        #region publics

        /// <summary>
        /// (Ordered) list of prefix sort keys (defines ordering of results)
        /// </summary>
        internal List<SortKey> PrefixSortKeys
        {
            get { return m_prefixSortKeys; }
        }

        /// <summary>
        /// Outputs of the NestOp. Includes the Keys obviously, and one Var for each of
        /// the collections produced. In addition, this may also include non-key vars
        /// from the outer row
        /// </summary>
        internal VarVec Outputs
        {
            get { return m_outputs; }
        }

        /// <summary>
        /// Information about each collection managed by the NestOp
        /// </summary>
        internal List<CollectionInfo> CollectionInfo
        {
            get { return m_collectionInfoList; }
        }

        #endregion

        #region constructors

        internal NestBaseOp(
            OpType opType, List<SortKey> prefixSortKeys,
            VarVec outputVars,
            List<CollectionInfo> collectionInfoList)
            : base(opType)
        {
            m_outputs = outputVars;
            m_collectionInfoList = collectionInfoList;
            m_prefixSortKeys = prefixSortKeys;
        }

        #endregion

        #region private state

        private readonly List<SortKey> m_prefixSortKeys; // list of sort key prefixes
        private readonly VarVec m_outputs; // list of all output vars
        private readonly List<CollectionInfo> m_collectionInfoList;

        #endregion
    }
}
