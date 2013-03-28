// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents an invocation of a function. This class cannot be inherited.</summary>
    public class DbFunctionExpression : DbExpression
    {
        private readonly EdmFunction _functionInfo;
        private readonly DbExpressionList _arguments;

        internal DbFunctionExpression()
        {
        }

        internal DbFunctionExpression(TypeUsage resultType, EdmFunction function, DbExpressionList arguments)
            : base(DbExpressionKind.Function, resultType)
        {
            DebugCheck.NotNull(function);
            DebugCheck.NotNull(arguments);
            Debug.Assert(
                ReferenceEquals(resultType, function.ReturnParameter.TypeUsage),
                "DbFunctionExpression result type must be function return type");

            _functionInfo = function;
            _arguments = arguments;
        }

        /// <summary>Gets the metadata for the function to invoke.</summary>
        /// <returns>The metadata for the function to invoke.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Function")]
        public virtual EdmFunction Function
        {
            get { return _functionInfo; }
        }

        /// <summary>
        ///     Gets an <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> list that provides the arguments to the function.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> list that provides the arguments to the function.
        /// </returns>
        public virtual IList<DbExpression> Arguments
        {
            get { return _arguments; }
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
