// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents REF(expr) expression.
    /// </summary>
    internal sealed class RefExpr : Node
    {
        private readonly Node _argExpr;

        /// <summary>
        /// Initializes REF expression node.
        /// </summary>
        internal RefExpr(Node refArgExpr)
        {
            _argExpr = refArgExpr;
        }

        /// <summary>
        /// Return ref argument expression.
        /// </summary>
        internal Node ArgExpr
        {
            get { return _argExpr; }
        }
    }
}
