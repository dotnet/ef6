// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>Represents the projection of a given input set over the specified expression. This class cannot be inherited.  </summary>
    public sealed class DbProjectExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly DbExpression _projection;

        internal DbProjectExpression(TypeUsage resultType, DbExpressionBinding input, DbExpression projection)
            : base(DbExpressionKind.Project, resultType)
        {
            DebugCheck.NotNull(input);
            DebugCheck.NotNull(projection);

            _input = input;
            _projection = projection;
        }

        /// <summary>
        ///     Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> that specifies the input set.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionBinding" /> that specifies the input set.
        /// </returns>
        public DbExpressionBinding Input
        {
            get { return _input; }
        }

        /// <summary>
        ///     Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the projection.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the projection.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The expression is not associated with the command tree of the
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbProjectExpression" />
        ///     , or its result type is not equal or promotable to the reference type of the current projection.
        /// </exception>
        public DbExpression Projection
        {
            get { return _projection; }
        }

        /// <summary>Implements the visitor pattern for expressions that do not produce a result value.</summary>
        /// <param name="visitor">
        ///     An instance of <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        /// <summary>Implements the visitor pattern for expressions that produce a result value of a specific type.</summary>
        /// <returns>
        ///     A result value of a specific type produced by
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />
        ///     .
        /// </returns>
        /// <param name="visitor">
        ///     An instance of a typed <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" /> that produces a result value of a specific type.
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
