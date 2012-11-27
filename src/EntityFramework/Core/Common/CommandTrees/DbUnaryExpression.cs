// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     The abstract base type for expressions that accept a single expression operand
    /// </summary>
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
        ///     Gets the <see cref="DbExpression" /> that defines the argument.
        /// </summary>
        public virtual DbExpression Argument
        {
            get { return _argument; }
        }
    }
}
