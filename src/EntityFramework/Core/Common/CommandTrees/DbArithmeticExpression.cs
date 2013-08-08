// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Represents an arithmetic operation applied to numeric arguments.
    /// Addition, subtraction, multiplication, division, modulo, and negation are arithmetic operations.
    /// This class cannot be inherited.
    /// </summary>
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

            DebugCheck.NotNull(args);

            Debug.Assert(
                (DbExpressionKind.UnaryMinus == kind && 1 == args.Count) ||
                2 == args.Count,
                "Incorrect number of arguments specified to DbArithmeticExpression"
                );

            _args = args;
        }

        /// <summary>
        /// Gets the list of <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> elements that define the current arguments.
        /// </summary>
        /// <returns>
        /// A fixed-size list of <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> elements.
        /// </returns>
        public IList<DbExpression> Arguments
        {
            get { return _args; }
        }

        /// <summary>Implements the visitor pattern for expressions that do not produce a result value.</summary>
        /// <param name="visitor">
        /// An instance of <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        /// <summary>Implements the visitor pattern for expressions that produce a result value of a specific type.</summary>
        /// <returns>
        /// A result value of a specific type produced by
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />
        /// .
        /// </returns>
        /// <param name="visitor">
        /// An instance of a typed <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" /> that produces a result value of a specific type.
        /// </param>
        /// <typeparam name="TResultType">The type of the result produced by  visitor .</typeparam>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
