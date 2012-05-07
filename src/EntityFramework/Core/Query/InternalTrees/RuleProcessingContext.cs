namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// A RuleProcessingContext encapsulates information needed by various rules to process
    /// the query tree.
    /// </summary>
    internal abstract class RuleProcessingContext
    {
        #region public surface

        internal Command Command
        {
            get { return m_command; }
        }

        /// <summary>
        /// Callback function to be applied to a node before any rules are applied
        /// </summary>
        /// <param name="node">the node</param>
        internal virtual void PreProcess(Node node)
        {
        }

        /// <summary>
        /// Callback function to be applied to the subtree rooted at the given 
        /// node before any rules are applied
        /// </summary>
        /// <param name="node">the node that is the root of the subtree</param>
        internal virtual void PreProcessSubTree(Node node)
        {
        }

        /// <summary>
        /// Callback function to be applied on a node after a rule has been applied
        /// that has modified the node
        /// </summary>
        /// <param name="node">current node</param>
        /// <param name="rule">the rule that modified the node</param>
        internal virtual void PostProcess(Node node, Rule rule)
        {
        }

        /// <summary>
        /// Callback function to be applied to the subtree rooted at the given 
        /// node after any rules are applied
        /// </summary>
        /// <param name="node">the node that is the root of the subtree</param>
        internal virtual void PostProcessSubTree(Node node)
        {
        }

        /// <summary>
        /// Get the hashcode for this node - to ensure that we don't loop forever
        /// </summary>
        /// <param name="node">current node</param>
        /// <returns>int hashcode</returns>
        internal virtual int GetHashCode(Node node)
        {
            return node.GetHashCode();
        }

        #endregion

        #region constructors

        internal RuleProcessingContext(Command command)
        {
            m_command = command;
        }

        #endregion

        #region private state

        private readonly Command m_command;

        #endregion
    }
}