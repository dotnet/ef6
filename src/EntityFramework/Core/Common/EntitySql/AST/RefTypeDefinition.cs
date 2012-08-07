// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    ///     Represents an ast node for a reference type definition.
    /// </summary>
    internal sealed class RefTypeDefinition : Node
    {
        private readonly Node _refTypeIdentifier;

        /// <summary>
        ///     Initializes reference type definition using the referenced type identifier.
        /// </summary>
        internal RefTypeDefinition(Node refTypeIdentifier)
        {
            _refTypeIdentifier = refTypeIdentifier;
        }

        /// <summary>
        ///     Returns referenced type identifier.
        /// </summary>
        internal Node RefTypeIdentifier
        {
            get { return _refTypeIdentifier; }
        }
    }
}
