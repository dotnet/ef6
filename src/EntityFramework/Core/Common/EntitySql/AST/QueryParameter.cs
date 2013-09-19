// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    using System.Data.Entity.Resources;

    /// <summary>
    /// Represents an ast node for a query parameter.
    /// </summary>
    internal sealed class QueryParameter : Node
    {
        private readonly string _name;

        /// <summary>
        /// Initializes parameter
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.Entity.Core.EntityException">Thrown if the parameter name does not conform to the expected format</exception>
        /// </remarks>
        internal QueryParameter(string parameterName, string query, int inputPos)
            : base(query, inputPos)
        {
            _name = parameterName.Substring(1);

            //
            // valid parameter format is: @({LETTER})(_|{LETTER}|{DIGIT})*
            //
            if (_name.StartsWith("_", StringComparison.OrdinalIgnoreCase)
                || Char.IsDigit(_name, 0))
            {
                var errCtx = ErrCtx;
                var message = Strings.InvalidParameterFormat(_name);
                throw EntitySqlException.Create(errCtx, message, null);
            }
        }

        /// <summary>
        /// Returns parameter parameterName (without @ sign).
        /// </summary>
        internal string Name
        {
            get { return _name; }
        }
    }
}
