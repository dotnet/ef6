// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Represents a comparison operation applied to two arguments. Equality, greater than, greater than or equal, less than, less than or equal, and inequality are comparison operations. This class cannot be inherited.  </summary>
    /// <remarks>
    /// DbComparisonExpression requires that its arguments have a common result type
    /// that is equality comparable (for <see cref="DbExpressionKind" />.Equals and <see cref="DbExpressionKind" />.NotEquals),
    /// order comparable (for <see cref="DbExpressionKind" />.GreaterThan and <see cref="DbExpressionKind" />.LessThan),
    /// or both (for <see cref="DbExpressionKind" />.GreaterThanOrEquals and <see cref="DbExpressionKind" />.LessThanOrEquals).
    /// </remarks> 
    public sealed class DbComparisonExpression : DbBinaryExpression
    {
        internal DbComparisonExpression(DbExpressionKind kind, TypeUsage booleanResultType, DbExpression left, DbExpression right)
            : base(kind, booleanResultType, left, right)
        {
            DebugCheck.NotNull(left);
            DebugCheck.NotNull(right);
            Debug.Assert(TypeSemantics.IsBooleanType(booleanResultType), "DbComparisonExpression result type must be a Boolean type");
            Debug.Assert(
                DbExpressionKind.Equals == kind ||
                DbExpressionKind.LessThan == kind ||
                DbExpressionKind.LessThanOrEquals == kind ||
                DbExpressionKind.GreaterThan == kind ||
                DbExpressionKind.GreaterThanOrEquals == kind ||
                DbExpressionKind.NotEquals == kind,
                "Invalid DbExpressionKind used in DbComparisonExpression: " + Enum.GetName(typeof(DbExpressionKind), kind)
                );
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
        /// <typeparam name="TResultType">The type of the result produced by  visitor. </typeparam>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
