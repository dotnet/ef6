// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.CustomCode.Utils
{
    using Microsoft.VisualStudio.Modeling.Diagrams;

    /// <summary>
    ///     Helper extension method for NodeShape objects
    /// </summary>
    internal static class NodeShapeExtensions
    {
        /// <summary>
        /// Ensures that a <see cref="NodeShape"/> has a particular expanded or collapsed state
        /// </summary>
        /// <param name="nodeShape">the <see cref="NodeShape"/> on which to ensure the state</param>
        /// <param name="expanded">if true, ensure that nodeShape is expanded, if false esnure that it is collapsed</param>
        public static void EnsureExpandedState(this NodeShape nodeShape, bool expanded)
        {
            if (expanded)
            {
                if (!nodeShape.IsExpanded)
                {
                    using (var txn = nodeShape.Store.TransactionManager.BeginTransaction())
                    {
                        nodeShape.IsExpanded = true;
                        txn.Commit();
                    }
                }
            }
            else
            {
                if (nodeShape.IsExpanded)
                {
                    using (var txn = nodeShape.Store.TransactionManager.BeginTransaction())
                    {
                        nodeShape.IsExpanded = false;
                        txn.Commit();
                    }
                }
            }
        }
    }
}
