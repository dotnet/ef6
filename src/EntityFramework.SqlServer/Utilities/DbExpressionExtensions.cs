// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DbExpressionExtensions
    {
        /// <summary>
        /// Uses a stack to non-recursively traverse a given tree structure and retrieve the leaf nodes.
        /// </summary>
        /// <param name="root">The node that represents the root of the tree.</param>
        /// <param name="kind">Expressions not of this kind are considered leaves.</param>
        /// <param name="getChildNodes">A function that traverses the tree by retrieving the <b>immediate</b>
        /// descendants of a (non-leaf) node.</param>
        /// <returns>An enumerable containing the leaf nodes.</returns>
        public static IEnumerable<DbExpression> GetLeafNodes(
            this DbExpression root,
            DbExpressionKind kind,
            Func<DbExpression, IEnumerable<DbExpression>> getChildNodes)
        {
            Contract.Requires(getChildNodes != null);

            var nodes = new Stack<DbExpression>();
            nodes.Push(root);

            while (nodes.Count > 0)
            {
                var current = nodes.Pop();
                if (current.ExpressionKind != kind)
                {
                    yield return current;
                }
                else
                {
                    foreach (var node in getChildNodes(current).Reverse())
                    {
                        nodes.Push(node);
                    }
                }
            }
        }
    }
}
