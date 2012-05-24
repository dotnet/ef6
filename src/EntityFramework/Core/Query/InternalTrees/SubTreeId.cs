namespace System.Data.Entity.Core.Query.InternalTrees
{
    internal class SubTreeId
    {
        #region private state

        public Node m_subTreeRoot;
        private readonly int m_hashCode;
        private readonly Node m_parent;
        private readonly int m_childIndex;

        #endregion

        #region constructors

        internal SubTreeId(RuleProcessingContext context, Node node, Node parent, int childIndex)
        {
            m_subTreeRoot = node;
            m_parent = parent;
            m_childIndex = childIndex;
            m_hashCode = context.GetHashCode(node);
        }

        #endregion

        #region public surface

        public override int GetHashCode()
        {
            return m_hashCode;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SubTreeId;
            return ((other != null) && (m_hashCode == other.m_hashCode) &&
                    ((other.m_subTreeRoot == m_subTreeRoot) ||
                     ((other.m_parent == m_parent) && (other.m_childIndex == m_childIndex))));
        }

        #endregion
    }
}
