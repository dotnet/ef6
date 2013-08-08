// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>Implements the basic functionality required by expressions that accept two expression operands.</summary>
    public abstract class DbBinaryExpression : DbExpression
    {
        private readonly DbExpression _left;
        private readonly DbExpression _right;

        internal DbBinaryExpression()
        {
        }

        internal DbBinaryExpression(DbExpressionKind kind, TypeUsage type, DbExpression left, DbExpression right)
            : base(kind, type)
        {
            DebugCheck.NotNull(left);
            DebugCheck.NotNull(right);

            _left = left;
            _right = right;
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the left argument.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the left argument.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// The expression is not associated with the command tree of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbBinaryExpression" />
        /// ,or its result type is not equal or promotable to the required type for the left argument.
        /// </exception>
        public virtual DbExpression Left
        {
            get { return _left; }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the right argument.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the right argument.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// The expression is not associated with the command tree of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbBinaryExpression" />
        /// ,or its result type is not equal or promotable to the required type for the right argument.
        /// </exception>
        public virtual DbExpression Right
        {
            get { return _right; }
        }
    }
}
