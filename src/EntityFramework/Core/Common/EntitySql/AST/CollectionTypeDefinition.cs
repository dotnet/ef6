// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents an ast node for a collection type definition.
    /// </summary>
    internal sealed class CollectionTypeDefinition : Node
    {
        private readonly Node _elementTypeDef;

        /// <summary>
        /// Initializes collection type definition using the element type definition.
        /// </summary>
        internal CollectionTypeDefinition(Node elementTypeDef)
        {
            _elementTypeDef = elementTypeDef;
        }

        /// <summary>
        /// Returns collection element type defintion.
        /// </summary>
        internal Node ElementTypeDef
        {
            get { return _elementTypeDef; }
        }
    }
}
