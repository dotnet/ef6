// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    using System.Data.Entity.Resources;

    /// <summary>
    /// Represents an ast node for namespace import (using nsABC;)
    /// </summary>
    internal sealed class NamespaceImport : Node
    {
        private readonly Identifier _namespaceAlias;
        private readonly Node _namespaceName;

        /// <summary>
        /// Initializes a single name import.
        /// </summary>
        internal NamespaceImport(Identifier idenitifier)
        {
            _namespaceName = idenitifier;
        }

        /// <summary>
        /// Initializes a single name import.
        /// </summary>
        internal NamespaceImport(DotExpr dorExpr)
        {
            _namespaceName = dorExpr;
        }

        /// <summary>
        /// Initializes aliased import.
        /// </summary>
        internal NamespaceImport(BuiltInExpr bltInExpr)
        {
            _namespaceAlias = null;

            var aliasId = bltInExpr.Arg1 as Identifier;
            if (aliasId == null)
            {
                var errCtx = bltInExpr.Arg1.ErrCtx;
                var message = Strings.InvalidNamespaceAlias;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            _namespaceAlias = aliasId;
            _namespaceName = bltInExpr.Arg2;
        }

        /// <summary>
        /// Returns ns alias id if exists.
        /// </summary>
        internal Identifier Alias
        {
            get { return _namespaceAlias; }
        }

        /// <summary>
        /// Returns namespace name.
        /// </summary>
        internal Node NamespaceName
        {
            get { return _namespaceName; }
        }
    }
}
