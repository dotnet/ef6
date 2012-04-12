using md = System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// Counts the number of nodes in a tree
    /// </summary>
    internal class NodeCounter : BasicOpVisitorOfT<int>
    {
        /// <summary>
        /// Public entry point - Calculates the nubmer of nodes in the given subTree
        /// </summary>
        /// <param name="subTree"></param>
        /// <returns></returns>
        internal static int Count(Node subTree)
        {
            var counter = new NodeCounter();
            return counter.VisitNode(subTree);
        }

        /// <summary>
        /// Common processing for all node types
        /// Count = 1 (self) + count of children
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override int VisitDefault(Node n)
        {
            var count = 1;
            foreach (var child in n.Children)
            {
                count += VisitNode(child);
            }
            return count;
        }
    }
}
