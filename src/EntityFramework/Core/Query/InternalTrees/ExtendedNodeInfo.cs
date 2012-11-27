// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     An ExtendedNodeInfo class adds additional information to a standard NodeInfo.
    ///     This class is usually applicable only to RelOps and PhysicalOps.
    ///     The ExtendedNodeInfo class has in addition to the information maintained by NodeInfo
    ///     the following
    ///     - a set of local definitions
    ///     - a set of definitions
    ///     - a set of keys
    ///     - a set of non-nullable definitions
    ///     - a set of non-nullable definitions that are visible at this node
    ///     NOTE: When adding a new member to track inforation, make sure to update the Clear method
    ///     in this class to set that member to the default value.
    /// </summary>
    internal class ExtendedNodeInfo : NodeInfo
    {
        #region private

        private readonly VarVec m_localDefinitions;
        private readonly VarVec m_definitions;
        private readonly KeyVec m_keys;
        private readonly VarVec m_nonNullableDefinitions;
        private readonly VarVec m_nonNullableVisibleDefinitions;
        private RowCount m_minRows;
        private RowCount m_maxRows;

        #endregion

        #region constructors

        internal ExtendedNodeInfo(Command cmd)
            : base(cmd)
        {
            m_localDefinitions = cmd.CreateVarVec();
            m_definitions = cmd.CreateVarVec();
            m_nonNullableDefinitions = cmd.CreateVarVec();
            m_nonNullableVisibleDefinitions = cmd.CreateVarVec();
            m_keys = new KeyVec(cmd);
            m_minRows = RowCount.Zero;
            m_maxRows = RowCount.Unbounded;
        }

        #endregion

        #region public methods

        internal override void Clear()
        {
            base.Clear();
            m_definitions.Clear();
            m_localDefinitions.Clear();
            m_nonNullableDefinitions.Clear();
            m_nonNullableVisibleDefinitions.Clear();
            m_keys.Clear();
            m_minRows = RowCount.Zero;
            m_maxRows = RowCount.Unbounded;
        }

        /// <summary>
        ///     Compute the hash value for this node
        /// </summary>
        /// <param name="cmd"> </param>
        /// <param name="n"> </param>
        internal override void ComputeHashValue(Command cmd, Node n)
        {
            base.ComputeHashValue(cmd, n);
            m_hashValue = (m_hashValue << 4) ^ GetHashValue(Definitions);
            m_hashValue = (m_hashValue << 4) ^ GetHashValue(Keys.KeyVars);
            return;
        }

        /// <summary>
        ///     Definitions made specifically by this node
        /// </summary>
        internal VarVec LocalDefinitions
        {
            get { return m_localDefinitions; }
        }

        /// <summary>
        ///     All definitions visible as outputs of this node
        /// </summary>
        internal VarVec Definitions
        {
            get { return m_definitions; }
        }

        /// <summary>
        ///     The keys for this node
        /// </summary>
        internal KeyVec Keys
        {
            get { return m_keys; }
        }

        /// <summary>
        ///     The definitions of vars that are guaranteed to be non-nullable when output from this node
        /// </summary>
        internal VarVec NonNullableDefinitions
        {
            get { return m_nonNullableDefinitions; }
        }

        /// <summary>
        ///     The definitions that come from the rel-op inputs of this node that are guaranteed to be non-nullable
        /// </summary>
        internal VarVec NonNullableVisibleDefinitions
        {
            get { return m_nonNullableVisibleDefinitions; }
        }

        /// <summary>
        ///     Min number of rows returned from this node
        /// </summary>
        internal RowCount MinRows
        {
            get { return m_minRows; }
            set
            {
                m_minRows = value;
                ValidateRowCount();
            }
        }

        /// <summary>
        ///     Max rows returned from this node
        /// </summary>
        internal RowCount MaxRows
        {
            get { return m_maxRows; }
            set
            {
                m_maxRows = value;
                ValidateRowCount();
            }
        }

        /// <summary>
        ///     Set the rowcount for this node
        /// </summary>
        /// <param name="minRows"> min rows produced by this node </param>
        /// <param name="maxRows"> max rows produced by this node </param>
        internal void SetRowCount(RowCount minRows, RowCount maxRows)
        {
            m_minRows = minRows;
            m_maxRows = maxRows;
            ValidateRowCount();
        }

        /// <summary>
        ///     Initialize the rowcounts for this node from the source node
        /// </summary>
        /// <param name="source"> nodeinfo of source </param>
        internal void InitRowCountFrom(ExtendedNodeInfo source)
        {
            m_minRows = source.m_minRows;
            m_maxRows = source.m_maxRows;
        }

        #endregion

        #region private methods

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Conditional("DEBUG")]
        private void ValidateRowCount()
        {
            Debug.Assert(m_maxRows >= m_minRows, "MaxRows less than MinRows?");
        }

        #endregion
    }
}
