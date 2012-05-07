namespace System.Data.Entity.Core.Query.InternalTrees
{

    /// <summary>
    /// A visitor implementation that allows subtrees to be modified (in a bottom-up
    /// fashion)
    /// </summary>
    internal abstract class BasicOpVisitorOfNode : BasicOpVisitorOfT<Node>
    {
        #region visitor helpers

        /// <summary>
        /// Simply iterates over all children, and manages any updates 
        /// </summary>
        /// <param name="n">The current node</param>
        protected override void VisitChildren(Node n)
        {
            for (var i = 0; i < n.Children.Count; i++)
            {
                n.Children[i] = VisitNode(n.Children[i]);
            }
        }

        /// <summary>
        /// Simply iterates over all children, and manages any updates, but in reverse order
        /// </summary>
        /// <param name="n">The current node</param>
        protected override void VisitChildrenReverse(Node n)
        {
            for (var i = n.Children.Count - 1; i >= 0; i--)
            {
                n.Children[i] = VisitNode(n.Children[i]);
            }
        }

        /// <summary>
        /// A default processor for any node. Visits the children and returns itself unmodified.
        /// </summary>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially new node</returns>
        protected override Node VisitDefault(Node n)
        {
            VisitChildren(n);
            return n;
        }

        #endregion

        #region AncillaryOp Visitors

        /// <summary>
        /// A default processor for all AncillaryOps.
        /// 
        /// Allows new visitors to just override this to handle all AncillaryOps
        /// </summary>
        /// <param name="op">the AncillaryOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected override Node VisitAncillaryOpDefault(AncillaryOp op, Node n)
        {
            return VisitDefault(n);
        }

        #endregion

        #region PhysicalOp Visitors

        /// <summary>
        /// A default processor for all PhysicalOps.
        /// 
        /// Allows new visitors to just override this to handle all PhysicalOps
        /// </summary>
        /// <param name="op">the PhysicalOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected override Node VisitPhysicalOpDefault(PhysicalOp op, Node n)
        {
            return VisitDefault(n);
        }

        #endregion

        #region RelOp Visitors

        /// <summary>
        /// A default processor for all RelOps.
        /// 
        /// Allows new visitors to just override this to handle all RelOps
        /// </summary>
        /// <param name="op">the RelOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected override Node VisitRelOpDefault(RelOp op, Node n)
        {
            return VisitDefault(n);
        }

        #endregion

        #region ScalarOp Visitors

        /// <summary>
        /// A default processor for all ScalarOps.
        /// 
        /// Allows new visitors to just override this to handle all ScalarOps
        /// </summary>
        /// <param name="op">the ScalarOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially new node</returns>
        protected override Node VisitScalarOpDefault(ScalarOp op, Node n)
        {
            return VisitDefault(n);
        }

        #endregion
    }
}
