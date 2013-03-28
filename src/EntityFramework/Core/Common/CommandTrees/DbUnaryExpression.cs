// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>Implements the basic functionality required by expressions that accept a single expression argument. </summary>
    public abstract class DbUnaryExpression : DbExpression
    {
        private readonly DbExpression _argument;

        internal DbUnaryExpression()
        {
        }

        internal DbUnaryExpression(DbExpressionKind kind, TypeUsage resultType, DbExpression argument)
            : base(kind, resultType)
        {
            DebugCheck.NotNull(argument);

            _argument = argument;
        }

        /// <summary>
        ///     Gets or sets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the argument.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the argument.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The expression is not associated with the command tree of a
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbUnaryExpression" />
        ///     , or its result type is not equal or promotable to the required type for the argument.
        /// </exception>
        public virtual DbExpression Argument
        {
            get { return _argument; }
        }
    }
}
