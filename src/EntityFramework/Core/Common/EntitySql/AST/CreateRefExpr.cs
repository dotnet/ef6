// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents CREATEREF(entitySet, keys) expression.
    /// </summary>
    internal sealed class CreateRefExpr : Node
    {
        private readonly Node _entitySet;
        private readonly Node _keys;
        private readonly Node _typeIdentifier;

        /// <summary>
        /// Initializes CreateRefExpr.
        /// </summary>
        /// <param name="entitySet"> expression representing the entity set </param>
        internal CreateRefExpr(Node entitySet, Node keys)
            : this(entitySet, keys, null)
        {
        }

        /// <summary>
        /// Initializes CreateRefExpr.
        /// </summary>
        internal CreateRefExpr(Node entitySet, Node keys, Node typeIdentifier)
        {
            _entitySet = entitySet;
            _keys = keys;
            _typeIdentifier = typeIdentifier;
        }

        /// <summary>
        /// Returns the expression for the entity set.
        /// </summary>
        internal Node EntitySet
        {
            get { return _entitySet; }
        }

        /// <summary>
        /// Returns the expression for the keys.
        /// </summary>
        internal Node Keys
        {
            get { return _keys; }
        }

        /// <summary>
        /// Gets optional typeidentifier. May be null.
        /// </summary>
        internal Node TypeIdentifier
        {
            get { return _typeIdentifier; }
        }
    }
}
