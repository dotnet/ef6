namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Aggregates are pseudo-expressions. They look and feel like expressions, but 
    /// are severely restricted in where they can appear - only in the aggregates clause
    /// of a group-by expression.
    /// </summary>
    public abstract class DbAggregate
    {
        private readonly DbExpressionList _args;
        private readonly TypeUsage _type;

        internal DbAggregate(TypeUsage resultType, DbExpressionList arguments)
        {
            Debug.Assert(resultType != null, "DbAggregate.ResultType cannot be null");
            Debug.Assert(arguments != null, "DbAggregate.Arguments cannot be null");
            Debug.Assert(arguments.Count == 1, "DbAggregate requires a single argument");

            _type = resultType;
            _args = arguments;
        }

        /// <summary>
        /// Gets the result type of this aggregate
        /// </summary>
        public TypeUsage ResultType
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the list of expressions that define the arguments to the aggregate.
        /// </summary>
        public IList<DbExpression> Arguments
        {
            get { return _args; }
        }
    }
}