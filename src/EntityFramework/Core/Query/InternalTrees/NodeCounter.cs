// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


namespace System.Data.Entity.Core.Query.InternalTrees
{
    // <summary>
    // Counts the number of nodes in a tree
    // </summary>
    internal class NodeCounter : BasicOpVisitorOfT<int>
    {
        // <summary>
        // Public entry point - Calculates the nubmer of nodes in the given subTree
        // </summary>
        internal static int Count(Node subTree)
        {
            var counter = new NodeCounter();
            return counter.VisitNode(subTree);
        }

        // <summary>
        // Common processing for all node types
        // Count = 1 (self) + count of children
        // </summary>
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
