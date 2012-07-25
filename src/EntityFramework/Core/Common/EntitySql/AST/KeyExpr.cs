// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents KEY(expr) expression.
    /// </summary>
    internal class KeyExpr : Node
    {
        private readonly Node _argExpr;

        /// <summary>
        /// Initializes KEY expression.
        /// </summary>
        internal KeyExpr(Node argExpr)
        {
            _argExpr = argExpr;
        }

        /// <summary>
        /// Returns KEY argument expression.
        /// </summary>
        internal Node ArgExpr
        {
            get { return _argExpr; }
        }
    }
}
