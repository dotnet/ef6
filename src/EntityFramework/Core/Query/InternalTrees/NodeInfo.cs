// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// The NodeInfo class represents additional information about a node in the tree.
    /// By default, this includes a set of external references for each node (ie) references
    /// to Vars that are not defined in the same subtree
    /// The NodeInfo class also includes a "hashValue" that is a hash value for the entire
    /// subtree rooted at this node
    /// NOTE: When adding a new member to track inforation, make sure to update the Clear method
    /// in this class to set that member to the default value.
    /// </summary>
    internal class NodeInfo
    {
        #region private state

        private readonly VarVec m_externalReferences;
        protected int m_hashValue; // hash value for the node

        #endregion

        #region constructors

        internal NodeInfo(Command cmd)
        {
            m_externalReferences = cmd.CreateVarVec();
        }

        #endregion

        #region public methods

        /// <summary>
        /// Clear out all information - usually used by a Recompute
        /// </summary>
        internal virtual void Clear()
        {
            m_externalReferences.Clear();
            m_hashValue = 0;
        }

        /// <summary>
        /// All external references from this node
        /// </summary>
        internal VarVec ExternalReferences
        {
            get { return m_externalReferences; }
        }

        /// <summary>
        /// Get the hash value for this nodeInfo
        /// </summary>
        internal int HashValue
        {
            get { return m_hashValue; }
        }

        /// <summary>
        /// Compute the hash value for a Vec
        /// </summary>
        internal static int GetHashValue(VarVec vec)
        {
            var hashValue = 0;
            foreach (var v in vec)
            {
                hashValue ^= v.GetHashCode();
            }
            return hashValue;
        }

        /// <summary>
        /// Computes the hash value for this node. The hash value is simply the
        /// local hash value for this node info added with the hash values of the child
        /// nodes
        /// </summary>
        /// <param name="cmd"> current command </param>
        /// <param name="n"> current node </param>
        internal virtual void ComputeHashValue(Command cmd, Node n)
        {
            m_hashValue = 0;
            foreach (var chi in n.Children)
            {
                var chiNodeInfo = cmd.GetNodeInfo(chi);
                m_hashValue ^= chiNodeInfo.HashValue;
            }

            m_hashValue = (m_hashValue << 4) ^ ((int)n.Op.OpType); // include the optype somehow
            // Now compute my local hash value
            m_hashValue = (m_hashValue << 4) ^ GetHashValue(m_externalReferences);
        }

        #endregion
    }
}
