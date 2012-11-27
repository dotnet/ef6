// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents the invocation of a function.
    /// </summary>
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
            Debug.Assert(function != null, "DbFunctionExpression function cannot be null");
            Debug.Assert(arguments != null, "DbFunctionExpression arguments cannot be null");
            Debug.Assert(
                ReferenceEquals(resultType, function.ReturnParameter.TypeUsage),
                "DbFunctionExpression result type must be function return type");

            _functionInfo = function;
            _arguments = arguments;
        }

        /// <summary>
        ///     Gets the metadata for the function to invoke.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Function")]
        public virtual EdmFunction Function
        {
            get { return _functionInfo; }
        }

        /// <summary>
        ///     Gets an <see cref="DbExpression" /> list that provides the arguments to the function.
        /// </summary>
        public virtual IList<DbExpression> Arguments
        {
            get { return _arguments; }
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor"> An instance of DbExpressionVisitor. </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null
        /// </exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor"> An instance of a typed DbExpressionVisitor that produces a result value of type TResultType. </param>
        /// <typeparam name="TResultType">
        ///     The type of the result produced by <paramref name="visitor" />
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null
        /// </exception>
        /// <returns>
        ///     An instance of <typeparamref name="TResultType" /> .
        /// </returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
