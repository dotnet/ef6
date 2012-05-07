namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents the retrieval of elements of the specified type from the given set argument.
    /// </summary>
    public sealed class DbOfTypeExpression : DbUnaryExpression
    {
        private readonly TypeUsage _ofType;

        internal DbOfTypeExpression(DbExpressionKind ofTypeKind, TypeUsage collectionResultType, DbExpression argument, TypeUsage type)
            : base(ofTypeKind, collectionResultType, argument)
        {
            Debug.Assert(
                DbExpressionKind.OfType == ofTypeKind ||
                DbExpressionKind.OfTypeOnly == ofTypeKind,
                "ExpressionKind for DbOfTypeExpression must be OfType or OfTypeOnly");

            //
            // Assign the requested element type to the OfType property.
            //
            _ofType = type;
        }

        /// <summary>
        /// Gets the metadata of the type of elements that should be retrieved from the set argument.
        /// </summary>
        public TypeUsage OfType
        {
            get { return _ofType; }
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