// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents having clause.
    /// </summary>
    internal sealed class HavingClause : Node
    {
        private readonly Node _havingExpr;
        private readonly uint _methodCallCount;

        /// <summary>
        /// Initializes having clause.
        /// </summary>
        internal HavingClause(Node havingExpr, uint methodCallCounter)
        {
            _havingExpr = havingExpr;
            _methodCallCount = methodCallCounter;
        }

        /// <summary>
        /// Returns having inner expression.
        /// </summary>
        internal Node HavingPredicate
        {
            get { return _havingExpr; }
        }

        /// <summary>
        /// True if predicate has method calls.
        /// </summary>
        internal bool HasMethodCall
        {
            get { return (_methodCallCount > 0); }
        }
    }
}
