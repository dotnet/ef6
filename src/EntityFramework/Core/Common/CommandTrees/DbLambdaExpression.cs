// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Allows the application of a lambda function to arguments represented by
    ///     <see
    ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" />
    ///     objects.
    /// </summary>
    public sealed class DbLambdaExpression : DbExpression
    {
        private readonly DbLambda _lambda;
        private readonly DbExpressionList _arguments;

        internal DbLambdaExpression(TypeUsage resultType, DbLambda lambda, DbExpressionList args)
            : base(DbExpressionKind.Lambda, resultType)
        {
            DebugCheck.NotNull(lambda);
            DebugCheck.NotNull(args);
            Debug.Assert(
                ReferenceEquals(resultType, lambda.Body.ResultType), "DbLambdaExpression result type must be Lambda body result type");
            Debug.Assert(lambda.Variables.Count == args.Count, "DbLambdaExpression argument count does not match Lambda parameter count");

            _lambda = lambda;
            _arguments = args;
        }

        /// <summary>
        ///     Gets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLambda" /> representing the Lambda function applied by this expression.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbLambda" /> representing the Lambda function applied by this expression.
        /// </returns>
        public DbLambda Lambda
        {
            get { return _lambda; }
        }

        /// <summary>
        ///     Gets a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> list that provides the arguments to which the Lambda function should be applied.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> list.
        /// </returns>
        public IList<DbExpression> Arguments
        {
            get { return _arguments; }
        }

        /// <summary>The visitor pattern method for expression visitors that do not produce a result value.</summary>
        /// <param name="visitor">
        ///     An instance of <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        /// <summary>The visitor pattern method for expression visitors that produce a result value of a specific type.</summary>
        /// <returns>The type of the result produced by the expression visitor.</returns>
        /// <param name="visitor">
        ///     An instance of a typed <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" /> that produces a result value of type TResultType.
        /// </param>
        /// <typeparam name="TResultType">The type of the result produced by  visitor </typeparam>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null</exception>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
