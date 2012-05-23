namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents a reference to a parameter declared on the command tree that contains this expression.
    /// </summary>
    public class DbParameterReferenceExpression : DbExpression
    {
        private readonly string _name;

        internal DbParameterReferenceExpression()
        {
        }

        internal DbParameterReferenceExpression(TypeUsage type, string name)
            : base(DbExpressionKind.ParameterReference, type)
        {
            Debug.Assert(DbCommandTree.IsValidParameterName(name), "DbParameterReferenceExpression name should be valid");

            _name = name;
        }

        /// <summary>
        /// Gets the name of the referenced parameter.
        /// </summary>
        public virtual string ParameterName
        {
            get { return _name; }
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
