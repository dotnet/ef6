// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    // <summary>
    // Abstract base class for nodes in normal form expressions, e.g. Conjunctive Normal Form
    // sentences.
    // </summary>
    // <typeparam name="T_Identifier"> Type of expression leaf term identifiers. </typeparam>
    internal abstract class NormalFormNode<T_Identifier>
    {
        private readonly BoolExpr<T_Identifier> _expr;

        // <summary>
        // Initialize a new normal form node representing the given expression. Caller must
        // ensure the expression is logically equivalent to the node.
        // </summary>
        // <param name="expr"> Expression logically equivalent to this node. </param>
        protected NormalFormNode(BoolExpr<T_Identifier> expr)
        {
            _expr = expr.Simplify();
        }

        // <summary>
        // Gets an expression that is logically equivalent to this node.
        // </summary>
        internal BoolExpr<T_Identifier> Expr
        {
            get { return _expr; }
        }

        // <summary>
        // Utility method for delegation that return the expression corresponding to a given
        // normal form node.
        // </summary>
        // <typeparam name="T_NormalFormNode"> Type of node </typeparam>
        // <param name="node"> Node to examine. </param>
        // <returns> Equivalent Boolean expression for the given node. </returns>
        protected static BoolExpr<T_Identifier> ExprSelector<T_NormalFormNode>(T_NormalFormNode node)
            where T_NormalFormNode : NormalFormNode<T_Identifier>
        {
            return node._expr;
        }
    }
}
