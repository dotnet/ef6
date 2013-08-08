// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Describes a binding for an expression. Conceptually similar to a foreach loop
    /// in C#. The DbExpression property defines the collection being iterated over,
    /// while the Var property provides a means to reference the current element
    /// of the collection during the iteration. DbExpressionBinding is used to describe the set arguments
    /// to relational expressions such as <see cref="DbFilterExpression" />, <see cref="DbProjectExpression" />
    /// and <see cref="DbJoinExpression" />.
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
        /// Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the input set.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the input set.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">The expression is not associated with the command tree of the binding, or its result type is not equal or promotable to the result type of the current value of the property.</exception>
        public DbExpression Expression
        {
            get { return _expr; }
        }

        /// <summary>Gets the name assigned to the element variable.</summary>
        /// <returns>The name assigned to the element variable.</returns>
        public string VariableName
        {
            get { return _varRef.VariableName; }
        }

        /// <summary>Gets the type metadata of the element variable.</summary>
        /// <returns>The type metadata of the element variable. </returns>
        public TypeUsage VariableType
        {
            get { return _varRef.ResultType; }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbVariableReferenceExpression" /> that references the element variable.
        /// </summary>
        /// <returns>The variable reference.</returns>
        public DbVariableReferenceExpression Variable
        {
            get { return _varRef; }
        }
    }
}
