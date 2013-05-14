// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    ///     Represents an ast node for a property definition (name/type)
    /// </summary>
    internal sealed class PropDefinition : Node
    {
        private readonly Identifier _name;
        private readonly Node _typeDefExpr;

        /// <summary>
        ///     Initializes property definition using the name and the type definition.
        /// </summary>
        internal PropDefinition(Identifier name, Node typeDefExpr)
        {
            _name = name;
            _typeDefExpr = typeDefExpr;
        }

        /// <summary>
        ///     Returns property name.
        /// </summary>
        internal Identifier Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Returns property type.
        /// </summary>
        internal Node Type
        {
            get { return _typeDefExpr; }
        }
    }
}
