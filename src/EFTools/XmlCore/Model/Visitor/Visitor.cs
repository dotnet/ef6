// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Visitor
{
    using System.Collections.Generic;

    internal abstract class Visitor
    {
        internal abstract void Visit(IVisitable visitable);

        /// <summary>
        ///     Performs a breadth-first traversal across the metadata tree starting at "item".
        ///     The traversal calls Accept on each node.  Each node can override the Accept to
        ///     determine if they want to allow the Visitor to visit.  Their Accept method should
        ///     return the child nodes to be processed next.
        /// </summary>
        /// <param name="item"></param>
        internal void Traverse(IVisitable visitable)
        {
            // do a BFS across the metadata graph starting from item.
            var toVisit = new Queue<IVisitable>();
            toVisit.Enqueue(visitable);
            while (toVisit.Count > 0)
            {
                var i = toVisit.Dequeue();
                var newNodes = i.Accept(this);
                if (newNodes != null)
                {
                    foreach (var v in newNodes)
                    {
                        toVisit.Enqueue(v);
                    }
                }
            }
        }
    }
}
