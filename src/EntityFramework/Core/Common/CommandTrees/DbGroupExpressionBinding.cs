// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Defines the binding for the input set to a <see cref="DbGroupByExpression" />.
    ///     In addition to the properties of <see cref="DbExpressionBinding" />, DbGroupExpressionBinding
    ///     also provides access to the group element via the <seealso cref="GroupVariable" /> variable reference
    ///     and to the group aggregate via the <seealso cref="GroupAggregate" /> property.
    /// </summary>
    public sealed class DbGroupExpressionBinding
    {
        private readonly DbExpression _expr;
        private readonly DbVariableReferenceExpression _varRef;
        private readonly DbVariableReferenceExpression _groupVarRef;
        private DbGroupAggregate _groupAggregate;

        internal DbGroupExpressionBinding(
            DbExpression input, DbVariableReferenceExpression inputRef, DbVariableReferenceExpression groupRef)
        {
            _expr = input;
            _varRef = inputRef;
            _groupVarRef = groupRef;
        }

        /// <summary>
        ///     Gets or sets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the input set.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the input set.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The expression is not associated with the command tree of the
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbGroupExpressionBinding" />
        ///     , or its result type is not equal or promotable to the result type of the current value of the property.
        /// </exception>
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
        /// <returns>The type metadata of the element variable.</returns>
        public TypeUsage VariableType
        {
            get { return _varRef.ResultType; }
        }

        /// <summary>
        ///     Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbVariableReferenceExpression" /> that references the element variable.
        /// </summary>
        /// <returns>A reference to the element variable.</returns>
        public DbVariableReferenceExpression Variable
        {
            get { return _varRef; }
        }

        /// <summary>Gets the name assigned to the group element variable.</summary>
        /// <returns>The name assigned to the group element variable.</returns>
        public string GroupVariableName
        {
            get { return _groupVarRef.VariableName; }
        }

        /// <summary>Gets the type metadata of the group element variable.</summary>
        /// <returns>The type metadata of the group element variable.</returns>
        public TypeUsage GroupVariableType
        {
            get { return _groupVarRef.ResultType; }
        }

        /// <summary>
        ///     Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbVariableReferenceExpression" /> that references the group element variable.
        /// </summary>
        /// <returns>A reference to the group element variable.</returns>
        public DbVariableReferenceExpression GroupVariable
        {
            get { return _groupVarRef; }
        }

        /// <summary>
        ///     Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbGroupAggregate" /> that represents the collection of elements in the group.
        /// </summary>
        /// <returns>The elements in the group.</returns>
        public DbGroupAggregate GroupAggregate
        {
            get
            {
                if (_groupAggregate == null)
                {
                    _groupAggregate = DbExpressionBuilder.GroupAggregate(GroupVariable);
                }
                return _groupAggregate;
            }
        }
    }
}
