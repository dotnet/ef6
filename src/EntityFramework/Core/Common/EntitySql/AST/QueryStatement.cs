// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    // <summary>
    // Represents query statement AST.
    // </summary>
    internal sealed class QueryStatement : Statement
    {
        private readonly NodeList<FunctionDefinition> _functionDefList;
        private readonly Node _expr;

        // <summary>
        // Initializes query statement.
        // </summary>
        // <param name="functionDefList"> optional function definitions </param>
        // <param name="expr"> query top level expression </param>
        internal QueryStatement(NodeList<FunctionDefinition> functionDefList, Node expr)
        {
            _functionDefList = functionDefList;
            _expr = expr;
        }

        // <summary>
        // Returns optional function defintions. May be null.
        // </summary>
        internal NodeList<FunctionDefinition> FunctionDefList
        {
            get { return _functionDefList; }
        }

        // <summary>
        // Returns query top-level expression.
        // </summary>
        internal Node Expr
        {
            get { return _expr; }
        }
    }
}
