// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Represents a quantifier operation of the specified kind over the elements of the specified input set. This class cannot be inherited.  </summary>
    public sealed class DbQuantifierExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly DbExpression _predicate;

        internal DbQuantifierExpression(
            DbExpressionKind kind, TypeUsage booleanResultType, DbExpressionBinding input, DbExpression predicate)
            : base(kind, booleanResultType)
        {
            DebugCheck.NotNull(input);
            DebugCheck.NotNull(predicate);
            Debug.Assert(
                TypeSemantics.IsPrimitiveType(booleanResultType, PrimitiveTypeKind.Boolean),
                "DbQuantifierExpression must have a Boolean result type");
            Debug.Assert(
                TypeSemantics.IsPrimitiveType(predicate.ResultType, PrimitiveTypeKind.Boolean),
                "DbQuantifierExpression predicate must have a Boolean result type");

            _input = input;
            _predicate = predicate;
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> that specifies the input set.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> that specifies the input set.
        /// </returns>
        public DbExpressionBinding Input
        {
            get { return _input; }
        }

        /// <summary>Gets the Boolean predicate that should be evaluated for each element in the input set.</summary>
        /// <returns>The Boolean predicate that should be evaluated for each element in the input set.</returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// The expression is not associated with the command tree for the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbQuantifierExpression" />
        /// ,or its result type is not a Boolean type.
        /// </exception>
        public DbExpression Predicate
        {
            get { return _predicate; }
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
