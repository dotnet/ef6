namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents an unconditional join operation between the given collection arguments
    /// </summary>
    public sealed class DbCrossJoinExpression : DbExpression
    {
        private readonly ReadOnlyCollection<DbExpressionBinding> _inputs;

        internal DbCrossJoinExpression(TypeUsage collectionOfRowResultType, ReadOnlyCollection<DbExpressionBinding> inputs)
            : base(DbExpressionKind.CrossJoin, collectionOfRowResultType)
        {
            Debug.Assert(inputs != null, "DbCrossJoin inputs cannot be null");
            Debug.Assert(inputs.Count >= 2, "DbCrossJoin requires at least two inputs");

            _inputs = inputs;
        }

        /// <summary>
        /// Gets an <see cref="DbExpressionBinding"/> list that provide the input sets to the join.
        /// </summary>
        public IList<DbExpressionBinding> Inputs
        {
            get { return _inputs; }
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
            else
            {
                throw new ArgumentNullException("visitor");
            }
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            if (visitor != null)
            {
                return visitor.Visit(this);
            }
            else
            {
                throw new ArgumentNullException("visitor");
            }
        }
    }
}
