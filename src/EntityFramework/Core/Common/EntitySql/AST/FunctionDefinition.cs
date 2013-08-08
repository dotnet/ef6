// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents an ast node for an inline function definition.
    /// </summary>
    internal sealed class FunctionDefinition : Node
    {
        private readonly Identifier _name;
        private readonly NodeList<PropDefinition> _paramDefList;
        private readonly Node _body;
        private readonly int _startPosition;
        private readonly int _endPosition;

        /// <summary>
        /// Initializes function definition using the name, the optional argument definitions and the body expression.
        /// </summary>
        internal FunctionDefinition(Identifier name, NodeList<PropDefinition> argDefList, Node body, int startPosition, int endPosition)
        {
            _name = name;
            _paramDefList = argDefList;
            _body = body;
            _startPosition = startPosition;
            _endPosition = endPosition;
        }

        /// <summary>
        /// Returns function name.
        /// </summary>
        internal string Name
        {
            get { return _name.Name; }
        }

        /// <summary>
        /// Returns optional parameter definition list. May be null.
        /// </summary>
        internal NodeList<PropDefinition> Parameters
        {
            get { return _paramDefList; }
        }

        /// <summary>
        /// Returns function body.
        /// </summary>
        internal Node Body
        {
            get { return _body; }
        }

        /// <summary>
        /// Returns start position of the function definition in the command text.
        /// </summary>
        internal int StartPosition
        {
            get { return _startPosition; }
        }

        /// <summary>
        /// Returns end position of the function definition in the command text.
        /// </summary>
        internal int EndPosition
        {
            get { return _endPosition; }
        }
    }
}
