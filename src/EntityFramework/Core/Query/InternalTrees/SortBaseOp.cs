// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Base type for SortOp and ConstrainedSortOp
    /// </summary>
    internal abstract class SortBaseOp : RelOp
    {
        #region private state

        private readonly List<SortKey> m_keys;

        #endregion

        #region Constructors

        // Pattern constructor
        internal SortBaseOp(OpType opType)
            : base(opType)
        {
            Debug.Assert(opType == OpType.Sort || opType == OpType.ConstrainedSort, "SortBaseOp OpType must be Sort or ConstrainedSort");
        }

        internal SortBaseOp(OpType opType, List<SortKey> sortKeys)
            : this(opType)
        {
            m_keys = sortKeys;
        }

        #endregion

        /// <summary>
        /// Sort keys
        /// </summary>
        internal List<SortKey> Keys
        {
            get { return m_keys; }
        }
    }
}
