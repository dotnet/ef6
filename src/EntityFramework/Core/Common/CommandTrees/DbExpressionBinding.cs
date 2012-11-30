// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Describes a binding for an expression. Conceptually similar to a foreach loop
    ///     in C#. The DbExpression property defines the collection being iterated over,
    ///     while the Var property provides a means to reference the current element
    ///     of the collection during the iteration. DbExpressionBinding is used to describe the set arguments
    ///     to relational expressions such as <see cref="DbFilterExpression" />, <see cref="DbProjectExpression" />
    ///     and <see cref="DbJoinExpression" />.
    /// </summary>
    /// <seealso cref="DbExpression" />
    /// <seealso cref="Variable" />
    public sealed class DbExpressionBinding
    {
        private readonly DbExpression _expr;
        private readonly DbVariableReferenceExpression _varRef;

        internal DbExpressionBinding(DbExpression input, DbVariableReferenceExpression varRef)
        {
            DebugCheck.NotNull(input);
            DebugCheck.NotNull(varRef);

            _expr = input;
            _varRef = varRef;
        }

        /// <summary>
        ///     Gets the <see cref="DbExpression" /> that defines the input set.
        /// </summary>
        public DbExpression Expression
        {
            get { return _expr; }
        }

        /// <summary>
        ///     Gets the name assigned to the element variable.
        /// </summary>
        public string VariableName
        {
            get { return _varRef.VariableName; }
        }

        /// <summary>
        ///     Gets the type metadata of the element variable.
        /// </summary>
        public TypeUsage VariableType
        {
            get { return _varRef.ResultType; }
        }

        /// <summary>
        ///     Gets the <see cref="DbVariableReferenceExpression" /> that references the element variable.
        /// </summary>
        public DbVariableReferenceExpression Variable
        {
            get { return _varRef; }
        }
    }
}
