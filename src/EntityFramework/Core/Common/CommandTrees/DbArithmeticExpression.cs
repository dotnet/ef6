namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents an arithmetic operation (addition, subtraction, multiplication, division, modulo or negation) applied to two numeric arguments.
    /// </summary>
    /// <remarks>DbArithmeticExpression requires that its arguments have a common numeric result type</remarks>
    public sealed class DbArithmeticExpression : DbExpression
    {
        private readonly DbExpressionList _args;

        internal DbArithmeticExpression(DbExpressionKind kind, TypeUsage numericResultType, DbExpressionList args)
            : base(kind, numericResultType)
        {
            Debug.Assert(TypeSemantics.IsNumericType(numericResultType), "DbArithmeticExpression result type must be numeric");

            Debug.Assert(
                DbExpressionKind.Divide == kind ||
                DbExpressionKind.Minus == kind ||
                DbExpressionKind.Modulo == kind ||
                DbExpressionKind.Multiply == kind ||
                DbExpressionKind.Plus == kind ||
                DbExpressionKind.UnaryMinus == kind,
                "Invalid DbExpressionKind used in DbArithmeticExpression: " + Enum.GetName(typeof(DbExpressionKind), kind)
                );

            Debug.Assert(args != null, "DbArithmeticExpression arguments cannot be null");

            Debug.Assert(
                (DbExpressionKind.UnaryMinus == kind && 1 == args.Count) ||
                2 == args.Count,
                "Incorrect number of arguments specified to DbArithmeticExpression"
                );

            _args = args;
        }

        /// <summary>
        /// Gets the list of expressions that define the current arguments.
        /// </summary>
        /// <remarks>
        ///     The <code>Arguments</code> property returns a fixed-size list of <see cref="DbExpression"/> elements.
        ///     <see cref="DbArithmeticExpression"/> requires that all elements of it's <code>Arguments</code> list
        ///     have a common numeric result type.
        /// </remarks>
        public IList<DbExpression> Arguments
        {
            get { return _args; }
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
