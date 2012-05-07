namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Defines the binding for the input set to a <see cref="DbGroupByExpression"/>.
    /// In addition to the properties of <see cref="DbExpressionBinding"/>, DbGroupExpressionBinding
    /// also provides access to the group element via the <seealso cref="GroupVariable"/> variable reference
    /// and to the group aggregate via the <seealso cref="GroupAggregate"/> property.
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
        /// Gets the <see cref="DbExpression"/> that defines the input set.
        /// </summary>
        public DbExpression Expression
        {
            get { return _expr; }
        }

        /// <summary>
        /// Gets the name assigned to the element variable.
        /// </summary>
        public string VariableName
        {
            get { return _varRef.VariableName; }
        }

        /// <summary>
        /// Gets the type metadata of the element variable.
        /// </summary>
        public TypeUsage VariableType
        {
            get { return _varRef.ResultType; }
        }

        /// <summary>
        /// Gets the DbVariableReferenceExpression that references the element variable.
        /// </summary>
        public DbVariableReferenceExpression Variable
        {
            get { return _varRef; }
        }

        /// <summary>
        /// Gets the name assigned to the group element variable.
        /// </summary>
        public string GroupVariableName
        {
            get { return _groupVarRef.VariableName; }
        }

        /// <summary>
        /// Gets the type metadata of the group element variable.
        /// </summary>
        public TypeUsage GroupVariableType
        {
            get { return _groupVarRef.ResultType; }
        }

        /// <summary>
        /// Gets the DbVariableReferenceExpression that references the group element variable.
        /// </summary>
        public DbVariableReferenceExpression GroupVariable
        {
            get { return _groupVarRef; }
        }

        /// <summary>
        /// Gets the DbGroupAggregate that represents the collection of elements of the group. 
        /// </summary>
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
